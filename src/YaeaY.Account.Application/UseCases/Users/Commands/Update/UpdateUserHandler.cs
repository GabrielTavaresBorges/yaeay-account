using MediatR;
using Microsoft.Extensions.Logging;
using YaeaY.Account.Domain.Abstraction.Errors;
using YaeaY.Account.Domain.Abstraction.Errors.Enumerators;
using YaeaY.Account.Domain.Abstraction.Exceptions;
using YaeaY.Account.Domain.Abstraction.Interfaces;
using YaeaY.Account.Domain.Abstraction.Result;
using YaeaY.Account.Domain.Entities.UserDocuments;
using YaeaY.Account.Domain.Errors.Users;
using YaeaY.Account.Application.Services.TelephoneNumbers.Interfaces;
using YaeaY.Account.Application.Services.DocumentImages.Interfaces;
using YaeaY.Account.Domain.Factories.Telephones;
using YaeaY.Account.Domain.Repositories.Users;
using YaeaY.Account.Domain.ValueObjects.Dates;
using YaeaY.Account.Domain.ValueObjects.Documents;
using YaeaY.Account.Domain.ValueObjects.Names;
using YaeaY.Account.Domain.ValueObjects.Telephones;

namespace YaeaY.Account.Application.UseCases.Users.Commands.Update;

public sealed class Handler(
    IUserRepository userRepository,
    IUnitOfWork unitOfWork,
    ITelephoneNumberService telephoneNumberService,
    ITelephoneNumberFactory telephoneNumberFactory,
    IDocumentImageStorage documentImageStorage,
    ILogger<Handler> logger)
    : IRequestHandler<Command, Result<Response>>
{
    public async Task<Result<Response>> Handle(Command command, CancellationToken cancellationToken)
    {
        try
        {
            var user = await userRepository.GetByIdWithDocumentsAsync(command.Id, cancellationToken);
            if (user is null)
                return Result<Response>.Failure(UserErrors.NotFound);

            var updatedFields = new List<string>();
            var updatedDocuments = new List<CpfDocumentResponse>();
            var addedPhones = new List<YaeaY.Account.Domain.Entities.UserPhones.UserPhone>();
            var documentImagesToDelete = new List<string>();

            if (command.FullName is not null &&
                !string.Equals(user.FullName.Name, command.FullName.Trim(), StringComparison.Ordinal))
            {
                var result = FullName.Create(command.FullName);
                if (result.IsFailure) return Result<Response>.Failure(result.Error);
                user.ChangeFullName(result.Value);
                updatedFields.Add(nameof(command.FullName));
            }

            if (command.BirthDate.HasValue && user.BirthDate.Date != command.BirthDate.Value)
            {
                var result = BirthDate.Create(command.BirthDate.Value);
                if (result.IsFailure) return Result<Response>.Failure(result.Error);
                user.ChangeBirthDate(result.Value);
                updatedFields.Add(nameof(command.BirthDate));
            }

            if (command.Gender.HasValue && user.Gender != command.Gender.Value)
            {
                user.ChangeGender(command.Gender.Value);
                updatedFields.Add(nameof(command.Gender));
            }

            if (command.Phones is not null)
            {
                var phonesChanged = false;
                Guid? selectedPrimaryPhoneId = null;

                foreach (var phoneInput in command.Phones)
                {
                    var phoneNumberResult = CreateTelephoneNumber(phoneInput);
                    if (phoneNumberResult.IsFailure)
                        return Result<Response>.Failure(phoneNumberResult.Error);

                    Guid phoneId;
                    if (phoneInput.Id.HasValue)
                    {
                        phoneId = phoneInput.Id.Value;
                        phonesChanged |= user.ChangePhone(phoneId, phoneNumberResult.Value);
                    }
                    else
                    {
                        var addedPhone = user.AddPhone(phoneNumberResult.Value);
                        phoneId = addedPhone.Id;
                        addedPhones.Add(addedPhone);
                        phonesChanged = true;
                    }

                    if (phoneInput.IsPrimary)
                        selectedPrimaryPhoneId = phoneId;
                }

                if (!selectedPrimaryPhoneId.HasValue)
                    return Result<Response>.Failure(UserErrors.PrimaryPhoneRequired);

                phonesChanged |= user.SetPrimaryPhone(selectedPrimaryPhoneId.Value);

                var requestedPhoneIds = command.Phones
                    .Where(phone => phone.Id.HasValue)
                    .Select(phone => phone.Id!.Value)
                    .ToHashSet();
                var addedPhoneIds = addedPhones.Select(phone => phone.Id).ToHashSet();

                foreach (var existingPhone in user.Phones.ToArray())
                {
                    if (requestedPhoneIds.Contains(existingPhone.Id) || addedPhoneIds.Contains(existingPhone.Id))
                        continue;

                    user.RemovePhone(existingPhone.Id);
                    phonesChanged = true;
                }

                if (phonesChanged)
                    updatedFields.Add(nameof(command.Phones));
            }

            foreach (var input in command.CpfDocumentsToAdd ?? [])
            {
                var cpfResult = Cpf.Create(input.Number);
                if (cpfResult.IsFailure) return Result<Response>.Failure(cpfResult.Error);

                var images = (input.Images ?? [])
                    .Select(image => UserDocumentImage.Create(
                        image.Position,
                        image.StorageObjectKey,
                        image.OriginalFileName,
                        image.ContentType,
                        image.FileSizeBytes,
                        image.Sha256Hash))
                    .ToArray();

                if (images.Any(image => !image.StorageObjectKey.StartsWith($"users/{user.Id:N}/", StringComparison.Ordinal))
                    || !(await Task.WhenAll(images.Select(image => documentImageStorage.ExistsAsync(image.StorageObjectKey, cancellationToken))).ConfigureAwait(false)).All(exists => exists))
                {
                    return Result<Response>.Failure(new Error(
                        "document_image.not_found",
                        "Uma ou mais imagens do documento não estão disponíveis para salvar.",
                        ErrorCategory.Validation,
                        ErrorRule.NotFound));
                }

                var oldStorageKeys = user.Documents
                    .Where(document => document.DocumentType == YaeaY.Account.Domain.Enumerators.DocumentType.Cpf)
                    .SelectMany(document => document.Images)
                    .Select(image => image.StorageObjectKey)
                    .ToHashSet(StringComparer.Ordinal);

                var document = user.UpsertCpfDocument(cpfResult.Value, images, out var documentChanged);
                if (!documentChanged)
                    continue;

                updatedDocuments.Add(ToResponse(document));
                oldStorageKeys.ExceptWith(images.Select(image => image.StorageObjectKey));
                documentImagesToDelete.AddRange(oldStorageKeys);
            }

            if (updatedDocuments.Count > 0)
                updatedFields.Add(nameof(command.CpfDocumentsToAdd));

            if (updatedFields.Count == 0)
                return Result<Response>.Success(new Response(user.Id, [], [], "No changes to apply."));

            await userRepository.UpdateUserAsync(user, addedPhones, cancellationToken);
            await unitOfWork.CommitAsync(cancellationToken);
            foreach (var storageObjectKey in documentImagesToDelete.Distinct(StringComparer.Ordinal))
            {
                try
                {
                    await documentImageStorage.DeleteAsync(storageObjectKey, cancellationToken);
                }
                catch (Exception exception)
                {
                    logger.LogWarning(exception, "Unable to remove replaced CPF image {StorageObjectKey} for user {UserId}.", storageObjectKey, user.Id);
                }
            }

            return Result<Response>.Success(new Response(user.Id, updatedFields, updatedDocuments, "User updated successfully."));
        }
        catch (DomainException exception)
        {
            logger.LogWarning(exception, "Domain error updating user {UserId}.", command.Id);
            return Result<Response>.Failure(exception.Error);
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Unexpected error updating user {UserId}.", command.Id);
            return Result<Response>.Failure(new Error(
                "unexpected.error",
                "An unexpected error occurred.",
                ErrorCategory.Unexpected,
                ErrorRule.Unexpected));
        }
    }

    private Result<TelephoneNumber> CreateTelephoneNumber(PhoneInput input)
    {
        var identificationResult = telephoneNumberService.ValidateAndIdentify(
            input.CallingCode,
            input.RegionCode,
            input.AreaCode,
            input.PhoneNumber,
            input.PhoneType);

        if (identificationResult.IsFailure)
            return Result<TelephoneNumber>.Failure(identificationResult.Error);

        var identification = identificationResult.Value;
        return telephoneNumberFactory.Create(
            identification.CallingCode,
            identification.RegionCode,
            identification.AreaCode,
            identification.TelephoneType,
            identification.NationalNumber,
            identification.InternationalNumber);
    }

    private static CpfDocumentResponse ToResponse(UserDocument document)
    {
        var cpf = document.Cpf ?? throw new InvalidOperationException("A CPF document must contain its CPF detail.");
        return new CpfDocumentResponse(
            document.Id,
            cpf.Id,
            cpf.Cpf.Number,
            document.IssuerCountry,
            document.CreatedAt,
            document.Images.Select(image => new DocumentImageResponse(
                image.Id,
                image.Position,
                image.StorageObjectKey,
                image.OriginalFileName,
                image.ContentType,
                image.FileSizeBytes,
                image.Sha256Hash,
                image.CreatedAt)).ToArray());
    }
}

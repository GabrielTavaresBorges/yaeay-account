using MediatR;
using Microsoft.Extensions.Logging;
using YaeaY.Account.Application.Services.TelephoneNumbers.Interfaces;
using YaeaY.Account.Domain.Abstraction.Errors;
using YaeaY.Account.Domain.Abstraction.Errors.Enumerators;
using YaeaY.Account.Domain.Abstraction.Exceptions;
using YaeaY.Account.Domain.Abstraction.Interfaces;
using YaeaY.Account.Domain.Abstraction.Result;
using YaeaY.Account.Domain.Entities.UserPhones;
using YaeaY.Account.Domain.Errors.Users;
using YaeaY.Account.Domain.Factories.Telephones;
using YaeaY.Account.Domain.Repositories.Users;
using YaeaY.Account.Domain.ValueObjects.Telephones;

namespace YaeaY.Account.Application.UseCases.Users.Commands.UpdatePhones;

public sealed class Handler(
    IUserRepository userRepository,
    IUnitOfWork unitOfWork,
    ITelephoneNumberService telephoneNumberService,
    ITelephoneNumberFactory telephoneNumberFactory,
    ILogger<Handler> logger) : IRequestHandler<Command, Result<Response>>
{
    public async Task<Result<Response>> Handle(Command command, CancellationToken cancellationToken)
    {
        try
        {
            var user = await userRepository.GetByIdWithPhonesAsync(command.Id, cancellationToken);
            if (user is null)
                return Result<Response>.Failure(UserErrors.NotFound);

            if (command.Phones is null) 
                return Result<Response>.Success(ToResponse(user.Id, user.Phones, false, "No changes to apply."));

            var changed = false;
            var addedPhones = new List<UserPhone>();
            Guid? primaryId = null;

            foreach (var input in command.Phones)
            {
                var number = CreateTelephoneNumber(input);
                if (number.IsFailure) 
                    return Result<Response>.Failure(number.Error);

                Guid phoneId;

                if (input.Id.HasValue)
                {
                    phoneId = input.Id.Value; changed |= user.ChangePhone(phoneId, number.Value);
                }
                else
                {
                    var phone = user.AddPhone(number.Value); phoneId = phone.Id; addedPhones.Add(phone); changed = true;
                }

                if (input.IsPrimary) primaryId = phoneId;
            }

            if (!primaryId.HasValue)
                return Result<Response>.Failure(UserErrors.PrimaryPhoneRequired);

            changed |= user.SetPrimaryPhone(primaryId.Value);
            var requested = command.Phones.Where(phone => phone.Id.HasValue).Select(phone => phone.Id!.Value).ToHashSet();
            var added = addedPhones.Select(phone => phone.Id).ToHashSet();

            foreach (var phone in user.Phones.ToArray())
            {
                if (requested.Contains(phone.Id) || added.Contains(phone.Id)) 
                    continue;

                user.RemovePhone(phone.Id);
                changed = true;
            }

            if (!changed)
                return Result<Response>.Success(ToResponse(user.Id, user.Phones, false, "No changes to apply."));

            await userRepository.UpdateUserPhonesAsync(user, addedPhones, cancellationToken);
            await unitOfWork.CommitAsync(cancellationToken);

            return Result<Response>.Success(ToResponse(user.Id, user.Phones, true, "Phones updated successfully."));
        }
        catch (DomainException exception)
        {
            logger.LogWarning(exception, "Domain error updating phones for user {UserId}.", command.Id); 
            return Result<Response>.Failure(exception.Error); 
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Unexpected error updating phones for user {UserId}.", command.Id); 
            return Result<Response>.Failure(new Error(
                "unexpected.error",
                "An unexpected error occurred.",
                ErrorCategory.Unexpected,
                ErrorRule.Unexpected));
        }
    }

    private Result<TelephoneNumber> CreateTelephoneNumber(PhoneInput input)
    {
        var identified = telephoneNumberService.ValidateAndIdentify(
            input.CallingCode,
            input.RegionCode,
            input.AreaCode,
            input.PhoneNumber,
            input.PhoneType);

        if (identified.IsFailure)
            return Result<TelephoneNumber>.Failure(identified.Error);

        var value = identified.Value;
        return telephoneNumberFactory.Create(
            value.CallingCode,
            value.RegionCode,
            value.AreaCode,
            value.TelephoneType,
            value.NationalNumber,
            value.InternationalNumber);
    }
    private static Response ToResponse(
        Guid id,
        IReadOnlyCollection<UserPhone> phones,
        bool hasChanges,
        string message) => new(id, phones.Select(phone => new PhoneResponse(
            phone.Id,
            phone.CallingCode,
            phone.RegionCode,
            phone.AreaCode,
            phone.PhoneType,
            phone.PhoneNumber,
            phone.IsPrimary)).ToArray(), hasChanges, message);
}

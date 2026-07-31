using MediatR;
using Microsoft.Extensions.Logging;
using YaeaY.Account.Application.Services.Security.Interfaces;
using YaeaY.Account.Application.Services.TelephoneNumbers.Interfaces;
using YaeaY.Account.Domain.Abstraction.Errors;
using YaeaY.Account.Domain.Abstraction.Errors.Enumerators;
using YaeaY.Account.Domain.Abstraction.Exceptions;
using YaeaY.Account.Domain.Abstraction.Interfaces;
using YaeaY.Account.Domain.Abstraction.Result;
using YaeaY.Account.Domain.Entities.AggregateRoots.Users;
using YaeaY.Account.Domain.Factories.Telephones;
using YaeaY.Account.Domain.Repositories.Users;
using YaeaY.Account.Domain.ValueObjects.Dates;
using YaeaY.Account.Domain.ValueObjects.Emails;
using YaeaY.Account.Domain.ValueObjects.Names;
using YaeaY.Account.Domain.ValueObjects.Securities;
using YaeaY.Account.Domain.ValueObjects.Telephones;

namespace YaeaY.Account.Application.UseCases.Users.Commands.Create;

public sealed class Handler : IRequestHandler<Command, Result<Response>>
{
    private readonly IUserRepository _userRepository;
    private readonly IUnityOfWork _unitOfWork;
    private readonly ILogger<Handler> _logger;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ITelephoneNumberService _telephoneNumberService;
    private readonly ITelephoneNumberFactory _telephoneNumberFactory;

    public Handler(
        IUserRepository usersRepository,
        IUnityOfWork unitOfWork,
        ILogger<Handler> logger,
        IPasswordHasher passwordHasher,
        ITelephoneNumberService telephoneNumberService,
        ITelephoneNumberFactory telephoneNumberFactory)
    {
        _userRepository = usersRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
        _passwordHasher = passwordHasher;
        _telephoneNumberService = telephoneNumberService;
        _telephoneNumberFactory = telephoneNumberFactory;
    }

    public async Task<Result<Response>> Handle(Command command, CancellationToken cancellationToken)
    {
        try
        {
            var emailResult = Email.Create(command.EmailAddress);
            if (emailResult.IsFailure)
                return Result<Response>.Failure(emailResult.Error);

            var passwordTextResult = PasswordText.Create(command.Password);
            if (passwordTextResult.IsFailure)
                return Result<Response>.Failure(passwordTextResult.Error);

            var passwordHashResult = _passwordHasher.Hash(passwordTextResult.Value);
            if (passwordHashResult.IsFailure)
                return Result<Response>.Failure(passwordHashResult.Error);

            var fullNameResult = FullName.Create(command.FullName);
            if (fullNameResult.IsFailure)
                return Result<Response>.Failure(fullNameResult.Error);

            var birthDateResult = BirthDate.Create(command.BirthDate);
            if (birthDateResult.IsFailure)
                return Result<Response>.Failure(birthDateResult.Error);

            var initialTelephoneNumberResult = CreateInitialTelephoneNumber(command);

            if (initialTelephoneNumberResult.IsFailure)
                return Result<Response>.Failure(
                    initialTelephoneNumberResult.Error);

            var user = User.Create(
                emailAddress: emailResult.Value,
                passwordHash: passwordHashResult.Value,
                fullName: fullNameResult.Value,
                birthDate: birthDateResult.Value,
                gender: command.Gender,
                initialPhoneNumber: initialTelephoneNumberResult.Value);

            await _userRepository.CreateUserAsync(user, cancellationToken);
            await _unitOfWork.CommitAsync();

            return Result<Response>.Success(
                new Response(
                    id: user.Id,
                    fullName: user.FullName.Name,
                    message: "User created successfully!")
                );
        }
        catch (DomainException ex)
        {
            _logger.LogError(ex, "Domain Error creating user.");
            return Result<Response>.Failure(
                new Error(
                    Code: ex.Code,
                    Message: ex.Message,
                    Category: ex.Category,
                    Rule: ex.Rule));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error creating user.");
            return Result<Response>.Failure(
                new Error(
                    Code: "unexpected.error",
                    Message: "An unexpected error occurred.",
                    Category: ErrorCategory.Unexpected,
                    Rule: ErrorRule.Unexpected));
        }
    }

    private Result<TelephoneNumber> CreateInitialTelephoneNumber(
        Command command)
    {
        var identificationResult =
            _telephoneNumberService.ValidateAndIdentify(
            callingCode: command.CallingCode,
            regionCode: command.RegionCode,
            areaCode: command.AreaCode,
            internationalNumber: command.PhoneNumber,
            expectedPhoneType: command.PhoneType);

        if (identificationResult.IsFailure)
            return Result<TelephoneNumber>.Failure(identificationResult.Error);

        var identification = identificationResult.Value;

        return _telephoneNumberFactory.Create(
            callingCode: identification.CallingCode,
            regionCode: identification.RegionCode,
            areaCode: identification.AreaCode,
            phoneType: identification.TelephoneType,
            nationalNumber: identification.NationalNumber,
            e164: identification.InternationalNumber);
    }
}

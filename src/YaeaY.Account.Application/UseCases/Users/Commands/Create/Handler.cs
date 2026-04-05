using MediatR;
using Microsoft.Extensions.Logging;
using YaeaY.Account.Application.Services.Security.Interfaces;
using YaeaY.Account.Domain.Abstraction.Exceptions;
using YaeaY.Account.Domain.Abstraction.Interfaces;
using YaeaY.Account.Domain.Abstraction.Records;
using YaeaY.Account.Domain.Entities.AggregateRoots.Users;
using YaeaY.Account.Domain.Entities.UserPhones;
using YaeaY.Account.Domain.Repositories.Users;
using YaeaY.Account.Domain.ValueObjects.Dates;
using YaeaY.Account.Domain.ValueObjects.Emails;
using YaeaY.Account.Domain.ValueObjects.Names;
using YaeaY.Account.Domain.ValueObjects.Securities;

namespace YaeaY.Account.Application.UseCases.Users.Commands.Create;

public sealed class Handler : IRequestHandler<Command, Result<Response>>
{
    private readonly IUserRepository _userRepository;
    private readonly IUnityOfWork _unitOfWork;
    private readonly ILogger<Handler> _logger;
    private readonly IPasswordHasher _passwordHasher;

    public Handler(
        IUserRepository usersRepository,
        IUnityOfWork unitOfWork,
        ILogger<Handler> logger,
        IPasswordHasher passwordHasher)
    {
        _userRepository = usersRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
        _passwordHasher = passwordHasher;
    }

    public async Task<Result<Response>> Handle(Command command, CancellationToken cancellationToken)
    {
        try
        {
            var emailResult = Email.Create(command.EmailAddress);
            if (emailResult.IsFailure)
            {
                return Result<Response>.Failure(emailResult.Error);
            }

            var passwordPlainTextResult = PasswordPlainText.Create(command.Password);
            if (passwordPlainTextResult.IsFailure)
                return Result<Response>.Failure(passwordPlainTextResult.Error);

            var hashed = _passwordHasher.Hash(passwordPlainTextResult.Value.Password);

            var passwordHashResult = PasswordHash.Create(hashed);
            if (passwordHashResult.IsFailure)
                return Result<Response>.Failure(passwordHashResult.Error);

            var userNameResult = UserName.Create(command.UserName);
            if (userNameResult.IsFailure)
            {
                return Result<Response>.Failure(userNameResult.Error);
            }

            var birhDateResult = BirthDate.Create(command.BirthDate);
            if (birhDateResult.IsFailure)
            {
                return Result<Response>.Failure(birhDateResult.Error);
            }

            var initialPhone = UserPhone.Create(
               callingCode: command.CallingCode,
               regionCode: command.RegionCode,
               areaCode: command.AreaCode,
               phoneType: command.PhoneType,
               phoneNumber: command.PhoneNumber,
               e164: command.E164,
               isPrimary: true);

            var user = User.Create(
                emailAddress: emailResult.Value,
                passwordHash: passwordHashResult.Value,
                userName: userNameResult.Value,
                birthDate: birhDateResult.Value,
                gender: command.Gender,
                initialPhone: initialPhone);

            await _userRepository.CreateUserAsync(user, cancellationToken);
            await _unitOfWork.CommitAsync();

            return Result<Response>.Success(
                new Response(
                    id: user.Id,
                    userName: user.UserName.Name,
                    message: "User created successfully!")
                );
        }
        catch (DomainException ex)
        {
            _logger.LogError(ex, "Domain Error creating user.");
            return Result<Response>.Failure(
                new Error(ex.Identifier ?? "DOMAIN_ERROR", ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error creating user.");
            return Result<Response>.Failure(
                new Error("UNEXPECTED_ERROR", "An unexpected error occurred."));
        }
    }
}

using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using YaeaY.Account.Application.Services.Identity.Errors;
using YaeaY.Account.Application.Services.Identity.Interfaces;
using YaeaY.Account.Domain.Abstraction.Interfaces;
using YaeaY.Account.Domain.Abstraction.Result;
using YaeaY.Account.Domain.Entities.AggregateRoots.Users;
using YaeaY.Account.Domain.Enumerators;
using YaeaY.Account.Domain.Errors.Users;
using YaeaY.Account.Domain.Repositories.Users;
using YaeaY.Account.Domain.ValueObjects.Dates;
using YaeaY.Account.Domain.ValueObjects.Emails;
using YaeaY.Account.Domain.ValueObjects.Names;
using YaeaY.Account.Domain.ValueObjects.Securities;
using YaeaY.Account.Domain.ValueObjects.Telephones;
using Login = YaeaY.Account.Application.UseCases.Authentication.Commands.Login;

namespace YaeaY.Account.Application.UnitTests.Authentication;

public sealed class LoginHandlerTests
{
    [Fact]
    public async Task Handle_WhenCredentialsAreInvalid_ShouldNotCreateSession()
    {
        var user = CreateUser();
        var identity = new StubIdentityAccountService(
            Result<IdentityOperation>.Failure(IdentityErrors.InvalidCredentials));
        var unitOfWork = new StubUnitOfWork();
        var handler = CreateHandler(user, identity, unitOfWork, DateTimeOffset.UtcNow);

        var result = await handler.Handle(
            new Login.Command(user.Email.EmailAddress, "invalid", false),
            CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(IdentityErrors.InvalidCredentials);
        identity.SignInCalls.Should().Be(0);
        unitOfWork.CommitCalls.Should().Be(0);
    }

    [Fact]
    public async Task Handle_WhenEmailIsPending_ShouldNotCreateSession()
    {
        var user = CreateUser();
        var identity = new StubIdentityAccountService(Success());
        var unitOfWork = new StubUnitOfWork();
        var handler = CreateHandler(user, identity, unitOfWork, DateTimeOffset.UtcNow);

        var result = await handler.Handle(
            new Login.Command(user.Email.EmailAddress, "Valid@123", false),
            CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(UserErrors.EmailConfirmationRequiredForLogin);
        identity.SignInCalls.Should().Be(0);
        unitOfWork.CommitCalls.Should().Be(0);
    }

    [Fact]
    public async Task Handle_WhenAccountIsActive_ShouldRegisterLoginAndCreateSession()
    {
        var user = CreateUser();
        var confirmedAt = user.CreatedAt.AddMinutes(1);
        var loggedInAt = confirmedAt.AddMinutes(1);
        user.ConfirmEmail(confirmedAt);
        var identity = new StubIdentityAccountService(Success());
        var unitOfWork = new StubUnitOfWork();
        var handler = CreateHandler(user, identity, unitOfWork, loggedInAt);

        var result = await handler.Handle(
            new Login.Command(user.Email.EmailAddress, "Valid@123", true),
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.UserId.Should().Be(user.Id);
        result.Value.LoggedInAt.Should().Be(loggedInAt);
        user.FirstLoginAt.Should().Be(loggedInAt);
        user.LastLoginAt.Should().Be(loggedInAt);
        unitOfWork.CommitCalls.Should().Be(1);
        identity.SignInCalls.Should().Be(1);
        identity.LastIsPersistent.Should().BeTrue();
    }

    private static Login.Handler CreateHandler(
        User user,
        StubIdentityAccountService identity,
        StubUnitOfWork unitOfWork,
        DateTimeOffset now) => new(
            new StubUserRepository(user), identity, unitOfWork,
            new FixedTimeProvider(now), NullLogger<Login.Handler>.Instance);

    private static Result<IdentityOperation> Success() =>
        Result<IdentityOperation>.Success(IdentityOperation.Success);

    private static User CreateUser() => User.Create(
        Email.Create("example@domain.com").Value,
        FullName.Create("Example Person").Value,
        BirthDate.Create(new DateOnly(2000, 1, 1)).Value,
        Gender.Male,
        TelephoneNumber.Create(
            "+55", "BR", "48", TelephoneType.Mobile, "999999999", "+5548999999999").Value);

    private sealed class StubUserRepository(User user) : IUserRepository
    {
        public Task<User?> GetByEmailAsync(Email email, CancellationToken cancellationToken) =>
            Task.FromResult<User?>(user);
        public Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken) =>
            Task.FromResult<User?>(user);
        public Task<bool> ExistsByEmailAsync(Email email, CancellationToken cancellationToken) =>
            Task.FromResult(true);
        public Task CreateUserAsync(User user, CancellationToken cancellationToken) =>
            throw new NotSupportedException();
        public Task UpdateUserAsync(User user, CancellationToken cancellationToken) =>
            throw new NotSupportedException();
    }

    private sealed class StubUnitOfWork : IUnitOfWork
    {
        public int CommitCalls { get; private set; }
        public Task CommitAsync(CancellationToken cancellationToken = default)
        {
            CommitCalls++;
            return Task.CompletedTask;
        }
        public Task<TResult> ExecuteInTransactionAsync<TResult>(
            Func<CancellationToken, Task<TResult>> operation,
            CancellationToken cancellationToken = default) => operation(cancellationToken);
    }

    private sealed class StubIdentityAccountService(
        Result<IdentityOperation> credentialResult) : IIdentityAccountService
    {
        public int SignInCalls { get; private set; }
        public bool LastIsPersistent { get; private set; }

        public Task<Result<IdentityOperation>> ValidateCredentialsAsync(
            Guid userId, string password, CancellationToken cancellationToken = default) =>
            Task.FromResult(credentialResult);

        public Task<Result<IdentityOperation>> SignInAsync(
            Guid userId, bool isPersistent, CancellationToken cancellationToken = default)
        {
            SignInCalls++;
            LastIsPersistent = isPersistent;
            return Task.FromResult(Success());
        }

        public Task<Result<IdentityOperation>> CreateAsync(
            Guid userId, Email email, PasswordText password,
            CancellationToken cancellationToken = default) => throw new NotSupportedException();
        public Task<Result<IdentityOperation>> ConfirmEmailAsync(
            Guid userId, CancellationToken cancellationToken = default) => throw new NotSupportedException();
        public Task SignOutAsync(CancellationToken cancellationToken = default) =>
            throw new NotSupportedException();
    }

    private sealed class FixedTimeProvider(DateTimeOffset now) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => now;
    }
}

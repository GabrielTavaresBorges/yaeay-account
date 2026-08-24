using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using YaeaY.Account.Application.UseCases.PasswordRecoveries.Commands.RequestPasswordRecovery;
using YaeaY.Account.Domain.Abstraction.Interfaces;
using YaeaY.Account.Domain.Entities.AggregateRoots.PasswordRecoveryChallenges;
using YaeaY.Account.Domain.Entities.AggregateRoots.Users;
using YaeaY.Account.Domain.Enumerators;
using YaeaY.Account.Domain.Policies.PasswordRecoveries;
using YaeaY.Account.Domain.Repositories.PasswordRecoveryChallenges;
using YaeaY.Account.Domain.Repositories.Users;
using YaeaY.Account.Domain.ValueObjects.Dates;
using YaeaY.Account.Domain.ValueObjects.Emails;
using YaeaY.Account.Domain.ValueObjects.Names;
using YaeaY.Account.Domain.ValueObjects.Securities;
using YaeaY.Account.Domain.ValueObjects.Telephones;

namespace YaeaY.Account.Application.UnitTests.UseCases.PasswordRecoveries.Commands.RequestPasswordRecoveryTests;

public sealed class RequestPasswordRecoveryHandlerTests
{
    private static readonly DateTimeOffset NowUtc = new(2026, 8, 20, 20, 0, 0, TimeSpan.Zero);

    [Fact]
    public async Task Handle_WhenCurrentCodeIsUsable_ShouldKeepCurrentChallenge()
    {
        var user = CreateConfirmedUser();
        var current = CreateIssuedChallenge(user, NowUtc.AddSeconds(-30));
        var repository = new StubChallengeRepository(current, [current.RequestedAt]);
        var handler = CreateHandler(user, repository);

        var result = await handler.Handle(new Command(user.Email.EmailAddress), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        repository.Created.Should().BeNull();
        current.InvalidatedAt.Should().BeNull();
    }

    [Fact]
    public async Task Handle_WhenCurrentCodeWasVerified_ShouldInvalidateItAndCreateNewChallenge()
    {
        var user = CreateConfirmedUser();
        var current = CreateIssuedChallenge(user, NowUtc.AddSeconds(-30));
        current.Verify(NowUtc.AddSeconds(-10), NowUtc.AddMinutes(10));
        var repository = new StubChallengeRepository(current, [current.RequestedAt]);
        var handler = CreateHandler(user, repository);

        var result = await handler.Handle(new Command(user.Email.EmailAddress), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        current.InvalidatedAt.Should().Be(NowUtc);
        current.InvalidationReason.Should().Be(PasswordRecoveryChallengeInvalidationReason.Superseded);
        repository.Created.Should().NotBeNull();
    }

    [Fact]
    public async Task Handle_WhenFiveRequestsReachedLimit_ShouldNotCreateChallenge()
    {
        var user = CreateConfirmedUser();
        var current = CreateIssuedChallenge(user, NowUtc.AddMinutes(-3));
        var requests = Enumerable.Range(0, 5)
            .Select(index => NowUtc.AddMinutes(-index * 2))
            .ToArray();
        var repository = new StubChallengeRepository(current, requests);
        var handler = CreateHandler(user, repository);

        var result = await handler.Handle(new Command(user.Email.EmailAddress), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        repository.Created.Should().BeNull();
        current.InvalidatedAt.Should().BeNull();
    }

    [Fact]
    public async Task Handle_WhenFourRequestsExist_ShouldAllowFifthChallenge()
    {
        var user = CreateConfirmedUser();
        var current = CreateIssuedChallenge(user, NowUtc.AddMinutes(-3));
        var requests = Enumerable.Range(1, 4)
            .Select(index => NowUtc.AddMinutes(-index * 2))
            .ToArray();
        var repository = new StubChallengeRepository(current, requests);
        var handler = CreateHandler(user, repository);

        var result = await handler.Handle(new Command(user.Email.EmailAddress), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        current.InvalidatedAt.Should().Be(NowUtc);
        repository.Created.Should().NotBeNull();
    }

    [Fact]
    public async Task Handle_WhenBlockingHourElapsed_ShouldStartNewAttemptCycle()
    {
        var user = CreateConfirmedUser();
        var current = CreateIssuedChallenge(user, NowUtc.AddMinutes(-70));
        var requests = Enumerable.Range(0, 5)
            .Select(index => NowUtc.AddMinutes(-61 - index * 2))
            .ToArray();
        var repository = new StubChallengeRepository(current, requests);
        var handler = CreateHandler(user, repository);

        var result = await handler.Handle(new Command(user.Email.EmailAddress), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        current.InvalidatedAt.Should().Be(NowUtc);
        repository.Created.Should().NotBeNull();
    }

    private static Handler CreateHandler(User user, StubChallengeRepository challengeRepository) => new(
        userRepository: new StubUserRepository(user),
        challengeRepository: challengeRepository,
        policy: new StubPolicy(),
        unitOfWork: new StubUnitOfWork(),
        timeProvider: new FixedTimeProvider(NowUtc),
        logger: NullLogger<Handler>.Instance);

    private static PasswordRecoveryChallenge CreateIssuedChallenge(User user, DateTimeOffset requestedAt)
    {
        var challenge = PasswordRecoveryChallenge.Create(user.Id, user.Email, requestedAt);
        challenge.Issue(
            PasswordRecoveryCodeHash.Create(new string('A', 64)).Value,
            requestedAt.AddSeconds(1),
            requestedAt.AddMinutes(2));
        return challenge;
    }

    private static User CreateConfirmedUser()
    {
        var user = User.Create(
            Email.Create("person@example.com").Value,
            FullName.Create("Example Person").Value,
            BirthDate.Create(new DateOnly(2000, 1, 1)).Value,
            Gender.Male,
            TelephoneNumber.Create(
                callingCode: "+55",
                regionCode: "BR",
                areaCode: "48",
                phoneType: TelephoneType.Mobile,
                nationalNumber: "999999999",
                e164: "+5548999999999").Value);
        user.ConfirmEmail(DateTimeOffset.UtcNow.AddMinutes(1));
        return user;
    }

    private sealed class StubChallengeRepository(
        PasswordRecoveryChallenge? current,
        IReadOnlyList<DateTimeOffset> requests) : IPasswordRecoveryChallengeRepository
    {
        public PasswordRecoveryChallenge? Created { get; private set; }

        public Task CreateAsync(PasswordRecoveryChallenge challenge, CancellationToken cancellationToken)
        {
            Created = challenge;
            return Task.CompletedTask;
        }

        public Task<PasswordRecoveryChallenge?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
            => Task.FromResult(current);

        public Task<PasswordRecoveryChallenge?> GetOpenByUserIdAsync(Guid userId, CancellationToken cancellationToken)
            => Task.FromResult(current);

        public Task<IReadOnlyList<DateTimeOffset>> GetMostRecentRequestedAtAsync(
            Guid userId,
            int count,
            CancellationToken cancellationToken)
            => Task.FromResult<IReadOnlyList<DateTimeOffset>>(requests.Take(count).ToArray());
    }

    private sealed class StubUserRepository(User user) : IUserRepository
    {
        public Task<User?> GetByEmailAsync(Email email, CancellationToken cancellationToken)
            => Task.FromResult<User?>(user);

        public Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
            => Task.FromResult<User?>(user);

        public Task<bool> ExistsByEmailAsync(Email email, CancellationToken cancellationToken)
            => throw new NotSupportedException();

        public Task CreateUserAsync(User userToCreate, CancellationToken cancellationToken)
            => throw new NotSupportedException();

        public Task UpdateUserAsync(User userToUpdate, CancellationToken cancellationToken)
            => throw new NotSupportedException();
    }

    private sealed class StubPolicy : IPasswordRecoveryPolicy
    {
        public TimeSpan CodeLifetime => TimeSpan.FromMinutes(2);
        public TimeSpan ResetAuthorizationLifetime => TimeSpan.FromMinutes(10);
        public TimeSpan ResendInterval => TimeSpan.FromMinutes(2);
        public TimeSpan RequestWindow => TimeSpan.FromHours(1);
        public int MaximumFailedAttempts => 5;
        public int MaximumRequestsPerWindow => 5;
    }

    private sealed class StubUnitOfWork : IUnitOfWork
    {
        public Task CommitAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;

        public Task<TResult> ExecuteInTransactionAsync<TResult>(
            Func<CancellationToken, Task<TResult>> operation,
            CancellationToken cancellationToken = default)
            => operation(cancellationToken);
    }

    private sealed class FixedTimeProvider(DateTimeOffset utcNow) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => utcNow;
    }
}

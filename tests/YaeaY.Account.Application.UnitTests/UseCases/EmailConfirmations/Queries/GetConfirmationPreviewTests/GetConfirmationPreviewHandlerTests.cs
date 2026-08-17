using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using YaeaY.Account.Application.Services.Emails;
using YaeaY.Account.Application.Services.Security.Interfaces;
using YaeaY.Account.Domain.Abstraction.Result;
using YaeaY.Account.Domain.Entities.AggregateRoots.EmailConfirmationTokens;
using YaeaY.Account.Domain.Entities.AggregateRoots.Users;
using YaeaY.Account.Domain.Enumerators;
using YaeaY.Account.Domain.Errors.Users;
using YaeaY.Account.Domain.Repositories.EmailConfirmationTokens;
using YaeaY.Account.Domain.Repositories.Users;
using YaeaY.Account.Domain.ValueObjects.Dates;
using YaeaY.Account.Domain.ValueObjects.Emails;
using YaeaY.Account.Domain.ValueObjects.Names;
using YaeaY.Account.Domain.ValueObjects.Securities;
using YaeaY.Account.Domain.ValueObjects.Telephones;
using GetConfirmationPreview = YaeaY.Account.Application.UseCases.EmailConfirmations.Queries.GetConfirmationPreview;

namespace YaeaY.Account.Application.UnitTests.UseCases.EmailConfirmations.Queries.GetConfirmationPreviewTests;

public sealed class GetConfirmationPreviewHandlerTests
{
    [Fact]
    public void ToString_WhenQueryContainsRawToken_ShouldNotRevealToken()
    {
        // Arrange

        const string rawToken = "raw-token-only-in-memory";
        var query = new GetConfirmationPreview.Query(rawToken);

        // Act

        var result = query.ToString();

        // Assert

        result.Should().Be(nameof(GetConfirmationPreview.Query));
        result.Should().NotContain(rawToken);
    }

    [Fact]
    public async Task Handle_WhenTokenIsUsable_ShouldReturnOnlyMaskedEmail()
    {
        // Arrange

        var user = CreateUser("personwithlongname@example.com");
        var tokenHash = TokenHash.Create(new string('A', 64)).Value;
        var token = EmailConfirmationToken.Create(
            userId: user.Id,
            email: user.Email,
            tokenHash: tokenHash,
            expiresAt: DateTimeOffset.UtcNow.AddHours(2),
            requestedBy: EmailConfirmationTokenRequestedBy.System,
            requestReason: EmailConfirmationTokenRequestReason.AccountCreated);
        var handler = new GetConfirmationPreview.Handler(
            tokenRepository: new StubTokenRepository(token),
            userRepository: new StubUserRepository(user),
            tokenService: new StubTokenService(tokenHash),
            emailAddressMasker: new EmailAddressMasker(),
            timeProvider: new FixedTimeProvider(DateTimeOffset.UtcNow.AddMinutes(1)),
            logger: NullLogger<GetConfirmationPreview.Handler>.Instance);

        // Act

        var result = await handler.Handle(
            new GetConfirmationPreview.Query("raw-token-only-in-memory"),
            CancellationToken.None);

        // Assert

        result.IsSuccess.Should().BeTrue();
        result.Value.MaskedEmail.Should().Be("pe******@example.com");
        result.Value.MaskedEmail.Should().NotContain("personwithlongname");
    }

    [Fact]
    public async Task Handle_WhenEmailWasAlreadyConfirmed_ShouldReturnSafeAccountState()
    {
        // Arrange

        var user = CreateUser("person@example.com");
        var tokenHash = TokenHash.Create(new string('B', 64)).Value;
        var token = EmailConfirmationToken.Create(
            userId: user.Id,
            email: user.Email,
            tokenHash: tokenHash,
            expiresAt: DateTimeOffset.UtcNow.AddHours(2),
            requestedBy: EmailConfirmationTokenRequestedBy.System,
            requestReason: EmailConfirmationTokenRequestReason.AccountCreated);
        var confirmedAt = DateTimeOffset.UtcNow.AddMinutes(1);
        user.ConfirmEmail(confirmedAt);
        token.MarkAsUsed(confirmedAt);
        var handler = new GetConfirmationPreview.Handler(
            tokenRepository: new StubTokenRepository(token),
            userRepository: new StubUserRepository(user),
            tokenService: new StubTokenService(tokenHash),
            emailAddressMasker: new EmailAddressMasker(),
            timeProvider: new FixedTimeProvider(confirmedAt.AddMinutes(1)),
            logger: NullLogger<GetConfirmationPreview.Handler>.Instance);

        // Act

        var result = await handler.Handle(
            new GetConfirmationPreview.Query("raw-token-only-in-memory"),
            CancellationToken.None);

        // Assert

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(UserErrors.EmailAlreadyConfirmed);
        result.Error.Code.Should().NotContain("token");
    }

    private static User CreateUser(string emailAddress)
    {
        var email = Email.Create(emailAddress).Value;
        var fullName = FullName.Create("Example Person").Value;
        var birthDate = BirthDate.Create(new DateOnly(2000, 1, 1)).Value;
        var telephoneNumber = TelephoneNumber.Create(
            callingCode: "+55",
            regionCode: "BR",
            areaCode: "48",
            phoneType: TelephoneType.Mobile,
            nationalNumber: "999999999",
            e164: "+5548999999999").Value;

        return User.Create(
            email,
            fullName,
            birthDate,
            Gender.Male,
            telephoneNumber);
    }

    private sealed class StubTokenService : IEmailConfirmationTokenService
    {
        private readonly TokenHash _tokenHash;

        public StubTokenService(TokenHash tokenHash)
        {
            _tokenHash = tokenHash;
        }

        public Task<GeneratedEmailConfirmationToken> GenerateTokenAsync()
            => throw new NotSupportedException();

        public Result<TokenHash> HashToken(string rawToken)
            => Result<TokenHash>.Success(_tokenHash);
    }

    private sealed class StubTokenRepository : IEmailConfirmationTokenRepository
    {
        private readonly EmailConfirmationToken _token;

        public StubTokenRepository(EmailConfirmationToken token)
        {
            _token = token;
        }

        public Task<EmailConfirmationToken?> GetByHashAsync(
            TokenHash tokenHash,
            CancellationToken cancellationToken)
            => Task.FromResult<EmailConfirmationToken?>(_token);

        public Task<bool> HasPendingTokenAsync(
            Guid userId,
            CancellationToken cancellationToken)
            => throw new NotSupportedException();

        public Task CreateEmailConfirmationTokenAsync(
            EmailConfirmationToken emailConfirmationToken,
            CancellationToken cancellationToken)
            => throw new NotSupportedException();
    }

    private sealed class StubUserRepository : IUserRepository
    {
        private readonly User _user;

        public StubUserRepository(User user)
        {
            _user = user;
        }

        public Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
            => Task.FromResult<User?>(_user);

        public Task<User?> GetByEmailAsync(Email email, CancellationToken cancellationToken)
            => Task.FromResult<User?>(_user);

        public Task<bool> ExistsByEmailAsync(
            Email email,
            CancellationToken cancellationToken)
            => throw new NotSupportedException();

        public Task CreateUserAsync(User user, CancellationToken cancellationToken)
            => throw new NotSupportedException();

        public Task UpdateUserAsync(User user, CancellationToken cancellationToken)
            => throw new NotSupportedException();
    }

    private sealed class FixedTimeProvider : TimeProvider
    {
        private readonly DateTimeOffset _utcNow;

        public FixedTimeProvider(DateTimeOffset utcNow)
        {
            _utcNow = utcNow;
        }

        public override DateTimeOffset GetUtcNow() => _utcNow;
    }
}

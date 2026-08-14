using FluentAssertions;
using YaeaY.Account.Domain.Abstraction.Exceptions;
using YaeaY.Account.Domain.Entities.AggregateRoots.EmailConfirmationTokens;
using YaeaY.Account.Domain.Enumerators;
using YaeaY.Account.Domain.Errors.EmailConfirmationTokens;
using YaeaY.Account.Domain.ValueObjects.Emails;
using YaeaY.Account.Domain.ValueObjects.Securities;

namespace YaeaY.Account.Domain.UnitTests.Entities.AggregateRoots.EmailConfirmationTokensTests;

public sealed class EmailConfirmationTokenTests
{
    [Fact]
    public void Create_WhenUserIdIsEmpty_ShouldFail_WithRequiredError()
    {
        // Arrange

        var userId = Guid.Empty;
        var email = CreateValidEmail();
        var tokenHash = CreateValidTokenHash();
        var expiresAt = DateTimeOffset.UtcNow.AddMinutes(30);

        // Act

        Action act = () => EmailConfirmationToken.Create(
            userId,
            email,
            tokenHash,
            expiresAt,
            EmailConfirmationTokenRequestedBy.System,
            EmailConfirmationTokenRequestReason.AccountCreated);

        // Assert

        var exception = act.Should().Throw<DomainException>().Which;
        exception.Error.Should().Be(EmailConfirmationTokenErrors.UserIdRequired);
    }

    [Fact]
    public void Create_WhenEmailIsNull_ShouldFail_WithRequiredError()
    {
        // Arrange

        Email email = null!;
        var tokenHash = CreateValidTokenHash();

        // Act

        Action act = () => EmailConfirmationToken.Create(
            Guid.NewGuid(),
            email,
            tokenHash,
            DateTimeOffset.UtcNow.AddMinutes(30),
            EmailConfirmationTokenRequestedBy.System,
            EmailConfirmationTokenRequestReason.AccountCreated);

        // Assert

        var exception = act.Should().Throw<DomainException>().Which;
        exception.Error.Should().Be(EmailConfirmationTokenErrors.EmailRequired);
    }

    [Fact]
    public void Create_WhenTokenHashIsNull_ShouldFail_WithRequiredError()
    {
        // Arrange

        TokenHash tokenHash = null!;

        // Act

        Action act = () => EmailConfirmationToken.Create(
            Guid.NewGuid(),
            CreateValidEmail(),
            tokenHash,
            DateTimeOffset.UtcNow.AddMinutes(30),
            EmailConfirmationTokenRequestedBy.System,
            EmailConfirmationTokenRequestReason.AccountCreated);

        // Assert

        var exception = act.Should().Throw<DomainException>().Which;
        exception.Error.Should().Be(EmailConfirmationTokenErrors.TokenHashRequired);
    }

    [Fact]
    public void Create_WhenExpirationIsNotAfterCreation_ShouldFail_WithInvariantError()
    {
        // Arrange

        var expiresAt = DateTimeOffset.UtcNow;

        // Act

        Action act = () => EmailConfirmationToken.Create(
            Guid.NewGuid(),
            CreateValidEmail(),
            CreateValidTokenHash(),
            expiresAt,
            EmailConfirmationTokenRequestedBy.System,
            EmailConfirmationTokenRequestReason.AccountCreated);

        // Assert

        var exception = act.Should().Throw<DomainException>().Which;
        exception.Error.Should().Be(EmailConfirmationTokenErrors.ExpirationNotAfterCreation);
    }

    [Theory]
    [InlineData(0, "required")]
    [InlineData(999, "invalid")]
    public void Create_WhenRequestedByIsNotValid_ShouldFail_WithExpectedError(
        int requestedByValue,
        string expectedError)
    {
        // Arrange

        var requestedBy = (EmailConfirmationTokenRequestedBy)requestedByValue;

        // Act

        Action act = () => EmailConfirmationToken.Create(
            Guid.NewGuid(),
            CreateValidEmail(),
            CreateValidTokenHash(),
            DateTimeOffset.UtcNow.AddMinutes(30),
            requestedBy,
            EmailConfirmationTokenRequestReason.AccountCreated);

        // Assert

        var exception = act.Should().Throw<DomainException>().Which;
        exception.Error.Should().Be(
            expectedError == "required"
                ? EmailConfirmationTokenErrors.RequestedByRequired
                : EmailConfirmationTokenErrors.RequestedByInvalid);
    }

    [Theory]
    [InlineData(0, "required")]
    [InlineData(999, "invalid")]
    public void Create_WhenRequestReasonIsNotValid_ShouldFail_WithExpectedError(
        int requestReasonValue,
        string expectedError)
    {
        // Arrange

        var requestReason = (EmailConfirmationTokenRequestReason)requestReasonValue;

        // Act

        Action act = () => EmailConfirmationToken.Create(
            Guid.NewGuid(),
            CreateValidEmail(),
            CreateValidTokenHash(),
            DateTimeOffset.UtcNow.AddMinutes(30),
            EmailConfirmationTokenRequestedBy.System,
            requestReason);

        // Assert

        var exception = act.Should().Throw<DomainException>().Which;
        exception.Error.Should().Be(
            expectedError == "required"
                ? EmailConfirmationTokenErrors.RequestReasonRequired
                : EmailConfirmationTokenErrors.RequestReasonInvalid);
    }

    [Fact]
    public void Create_WhenAllDataIsValid_ShouldSucceed()
    {
        // Arrange

        var userId = Guid.NewGuid();
        var email = CreateValidEmail();
        var tokenHash = CreateValidTokenHash();
        var expiresAt = DateTimeOffset.UtcNow.AddMinutes(30);

        // Act

        var token = EmailConfirmationToken.Create(
            userId,
            email,
            tokenHash,
            expiresAt,
            EmailConfirmationTokenRequestedBy.System,
            EmailConfirmationTokenRequestReason.AccountCreated);

        // Assert

        token.UserId.Should().Be(userId);
        token.Email.Should().Be(email);
        token.TokenHash.Should().Be(tokenHash);
        token.CreatedAt.Should().BeBefore(expiresAt);
        token.ExpiresAt.Should().Be(expiresAt);
        token.RequestedBy.Should().Be(EmailConfirmationTokenRequestedBy.System);
        token.RequestReason.Should().Be(EmailConfirmationTokenRequestReason.AccountCreated);
        token.UsedAt.Should().BeNull();
        token.InvalidatedAt.Should().BeNull();
        token.InvalidationReason.Should().BeNull();
        token.IsUsable(token.CreatedAt).Should().BeTrue();
    }

    [Fact]
    public void MarkAsUsed_WhenTokenIsUsable_ShouldSetUsageDate()
    {
        // Arrange

        var token = CreateValidToken();
        var usedAt = token.CreatedAt.AddMinutes(1);

        // Act

        token.MarkAsUsed(usedAt);

        // Assert

        token.UsedAt.Should().Be(usedAt);
        token.IsUsed().Should().BeTrue();
        token.IsUsable(usedAt).Should().BeFalse();
    }

    [Fact]
    public void MarkAsUsed_WhenTokenIsExpired_ShouldFail_WithoutChangingState()
    {
        // Arrange

        var token = CreateValidToken();

        // Act

        Action act = () => token.MarkAsUsed(token.ExpiresAt);

        // Assert

        var exception = act.Should().Throw<DomainException>().Which;
        exception.Error.Should().Be(EmailConfirmationTokenErrors.Expired);
        token.UsedAt.Should().BeNull();
    }

    [Fact]
    public void MarkAsUsed_WhenTokenIsInvalidated_ShouldFail_WithoutChangingState()
    {
        // Arrange

        var token = CreateValidToken();
        token.Invalidate(
            EmailConfirmationTokenInvalidationReason.Superseded,
            token.CreatedAt.AddMinutes(1));

        // Act

        Action act = () => token.MarkAsUsed(token.CreatedAt.AddMinutes(2));

        // Assert

        var exception = act.Should().Throw<DomainException>().Which;
        exception.Error.Should().Be(EmailConfirmationTokenErrors.Invalidated);
        token.UsedAt.Should().BeNull();
    }

    [Fact]
    public void MarkAsUsed_WhenTokenWasAlreadyUsed_ShouldFail_WithoutChangingState()
    {
        // Arrange

        var token = CreateValidToken();
        var firstUsage = token.CreatedAt.AddMinutes(1);
        token.MarkAsUsed(firstUsage);

        // Act

        Action act = () => token.MarkAsUsed(token.CreatedAt.AddMinutes(2));

        // Assert

        var exception = act.Should().Throw<DomainException>().Which;
        exception.Error.Should().Be(EmailConfirmationTokenErrors.AlreadyUsed);
        token.UsedAt.Should().Be(firstUsage);
    }

    [Fact]
    public void Invalidate_WhenTokenIsPending_ShouldSetInvalidationState()
    {
        // Arrange

        var token = CreateValidToken();
        var invalidatedAt = token.CreatedAt.AddMinutes(1);

        // Act

        token.Invalidate(EmailConfirmationTokenInvalidationReason.Superseded, invalidatedAt);

        // Assert

        token.InvalidatedAt.Should().Be(invalidatedAt);
        token.InvalidationReason.Should().Be(EmailConfirmationTokenInvalidationReason.Superseded);
        token.IsInvalidated().Should().BeTrue();
        token.IsUsable(invalidatedAt).Should().BeFalse();
    }

    [Fact]
    public void Invalidate_WhenReasonIsUnknown_ShouldFail_WithoutChangingState()
    {
        // Arrange

        var token = CreateValidToken();

        // Act

        Action act = () => token.Invalidate(
            EmailConfirmationTokenInvalidationReason.Unknown,
            token.CreatedAt.AddMinutes(1));

        // Assert

        var exception = act.Should().Throw<DomainException>().Which;
        exception.Error.Should().Be(EmailConfirmationTokenErrors.InvalidationReasonRequired);
        token.InvalidatedAt.Should().BeNull();
        token.InvalidationReason.Should().BeNull();
    }

    [Fact]
    public void Invalidate_WhenTokenWasAlreadyInvalidated_ShouldFail_WithoutChangingState()
    {
        // Arrange

        var token = CreateValidToken();
        var firstInvalidation = token.CreatedAt.AddMinutes(1);
        token.Invalidate(EmailConfirmationTokenInvalidationReason.Superseded, firstInvalidation);

        // Act

        Action act = () => token.Invalidate(
            EmailConfirmationTokenInvalidationReason.AdminRevoked,
            token.CreatedAt.AddMinutes(2));

        // Assert

        var exception = act.Should().Throw<DomainException>().Which;
        exception.Error.Should().Be(EmailConfirmationTokenErrors.AlreadyInvalidated);
        token.InvalidatedAt.Should().Be(firstInvalidation);
        token.InvalidationReason.Should().Be(EmailConfirmationTokenInvalidationReason.Superseded);
    }

    [Fact]
    public void Invalidate_WhenTokenWasUsed_ShouldFail_WithoutChangingState()
    {
        // Arrange

        var token = CreateValidToken();
        var usedAt = token.CreatedAt.AddMinutes(1);
        token.MarkAsUsed(usedAt);

        // Act

        Action act = () => token.Invalidate(
            EmailConfirmationTokenInvalidationReason.AdminRevoked,
            token.CreatedAt.AddMinutes(2));

        // Assert

        var exception = act.Should().Throw<DomainException>().Which;
        exception.Error.Should().Be(EmailConfirmationTokenErrors.UsedTokenCannotBeInvalidated);
        token.UsedAt.Should().Be(usedAt);
        token.InvalidatedAt.Should().BeNull();
    }

    private static EmailConfirmationToken CreateValidToken()
        => EmailConfirmationToken.Create(
            Guid.NewGuid(),
            CreateValidEmail(),
            CreateValidTokenHash(),
            DateTimeOffset.UtcNow.AddDays(1),
            EmailConfirmationTokenRequestedBy.System,
            EmailConfirmationTokenRequestReason.AccountCreated);

    private static Email CreateValidEmail()
        => Email.Create("user@example.com").Value;

    private static TokenHash CreateValidTokenHash()
        => TokenHash.Create("abc123hashedtokenvalue").Value;
}

using FluentAssertions;
using YaeaY.Account.Domain.Abstraction.Exceptions;
using YaeaY.Account.Domain.Entities.AggregateRoots.EmailConfirmationTokens;
using YaeaY.Account.Domain.ValueObjects.Securities;

namespace YaeaY.Account.Domain.UnitTests.Entities.AggregateRoots.EmailConfirmationTokensTests;

public class EmailConfirmationTokenTests
{
    // IsFailure

    [Fact]
    public void Create_WhenUserIdIsEmpty_ShouldThrowDomainException()
    {
        // Arrange

        var userId = Guid.Empty;

        var tokenHashValid = "abc123hashedtokenvalue";
        var tokenHashResult = TokenHash.Create(tokenHashValid);
        var tokenHash = tokenHashResult.Value;

        var expiresAt = DateTimeOffset.UtcNow.AddMinutes(30);

        // Act

        Action act = () => EmailConfirmationToken.Create(userId, tokenHash, expiresAt);

        // Assert

        var exception = act.Should().Throw<DomainException>().Which;
        exception.Identifier.Should().Be("USER_ID_INVALID");
        exception.Message.Should().Be("UserId cannot be empty.");
    }

    [Fact]
    public void Create_WhenTokenHashIsNull_ShouldThrowDomainException()
    {
        // Arrange

        TokenHash tokenHash = null!;

        var userId = Guid.NewGuid();
        var expiresAt = DateTimeOffset.UtcNow.AddMinutes(30);

        // Act

        Action act = () => EmailConfirmationToken.Create(userId, tokenHash, expiresAt);

        // Assert

        var exception = act.Should().Throw<DomainException>().Which;
        exception.Identifier.Should().Be("TOKEN_HASH_NULL");
        exception.Message.Should().Be("Token hash cannot be null.");
    }

    [Fact]
    public void Create_WhenExpiresAtIsInPast_ShouldThrowDomainException()
    {
        // Arrange

        var userId = Guid.NewGuid();

        var tokenHashValid = "abc123hashedtokenvalue";
        var tokenHashResult = TokenHash.Create(tokenHashValid);
        var tokenHash = tokenHashResult.Value;

        var expiresAt = DateTimeOffset.UtcNow.AddMinutes(-1);

        // Act

        Action act = () => EmailConfirmationToken.Create(userId, tokenHash, expiresAt);

        // Assert

        var exception = act.Should().Throw<DomainException>().Which;
        exception.Identifier.Should().Be("TOKEN_EXPIRATION_INVALID");
        exception.Message.Should().Be("Expiration date must be in the future.");
    }

    [Fact]
    public void Create_WhenExpiresAtIsNow_ShouldThrowDomainException()
    {
        // Arrange

        var userId = Guid.NewGuid();

        var tokenHashValid = "abc123hashedtokenvalue";
        var tokenHashResult = TokenHash.Create(tokenHashValid);
        var tokenHash = tokenHashResult.Value;

        var expiresAt = DateTimeOffset.UtcNow;

        // Act

        Action act = () => EmailConfirmationToken.Create(userId, tokenHash, expiresAt);

        // Assert

        var exception = act.Should().Throw<DomainException>().Which;
        exception.Identifier.Should().Be("TOKEN_EXPIRATION_INVALID");
        exception.Message.Should().Be("Expiration date must be in the future.");
    }

    // IsSuccess

    [Fact]
    public void Create_WhenAllDataIsValid_ShouldSuccess()
    {
        // Arrange

        var userId = Guid.NewGuid();

        var tokenHashValid = "abc123hashedtokenvalue";
        var tokenHashResult = TokenHash.Create(tokenHashValid);
        var tokenHash = tokenHashResult.Value;

        var expiresAt = DateTimeOffset.UtcNow.AddMinutes(30);

        // Act

        var emailConfirmationToken = EmailConfirmationToken.Create(userId, tokenHash, expiresAt);

        // Assert

        emailConfirmationToken.Should().NotBeNull();
        emailConfirmationToken.UserId.Should().Be(userId);
        emailConfirmationToken.TokenHash.Should().Be(tokenHash);
        emailConfirmationToken.ExpiresAt.Should().Be(expiresAt);
        emailConfirmationToken.UsedAt.Should().BeNull();
        emailConfirmationToken.IsUsed().Should().BeFalse();
    }

    // IsExpired

    [Fact]
    public void IsExpired_WhenCurrentTimeIsBeforeExpiration_ShouldReturnFalse()
    {
        // Arrange

        var userId = Guid.NewGuid();

        var tokenHashValid = "abc123hashedtokenvalue";
        var tokenHashResult = TokenHash.Create(tokenHashValid);
        var tokenHash = tokenHashResult.Value;

        var expiresAt = DateTimeOffset.UtcNow.AddMinutes(30);

        var emailConfirmationToken = EmailConfirmationToken.Create(userId, tokenHash, expiresAt);

        var nowUtc = expiresAt.AddSeconds(-1);

        // Act

        var result = emailConfirmationToken.IsExpired(nowUtc);

        // Assert

        result.Should().BeFalse();
    }
}
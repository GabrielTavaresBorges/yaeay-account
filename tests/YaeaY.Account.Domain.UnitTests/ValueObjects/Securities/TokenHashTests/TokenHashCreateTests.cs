using FluentAssertions;
using YaeaY.Account.Domain.ValueObjects.Securities;

namespace YaeaY.Account.Domain.UnitTests.ValueObjects.Securities.TokenHashTests;

public class TokenHashCreateTests
{
    // IsFailure

    [Fact]
    public void Create_WhenTokenHashIsNull_ShouldFailure()
    {
        // Arrange

        string tokenHash = null!;

        // Act

        var result = TokenHash.Create(tokenHash);

        // Assert

        result.IsFailure.Should().BeTrue();
        result.Error.Identifier.Should().Be("TOKEN_HASH_NULL_EMPTY_WHITE_SPACE");
        result.Error.Message.Should().Be("Token hash cannot be nul, empty or white space.");
    }

    [Fact]
    public void Create_WhenTokenHashIsEmpty_ShouldFailure()
    {
        // Arrange

        string tokenHash = string.Empty;

        // Act

        var result = TokenHash.Create(tokenHash);

        // Assert

        result.IsFailure.Should().BeTrue();
        result.Error.Identifier.Should().Be("TOKEN_HASH_NULL_EMPTY_WHITE_SPACE");
        result.Error.Message.Should().Be("Token hash cannot be nul, empty or white space.");
    }

    [Fact]
    public void Create_WhenTokenHashContainsWhiteSpaceOnly_ShouldFailure()
    {
        // Arrange

        string tokenHash = " ";

        // Act

        var result = TokenHash.Create(tokenHash);

        // Assert

        result.IsFailure.Should().BeTrue();
        result.Error.Identifier.Should().Be("TOKEN_HASH_NULL_EMPTY_WHITE_SPACE");
        result.Error.Message.Should().Be("Token hash cannot be nul, empty or white space.");
    }

    [Fact]
    public void Create_WhenTokenHashIsTooLong_ShouldFailure()
    {
        // Arrange

        string tokenHash = new string('a', 1025);

        // Act

        var result = TokenHash.Create(tokenHash);

        // Assert

        result.IsFailure.Should().BeTrue();
        result.Error.Identifier.Should().Be("TOKEN_HASH_TOO_LONG");
        result.Error.Message.Should().Be("Token hash is too long. Current length: 1025. Max: 1024.");
    }

    // IsSuccess

    [Fact]
    public void Create_WhenTokenHashIsValid_ShouldSuccess()
    {
        // Arrange

        string tokenHash = "abc123hashedtokenvalue";

        // Act

        var result = TokenHash.Create(tokenHash);

        // Assert

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Token.Should().Be("abc123hashedtokenvalue");
    }

    [Fact]
    public void Create_WhenTokenHashHasLeadingOrTrailingSpaces_ShouldSuccess()
    {
        // Arrange

        string tokenHash = " abc123hashedtokenvalue ";

        // Act

        var result = TokenHash.Create(tokenHash);

        // Assert

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Token.Should().Be("abc123hashedtokenvalue");
    }
}

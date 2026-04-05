using FluentAssertions;
using YaeaY.Account.Domain.ValueObjects.Names;

namespace YaeaY.Account.Domain.UnitTests.ValueObjects.Names.UserNameTests;

public class UserNameCreateTests
{
    // IsFailure

    [Fact]
    public void Create_WhenUserNameIsNull_ShouldFailure()
    {
        // Arrange

        string userName = null!;

        // Act

        var result = UserName.Create(userName);

        // Assert

        result.IsFailure.Should().BeTrue();
        result.Error.Identifier.Should().Be("USER_NAME_NULL_EMPTY_WHITE_SPACE");
        result.Error.Message.Should().Be("Name cannot be null, empty or white space.");
    }

    [Fact]
    public void Create_WhenUserNameIsEmpty_ShouldFailure()
    {
        // Arrange

        string userName = string.Empty;

        // Act

        var result = UserName.Create(userName);

        // Assert

        result.IsFailure.Should().BeTrue();
        result.Error.Identifier.Should().Be("USER_NAME_NULL_EMPTY_WHITE_SPACE");
        result.Error.Message.Should().Be("Name cannot be null, empty or white space.");
    }

    [Fact]
    public void Create_WhenUserNameContainsWhiteSpace_ShouldFailure()
    {
        // Arrange

        string userName = " ";

        // Act

        var result = UserName.Create(userName);

        // Assert

        result.IsFailure.Should().BeTrue();
        result.Error.Identifier.Should().Be("USER_NAME_NULL_EMPTY_WHITE_SPACE");
        result.Error.Message.Should().Be("Name cannot be null, empty or white space.");
    }

    [Fact]
    public void Create_WhenUserNameHasLessThan2Characters_ShouldFailure()
    {
        // Arrange

        string userName = "A";

        // Act

        var result = UserName.Create(userName);

        // Assert

        result.IsFailure.Should().BeTrue();
        result.Error.Identifier.Should().Be("USER_NAME_INVALID_LENGTH");
        result.Error.Message.Should().Be("Name must be between 2 and 100 characters.");
    }

    [Fact]
    public void Create_WhenUserNameHasMoreThan100Characters_ShouldFailure()
    {
        // Arrange

        string userName = new string('A', 101);

        // Act

        var result = UserName.Create(userName);

        // Assert

        result.IsFailure.Should().BeTrue();
        result.Error.Identifier.Should().Be("USER_NAME_INVALID_LENGTH");
        result.Error.Message.Should().Be("Name must be between 2 and 100 characters.");
    }

    // IsSuccess

    [Fact]
    public void Create_WhenUserNameIsValid_ShouldSuccess()
    {
        // Arrange

        string userName = "Example Name";

        // Act

        var result = UserName.Create(userName);

        // Assert

        result.IsSuccess.Should().BeTrue();
        result.Value.Name.Should().Be("Example Name");
    }

    [Fact]
    public void Create_WhenUserNameHasLeadingOrTrailingSpaces_ShouldSuccess()
    {
        // Arrange

        string userName = " Example Name ";

        // Act

        var result = UserName.Create(userName);

        // Assert

        result.IsSuccess.Should().BeTrue();
        result.Value.Name.Should().Be("Example Name");
    }

    [Fact]
    public void Create_WhenUserNameHasExactlyMaxLength_ShouldSuccess()
    {
        // Arrange

        string userName = new string('a', 100);

        // Act

        var result = UserName.Create(userName);

        // Assert

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
    }

    [Fact]
    public void Create_WhenUserNameHasExactlyMinLength_ShouldSuccess()
    {
        // Arrange

        string userName = "Ab";

        // Act

        var result = UserName.Create(userName);

        // Assert

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Name.Should().Be(userName);
    }
}

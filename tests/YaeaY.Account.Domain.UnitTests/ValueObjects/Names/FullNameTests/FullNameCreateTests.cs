using FluentAssertions;
using YaeaY.Account.Domain.Errors.FullName;
using YaeaY.Account.Domain.ValueObjects.Names;

namespace YaeaY.Account.Domain.UnitTests.ValueObjects.Names.FullNameTests;

public class FullNameCreateTests
{
    // IsFailure

    [Fact]
    public void Create_WhenFullNameIsNull_ShouldFail_WithFullNameErrorsRequired()
    {
        // Arrange

        string fullNameInvalid = null!;

        // Act

        var result = FullName.Create(fullNameInvalid);

        // Assert

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(FullNameErrors.Required);
    }

    [Fact]
    public void Create_WhenFullNameIsEmpty_ShouldFail_WithFullNameErrorsRequired()
    {
        // Arrange

        string fullNameInvalid = string.Empty;

        // Act

        var result = FullName.Create(fullNameInvalid);

        // Assert

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(FullNameErrors.Required);
    }

    [Fact]
    public void Create_WhenFullNameContainsWhiteSpaceOnly_ShouldFail_WithFullNameErrorsRequired()
    {
        // Arrange

        string fullNameInvalid = " ";

        // Act

        var result = FullName.Create(fullNameInvalid);

        // Assert

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(FullNameErrors.Required);
    }

    [Fact]
    public void Create_WhenFullNameIsShorterThanMinimumLength_ShouldFail_WithFullNameErrorsTooShort()
    {
        // Arrange

        string fullNameInvalid = "A";

        // Act

        var result = FullName.Create(fullNameInvalid);

        // Assert

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(FullNameErrors.TooShort(fullNameInvalid.Length, 2));
    }

    [Fact]
    public void Create_WhenFullNameIsLongerThanMaximumLength_ShouldFail_WithFullNameErrorsTooLong()
    {
        // Arrange

        string fullNameInvalid = new string('A', 101);

        // Act

        var result = FullName.Create(fullNameInvalid);

        // Assert

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(FullNameErrors.TooLong(fullNameInvalid.Length, 100));
    }

    // IsSuccess

    [Fact]
    public void Create_WhenFullNameIsValid_ShouldSucceed()
    {
        // Arrange

        string fullName = "Example Name";

        // Act

        var result = FullName.Create(fullName);

        // Assert

        result.IsSuccess.Should().BeTrue();
        result.Value.Name.Should().Be("Example Name");
    }

    [Fact]
    public void Create_WhenFullNameHasLeadingOrTrailingSpaces_ShouldSucceed_WithTrimmedFullName()
    {
        // Arrange

        string fullName = " Example Name ";

        // Act

        var result = FullName.Create(fullName);

        // Assert

        result.IsSuccess.Should().BeTrue();
        result.Value.Name.Should().Be("Example Name");
    }

    [Fact]
    public void Create_WhenFullNameContainsMultipleSpacesBetweenWords_ShouldSucceed_WithSingleSpacesBetweenWords()
    {
        // Arrange

        string fullName = "Example    Name";

        // Act

        var result = FullName.Create(fullName);

        // Assert

        result.IsSuccess.Should().BeTrue();
        result.Value.Name.Should().Be("Example Name");
    }

    [Fact]
    public void Create_WhenFullNameHasExactlyMaximumLength_ShouldSucceed()
    {
        // Arrange

        string fullName = new string('a', 100);

        // Act

        var result = FullName.Create(fullName);

        // Assert

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
    }

    [Fact]
    public void Create_WhenFullNameHasExactlyMinimumLength_ShouldSucceed()
    {
        // Arrange

        string fullName = "Ab";

        // Act

        var result = FullName.Create(fullName);

        // Assert

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Name.Should().Be(fullName);
    }
}

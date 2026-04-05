using FluentAssertions;
using YaeaY.Account.Domain.Enumerators;
using YaeaY.Account.Domain.ValueObjects.Phones;

namespace YaeaY.Account.Domain.UnitTests.ValueObjects.Phones.PhoneNumberTests;

public class PhoneNumberCreateTests
{
    // IsFailure

    [Fact]
    public void Create_WhenPhoneTypeIsUnknown_ShouldFailure()
    {
        // Arrange

        var ddi = CountryCallingCode.Brazil;
        var phoneType = PhoneType.Unknown;
        var inputNumber = "11987654321";

        // Act

        var result = PhoneNumber.Create(ddi, phoneType, inputNumber);

        // Assert

        result.IsFailure.Should().BeTrue();
        result.Error.Identifier.Should().Be("PHONE_TYPE_UNKNOWN");
        result.Error.Message.Should().Be("Phone type cannot be unknown.");
    }

    [Fact]
    public void Create_WhenPhoneNumberIsNull_ShouldFailure()
    {
        // Arrange

        var ddi = CountryCallingCode.Brazil;
        var phoneType = PhoneType.Mobile;
        string inputNumber = null!;

        // Act

        var result = PhoneNumber.Create(ddi, phoneType, inputNumber);

        // Assert

        result.IsFailure.Should().BeTrue();
        result.Error.Identifier.Should().Be("PHONE_NULL_EMPTY_WHITE_SPACE");
        result.Error.Message.Should().Be("Phone number cannot be null, empty or white space.");
    }

    [Fact]
    public void Create_WhenPhoneNumberIsEmpty_ShouldFailure()
    {
        // Arrange

        var ddi = CountryCallingCode.Brazil;
        var phoneType = PhoneType.Mobile;
        var inputNumber = string.Empty;

        // Act

        var result = PhoneNumber.Create(ddi, phoneType, inputNumber);

        // Assert

        result.IsFailure.Should().BeTrue();
        result.Error.Identifier.Should().Be("PHONE_NULL_EMPTY_WHITE_SPACE");
        result.Error.Message.Should().Be("Phone number cannot be null, empty or white space.");
    }

    [Fact]
    public void Create_WhenPhoneNumberContainsWhiteSpaceOnly_ShouldFailure()
    {
        // Arrange

        var ddi = CountryCallingCode.Brazil;
        var phoneType = PhoneType.Mobile;
        var inputNumber = "   ";

        // Act

        var result = PhoneNumber.Create(ddi, phoneType, inputNumber);

        // Assert

        result.IsFailure.Should().BeTrue();
        result.Error.Identifier.Should().Be("PHONE_NULL_EMPTY_WHITE_SPACE");
        result.Error.Message.Should().Be("Phone number cannot be null, empty or white space.");
    }

    [Fact]
    public void Create_WhenPhoneNumberHasNoDigits_ShouldFailure()
    {
        // Arrange

        var ddi = CountryCallingCode.Brazil;
        var phoneType = PhoneType.Mobile;
        var inputNumber = "(ab) cdef-ghij";

        // Act

        var result = PhoneNumber.Create(ddi, phoneType, inputNumber);

        // Assert

        result.IsFailure.Should().BeTrue();
        result.Error.Identifier.Should().Be("PHONE_INVALID");
        result.Error.Message.Should().Be("Phone number must contain digits.");
    }

    // IsSuccess

    [Fact]
    public void Create_WhenPhoneNumberIsValid_ShouldSuccess()
    {
        // Arrange

        var ddi = CountryCallingCode.Brazil;
        var phoneType = PhoneType.Mobile;
        var inputNumber = "11987654321";

        // Act

        var result = PhoneNumber.Create(ddi, phoneType, inputNumber);

        // Assert

        result.IsSuccess.Should().BeTrue();
        result.Value.Ddi.Should().Be(ddi);
        result.Value.PhoneType.Should().Be(phoneType);
        result.Value.Number.Should().Be("11987654321");
        result.Value.E164.Should().Be($"+{(int)ddi}11987654321");
    }

    [Fact]
    public void Create_WhenPhoneNumberContainsFormatting_ShouldNormalizeAndSuccess()
    {
        // Arrange

        var ddi = CountryCallingCode.Brazil;
        var phoneType = PhoneType.Mobile;
        var inputNumber = "(11) 98765-4321";

        // Act

        var result = PhoneNumber.Create(ddi, phoneType, inputNumber);

        // Assert

        result.IsSuccess.Should().BeTrue();
        result.Value.Number.Should().Be("11987654321");
        result.Value.E164.Should().Be($"+{(int)ddi}11987654321");
    }
}
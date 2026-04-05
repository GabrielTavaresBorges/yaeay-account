using FluentAssertions;
using YaeaY.Account.Domain.Abstraction.Exceptions;
using YaeaY.Account.Domain.Entities.UserPhones;
using YaeaY.Account.Domain.Enumerators;

namespace YaeaY.Account.Domain.UnitTests.Entities.UserPhonesTests;

public class UserPhonesCreateTests
{
    // IsFailure

    #region CallingCode

    [Fact]
    public void Create_WhenCallingCodeIsNull_ShouldThrowDomainException()
    {
        // Arrange

        string callingCode = null!;

        var regionCode = "BR";
        var areaCode = "48";
        var phoneType = PhoneType.Mobile;
        var phoneNumber = "12345678";
        var e164 = "+554812345678";
        var isPrimary = true;

        // Act

        Action act = () => UserPhone.Create(callingCode,
            regionCode,
            areaCode,
            phoneType,
            phoneNumber,
            e164,
            isPrimary);

        // Assert

        var exception = act.Should().Throw<DomainException>().Which;
        exception.Identifier.Should().Be("PHONE_CALLING_CODE_NULL_EMPTY_WHITE_SPACE");
        exception.Message.Should().Be("CallingCode cannot be null, empty or white space.");
    }

    [Fact]
    public void Create_WhenCallingCodeIsEmpty_ShouldThrowDomainException()
    {
        // Arrange

        string callingCode = string.Empty;

        var regionCode = "BR";
        var areaCode = "48";
        var phoneType = PhoneType.Mobile;
        var phoneNumber = "12345678";
        var e164 = "+554812345678";
        var isPrimary = true;

        // Act

        Action act = () => UserPhone.Create(callingCode,
            regionCode,
            areaCode,
            phoneType,
            phoneNumber,
            e164,
            isPrimary);

        // Assert

        var exception = act.Should().Throw<DomainException>().Which;
        exception.Identifier.Should().Be("PHONE_CALLING_CODE_NULL_EMPTY_WHITE_SPACE");
        exception.Message.Should().Be("CallingCode cannot be null, empty or white space.");
    }

    [Fact]
    public void Create_WhenCallingCodeContainsWhiteSpaceOnly_ShouldThrowDomainException()
    {
        // Arrange

        string callingCode = " ";

        var regionCode = "BR";
        var areaCode = "48";
        var phoneType = PhoneType.Mobile;
        var phoneNumber = "12345678";
        var e164 = "+554812345678";
        var isPrimary = true;

        // Act

        Action act = () => UserPhone.Create(callingCode,
            regionCode,
            areaCode,
            phoneType,
            phoneNumber,
            e164,
            isPrimary);

        // Assert

        var exception = act.Should().Throw<DomainException>().Which;
        exception.Identifier.Should().Be("PHONE_CALLING_CODE_NULL_EMPTY_WHITE_SPACE");
        exception.Message.Should().Be("CallingCode cannot be null, empty or white space.");
    }

    [Fact]
    public void Create_WhenCallingCodeLengthIsLessThanTwo_ShouldThrowDomainException()
    {
        // Arrange

        var callingCode = "+";
        var regionCode = "BR";
        var areaCode = "48";
        var phoneType = PhoneType.Mobile;
        var phoneNumber = "12345678";
        var e164 = "+554812345678";
        var isPrimary = true;

        // Act

        Action act = () => UserPhone.Create(
            callingCode,
            regionCode,
            areaCode,
            phoneType,
            phoneNumber,
            e164,
            isPrimary);

        // Assert

        var exception = act.Should().Throw<DomainException>().Which;
        exception.Identifier.Should().Be("PHONE_CALLING_CODE_INVALID");
        exception.Message.Should().Be("CallingCode must be in format +<digits> (e.g., +55, +1).");
    }

    [Fact]
    public void Create_WhenCallingCodeDoesNotStartWithPlus_ShouldThrowDomainException()
    {
        // Arrange

        var callingCode = "55";
        var regionCode = "BR";
        var areaCode = "48";
        var phoneType = PhoneType.Mobile;
        var phoneNumber = "12345678";
        var e164 = "+554812345678";
        var isPrimary = true;

        // Act

        Action act = () => UserPhone.Create(
            callingCode,
            regionCode,
            areaCode,
            phoneType,
            phoneNumber,
            e164,
            isPrimary);

        // Assert

        var exception = act.Should().Throw<DomainException>().Which;
        exception.Identifier.Should().Be("PHONE_CALLING_CODE_INVALID");
        exception.Message.Should().Be("CallingCode must be in format +<digits> (e.g., +55, +1).");
    }

    #endregion

    #region RegionCode

    [Fact]
    public void Create_WhenRegionCodeIsNull_ShouldThrowDomainException()
    {
        // Arrange

        string regionCode = null!;

        string callingCode = "+55";
        var areaCode = "48";
        var phoneType = PhoneType.Mobile;
        var phoneNumber = "12345678";
        var e164 = "+554812345678";
        var isPrimary = true;

        // Act

        Action act = () => UserPhone.Create(callingCode,
            regionCode,
            areaCode,
            phoneType,
            phoneNumber,
            e164,
            isPrimary);

        // Assert

        var exception = act.Should().Throw<DomainException>().Which;
        exception.Identifier.Should().Be("PHONE_REGION_CODE_NULL_EMPTY_WHITE_SPACE");
        exception.Message.Should().Be("RegionCode cannot be null, empty or white space.");
    }

    [Fact]
    public void Create_WhenRegionCodeIsEmpty_ShouldThrowDomainException()
    {
        // Arrange

        var regionCode = string.Empty;

        string callingCode = "+55";
        var areaCode = "48";
        var phoneType = PhoneType.Mobile;
        var phoneNumber = "12345678";
        var e164 = "+554812345678";
        var isPrimary = true;

        // Act

        Action act = () => UserPhone.Create(callingCode,
            regionCode,
            areaCode,
            phoneType,
            phoneNumber,
            e164,
            isPrimary);

        // Assert

        var exception = act.Should().Throw<DomainException>().Which;
        exception.Identifier.Should().Be("PHONE_REGION_CODE_NULL_EMPTY_WHITE_SPACE");
        exception.Message.Should().Be("RegionCode cannot be null, empty or white space.");
    }

    [Fact]
    public void Create_WhenRegionCodeContainsWhiteSpaceOnly_ShouldThrowDomainException()
    {
        // Arrange

        var regionCode = " ";

        string callingCode = "+55";
        var areaCode = "48";
        var phoneType = PhoneType.Mobile;
        var phoneNumber = "12345678";
        var e164 = "+554812345678";
        var isPrimary = true;

        // Act

        Action act = () => UserPhone.Create(callingCode,
            regionCode,
            areaCode,
            phoneType,
            phoneNumber,
            e164,
            isPrimary);

        // Assert

        var exception = act.Should().Throw<DomainException>().Which;
        exception.Identifier.Should().Be("PHONE_REGION_CODE_NULL_EMPTY_WHITE_SPACE");
        exception.Message.Should().Be("RegionCode cannot be null, empty or white space.");
    }

    [Fact]
    public void Create_WhenRegionCodeLengthIsLessThanTwo_ShouldThrowDomainException()
    {
        // Arrange

        var regionCode = "B";

        var callingCode = "+55";
        var areaCode = "48";
        var phoneType = PhoneType.Mobile;
        var phoneNumber = "12345678";
        var e164 = "+554812345678";
        var isPrimary = true;

        // Act

        Action act = () => UserPhone.Create(
            callingCode,
            regionCode,
            areaCode,
            phoneType,
            phoneNumber,
            e164,
            isPrimary);

        // Assert

        var exception = act.Should().Throw<DomainException>().Which;
        exception.Identifier.Should().Be("PHONE_REGION_CODE_INVALID");
        exception.Message.Should().Be("RegionCode must be a valid ISO2 code (e.g., BR, US, CA).");
    }

    [Fact]
    public void Create_WhenRegionCodeLengthIsGreaterThanTwo_ShouldThrowDomainException()
    {
        // Arrange

        var regionCode = "BRA";

        var callingCode = "+55";
        var areaCode = "48";
        var phoneType = PhoneType.Mobile;
        var phoneNumber = "12345678";
        var e164 = "+554812345678";
        var isPrimary = true;

        // Act

        Action act = () => UserPhone.Create(
            callingCode,
            regionCode,
            areaCode,
            phoneType,
            phoneNumber,
            e164,
            isPrimary);

        // Assert

        var exception = act.Should().Throw<DomainException>().Which;
        exception.Identifier.Should().Be("PHONE_REGION_CODE_INVALID");
        exception.Message.Should().Be("RegionCode must be a valid ISO2 code (e.g., BR, US, CA).");
    }

    [Fact]
    public void Create_WhenRegionCodeContainsNumber_ShouldThrowDomainException()
    {
        // Arrange

        var regionCode = "B1";

        var callingCode = "+55";
        var areaCode = "48";
        var phoneType = PhoneType.Mobile;
        var phoneNumber = "12345678";
        var e164 = "+554812345678";
        var isPrimary = true;

        // Act

        Action act = () => UserPhone.Create(
            callingCode,
            regionCode,
            areaCode,
            phoneType,
            phoneNumber,
            e164,
            isPrimary);

        // Assert

        var exception = act.Should().Throw<DomainException>().Which;
        exception.Identifier.Should().Be("PHONE_REGION_CODE_INVALID");
        exception.Message.Should().Be("RegionCode must be a valid ISO2 code (e.g., BR, US, CA).");
    }

    [Fact]
    public void Create_WhenRegionCodeContainsSpecialCharacter_ShouldThrowDomainException()
    {
        // Arrange

        var regionCode = "B@";

        var callingCode = "+55";
        var areaCode = "48";
        var phoneType = PhoneType.Mobile;
        var phoneNumber = "12345678";
        var e164 = "+554812345678";
        var isPrimary = true;

        // Act

        Action act = () => UserPhone.Create(
            callingCode,
            regionCode,
            areaCode,
            phoneType,
            phoneNumber,
            e164,
            isPrimary);

        // Assert

        var exception = act.Should().Throw<DomainException>().Which;
        exception.Identifier.Should().Be("PHONE_REGION_CODE_INVALID");
        exception.Message.Should().Be("RegionCode must be a valid ISO2 code (e.g., BR, US, CA).");
    }

    #endregion

    #region AreaCode

    [Fact]
    public void Create_WhenAreaCodeContainsNonDigitCharacters_ShouldThrowDomainException()
    {
        // Arrange

        var areaCode = "4A";

        var callingCode = "+55";
        var regionCode = "BR";
        var phoneType = PhoneType.Mobile;
        var phoneNumber = "12345678";
        var e164 = "+554812345678";
        var isPrimary = true;

        // Act

        Action act = () => UserPhone.Create(
            callingCode,
            regionCode,
            areaCode,
            phoneType,
            phoneNumber,
            e164,
            isPrimary);

        // Assert

        var exception = act.Should().Throw<DomainException>().Which;
        exception.Identifier.Should().Be("PHONE_AREA_CODE_INVALID");
        exception.Message.Should().Be("AreaCode must contain digits only.");
    }

    #endregion

    #region PhoneType

    [Fact]
    public void Create_WhenPhoneTypeIsUnknown_ShouldThrowDomainException()
    {
        // Arrange

        var phoneType = PhoneType.Unknown;

        var callingCode = "+55";
        var regionCode = "BR";
        var areaCode = "48";
        var phoneNumber = "12345678";
        var e164 = "+554812345678";
        var isPrimary = true;

        // Act

        Action act = () => UserPhone.Create(
            callingCode,
            regionCode,
            areaCode,
            phoneType,
            phoneNumber,
            e164,
            isPrimary);

        // Assert

        var exception = act.Should().Throw<DomainException>().Which;
        exception.Identifier.Should().Be("PHONE_TYPE_UNKNOWN");
        exception.Message.Should().Be("Phone type cannot be unknown.");
    }

    [Fact]
    public void Create_WhenPhoneTypeIsInvalid_ShouldThrowDomainException()
    {
        // Arrange

        var phoneType = (PhoneType)999;

        var callingCode = "+55";
        var regionCode = "BR";
        var areaCode = "48";
        var phoneNumber = "12345678";
        var e164 = "+554812345678";
        var isPrimary = true;

        // Act

        Action act = () => UserPhone.Create(
            callingCode,
            regionCode,
            areaCode,
            phoneType,
            phoneNumber,
            e164,
            isPrimary);

        // Assert

        var exception = act.Should().Throw<DomainException>().Which;
        exception.Identifier.Should().Be("PHONE_TYPE_INVALID");
        exception.Message.Should().Be("Phone type is invalid.");
    }

    #endregion

    #region PhoneNumber

    [Fact]
    public void Create_WhenPhoneNumberIsNull_ShouldThrowDomainException()
    {
        // Arrange

        string phoneNumber = null!;

        var callingCode = "+55";
        var regionCode = "BR";
        var areaCode = "48";
        var phoneType = PhoneType.Mobile;
        var e164 = "+554812345678";
        var isPrimary = true;

        // Act

        Action act = () => UserPhone.Create(
            callingCode,
            regionCode,
            areaCode,
            phoneType,
            phoneNumber,
            e164,
            isPrimary);

        // Assert

        var exception = act.Should().Throw<DomainException>().Which;
        exception.Identifier.Should().Be("PHONE_NUMBER_NULL_EMPTY_WHITE_SPACE");
        exception.Message.Should().Be("Phone number cannot be null, empty or white space.");
    }

    [Fact]
    public void Create_WhenPhoneNumberIsEmpty_ShouldThrowDomainException()
    {
        // Arrange

        var phoneNumber = string.Empty;

        var callingCode = "+55";
        var regionCode = "BR";
        var areaCode = "48";
        var phoneType = PhoneType.Mobile;
        var e164 = "+554812345678";
        var isPrimary = true;

        // Act

        Action act = () => UserPhone.Create(
            callingCode,
            regionCode,
            areaCode,
            phoneType,
            phoneNumber,
            e164,
            isPrimary);

        // Assert

        var exception = act.Should().Throw<DomainException>().Which;
        exception.Identifier.Should().Be("PHONE_NUMBER_NULL_EMPTY_WHITE_SPACE");
        exception.Message.Should().Be("Phone number cannot be null, empty or white space.");
    }

    [Fact]
    public void Create_WhenPhoneNumberContainsWhiteSpaceOnly_ShouldThrowDomainException()
    {
        // Arrange

        var phoneNumber = "  ";

        var callingCode = "+55";
        var regionCode = "BR";
        var areaCode = "48";
        var phoneType = PhoneType.Mobile;
        var e164 = "+554812345678";
        var isPrimary = true;

        // Act

        Action act = () => UserPhone.Create(
            callingCode,
            regionCode,
            areaCode,
            phoneType,
            phoneNumber,
            e164,
            isPrimary);

        // Assert

        var exception = act.Should().Throw<DomainException>().Which;
        exception.Identifier.Should().Be("PHONE_NUMBER_NULL_EMPTY_WHITE_SPACE");
        exception.Message.Should().Be("Phone number cannot be null, empty or white space.");
    }

    [Fact]
    public void Create_WhenPhoneNumberContainsLetters_ShouldThrowDomainException()
    {
        // Arrange

        var phoneNumber = "1234ABCD";

        var callingCode = "+55";
        var regionCode = "BR";
        var areaCode = "48";
        var phoneType = PhoneType.Mobile;
        var e164 = "+554812345678";
        var isPrimary = true;

        // Act

        Action act = () => UserPhone.Create(
            callingCode,
            regionCode,
            areaCode,
            phoneType,
            phoneNumber,
            e164,
            isPrimary);

        // Assert

        var exception = act.Should().Throw<DomainException>().Which;
        exception.Identifier.Should().Be("PHONE_NUMBER_INVALID");
        exception.Message.Should().Be("Phone number must contain digits only.");
    }

    [Fact]
    public void Create_WhenPhoneNumberContainsSpecialCharacters_ShouldThrowDomainException()
    {
        // Arrange

        var phoneNumber = "1234-5678";

        var callingCode = "+55";
        var regionCode = "BR";
        var areaCode = "48";
        var phoneType = PhoneType.Mobile;
        var e164 = "+554812345678";
        var isPrimary = true;

        // Act

        Action act = () => UserPhone.Create(
            callingCode,
            regionCode,
            areaCode,
            phoneType,
            phoneNumber,
            e164,
            isPrimary);

        // Assert

        var exception = act.Should().Throw<DomainException>().Which;
        exception.Identifier.Should().Be("PHONE_NUMBER_INVALID");
        exception.Message.Should().Be("Phone number must contain digits only.");
    }

    [Fact]
    public void Create_WhenPhoneNumberContainsInternalWhiteSpace_ShouldThrowDomainException()
    {
        // Arrange

        var phoneNumber = "1234 5678";

        var callingCode = "+55";
        var regionCode = "BR";
        var areaCode = "48";
        var phoneType = PhoneType.Mobile;
        var e164 = "+554812345678";
        var isPrimary = true;

        // Act

        Action act = () => UserPhone.Create(
            callingCode,
            regionCode,
            areaCode,
            phoneType,
            phoneNumber,
            e164,
            isPrimary);

        // Assert

        var exception = act.Should().Throw<DomainException>().Which;
        exception.Identifier.Should().Be("PHONE_NUMBER_INVALID");
        exception.Message.Should().Be("Phone number must contain digits only.");
    }

    #endregion     

    #region E164

    [Fact]
    public void Create_WhenE164IsNull_ShouldThrowDomainException()
    {
        // Arrange

        string e164 = null!;

        var callingCode = "+55";
        var regionCode = "BR";
        var areaCode = "48";
        var phoneType = PhoneType.Mobile;
        var phoneNumber = "12345678";
        var isPrimary = true;

        // Act

        Action act = () => UserPhone.Create(
            callingCode,
            regionCode,
            areaCode,
            phoneType,
            phoneNumber,
            e164,
            isPrimary);

        // Assert

        var exception = act.Should().Throw<DomainException>().Which;
        exception.Identifier.Should().Be("PHONE_E164_NULL_EMPTY_WHITE_SPACE");
        exception.Message.Should().Be("E164 cannot be null, empty or white space.");
    }

    [Fact]
    public void Create_WhenE164IsEmpty_ShouldThrowDomainException()
    {
        // Arrange

        var e164 = string.Empty;

        var callingCode = "+55";
        var regionCode = "BR";
        var areaCode = "48";
        var phoneType = PhoneType.Mobile;
        var phoneNumber = "12345678";
        var isPrimary = true;

        // Act

        Action act = () => UserPhone.Create(
            callingCode,
            regionCode,
            areaCode,
            phoneType,
            phoneNumber,
            e164,
            isPrimary);

        // Assert

        var exception = act.Should().Throw<DomainException>().Which;
        exception.Identifier.Should().Be("PHONE_E164_NULL_EMPTY_WHITE_SPACE");
        exception.Message.Should().Be("E164 cannot be null, empty or white space.");
    }

    [Fact]
    public void Create_WhenE164ContainsWhiteSpaceOnly_ShouldThrowDomainException()
    {
        // Arrange

        var e164 = "   ";

        var callingCode = "+55";
        var regionCode = "BR";
        var areaCode = "48";
        var phoneType = PhoneType.Mobile;
        var phoneNumber = "12345678";
        var isPrimary = true;

        // Act

        Action act = () => UserPhone.Create(
            callingCode,
            regionCode,
            areaCode,
            phoneType,
            phoneNumber,
            e164,
            isPrimary);

        // Assert

        var exception = act.Should().Throw<DomainException>().Which;
        exception.Identifier.Should().Be("PHONE_E164_NULL_EMPTY_WHITE_SPACE");
        exception.Message.Should().Be("E164 cannot be null, empty or white space.");
    }

    [Fact]
    public void Create_WhenE164DoesNotStartWithPlus_ShouldThrowDomainException()
    {
        // Arrange

        var e164 = "554812345678";

        var callingCode = "+55";
        var regionCode = "BR";
        var areaCode = "48";
        var phoneType = PhoneType.Mobile;
        var phoneNumber = "12345678";
        var isPrimary = true;

        // Act

        Action act = () => UserPhone.Create(
            callingCode,
            regionCode,
            areaCode,
            phoneType,
            phoneNumber,
            e164,
            isPrimary);

        // Assert

        var exception = act.Should().Throw<DomainException>().Which;
        exception.Identifier.Should().Be("PHONE_E164_INVALID");
        exception.Message.Should().Be("E164 must be in format +<digits>.");
    }

    [Fact]
    public void Create_WhenE164LengthIsLessThanTwo_ShouldThrowDomainException()
    {
        // Arrange

        var e164 = "+";

        var callingCode = "+55";
        var regionCode = "BR";
        var areaCode = "48";
        var phoneType = PhoneType.Mobile;
        var phoneNumber = "12345678";
        var isPrimary = true;

        // Act

        Action act = () => UserPhone.Create(
            callingCode,
            regionCode,
            areaCode,
            phoneType,
            phoneNumber,
            e164,
            isPrimary);

        // Assert

        var exception = act.Should().Throw<DomainException>().Which;
        exception.Identifier.Should().Be("PHONE_E164_INVALID");
        exception.Message.Should().Be("E164 must be in format +<digits>.");
    }

    [Fact]
    public void Create_WhenE164ContainsLetters_ShouldThrowDomainException()
    {
        // Arrange

        var e164 = "+5548ABCD";

        var callingCode = "+55";
        var regionCode = "BR";
        var areaCode = "48";
        var phoneType = PhoneType.Mobile;
        var phoneNumber = "12345678";
        var isPrimary = true;

        // Act

        Action act = () => UserPhone.Create(
            callingCode,
            regionCode,
            areaCode,
            phoneType,
            phoneNumber,
            e164,
            isPrimary);

        // Assert

        var exception = act.Should().Throw<DomainException>().Which;
        exception.Identifier.Should().Be("PHONE_E164_INVALID");
        exception.Message.Should().Be("E164 must be in format +<digits>.");
    }

    [Fact]
    public void Create_WhenE164ContainsSpecialCharacters_ShouldThrowDomainException()
    {
        // Arrange

        var e164 = "+55-48-12345678";

        var callingCode = "+55";
        var regionCode = "BR";
        var areaCode = "48";
        var phoneType = PhoneType.Mobile;
        var phoneNumber = "12345678";
        var isPrimary = true;

        // Act

        Action act = () => UserPhone.Create(
            callingCode,
            regionCode,
            areaCode,
            phoneType,
            phoneNumber,
            e164,
            isPrimary);

        // Assert

        var exception = act.Should().Throw<DomainException>().Which;
        exception.Identifier.Should().Be("PHONE_E164_INVALID");
        exception.Message.Should().Be("E164 must be in format +<digits>.");
    }

    [Fact]
    public void Create_WhenE164ContainsInternalWhiteSpace_ShouldThrowDomainException()
    {
        // Arrange

        var e164 = "+55 48 12345678";

        var callingCode = "+55";
        var regionCode = "BR";
        var areaCode = "48";
        var phoneType = PhoneType.Mobile;
        var phoneNumber = "12345678";
        var isPrimary = true;

        // Act

        Action act = () => UserPhone.Create(
            callingCode,
            regionCode,
            areaCode,
            phoneType,
            phoneNumber,
            e164,
            isPrimary);

        // Assert

        var exception = act.Should().Throw<DomainException>().Which;
        exception.Identifier.Should().Be("PHONE_E164_INVALID");
        exception.Message.Should().Be("E164 must be in format +<digits>.");
    }

    #endregion

    // IsSuccess

    #region AreaCode

    [Fact]
    public void Create_WhenAreaCodeIsNull_ShouldSuccess()
    {
        // Arrange

        string? areaCode = null;

        var callingCode = "+55";
        var regionCode = "BR";
        var phoneType = PhoneType.Mobile;
        var phoneNumber = "12345678";
        var e164 = "+554812345678";
        var isPrimary = true;

        // Act

        var userPhone = UserPhone.Create(
            callingCode,
            regionCode,
            areaCode,
            phoneType,
            phoneNumber,
            e164,
            isPrimary);

        // Assert

        userPhone.Should().NotBeNull();
        userPhone.AreaCode.Should().BeNull();
    }

    [Fact]
    public void Create_WhenAreaCodeIsEmpty_ShouldSuccess()
    {
        // Arrange

        var areaCode = string.Empty;

        var callingCode = "+55";
        var regionCode = "BR";
        var phoneType = PhoneType.Mobile;
        var phoneNumber = "12345678";
        var e164 = "+554812345678";
        var isPrimary = true;

        // Act

        var userPhone = UserPhone.Create(
            callingCode,
            regionCode,
            areaCode,
            phoneType,
            phoneNumber,
            e164,
            isPrimary);

        // Assert

        userPhone.Should().NotBeNull();
        userPhone.AreaCode.Should().BeNull();
    }

    [Fact]
    public void Create_WhenAreaCodeContainsWhiteSpaceOnly_ShouldSuccess()
    {
        // Arrange

        var areaCode = " ";

        var callingCode = "+55";
        var regionCode = "BR";
        var phoneType = PhoneType.Mobile;
        var phoneNumber = "12345678";
        var e164 = "+554812345678";
        var isPrimary = true;

        // Act

        var userPhone = UserPhone.Create(
            callingCode,
            regionCode,
            areaCode,
            phoneType,
            phoneNumber,
            e164,
            isPrimary);

        // Assert

        userPhone.Should().NotBeNull();
        userPhone.AreaCode.Should().BeNull();
    }

    [Fact]
    public void Create_WhenAreaCodeHasLeadingOrTrailingSpaces_ShouldTrimAndSuccess()
    {
        // Arrange

        var areaCode = " 48 ";

        var callingCode = "+55";
        var regionCode = "BR";
        var phoneType = PhoneType.Mobile;
        var phoneNumber = "12345678";
        var e164 = "+554812345678";
        var isPrimary = true;

        // Act

        var userPhone = UserPhone.Create(
            callingCode,
            regionCode,
            areaCode,
            phoneType,
            phoneNumber,
            e164,
            isPrimary);

        // Assert

        userPhone.Should().NotBeNull();
        userPhone.AreaCode.Should().Be("48");
    }

    #endregion

    [Fact]
    public void Create_WhenAllDataIsValid_ShouldSucceed()
    {
        // Arrange

        var callingCode = "+55";
        var regionCode = "BR";
        var areaCode = "48";
        var phoneType = PhoneType.Mobile;
        var phoneNumber = "12345678";
        var e164 = "+554812345678";
        var isPrimary = true;

        // Act

        var userPhone = UserPhone.Create(
            callingCode,
            regionCode,
            areaCode,
            phoneType,
            phoneNumber,
            e164,
            isPrimary);

        // Assert

        userPhone.Should().NotBeNull();
        userPhone.CallingCode.Should().Be(callingCode);
        userPhone.RegionCode.Should().Be(regionCode);
        userPhone.AreaCode.Should().Be(areaCode);
        userPhone.PhoneType.Should().Be(phoneType);
        userPhone.PhoneNumber.Should().Be(phoneNumber);
        userPhone.E164.Should().Be(e164);
        userPhone.IsPrimary.Should().BeTrue();
        userPhone.IsVerified.Should().BeFalse();
        userPhone.VerifiedAt.Should().BeNull();
        userPhone.CreatedAt.Should().NotBe(default);
    }
}

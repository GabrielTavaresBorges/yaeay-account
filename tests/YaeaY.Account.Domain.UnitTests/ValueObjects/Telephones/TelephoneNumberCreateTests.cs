using FluentAssertions;
using YaeaY.Account.Domain.Enumerators;
using YaeaY.Account.Domain.Errors.Telephones;
using YaeaY.Account.Domain.ValueObjects.Telephones;

namespace YaeaY.Account.Domain.UnitTests.ValueObjects.Telephones;

public class TelephoneNumberCreateTests
{
    // IsFailure

    #region CallingCode
    [Fact]
    public void Create_WhenCallingCodeIsNull_ShouldFail_WithTelephoneNumberErrorsCallingCodeRequired()
    {
        // Arrange

        string callingCodeInvalid = null!;

        string regionCode = "BR";
        string areaCode = "48";
        TelephoneType phoneType = TelephoneType.Mobile;
        string nationalNumber = "984441122";
        string e164 = "+5548984441122";

        // Act

        var result = TelephoneNumber.Create(
            callingCodeInvalid,
            regionCode,
            areaCode,
            phoneType,
            nationalNumber,
            e164);

        // Assert

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(TelephoneNumberErrors.CallingCodeRequired);
    }

    [Fact]
    public void Create_WhenCallingCodeIsEmpty_ShouldFail_WithTelephoneNumberErrorsCallingCodeRequired()
    {
        // Arrange

        string callingCodeInvalid = string.Empty;

        string regionCode = "BR";
        string areaCode = "48";
        TelephoneType phoneType = TelephoneType.Mobile;
        string nationalNumber = "984441122";
        string e164 = "+5548984441122";

        // Act

        var result = TelephoneNumber.Create(
            callingCodeInvalid,
            regionCode,
            areaCode,
            phoneType,
            nationalNumber,
            e164);

        // Assert

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(TelephoneNumberErrors.CallingCodeRequired);
    }

    [Fact]
    public void Create_WhenCallingCodeContainsWhitespaceOnly_ShouldFail_WithTelephoneNumberErrorsCallingCodeRequired()
    {
        // Arrange

        string callingCodeInvalid = " ";

        string regionCode = "BR";
        string areaCode = "48";
        TelephoneType phoneType = TelephoneType.Mobile;
        string nationalNumber = "984441122";
        string e164 = "+5548984441122";

        // Act

        var result = TelephoneNumber.Create(
            callingCodeInvalid,
            regionCode,
            areaCode,
            phoneType,
            nationalNumber,
            e164);

        // Assert

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(TelephoneNumberErrors.CallingCodeRequired);
    }

    [Fact]
    public void Create_WhenCallingCodeDoesNotStartWithPlus_ShouldFail_WithTelephoneNumberErrorsCallingCodeInvalid()
    {
        // Arrange

        string callingCodeInvalid = "55";

        string regionCode = "BR";
        string areaCode = "48";
        TelephoneType phoneType = TelephoneType.Mobile;
        string nationalNumber = "984441122";
        string e164 = "+5548984441122";

        // Act

        var result = TelephoneNumber.Create(
            callingCodeInvalid,
            regionCode,
            areaCode,
            phoneType,
            nationalNumber,
            e164);

        // Assert

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(TelephoneNumberErrors.CallingCodeInvalid);
    }

    [Fact]
    public void Create_WhenCallingCodeDoesNotContainDigits_ShouldFail_WithTelephoneNumberErrorsCallingCodeInvalid()
    {
        // Arrange

        string callingCodeInvalid = "+";

        string regionCode = "BR";
        string areaCode = "48";
        TelephoneType phoneType = TelephoneType.Mobile;
        string nationalNumber = "984441122";
        string e164 = "+5548984441122";

        // Act

        var result = TelephoneNumber.Create(
            callingCodeInvalid,
            regionCode,
            areaCode,
            phoneType,
            nationalNumber,
            e164);

        // Assert

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(TelephoneNumberErrors.CallingCodeInvalid);
    }

    [Fact]
    public void Create_WhenCallingCodeContainsNonDigitCharacters_ShouldFail_WithTelephoneNumberErrorsCallingCodeInvalid()
    {
        // Arrange

        string callingCodeInvalid = "+5A";

        string regionCode = "BR";
        string areaCode = "48";
        TelephoneType phoneType = TelephoneType.Mobile;
        string nationalNumber = "984441122";
        string e164 = "+5548984441122";

        // Act

        var result = TelephoneNumber.Create(
            callingCodeInvalid,
            regionCode,
            areaCode,
            phoneType,
            nationalNumber,
            e164);

        // Assert

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(TelephoneNumberErrors.CallingCodeInvalid);
    }
    #endregion

    #region RegionCode

    [Fact]
    public void Create_WhenRegionCodeIsNull_ShouldFail_WithTelephoneNumberErrorsRegionCodeRequired()
    {
        // Arrange

        string regionCodeInvalid = null!;

        string callingCode = "+55";
        string areaCode = "48";
        TelephoneType phoneType = TelephoneType.Mobile;
        string nationalNumber = "984441122";
        string e164 = "+5548984441122";

        // Act

        var result = TelephoneNumber.Create(
            callingCode,
            regionCodeInvalid,
            areaCode,
            phoneType,
            nationalNumber,
            e164);

        // Assert

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(TelephoneNumberErrors.RegionCodeRequired);
    }

    [Fact]
    public void Create_WhenRegionCodeIsEmpty_ShouldFail_WithTelephoneNumberErrorsRegionCodeRequired()
    {
        // Arrange

        string regionCodeInvalid = string.Empty;

        string callingCode = "+55";
        string areaCode = "48";
        TelephoneType phoneType = TelephoneType.Mobile;
        string nationalNumber = "984441122";
        string e164 = "+5548984441122";

        // Act

        var result = TelephoneNumber.Create(
            callingCode,
            regionCodeInvalid,
            areaCode,
            phoneType,
            nationalNumber,
            e164);

        // Assert

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(TelephoneNumberErrors.RegionCodeRequired);
    }

    [Fact]
    public void Create_WhenRegionCodeContainsWhitespaceOnly_ShouldFail_WithTelephoneNumberErrorsRegionCodeRequired()
    {
        // Arrange

        string regionCodeInvalid = " ";

        string callingCode = "+55";
        string areaCode = "48";
        TelephoneType phoneType = TelephoneType.Mobile;
        string nationalNumber = "984441122";
        string e164 = "+5548984441122";

        // Act

        var result = TelephoneNumber.Create(
            callingCode,
            regionCodeInvalid,
            areaCode,
            phoneType,
            nationalNumber,
            e164);

        // Assert

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(TelephoneNumberErrors.RegionCodeRequired);
    }

    [Fact]
    public void Create_WhenRegionCodeDoesNotHaveTwoCharacters_ShouldFail_WithTelephoneNumberErrorsRegionCodeInvalid()
    {
        // Arrange

        string regionCodeInvalid = "BRA";

        string callingCode = "+55";
        string areaCode = "48";
        TelephoneType phoneType = TelephoneType.Mobile;
        string nationalNumber = "984441122";
        string e164 = "+5548984441122";

        // Act

        var result = TelephoneNumber.Create(
            callingCode,
            regionCodeInvalid,
            areaCode,
            phoneType,
            nationalNumber,
            e164);

        // Assert

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(TelephoneNumberErrors.RegionCodeInvalid);
    }

    [Fact]
    public void Create_WhenRegionCodeContainsNonLetterCharacters_ShouldFail_WithTelephoneNumberErrorsRegionCodeInvalid()
    {
        // Arrange

        string regionCodeInvalid = "B1";

        string callingCode = "+55";
        string areaCode = "48";
        TelephoneType phoneType = TelephoneType.Mobile;
        string nationalNumber = "984441122";
        string e164 = "+5548984441122";

        // Act

        var result = TelephoneNumber.Create(
            callingCode,
            regionCodeInvalid,
            areaCode,
            phoneType,
            nationalNumber,
            e164);

        // Assert

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(TelephoneNumberErrors.RegionCodeInvalid);
    }
    #endregion

    #region AreaCode
    [Fact]
    public void Create_WhenAreaCodeContainsNonDigitCharacters_ShouldFail_WithTelephoneNumberErrorsAreaCodeInvalid()
    {
        // Arrange

        string areaCodeInvalid = "4A";

        string callingCode = "+55";
        string regionCode = "BR";        
        TelephoneType phoneType = TelephoneType.Mobile;
        string nationalNumber = "984441122";
        string e164 = "+5548984441122";

        // Act

        var result = TelephoneNumber.Create(
            callingCode,
            regionCode,
            areaCodeInvalid,
            phoneType,
            nationalNumber,
            e164);

        // Assert

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(TelephoneNumberErrors.AreaCodeInvalid);
    }
    #endregion

    #region PhoneType
    [Fact]
    public void Create_WhenPhoneTypeIsUnknown_ShouldFail_WithTelephoneNumberErrorsPhoneTypeRequired()
    {
        // Arrange

        TelephoneType phoneTypeInvalid = TelephoneType.Unknown;

        string callingCode = "+55";
        string regionCode = "BR";
        string areaCode = "48";        
        string nationalNumber = "984441122";
        string e164 = "+5548984441122";

        // Act

        var result = TelephoneNumber.Create(
            callingCode,
            regionCode,
            areaCode,
            phoneTypeInvalid,
            nationalNumber,
            e164);

        // Assert

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(TelephoneNumberErrors.PhoneTypeRequired);
    }

    [Fact]
    public void Create_WhenPhoneTypeIsNotDefined_ShouldFail_WithTelephoneNumberErrorsPhoneTypeInvalid()
    {
        // Arrange

        TelephoneType phoneTypeInvalid = (TelephoneType)999;

        string callingCode = "+55";
        string regionCode = "BR";
        string areaCode = "48";        
        string nationalNumber = "984441122";
        string e164 = "+5548984441122";

        // Act

        var result = TelephoneNumber.Create(
            callingCode,
            regionCode,
            areaCode,
            phoneTypeInvalid,
            nationalNumber,
            e164);

        // Assert

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(TelephoneNumberErrors.PhoneTypeInvalid);
    }
    #endregion

    #region NationalNumber
    [Fact]
    public void Create_WhenNationalNumberIsNull_ShouldFail_WithTelephoneNumberErrorsNationalNumberRequired()
    {
        // Arrange

        string nationalNumberInvalid = null!;

        string callingCode = "+55";
        string regionCode = "BR";
        string areaCode = "48";
        TelephoneType phoneType = TelephoneType.Mobile;
        string e164 = "+5548984441122";

        // Act

        var result = TelephoneNumber.Create(
            callingCode,
            regionCode,
            areaCode,
            phoneType,
            nationalNumberInvalid,
            e164);

        // Assert

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(TelephoneNumberErrors.NationalNumberRequired);
    }

    [Fact]
    public void Create_WhenNationalNumberIsEmpty_ShouldFail_WithTelephoneNumberErrorsNationalNumberRequired()
    {
        // Arrange

        string nationalNumberInvalid = string.Empty;

        string callingCode = "+55";
        string regionCode = "BR";
        string areaCode = "48";
        TelephoneType phoneType = TelephoneType.Mobile;        
        string e164 = "+5548984441122";

        // Act

        var result = TelephoneNumber.Create(
            callingCode,
            regionCode,
            areaCode,
            phoneType,
            nationalNumberInvalid,
            e164);

        // Assert

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(TelephoneNumberErrors.NationalNumberRequired);
    }

    [Fact]
    public void Create_WhenNationalNumberContainsWhitespaceOnly_ShouldFail_WithTelephoneNumberErrorsNationalNumberRequired()
    {
        // Arrange

        string nationalNumberInvalid = " ";

        string callingCode = "+55";
        string regionCode = "BR";
        string areaCode = "48";
        TelephoneType phoneType = TelephoneType.Mobile;        
        string e164 = "+5548984441122";

        // Act

        var result = TelephoneNumber.Create(
            callingCode,
            regionCode,
            areaCode,
            phoneType,
            nationalNumberInvalid,
            e164);

        // Assert

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(TelephoneNumberErrors.NationalNumberRequired);
    }

    [Fact]
    public void Create_WhenNationalNumberContainsNonDigitCharacters_ShouldFail_WithTelephoneNumberErrorsNationalNumberInvalid()
    {
        // Arrange

        string nationalNumberInvalid = "98444A122";

        string callingCode = "+55";
        string regionCode = "BR";
        string areaCode = "48";
        TelephoneType phoneType = TelephoneType.Mobile;        
        string e164 = "+5548984441122";

        // Act

        var result = TelephoneNumber.Create(
            callingCode,
            regionCode,
            areaCode,
            phoneType,
            nationalNumberInvalid,
            e164);

        // Assert

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(TelephoneNumberErrors.NationalNumberInvalid);
    }
    #endregion

    #region E164
    [Fact]
    public void Create_WhenE164IsNull_ShouldFail_WithTelephoneNumberErrorsE164Required()
    {
        // Arrange

        string e164Invalid = null!;

        string callingCode = "+55";
        string regionCode = "BR";
        string areaCode = "48";
        TelephoneType phoneType = TelephoneType.Mobile;
        string nationalNumber = "984441122";

        // Act

        var result = TelephoneNumber.Create(
            callingCode,
            regionCode,
            areaCode,
            phoneType,
            nationalNumber,
            e164Invalid);

        // Assert

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(TelephoneNumberErrors.E164Required);
    }

    [Fact]
    public void Create_WhenE164IsEmpty_ShouldFail_WithTelephoneNumberErrorsE164Required()
    {
        // Arrange

        string e164Invalid = string.Empty;

        string callingCode = "+55";
        string regionCode = "BR";
        string areaCode = "48";
        TelephoneType phoneType = TelephoneType.Mobile;
        string nationalNumber = "984441122";

        // Act

        var result = TelephoneNumber.Create(
            callingCode,
            regionCode,
            areaCode,
            phoneType,
            nationalNumber,
            e164Invalid);

        // Assert

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(TelephoneNumberErrors.E164Required);
    }

    [Fact]
    public void Create_WhenE164ContainsWhitespaceOnly_ShouldFail_WithTelephoneNumberErrorsE164Required()
    {
        // Arrange

        string e164Invalid = " ";

        string callingCode = "+55";
        string regionCode = "BR";
        string areaCode = "48";
        TelephoneType phoneType = TelephoneType.Mobile;
        string nationalNumber = "984441122";

        // Act

        var result = TelephoneNumber.Create(
            callingCode,
            regionCode,
            areaCode,
            phoneType,
            nationalNumber,
            e164Invalid);

        // Assert

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(TelephoneNumberErrors.E164Required);
    }

    [Fact]
    public void Create_WhenE164DoesNotStartWithPlus_ShouldFail_WithTelephoneNumberErrorsE164Invalid()
    {
        // Arrange

        string e164Invalid = "5548984441122";

        string callingCode = "+55";
        string regionCode = "BR";
        string areaCode = "48";
        TelephoneType phoneType = TelephoneType.Mobile;
        string nationalNumber = "984441122";

        // Act

        var result = TelephoneNumber.Create(
            callingCode,
            regionCode,
            areaCode,
            phoneType,
            nationalNumber,
            e164Invalid);

        // Assert

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(TelephoneNumberErrors.E164Invalid);
    }

    [Fact]
    public void Create_WhenE164DoesNotContainDigits_ShouldFail_WithTelephoneNumberErrorsE164Invalid()
    {
        // Arrange

        string e164Invalid = "+";

        string callingCode = "+55";
        string regionCode = "BR";
        string areaCode = "48";
        TelephoneType phoneType = TelephoneType.Mobile;
        string nationalNumber = "984441122";

        // Act

        var result = TelephoneNumber.Create(
            callingCode,
            regionCode,
            areaCode,
            phoneType,
            nationalNumber,
            e164Invalid);

        // Assert

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(TelephoneNumberErrors.E164Invalid);
    }

    [Fact]
    public void Create_WhenE164ContainsNonDigitCharacters_ShouldFail_WithTelephoneNumberErrorsE164Invalid()
    {
        // Arrange

        string e164Invalid = "+554898444112A";

        string callingCode = "+55";
        string regionCode = "BR";
        string areaCode = "48";
        TelephoneType phoneType = TelephoneType.Mobile;
        string nationalNumber = "984441122";

        // Act

        var result = TelephoneNumber.Create(
            callingCode,
            regionCode,
            areaCode,
            phoneType,
            nationalNumber,
            e164Invalid);

        // Assert

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(TelephoneNumberErrors.E164Invalid);
    }

    [Fact]
    public void Create_WhenE164DoesNotMatchDataInconsistent_ShouldFail_WithTelephoneNumberErrorsDataInconsistent()
    {
        // Arrange

        string e164Invalid = "+5548984441123";

        string callingCode = "+55";
        string regionCode = "BR";
        string areaCode = "48";
        TelephoneType phoneType = TelephoneType.Mobile;
        string nationalNumber = "984441122";

        // Act

        var result = TelephoneNumber.Create(
            callingCode,
            regionCode,
            areaCode,
            phoneType,
            nationalNumber,
            e164Invalid);

        // Assert

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(TelephoneNumberErrors.DataInconsistent);
    }
    #endregion

    // IsSuccess

    [Fact]
    public void Create_WhenTelephoneNumberIsValid_ShouldSucceed()
    {
        // Arrange

        string callingCode = "+55";
        string regionCode = "BR";
        string areaCode = "48";
        TelephoneType phoneType = TelephoneType.Mobile;
        string nationalNumber = "984441122";
        string e164 = "+5548984441122";

        // Act

        var result = TelephoneNumber.Create(
            callingCode,
            regionCode,
            areaCode,
            phoneType,
            nationalNumber,
            e164);

        // Assert

        result.IsSuccess.Should().BeTrue();
        result.IsFailure.Should().BeFalse();
        result.Value.Should().NotBeNull();
        result.Value.CallingCode.Should().Be(callingCode);
        result.Value.RegionCode.Should().Be(regionCode);
        result.Value.AreaCode.Should().Be(areaCode);
        result.Value.PhoneType.Should().Be(phoneType);
        result.Value.NationalNumber.Should().Be(nationalNumber);
        result.Value.E164.Should().Be(e164);
    }
}

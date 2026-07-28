using FluentAssertions;
using YaeaY.Account.Domain.Enumerators;
using YaeaY.Account.Domain.Errors.Telephones;
using YaeaY.Account.Domain.ValueObjects.Telephones;

namespace YaeaY.Account.Domain.UnitTests.ValueObjects.Telephones;

public class TelephoneNumberCreateTests
{
    // IsFailure

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

    // IsSuccess
}

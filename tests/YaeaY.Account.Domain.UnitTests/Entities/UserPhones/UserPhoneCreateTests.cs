using FluentAssertions;
using YaeaY.Account.Domain.Abstraction.Exceptions;
using YaeaY.Account.Domain.Entities.UserPhones;
using YaeaY.Account.Domain.Enumerators;
using YaeaY.Account.Domain.Errors.UserPhones;
using YaeaY.Account.Domain.ValueObjects.Telephones;

namespace YaeaY.Account.Domain.UnitTests.Entities.UserPhones;

public class UserPhoneCreateTests
{
    [Fact]
    public void Create_WhenNumberIsNull_ShouldThrowDomainException_WithUserPhoneErrorsNumberRequired()
    {
        // Arrange

        TelephoneNumber numberInvalid = null!;

        // Act

        Action act = () => UserPhone.Create(numberInvalid, isPrimary: true);

        // Assert

        var exception = act.Should().Throw<DomainException>().Which;
        exception.Error.Should().Be(UserPhoneErrors.NumberRequired);
    }

    [Fact]
    public void Create_WhenNumberIsValid_ShouldSucceed()
    {
        // Arrange

        var numberResult = TelephoneNumber.Create(
            callingCode: "+55",
            regionCode: "BR",
            areaCode: "48",
            phoneType: TelephoneType.Mobile,
            nationalNumber: "12345678",
            e164: "+554812345678");

        var number = numberResult.Value;
        var beforeCreation = DateTimeOffset.UtcNow;

        // Act

        var userPhone = UserPhone.Create(number, isPrimary: true);

        var afterCreation = DateTimeOffset.UtcNow;

        // Assert

        userPhone.TelephoneNumber.Should().Be(number);
        userPhone.CallingCode.Should().Be(number.CallingCode);
        userPhone.RegionCode.Should().Be(number.RegionCode);
        userPhone.AreaCode.Should().Be(number.AreaCode);
        userPhone.PhoneType.Should().Be(number.PhoneType);
        userPhone.PhoneNumber.Should().Be(number.NationalNumber);
        userPhone.E164.Should().Be(number.E164);
        userPhone.IsPrimary.Should().BeTrue();
        userPhone.IsVerified.Should().BeFalse();
        userPhone.VerifiedAt.Should().BeNull();
        userPhone.CreatedAt.Should().BeOnOrAfter(beforeCreation);
        userPhone.CreatedAt.Should().BeOnOrBefore(afterCreation);
    }
}

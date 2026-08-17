using FluentAssertions;
using YaeaY.Account.Domain.Abstraction.Exceptions;
using YaeaY.Account.Domain.Entities.AggregateRoots.Users;
using YaeaY.Account.Domain.Enumerators;
using YaeaY.Account.Domain.Errors.Users;
using YaeaY.Account.Domain.ValueObjects.Dates;
using YaeaY.Account.Domain.ValueObjects.Emails;
using YaeaY.Account.Domain.ValueObjects.Names;
using YaeaY.Account.Domain.ValueObjects.Telephones;

namespace YaeaY.Account.Domain.UnitTests.Entities.AggregateRoots.UsersTests;

public sealed class UserLoginTests
{
    // IsFailure

    [Fact]
    public void Login_WhenEmailIsNotConfirmed_ShouldThrowDomainException_WithUserErrorsAccountCannotLogin()
    {
        // Arrange

        var emailAddressTest = "example@domain.com";
        var emailResult = Email.Create(emailAddressTest);
        var email = emailResult.Value;

        var fullNameTest = "Full Name Test";
        var fullNameResult = FullName.Create(fullNameTest);
        var fullName = fullNameResult.Value;

        var gender = Gender.Male;

        var birthDateTest = new DateOnly(2026, 1, 1);
        var birthDateResult = BirthDate.Create(birthDateTest);
        var birthDate = birthDateResult.Value;

        var callingCode = "+55";
        var regionCode = "BR";
        var areaCode = "48";
        var phoneType = TelephoneType.Mobile;
        var nationalNumber = "12345678";
        var e164 = "+554812345678";
        var telephoneNumberResult = TelephoneNumber.Create(
            callingCode,
            regionCode,
            areaCode,
            phoneType,
            nationalNumber,
            e164);

        var telephoneNumber = telephoneNumberResult.Value;

        var user = User.Create(
            email,
            fullName,
            birthDate,
            gender,
            telephoneNumber);

        var loggedInAt = user.CreatedAt.AddMinutes(1);

        // Action

        Action act = () => user.RegisterSuccessfulLogin(loggedInAt);

        // Assert

        var exception = act.Should().Throw<DomainException>().Which;
        exception.Error.Should().Be(UserErrors.AccountCannotLogin);
        user.FirstLoginAt.Should().BeNull();
        user.LastLoginAt.Should().BeNull();
    }

    // IsSuccess

    [Fact]
    public void Login_WhenItIsFirstAccess_ShouldSetFirstAndLastLogin()
    {
        // Arrange

        var emailAddressTest = "example@domain.com";
        var emailResult = Email.Create(emailAddressTest);
        var email = emailResult.Value;

        var fullNameTest = "Full Name Test";
        var fullNameResult = FullName.Create(fullNameTest);
        var fullName = fullNameResult.Value;

        var gender = Gender.Male;

        var birthDateTest = new DateOnly(2026, 1, 1);
        var birthDateResult = BirthDate.Create(birthDateTest);
        var birthDate = birthDateResult.Value;

        var callingCode = "+55";
        var regionCode = "BR";
        var areaCode = "48";
        var phoneType = TelephoneType.Mobile;
        var nationalNumber = "12345678";
        var e164 = "+554812345678";
        var telephoneNumberResult = TelephoneNumber.Create(
            callingCode,
            regionCode,
            areaCode,
            phoneType,
            nationalNumber,
            e164);

        var telephoneNumber = telephoneNumberResult.Value;

        var user = User.Create(
            email,
            fullName,
            birthDate,
            gender,
            telephoneNumber);

        var confirmedAt = user.CreatedAt.AddMinutes(1);
        var loggedInAt = confirmedAt.AddMinutes(1);

        user.ConfirmEmail(confirmedAt);

        // Action

        user.RegisterSuccessfulLogin(loggedInAt);

        // Assert

        user.FirstLoginAt.Should().Be(loggedInAt);
        user.LastLoginAt.Should().Be(loggedInAt);
    }

    [Fact]
    public void Login_WhenItIsNotFirstAccess_ShouldKeepFirstAndUpdateLastLogin()
    {
        // Arrange

        var emailAddressTest = "example@domain.com";
        var emailResult = Email.Create(emailAddressTest);
        var email = emailResult.Value;

        var fullNameTest = "Full Name Test";
        var fullNameResult = FullName.Create(fullNameTest);
        var fullName = fullNameResult.Value;

        var gender = Gender.Male;

        var birthDateTest = new DateOnly(2026, 1, 1);
        var birthDateResult = BirthDate.Create(birthDateTest);
        var birthDate = birthDateResult.Value;

        var callingCode = "+55";
        var regionCode = "BR";
        var areaCode = "48";
        var phoneType = TelephoneType.Mobile;
        var nationalNumber = "12345678";
        var e164 = "+554812345678";
        var telephoneNumberResult = TelephoneNumber.Create(
            callingCode,
            regionCode,
            areaCode,
            phoneType,
            nationalNumber,
            e164);

        var telephoneNumber = telephoneNumberResult.Value;

        var user = User.Create(
            email,
            fullName,
            birthDate,
            gender,
            telephoneNumber);

        var confirmedAt = user.CreatedAt.AddMinutes(1);
        var firstLoginAt = confirmedAt.AddMinutes(1);
        var nextLoginAt = firstLoginAt.AddDays(1);

        user.ConfirmEmail(confirmedAt);
        user.RegisterSuccessfulLogin(firstLoginAt);

        // Action

        user.RegisterSuccessfulLogin(nextLoginAt);

        // Assert

        user.FirstLoginAt.Should().Be(firstLoginAt);
        user.LastLoginAt.Should().Be(nextLoginAt);
    }
}

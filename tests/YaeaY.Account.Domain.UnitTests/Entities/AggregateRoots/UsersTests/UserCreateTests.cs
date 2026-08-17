using FluentAssertions;
using YaeaY.Account.Domain.Abstraction.Exceptions;
using YaeaY.Account.Domain.Entities.AggregateRoots.Users;
using YaeaY.Account.Domain.Enumerators;
using YaeaY.Account.Domain.Errors.Users;
using YaeaY.Account.Domain.Events.Users;
using YaeaY.Account.Domain.ValueObjects.Dates;
using YaeaY.Account.Domain.ValueObjects.Emails;
using YaeaY.Account.Domain.ValueObjects.Names;
using YaeaY.Account.Domain.ValueObjects.Telephones;

namespace YaeaY.Account.Domain.UnitTests.Entities.AggregateRoots.UsersTests;

public class UserCreateTests
{
    // IsFailure

    [Fact]
    public void Create_WhenEmailIsNull_ShouldThrowDomainException_WithUserErrorsEmailRequired()
    {
        // Arrange

        Email emailInvalid = null!;

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

        // Action

        Action act = () => User.Create(
            emailInvalid,
            fullName,
            birthDate,
            gender,
            telephoneNumber);

        // Assert

        var exception = act.Should().Throw<DomainException>().Which;
        exception.Error.Should().Be(UserErrors.EmailRequired);
    }

    [Fact]
    public void Create_WhenFullNameIsNull_ShouldThrowDomainException_WithUserErrorsFullNameRequired()
    {
        // Arrange

        var emailAddressTest = "example@domain.com";
        var emailResult = Email.Create(emailAddressTest);
        var email = emailResult.Value;

        FullName fullNameInvalid = null!;

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

        // Action

        Action act = () => User.Create(
            email,
            fullNameInvalid,
            birthDate,
            gender,
            telephoneNumber);

        // Assert

        var exception = act.Should().Throw<DomainException>().Which;
        exception.Error.Should().Be(UserErrors.FullNameRequired);
    }

    [Fact]
    public void Create_WhenBirthDateIsNull_ShouldThrowDomainException_WithUserErrorsBirthDateRequired()
    {
        // Arrange

        var emailAddressTest = "example@domain.com";
        var emailResult = Email.Create(emailAddressTest);
        var email = emailResult.Value;

        var fullNameTest = "Full Name Test";
        var fullNameResult = FullName.Create(fullNameTest);
        var fullName = fullNameResult.Value;

        var gender = Gender.Male;

        BirthDate birthDateInvalid = null!;

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

        // Action

        Action act = () => User.Create(
            email,
            fullName,
            birthDateInvalid,
            gender,
            telephoneNumber);

        // Assert

        var exception = act.Should().Throw<DomainException>().Which;
        exception.Error.Should().Be(UserErrors.BirthDateRequired);
    }

    [Fact]
    public void Create_WhenGenderIsUnknown_ShouldThrowDomainException_WithUserErrorsGenderRequired()
    {
        // Arrange

        var emailAddressTest = "example@domain.com";
        var emailResult = Email.Create(emailAddressTest);
        var email = emailResult.Value;

        var fullNameTest = "Full Name Test";
        var fullNameResult = FullName.Create(fullNameTest);
        var fullName = fullNameResult.Value;

        var genderInvalid = Gender.Unknown;

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

        // Action

        Action act = () => User.Create(
            email,
            fullName,
            birthDate,
            genderInvalid,
            telephoneNumber);

        // Assert

        var exception = act.Should().Throw<DomainException>().Which;
        exception.Error.Should().Be(UserErrors.GenderRequired);
    }

    [Fact]
    public void Create_WhenGenderIsNotDefined_ShouldThrowDomainException_WithUserErrorsGenderInvalid()
    {
        // Arrange

        var emailAddressTest = "example@domain.com";
        var emailResult = Email.Create(emailAddressTest);
        var email = emailResult.Value;

        var fullNameTest = "Full Name Test";
        var fullNameResult = FullName.Create(fullNameTest);
        var fullName = fullNameResult.Value;

        var genderInvalid = (Gender)999;

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

        // Action

        Action act = () => User.Create(
            email,
            fullName,
            birthDate,
            genderInvalid,
            telephoneNumber);

        // Assert

        var exception = act.Should().Throw<DomainException>().Which;
        exception.Error.Should().Be(UserErrors.GenderInvalid);
    }

    [Fact]
    public void Create_WhenInitialPhoneIsNull_ShouldThrowDomainException_WithUserErrorsPhoneRequired()
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

        TelephoneNumber telephoneNumberInvalid = null!;

        // Action

        Action act = () => User.Create(
            email,
            fullName,
            birthDate,
            gender,
            telephoneNumberInvalid);

        // Assert

        var exception = act.Should().Throw<DomainException>().Which;
        exception.Error.Should().Be(UserErrors.PhoneRequired);
    }

    // IsSuccess

    [Fact]
    public void Create_WhenAllUserDataIsValid_ShouldSucceed()
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

        // Action

        var beforeCreation = DateTimeOffset.UtcNow;

        var resultUser = User.Create(
            email,
            fullName,
            birthDate,
            gender,
            telephoneNumber);

        var afterCreation = DateTimeOffset.UtcNow;

        // Assert

        resultUser.Should().NotBeNull();
        resultUser.Email.Should().Be(email);
        resultUser.FullName.Should().Be(fullName);
        resultUser.BirthDate.Should().Be(birthDate);
        resultUser.Gender.Should().Be(gender);

        var userPhone = resultUser.Phones
            .Should()
            .ContainSingle()
            .Which;

        userPhone.TelephoneNumber.Should().Be(telephoneNumber);
        userPhone.IsPrimary.Should().BeTrue();

        resultUser.Status.Should().Be(AccountStatus.PendingEmailConfirmation);
        resultUser.CreatedAt.Should().BeOnOrAfter(beforeCreation);
        resultUser.CreatedAt.Should().BeOnOrBefore(afterCreation);
        resultUser.EmailConfirmedAt.Should().BeNull();
        resultUser.FirstLoginAt.Should().BeNull();
        resultUser.LastLoginAt.Should().BeNull();
        resultUser.SuspensionInfo.Should().BeNull();

        var domainEvent = resultUser.DomainEvents.Should().ContainSingle().Which;
        domainEvent.Should().BeOfType<UserRegisteredDomainEvent>();

        var userRegisteredEvent = (UserRegisteredDomainEvent)domainEvent;
        userRegisteredEvent.UserId.Should().Be(resultUser.Id);
        userRegisteredEvent.Email.Should().Be(resultUser.Email.EmailAddress);
        userRegisteredEvent.FullName.Should().Be(resultUser.FullName.Name);
    }
}

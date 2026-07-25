using FluentAssertions;
using YaeaY.Account.Domain.Abstraction.Exceptions;
using YaeaY.Account.Domain.Entities.AggregateRoots.Users;
using YaeaY.Account.Domain.Entities.UserPhones;
using YaeaY.Account.Domain.Enumerators;
using YaeaY.Account.Domain.Errors.Users;
using YaeaY.Account.Domain.Events.Users;
using YaeaY.Account.Domain.ValueObjects.Dates;
using YaeaY.Account.Domain.ValueObjects.Emails;
using YaeaY.Account.Domain.ValueObjects.Names;
using YaeaY.Account.Domain.ValueObjects.Securities;

namespace YaeaY.Account.Domain.UnitTests.Entities.AggregateRoots.UsersTests;

public class UserCreateTests
{
    // IsFailure

    [Fact]
    public void Create_WhenEmailIsNull_ShouldThrowDomainException_WithUserErrorsEmailRequired()
    {
        // Arrange

        Email emailInvalid = null!;

        var passwordHashTest = "hashed_password_test";
        var passwordHashResult = PasswordHash.Create(passwordHashTest);
        var passwordHash = passwordHashResult.Value;

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
        var phoneType = PhoneType.Mobile;
        var phoneNumber = "12345678";
        var e164 = "+554812345678";
        var isPrimary = true;

        var initialPhone = UserPhone.Create(
            callingCode,
            regionCode,
            areaCode,
            phoneType,
            phoneNumber,
            e164,
            isPrimary);

        var phone = initialPhone;

        // Act

        Action act = () => User.Create(
            emailInvalid,
            passwordHash,
            fullName,
            birthDate,
            gender,
            phone);

        // Assert

        var exception = act.Should().Throw<DomainException>().Which;
        exception.Error.Should().Be(UserErrors.EmailRequired);
    }

    [Fact]
    public void Create_WhenPasswordHashIsNull_ShouldThrowDomainException_WithUserErrorsPasswordRequired()
    {
        // Arrange

        var emailAddressTest = "example@domain.com";
        var emailResult = Email.Create(emailAddressTest);
        var email = emailResult.Value;

        PasswordHash passwordHashInvalid = null!;

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
        var phoneType = PhoneType.Mobile;
        var phoneNumber = "12345678";
        var e164 = "+554812345678";
        var isPrimary = true;

        var initialPhone = UserPhone.Create(
            callingCode,
            regionCode,
            areaCode,
            phoneType,
            phoneNumber,
            e164,
            isPrimary);

        var phone = initialPhone;

        // Act

        Action act = () => User.Create(
            email,
            passwordHashInvalid,
            fullName,
            birthDate,
            gender,
            phone);

        // Assert

        var exception = act.Should().Throw<DomainException>().Which;
        exception.Error.Should().Be(UserErrors.PasswordRequired);
    }

    [Fact]
    public void Create_WhenFullNameIsNull_ShouldThrowDomainException_WithUserErrorsFullNameRequired()
    {
        // Arrange

        var emailAddressTest = "example@domain.com";
        var emailResult = Email.Create(emailAddressTest);
        var email = emailResult.Value;

        var passwordHashTest = "hashed_password_test";
        var passwordHashResult = PasswordHash.Create(passwordHashTest);
        var passwordHash = passwordHashResult.Value;

        FullName fullNameInvalid = null!;

        var gender = Gender.Male;

        var birthDateTest = new DateOnly(2026, 1, 1);
        var birthDateResult = BirthDate.Create(birthDateTest);
        var birthDate = birthDateResult.Value;

        var callingCode = "+55";
        var regionCode = "BR";
        var areaCode = "48";
        var phoneType = PhoneType.Mobile;
        var phoneNumber = "12345678";
        var e164 = "+554812345678";
        var isPrimary = true;

        var initialPhone = UserPhone.Create(
            callingCode,
            regionCode,
            areaCode,
            phoneType,
            phoneNumber,
            e164,
            isPrimary);

        var phone = initialPhone;

        // Act

        Action act = () => User.Create(
            email,
            passwordHash,
            fullNameInvalid,
            birthDate,
            gender,
            phone);

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

        var passwordHashTest = "hashed_password_test";
        var passwordHashResult = PasswordHash.Create(passwordHashTest);
        var passwordHash = passwordHashResult.Value;

        var fullNameTest = "Full Name Test";
        var fullNameResult = FullName.Create(fullNameTest);
        var fullName = fullNameResult.Value;

        var gender = Gender.Male;

        BirthDate birthDateInvalid = null!;

        var callingCode = "+55";
        var regionCode = "BR";
        var areaCode = "48";
        var phoneType = PhoneType.Mobile;
        var phoneNumber = "12345678";
        var e164 = "+554812345678";
        var isPrimary = true;

        var initialPhone = UserPhone.Create(
            callingCode,
            regionCode,
            areaCode,
            phoneType,
            phoneNumber,
            e164,
            isPrimary);

        var phone = initialPhone;

        // Act

        Action act = () => User.Create(
            email,
            passwordHash,
            fullName,
            birthDateInvalid,
            gender,
            phone);

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

        var passwordHashTest = "hashed_password_test";
        var passwordHashResult = PasswordHash.Create(passwordHashTest);
        var passwordHash = passwordHashResult.Value;

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
        var phoneType = PhoneType.Mobile;
        var phoneNumber = "12345678";
        var e164 = "+554812345678";
        var isPrimary = true;

        var initialPhone = UserPhone.Create(
            callingCode,
            regionCode,
            areaCode,
            phoneType,
            phoneNumber,
            e164,
            isPrimary);

        var phone = initialPhone;

        // Act

        Action act = () => User.Create(
            email,
            passwordHash,
            fullName,
            birthDate,
            genderInvalid,
            phone);

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

        var passwordHashTest = "hashed_password_test";
        var passwordHashResult = PasswordHash.Create(passwordHashTest);
        var passwordHash = passwordHashResult.Value;

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
        var phoneType = PhoneType.Mobile;
        var phoneNumber = "12345678";
        var e164 = "+554812345678";
        var isPrimary = true;

        var initialPhone = UserPhone.Create(
            callingCode,
            regionCode,
            areaCode,
            phoneType,
            phoneNumber,
            e164,
            isPrimary);

        var phone = initialPhone;

        // Act

        Action act = () => User.Create(
            email,
            passwordHash,
            fullName,
            birthDate,
            genderInvalid,
            phone);

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

        var passwordHashTest = "hashed_password_test";
        var passwordHashResult = PasswordHash.Create(passwordHashTest);
        var passwordHash = passwordHashResult.Value;

        var fullNameTest = "Full Name Test";
        var fullNameResult = FullName.Create(fullNameTest);
        var fullName = fullNameResult.Value;

        var gender= Gender.Male;

        var birthDateTest = new DateOnly(2026, 1, 1);
        var birthDateResult = BirthDate.Create(birthDateTest);
        var birthDate = birthDateResult.Value;

        UserPhone userPhoneInvalid = null!;

        // Act

        Action act = () => User.Create(
            email,
            passwordHash,
            fullName,
            birthDate,
            gender,
            userPhoneInvalid);

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

        var passwordHashTest = "hashed_password_test";
        var passwordHashResult = PasswordHash.Create(passwordHashTest);
        var passwordHash = passwordHashResult.Value;

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
        var phoneType = PhoneType.Mobile;
        var phoneNumber = "12345678";
        var e164 = "+554812345678";
        var isPrimary = false;

        var initialPhone = UserPhone.Create(
            callingCode,
            regionCode,
            areaCode,
            phoneType,
            phoneNumber,
            e164,
            isPrimary);

        var phone = initialPhone;

        // Act

        var beforeCreation = DateTimeOffset.UtcNow;

        var resultUser = User.Create(
            email,
            passwordHash,
            fullName,
            birthDate,
            gender,
            phone);

        var afterCreation = DateTimeOffset.UtcNow;

        // Assert

        resultUser.Should().NotBeNull();
        resultUser.Email.Should().Be(email);
        resultUser.PasswordHash.Should().Be(passwordHash);
        resultUser.FullName.Should().Be(fullName);
        resultUser.BirthDate.Should().Be(birthDate);
        resultUser.Gender.Should().Be(gender);

        resultUser.Phones.Should().HaveCount(1);
        resultUser.Phones.First().Should().Be(phone);
        resultUser.Phones.First().IsPrimary.Should().BeTrue();

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

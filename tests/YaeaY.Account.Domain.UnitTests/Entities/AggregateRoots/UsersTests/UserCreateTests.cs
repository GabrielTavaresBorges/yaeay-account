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
    public void Create_WhenEmailIsNull_ShouldThrowDomainException()
    {
        // Arrange

        Email emailInvalid = null!;

        var passwordHashTest = "hashed_password_test";
        var passwordHashResult = PasswordHash.Create(passwordHashTest);
        var passwordHash = passwordHashResult.Value;

        var userNameTest = "User Name Test";
        var userNameResult = UserName.Create(userNameTest);
        var userName = userNameResult.Value;

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
            userName,
            birthDate,
            gender,
            phone);

        // Assert

        var exception = act.Should().Throw<DomainException>().Which;
        exception.Code.Should().Be(UserErrors.EmailRequired.Code);
        exception.Message.Should().Be(UserErrors.EmailRequired.Message);
    }

    [Fact]
    public void Create_WhenPasswordHashIsNull_ShouldThrowDomainException()
    {
        // Arrange

        var emailAddressTest = "example@domain.com";
        var emailResult = Email.Create(emailAddressTest);
        var email = emailResult.Value;

        PasswordHash passwordHashInvalid = null!;

        var userNameTest = "User Name Test";
        var userNameResult = UserName.Create(userNameTest);
        var userName = userNameResult.Value;

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
            userName,
            birthDate,
            gender,
            phone);

        // Assert

        var exception = act.Should().Throw<DomainException>().Which;
        exception.Code.Should().Be(UserErrors.PasswordRequired.Code);
        exception.Message.Should().Be(UserErrors.PasswordRequired.Message);
    }

    [Fact]
    public void Create_WhenUserNameIsNull_ShouldThrowDomainException()
    {
        // Arrange

        var emailAddressTest = "example@domain.com";
        var emailResult = Email.Create(emailAddressTest);
        var email = emailResult.Value;

        var passwordHashTest = "hashed_password_test";
        var passwordHashResult = PasswordHash.Create(passwordHashTest);
        var passwordHash = passwordHashResult.Value;

        UserName userNameInvalid = null!;

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
            userNameInvalid,
            birthDate,
            gender,
            phone);

        // Assert

        var exception = act.Should().Throw<DomainException>().Which;
        exception.Code.Should().Be(UserErrors.NameRequired.Code);
        exception.Message.Should().Be(UserErrors.NameRequired.Message);
    }

    [Fact]
    public void Create_WhenBirthDateIsNull_ShouldThrowDomainException()
    {
        // Arrange

        var emailAddressTest = "example@domain.com";
        var emailResult = Email.Create(emailAddressTest);
        var email = emailResult.Value;

        var passwordHashTest = "hashed_password_test";
        var passwordHashResult = PasswordHash.Create(passwordHashTest);
        var passwordHash = passwordHashResult.Value;

        var userNameTest = "User Name Test";
        var userNameResult = UserName.Create(userNameTest);
        var userName = userNameResult.Value;

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
            userName,
            birthDateInvalid,
            gender,
            phone);

        // Assert

        var exception = act.Should().Throw<DomainException>().Which;
        exception.Code.Should().Be(UserErrors.BirthDateRequired.Code);
        exception.Message.Should().Be(UserErrors.BirthDateRequired.Message);
    }

    [Fact]
    public void Create_WhenGenderIsInvalid_ShouldThrowDomainException()
    {
        // Arrange

        var emailAddressTest = "example@domain.com";
        var emailResult = Email.Create(emailAddressTest);
        var email = emailResult.Value;

        var passwordHashTest = "hashed_password_test";
        var passwordHashResult = PasswordHash.Create(passwordHashTest);
        var passwordHash = passwordHashResult.Value;

        var userNameTest = "User Name Test";
        var userNameResult = UserName.Create(userNameTest);
        var userName = userNameResult.Value;

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
            userName,
            birthDate,
            genderInvalid,
            phone);

        // Assert

        var exception = act.Should().Throw<DomainException>().Which;
        exception.Code.Should().Be(UserErrors.GenderRequired.Code);
        exception.Message.Should().Be(UserErrors.GenderRequired.Message);
    }

    [Fact]
    public void Create_WhenGenderIsNotDefined_ShouldThrowDomainException()
    {
        // Arrange

        var emailAddressTest = "example@domain.com";
        var emailResult = Email.Create(emailAddressTest);
        var email = emailResult.Value;

        var passwordHashTest = "hashed_password_test";
        var passwordHashResult = PasswordHash.Create(passwordHashTest);
        var passwordHash = passwordHashResult.Value;

        var userNameTest = "User Name Test";
        var userNameResult = UserName.Create(userNameTest);
        var userName = userNameResult.Value;

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
            userName,
            birthDate,
            genderInvalid,
            phone);

        // Assert

        var exception = act.Should().Throw<DomainException>().Which;
        exception.Code.Should().Be(UserErrors.GenderInvalid.Code);
        exception.Message.Should().Be(UserErrors.GenderInvalid.Message);
    }

    [Fact]
    public void Create_WhenInitialPhoneIsNull_ShouldThrowDomainException()
    {
        // Arrange

        var emailAddressTest = "example@domain.com";
        var emailResult = Email.Create(emailAddressTest);
        var email = emailResult.Value;

        var passwordHashTest = "hashed_password_test";
        var passwordHashResult = PasswordHash.Create(passwordHashTest);
        var passwordHash = passwordHashResult.Value;

        var userNameTest = "User Name Test";
        var userNameResult = UserName.Create(userNameTest);
        var userName = userNameResult.Value;

        var gender= Gender.Male;

        var birthDateTest = new DateOnly(2026, 1, 1);
        var birthDateResult = BirthDate.Create(birthDateTest);
        var birthDate = birthDateResult.Value;

        UserPhone userPhoneInvalid = null!;

        // Act

        Action act = () => User.Create(
            email,
            passwordHash,
            userName,
            birthDate,
            gender,
            userPhoneInvalid);

        // Assert

        var exception = act.Should().Throw<DomainException>().Which;
        exception.Code.Should().Be(UserErrors.PhoneRequired.Code);
        exception.Message.Should().Be(UserErrors.PhoneRequired.Message);
    }

    // IsSuccess

    [Fact]
    public void Create_WhenAllUserDataIsValid_ShouldSuccess()
    {
        // Arrange

        var emailAddressTest = "example@domain.com";
        var emailResult = Email.Create(emailAddressTest);
        var email = emailResult.Value;

        var passwordHashTest = "hashed_password_test";
        var passwordHashResult = PasswordHash.Create(passwordHashTest);
        var passwordHash = passwordHashResult.Value;

        var userNameTest = "User Name Test";
        var userNameResult = UserName.Create(userNameTest);
        var userName = userNameResult.Value;

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
            userName,
            birthDate,
            gender,
            phone);

        var afterCreation = DateTimeOffset.UtcNow;

        // Assert

        resultUser.Should().NotBeNull();
        resultUser.Email.Should().Be(email);
        resultUser.PasswordHash.Should().Be(passwordHash);
        resultUser.UserName.Should().Be(userName);
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
        userRegisteredEvent.UserName.Should().Be(resultUser.UserName.Name);
    }
}

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

public class UserPhoneUpdateTests
{
    [Fact]
    public void AddPhone_WhenNumberIsValid_ShouldReturnAddedPhone()
    {
        // Arrange

        var user = CreateUser();
        var phoneNumber = CreateTelephoneNumber("87654321", "+554887654321");

        // Act

        var addedPhone = user.AddPhone(phoneNumber);

        // Assert

        user.Phones.Should().Contain(addedPhone);
        addedPhone.TelephoneNumber.Should().Be(phoneNumber);
        addedPhone.IsPrimary.Should().BeFalse();
    }

    [Fact]
    public void ChangePhone_WhenNumberIsDifferent_ShouldUpdatePhone()
    {
        // Arrange

        var user = CreateUser();
        var phone = user.Phones.Single();
        var updatedNumber = CreateTelephoneNumber("87654321", "+554887654321");

        // Act

        var changed = user.ChangePhone(phone.Id, updatedNumber);

        // Assert

        changed.Should().BeTrue();
        phone.TelephoneNumber.Should().Be(updatedNumber);
    }

    [Fact]
    public void ChangePhone_WhenNumberBelongsToAnotherPhone_ShouldThrowDomainException()
    {
        // Arrange

        var user = CreateUser();
        var existingPhone = user.Phones.Single();
        var addedPhone = user.AddPhone(CreateTelephoneNumber("87654321", "+554887654321"));

        // Act

        Action act = () => user.ChangePhone(addedPhone.Id, existingPhone.TelephoneNumber);

        // Assert

        var exception = act.Should().Throw<DomainException>().Which;
        exception.Error.Should().Be(UserErrors.PhoneAlreadyExists);
        addedPhone.TelephoneNumber.Should().NotBe(existingPhone.TelephoneNumber);
    }

    [Fact]
    public void SetPrimaryPhone_WhenAnotherPhoneIsSelected_ShouldSwitchPrimaryPhone()
    {
        // Arrange

        var user = CreateUser();
        var initialPhone = user.Phones.Single();
        var addedPhone = user.AddPhone(CreateTelephoneNumber("87654321", "+554887654321"));

        // Act

        var changed = user.SetPrimaryPhone(addedPhone.Id);

        // Assert

        changed.Should().BeTrue();
        addedPhone.IsPrimary.Should().BeTrue();
        initialPhone.IsPrimary.Should().BeFalse();
    }

    [Fact]
    public void RemovePhone_WhenPhoneIsSecondary_ShouldRemovePhone()
    {
        // Arrange

        var user = CreateUser();
        var addedPhone = user.AddPhone(CreateTelephoneNumber("87654321", "+554887654321"));

        // Act

        user.RemovePhone(addedPhone.Id);

        // Assert

        user.Phones.Should().ContainSingle();
        user.Phones.Should().NotContain(addedPhone);
        user.Phones.Single().IsPrimary.Should().BeTrue();
    }

    [Fact]
    public void RemovePhone_WhenItIsTheOnlyPhone_ShouldThrowDomainException()
    {
        // Arrange

        var user = CreateUser();
        var onlyPhone = user.Phones.Single();

        // Act

        Action act = () => user.RemovePhone(onlyPhone.Id);

        // Assert

        var exception = act.Should().Throw<DomainException>().Which;
        exception.Error.Should().Be(UserErrors.AtLeastOnePhoneRequired);
    }

    [Fact]
    public void RemovePhone_WhenItIsPrimary_ShouldThrowDomainException()
    {
        // Arrange

        var user = CreateUser();
        user.AddPhone(CreateTelephoneNumber("87654321", "+554887654321"));
        var primaryPhone = user.Phones.Single(phone => phone.IsPrimary);

        // Act

        Action act = () => user.RemovePhone(primaryPhone.Id);

        // Assert

        var exception = act.Should().Throw<DomainException>().Which;
        exception.Error.Should().Be(UserErrors.PrimaryPhoneCannotBeRemoved);
    }

    private static User CreateUser()
    {
        var email = Email.Create("user@example.com").Value;
        var fullName = FullName.Create("Example User").Value;
        var birthDate = BirthDate.Create(new DateOnly(1990, 1, 1)).Value;
        var initialPhoneNumber = CreateTelephoneNumber("12345678", "+554812345678");

        return User.Create(email, fullName, birthDate, Gender.Male, initialPhoneNumber);
    }

    private static TelephoneNumber CreateTelephoneNumber(string nationalNumber, string e164) =>
        TelephoneNumber.Create("+55", "BR", "48", TelephoneType.Mobile, nationalNumber, e164).Value;
}

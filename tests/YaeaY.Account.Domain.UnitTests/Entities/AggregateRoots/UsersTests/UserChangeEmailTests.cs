using FluentAssertions;
using YaeaY.Account.Domain.Entities.AggregateRoots.Users;
using YaeaY.Account.Domain.Enumerators;
using YaeaY.Account.Domain.Events.Users;
using YaeaY.Account.Domain.ValueObjects.Dates;
using YaeaY.Account.Domain.ValueObjects.Emails;
using YaeaY.Account.Domain.ValueObjects.Names;
using YaeaY.Account.Domain.ValueObjects.Telephones;

namespace YaeaY.Account.Domain.UnitTests.Entities.AggregateRoots.UsersTests;

public sealed class UserChangeEmailTests
{
    [Fact]
    public void ChangeEmail_WhenEmailChanges_ShouldRaiseProfileChangedEvent()
    {
        var user = CreateUser();
        user.ClearDomainEvents();
        var newEmail = Email.Create("new-email@yaeay.com").Value;

        user.ChangeEmail(newEmail);

        user.Email.Should().Be(newEmail);
        user.DomainEvents.Should().ContainSingle()
            .Which.Should().BeOfType<UserProfileChangedDomainEvent>()
            .Which.UserId.Should().Be(user.Id);
    }

    private static User CreateUser()
    {
        return User.Create(
            Email.Create("original@yaeay.com").Value,
            FullName.Create("YaeaY Account").Value,
            BirthDate.Create(new DateOnly(1990, 1, 1)).Value,
            Gender.Male,
            TelephoneNumber.Create("+55", "BR", "48", TelephoneType.Mobile, "999999999", "+5548999999999").Value);
    }
}

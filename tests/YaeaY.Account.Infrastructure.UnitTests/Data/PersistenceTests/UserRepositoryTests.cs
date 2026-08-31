using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using YaeaY.Account.Domain.Entities.AggregateRoots.Users;
using YaeaY.Account.Domain.Enumerators;
using YaeaY.Account.Domain.ValueObjects.Dates;
using YaeaY.Account.Domain.ValueObjects.Emails;
using YaeaY.Account.Domain.ValueObjects.Names;
using YaeaY.Account.Domain.ValueObjects.Telephones;
using YaeaY.Account.Infrastructure.Data.Context;
using YaeaY.Account.Infrastructure.Data.Repositories.Users;

namespace YaeaY.Account.Infrastructure.UnitTests.Data.PersistenceTests;

public sealed class UserRepositoryTests
{
    [Fact]
    public async Task UpdateUserAsync_WhenTrackedUserReceivesNewPhone_ShouldTrackPhoneAsAdded()
    {
        // Arrange

        using var context = CreateContext();
        var user = CreateUser();
        context.Attach(user);
        var addedPhone = user.AddPhone(CreateTelephoneNumber("987654321"));
        var repository = new UserRepository(context);

        // Act

        await repository.UpdateUserAsync(user, [addedPhone], CancellationToken.None);

        // Assert

        context.Entry(addedPhone).State.Should().Be(EntityState.Added);
    }

    private static AppDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseNpgsql("Host=localhost;Database=unused;Username=unused;Password=unused")
            .Options;

        return new AppDbContext(options);
    }

    private static User CreateUser() => User.Create(
        Email.Create("test.user@example.com").Value,
        FullName.Create("Test User").Value,
        BirthDate.Create(new DateOnly(1990, 1, 1)).Value,
        Gender.Male,
        CreateTelephoneNumber("912345678"));

    private static TelephoneNumber CreateTelephoneNumber(string nationalNumber) =>
        TelephoneNumber.Create(
            callingCode: "+55",
            regionCode: "BR",
            areaCode: "11",
            phoneType: TelephoneType.Mobile,
            nationalNumber: nationalNumber,
            e164: $"+5511{nationalNumber}").Value;
}

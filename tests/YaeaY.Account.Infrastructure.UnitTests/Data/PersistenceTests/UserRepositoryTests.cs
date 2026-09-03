using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using YaeaY.Account.Domain.Entities.AggregateRoots.Users;
using YaeaY.Account.Domain.Entities.UserDocuments;
using YaeaY.Account.Domain.Enumerators;
using YaeaY.Account.Domain.ValueObjects.Dates;
using YaeaY.Account.Domain.ValueObjects.Documents;
using YaeaY.Account.Domain.ValueObjects.Emails;
using YaeaY.Account.Domain.ValueObjects.Names;
using YaeaY.Account.Domain.ValueObjects.Telephones;
using YaeaY.Account.Infrastructure.Data.Context;
using YaeaY.Account.Infrastructure.Data.Repositories.Users;

namespace YaeaY.Account.Infrastructure.UnitTests.Data.PersistenceTests;

public sealed class UserRepositoryTests
{
    [Fact]
    public async Task UpdateUserPhonesAsync_WhenTrackedUserReceivesNewPhone_ShouldTrackPhoneAsAdded()
    {
        // Arrange

        using var context = CreateContext();
        var user = CreateUser();
        context.Attach(user);
        var addedPhone = user.AddPhone(CreateTelephoneNumber("987654321"));
        var repository = new UserRepository(context);

        // Act

        await repository.UpdateUserPhonesAsync(user, [addedPhone], CancellationToken.None);

        // Assert

        context.Entry(addedPhone).State.Should().Be(EntityState.Added);
    }

    [Fact]
    public async Task UpdateUserDocumentsAsync_WhenTrackedUserReceivesNewRgDocument_ShouldTrackDocumentGraphAsAdded()
    {
        // Arrange

        using var context = CreateContext();
        var user = CreateUser();
        context.Attach(user);
        var rg = Rg.Create("123456789", new DateOnly(2020, 1, 15), "SSP", "SP").Value;
        var document = user.UpsertRgDocument(rg, [], out var changed);
        var repository = new UserRepository(context);

        // Act

        await repository.UpdateUserDocumentsAsync(user, [document], [], CancellationToken.None);

        // Assert

        changed.Should().BeTrue();
        context.Entry(document).State.Should().Be(EntityState.Added);
        context.Entry(document.Rg!).State.Should().Be(EntityState.Added);
    }

    [Fact]
    public async Task UpdateUserDocumentsAsync_WhenTrackedRgReplacesImages_ShouldDeleteOldImagesAndAddNewImages()
    {
        // Arrange

        using var context = CreateContext();
        var user = CreateUser();
        var initialRg = Rg.Create("123456789", new DateOnly(2020, 1, 15), "SSP", "SP").Value;
        var initialImage = CreateImage(1, "initial");
        var document = user.UpsertRgDocument(initialRg, [initialImage], out _);
        context.Attach(user);
        var replacementImage = CreateImage(1, "replacement");
        var updatedRg = Rg.Create("987654321", new DateOnly(2021, 2, 16), "SSP", "SP").Value;
        user.UpsertRgDocument(updatedRg, [replacementImage], out var changed);
        var repository = new UserRepository(context);

        // Act

        await repository.UpdateUserDocumentsAsync(user, [], [replacementImage], CancellationToken.None);

        // Assert

        changed.Should().BeTrue();
        context.Entry(initialImage).State.Should().Be(EntityState.Deleted);
        context.Entry(replacementImage).State.Should().Be(EntityState.Added);
    }

    [Fact]
    public async Task UpdateUserDocumentsAsync_WhenPersistedRgReplacesImages_ShouldPersistReplacementWithoutConcurrencyFailure()
    {
        // Arrange

        using var context = CreateContext();
        var user = CreateUser();
        var initialRg = Rg.Create("123456789", new DateOnly(2020, 1, 15), "SSP", "SP").Value;
        user.UpsertRgDocument(initialRg, [CreateImage(1, "initial-persisted")], out _);
        context.Add(user);
        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();
        var persistedUser = await context.DomainUsers
            .Include(current => current.Documents)
                .ThenInclude(document => document.Rg)
            .Include(current => current.Documents)
                .ThenInclude(document => document.Images)
            .SingleAsync();
        var replacementImage = CreateImage(1, "replacement-persisted");
        var updatedRg = Rg.Create("987654321", new DateOnly(2021, 2, 16), "SSP", "SP").Value;
        persistedUser.UpsertRgDocument(updatedRg, [replacementImage], out var changed);
        var repository = new UserRepository(context);

        // Act

        await repository.UpdateUserDocumentsAsync(persistedUser, [], [replacementImage], CancellationToken.None);
        var save = () => context.SaveChangesAsync();

        // Assert

        changed.Should().BeTrue();
        await save.Should().NotThrowAsync();
        context.ChangeTracker.Clear();
        var storedDocument = await context.Set<UserDocument>().Include(document => document.Rg).Include(document => document.Images).SingleAsync();
        storedDocument.Rg!.Rg.Number.Should().Be("987654321");
        storedDocument.Images.Should().ContainSingle(image => image.StorageObjectKey.EndsWith("replacement-persisted.png"));
    }

    private static AppDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString("N"))
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

    private static UserDocumentImage CreateImage(short position, string name) => UserDocumentImage.Create(
        position,
        $"users/test/{name}.png",
        $"{name}.png",
        "image/png",
        128,
        new string('a', 64));
}

using FluentAssertions;
using YaeaY.Account.Domain.Entities.AggregateRoots.Users;
using YaeaY.Account.Domain.Entities.UserDocuments;
using YaeaY.Account.Domain.Enumerators;
using YaeaY.Account.Domain.Events.Users;
using YaeaY.Account.Domain.ValueObjects.Dates;
using YaeaY.Account.Domain.ValueObjects.Documents;
using YaeaY.Account.Domain.ValueObjects.Emails;
using YaeaY.Account.Domain.ValueObjects.Names;
using YaeaY.Account.Domain.ValueObjects.Telephones;

namespace YaeaY.Account.Domain.UnitTests.Entities.AggregateRoots.UsersTests;

public sealed class UserUpsertCpfDocumentTests
{
    [Fact]
    public void UpsertCpfDocument_WhenCpfAlreadyExists_ShouldUpdateCurrentDocumentWithoutCreatingHistory()
    {
        // Arrange

        var user = CreateUser();
        var originalCpf = Cpf.Create("529.982.247-25").Value;
        var originalDocument = user.UpsertCpfDocument(originalCpf, [CreateImage(1, "original")], out _);
        user.ClearDomainEvents();
        var replacementCpf = Cpf.Create("111.444.777-35").Value;

        // Act

        var updatedDocument = user.UpsertCpfDocument(replacementCpf, [CreateImage(1, "replacement")], out var changed);

        // Assert

        changed.Should().BeTrue();
        updatedDocument.Id.Should().Be(originalDocument.Id);
        user.Documents.Should().ContainSingle();
        updatedDocument.Cpf!.Cpf.Number.Should().Be("11144477735");
        updatedDocument.Images.Should().ContainSingle()
            .Which.StorageObjectKey.Should().Be("users/test/cpf/replacement.png");
        user.DomainEvents.Should().ContainSingle()
            .Which.Should().BeOfType<UserProfileChangedDomainEvent>()
            .Which.UserId.Should().Be(user.Id);
    }

    private static UserDocumentImage CreateImage(short position, string name) => UserDocumentImage.Create(
        position,
        $"users/test/cpf/{name}.png",
        $"{name}.png",
        "image/png",
        1024,
        new string('a', 64));

    private static User CreateUser() => User.Create(
        Email.Create("cpf-document@yaeay.test").Value,
        FullName.Create("YaeaY Account").Value,
        BirthDate.Create(new DateOnly(1990, 1, 1)).Value,
        Gender.Male,
        TelephoneNumber.Create("+55", "BR", "48", TelephoneType.Mobile, "999999999", "+5548999999999").Value);
}

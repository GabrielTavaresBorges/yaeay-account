using FluentAssertions;
using YaeaY.Account.Domain.Entities.AggregateRoots.Users;
using YaeaY.Account.Domain.Enumerators;
using YaeaY.Account.Domain.Errors.Users;
using YaeaY.Account.Domain.Repositories.Users;
using YaeaY.Account.Domain.ValueObjects.Dates;
using YaeaY.Account.Domain.ValueObjects.Emails;
using YaeaY.Account.Domain.ValueObjects.Names;
using YaeaY.Account.Domain.ValueObjects.Telephones;
using GetCurrentSession = YaeaY.Account.Application.UseCases.Authentication.Queries.GetCurrentSession;

namespace YaeaY.Account.Application.UnitTests.Authentication;

public sealed class GetCurrentSessionHandlerTests
{
    [Fact]
    public async Task Handle_WhenUserExists_ShouldReturnCurrentSession()
    {
        // Arrange

        var email = Email.Create("current-session@example.com").Value;
        var fullName = FullName.Create("Current Session Person").Value;
        var birthDate = BirthDate.Create(new DateOnly(1990, 8, 16)).Value;
        var telephoneNumber = TelephoneNumber.Create(
            "+55",
            "BR",
            "48",
            TelephoneType.Mobile,
            "999999999",
            "+5548999999999").Value;
        var user = User.Create(
            email,
            fullName,
            birthDate,
            Gender.Male,
            telephoneNumber);
        var confirmedAt = user.CreatedAt.AddMinutes(1);
        var loggedInAt = confirmedAt.AddMinutes(1);
        user.ConfirmEmail(confirmedAt);
        user.RegisterSuccessfulLogin(loggedInAt);
        var repository = new StubUserRepository(user);
        var handler = new GetCurrentSession.Handler(repository);
        var query = new GetCurrentSession.Query(user.Id);

        // Act

        var result = await handler.Handle(query, CancellationToken.None);

        // Assert

        result.IsSuccess.Should().BeTrue();
        result.Value.UserId.Should().Be(user.Id);
        result.Value.FullName.Should().Be("Current Session Person");
        result.Value.LastLoginAt.Should().Be(loggedInAt);
    }

    [Fact]
    public async Task Handle_WhenUserDoesNotExist_ShouldFail_WithUserErrorsNotFound()
    {
        // Arrange

        var userId = Guid.NewGuid();
        var repository = new StubUserRepository(null);
        var handler = new GetCurrentSession.Handler(repository);
        var query = new GetCurrentSession.Query(userId);

        // Act

        var result = await handler.Handle(query, CancellationToken.None);

        // Assert

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(UserErrors.NotFound);
    }

    private sealed class StubUserRepository(User? user) : IUserRepository
    {
        public Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken) =>
            Task.FromResult(user?.Id == id ? user : null);

        public Task<User?> GetByEmailAsync(Email email, CancellationToken cancellationToken) =>
            throw new NotSupportedException();

        public Task<bool> ExistsByEmailAsync(Email email, CancellationToken cancellationToken) =>
            throw new NotSupportedException();

        public Task CreateUserAsync(User user, CancellationToken cancellationToken) =>
            throw new NotSupportedException();

        public Task UpdateUserAsync(User user, CancellationToken cancellationToken) =>
            throw new NotSupportedException();
    }
}

using FluentAssertions;
using YaeaY.Account.Application.Services.ReadModels.Interfaces;
using GetMyData = YaeaY.Account.Application.UseCases.Users.Queries.GetMyData;

namespace YaeaY.Account.Application.UnitTests.UseCases.Users.Queries;

public sealed class GetMyDataHandlerTests
{
    [Fact]
    public async Task Handle_WhenProjectionExists_ShouldReturnReadModel()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var expected = new GetMyData.Response(
            userId, "user@yaeay.com", "User Test", new DateOnly(2000, 1, 1), "Male", "Active",
            DateTimeOffset.UtcNow, null, null, null, [], [], DateTimeOffset.UtcNow);
        var handler = new GetMyData.Handler(new StubReader(expected));

        // Action
        var result = await handler.Handle(new GetMyData.Query(userId), CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(expected);
    }

    [Fact]
    public async Task Handle_WhenProjectionDoesNotExist_ShouldReturnNotFound()
    {
        // Arrange
        var handler = new GetMyData.Handler(new StubReader(null));

        // Action
        var result = await handler.Handle(new GetMyData.Query(Guid.NewGuid()), CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("user.not-found");
    }

    private sealed class StubReader(GetMyData.Response? response) : IMyDataReader
    {
        public Task<GetMyData.Response?> GetAsync(Guid userId, CancellationToken cancellationToken) => Task.FromResult(response);
    }
}

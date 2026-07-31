using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using YaeaY.Account.Infrastructure.Data.Context;

namespace YaeaY.Account.Infrastructure.UnitTests.Data.ContextTests;

public sealed class AppDbContextTests
{
    [Fact]
    public void Model_Should_Be_Built_Without_Errors()
    {
        // Arrange

        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseNpgsql(
                "Host=localhost;Database=unused;Username=unused;Password=unused")
            .Options;

        using var context = new AppDbContext(options);

        // Act

        var action = () => context.Model.GetRelationalModel();

        // Assert

        action.Should().NotThrow();
    }
}

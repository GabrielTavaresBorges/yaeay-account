using FluentAssertions;
using ConfirmEmail = YaeaY.Account.Application.UseCases.EmailConfirmations.Commands.ConfirmEmail;

namespace YaeaY.Account.Application.UnitTests.UseCases.EmailConfirmations.Commands;

public sealed class ConfirmEmailTests
{
    [Fact]
    public void ToString_WhenCommandContainsRawToken_ShouldNotRevealToken()
    {
        // Arrange

        const string rawToken = "raw-token-only-in-memory";
        var command = new ConfirmEmail.Command(rawToken);

        // Act

        var result = command.ToString();

        // Assert

        result.Should().Be(nameof(ConfirmEmail.Command));
        result.Should().NotContain(rawToken);
    }
}

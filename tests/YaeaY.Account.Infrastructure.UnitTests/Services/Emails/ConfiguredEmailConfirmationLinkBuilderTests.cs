using FluentAssertions;
using Microsoft.Extensions.Options;
using YaeaY.Account.Infrastructure.Services.Emails;

namespace YaeaY.Account.Infrastructure.UnitTests.Services.Emails;

public sealed class ConfiguredEmailConfirmationLinkBuilderTests
{
    [Fact]
    public void Build_WhenConfigurationAndTokenAreValid_ShouldUseUrlFragment()
    {
        // Arrange

        var builder = CreateBuilder(
            "https://account.example.com/confirm-email");

        // Act

        var result = builder.Build("token<secret>");

        // Assert

        result.Should().Be(
            "https://account.example.com/confirm-email#token=token%3Csecret%3E");
    }

    [Theory]
    [InlineData("http://account.example.com/confirm-email")]
    [InlineData("https://account.example.com/confirm-email?source=email")]
    [InlineData("https://account.example.com/confirm-email#existing")]
    [InlineData("not-a-url")]
    public void Constructor_WhenConfigurationIsUnsafe_ShouldThrow(
        string confirmationPageUrl)
    {
        // Act

        Action act = () => CreateBuilder(confirmationPageUrl);

        // Assert

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*absolute HTTPS URL without query or fragment*");
    }

    [Fact]
    public void Build_WhenTokenIsMissing_ShouldThrow()
    {
        // Arrange

        var builder = CreateBuilder(
            "https://account.example.com/confirm-email");

        // Act

        Action act = () => builder.Build(string.Empty);

        // Assert

        act.Should().Throw<ArgumentException>();
    }

    private static ConfiguredEmailConfirmationLinkBuilder CreateBuilder(
        string confirmationPageUrl)
        => new(
            Options.Create(
                new EmailConfirmationLinkOptions
                {
                    ConfirmationPageUrl = confirmationPageUrl
                }));
}

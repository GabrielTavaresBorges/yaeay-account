using FluentAssertions;
using YaeaY.Account.Application.Services.Emails;
using YaeaY.Account.Domain.ValueObjects.Emails;

namespace YaeaY.Account.Application.UnitTests.Services.Emails.EmailAddressMaskerTests;

public sealed class EmailAddressMaskerTests
{
    [Theory]
    [InlineData("longlocalidentifier@example.com", "lo******@example.com")]
    [InlineData("person@example.com", "pe******@example.com")]
    [InlineData("a@example.com", "a******@example.com")]
    public void Mask_WhenEmailIsValid_ShouldUseFixedLengthMask(
        string emailAddress,
        string expectedMaskedEmail)
    {
        // Arrange

        var email = Email.Create(emailAddress).Value;
        var masker = new EmailAddressMasker();

        // Act

        var result = masker.Mask(email);

        // Assert

        result.Should().Be(expectedMaskedEmail);
        result.Count(character => character == '*').Should().Be(6);
    }
}

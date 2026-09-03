using FluentAssertions;
using YaeaY.Account.Domain.ValueObjects.Documents;

namespace YaeaY.Account.Domain.UnitTests.ValueObjects.Documents.RgTests;

public sealed class RgCreateTests
{
    [Fact]
    public void Create_WhenIssuingStateIsInvalid_ShouldFail()
    {
        // Arrange
        var issuedAt = new DateOnly(2018, 4, 12);

        // Act
        var result = Rg.Create("12.345.678-9", issuedAt, "SSP", "XX");

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("rg.issuing-state.invalid");
    }

    [Fact]
    public void Create_WhenDetailsAreValid_ShouldNormalizeAndSucceed()
    {
        // Arrange
        var issuedAt = new DateOnly(2018, 4, 12);

        // Act
        var result = Rg.Create(" 12.345.678-9 ", issuedAt, " ssp ", " sc ");

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Number.Should().Be("12.345.678-9");
        result.Value.IssuingAuthority.Should().Be("ssp");
        result.Value.IssuingState.Should().Be("SC");
        result.Value.IssuedAt.Should().Be(issuedAt);
    }
}

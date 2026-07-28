using FluentAssertions;
using YaeaY.Account.Domain.ValueObjects.Documents;

namespace YaeaY.Account.Domain.UnitTests.ValueObjects.Documents.CpfTests;

public class CpfCreateTests
{
    // IsFailure

    [Fact]
    public void Create_WhenCpfIsNull_ShouldFailure()
    {
        // Arrange

        string cfpNumberInvalid = null!;

        // Act

        var result = Cpf.Create(cfpNumberInvalid);

        // Assert              

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("CPF_NUMBER_NULL_EMPTY_WHITE_SPACE");
        result.Error.Message.Should().Be("CPF number cannot be null, empty or white space.");
    }

    [Fact]
    public void Create_WhenCpfIsEmpty_ShouldFailure()
    {
        // Arrange

        string cfpNumberInvalid = string.Empty;

        // Act

        var result = Cpf.Create(cfpNumberInvalid);

        // Assert              

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("CPF_NUMBER_NULL_EMPTY_WHITE_SPACE");
        result.Error.Message.Should().Be("CPF number cannot be null, empty or white space.");
    }

    [Fact]
    public void Create_WhenCpfContainsWhiteSpaceOnly_ShouldFailure()
    {
        // Arrange

        string cfpNumberInvalid = " ";

        // Act

        var result = Cpf.Create(cfpNumberInvalid);

        // Assert              

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("CPF_NUMBER_NULL_EMPTY_WHITE_SPACE");
        result.Error.Message.Should().Be("CPF number cannot be null, empty or white space.");
    }

    [Fact]
    public void Create_WhenCpfHasInvalidLength_ShouldFailure()
    {
        // Arrange

        string cfpNumberInvalid = "560.350.200-200";

        // Act

        var result = Cpf.Create(cfpNumberInvalid);

        // Assert              

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("CPF_NUMBER_INVALID_LENGTH");
        result.Error.Message.Should().Be("CPF number must be 11 digits long and contain only numbers.");
    }

    [Fact]
    public void Create_WhenCpfHasAllDigitsEqual_ShouldFailure()
    {
        // Arrange

        string cfpNumberInvalid = "111.111.111-11";

        // Act

        var result = Cpf.Create(cfpNumberInvalid);

        // Assert

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("CPF_NUMBER_CHECKSUM_INVALID");
        result.Error.Message.Should().Be("CPF failed validation.");
    }

    [Fact]
    public void Create_WhenCpfChecksumIsInvalid_ShouldFailure()
    {
        // Arrange

        string cpfNumberInvalid = "123.456.789-00";

        // Act

        var result = Cpf.Create(cpfNumberInvalid);

        // Assert

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("CPF_NUMBER_CHECKSUM_INVALID");
        result.Error.Message.Should().Be("CPF failed validation.");
    }

    // IsSuccess

    [Fact]
    public void Create_WhenCpfIsValid_ShouldSuccess()
    {
        // Arrange

        string cpfNumber = "560.350.200-20"; //Fake CPF number for testing purposes

        // Act

        var result = Cpf.Create(cpfNumber);

        // Assert

        result.IsSuccess.Should().BeTrue();
        result.IsFailure.Should().BeFalse();
        result.Value.Should().NotBeNull();
        result.Value.Number.Should().Be("56035020020");
    }
}

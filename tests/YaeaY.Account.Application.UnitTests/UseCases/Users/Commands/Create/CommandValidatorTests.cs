using FluentAssertions;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using YaeaY.Account.Application;
using YaeaY.Account.Application.Behaviors;
using YaeaY.Account.Domain.Abstraction.Errors;
using YaeaY.Account.Domain.Abstraction.Result;
using YaeaY.Account.Domain.Enumerators;
using YaeaY.Account.Domain.Errors.BirthDate;
using YaeaY.Account.Domain.Errors.Emails;
using YaeaY.Account.Domain.Errors.FullName;
using YaeaY.Account.Domain.Errors.PasswordText;
using YaeaY.Account.Domain.Errors.Telephones;
using YaeaY.Account.Domain.Errors.Users;
using CreateUser = YaeaY.Account.Application.UseCases.Users.Commands.Create;

namespace YaeaY.Account.Application.UnitTests.UseCases.Users.Commands.Create;

public sealed class CommandValidatorTests
{
    public static TheoryData<CreateUser.Command, string, Error> InvalidCommands => new()
    {
        {
            ValidCommand() with { EmailAddress = "" },
            nameof(CreateUser.Command.EmailAddress),
            EmailErrors.Required
        },
        {
            ValidCommand() with { Password = "lowercase@123" },
            nameof(CreateUser.Command.Password),
            PasswordTextErrors.MissingUppercase
        },
        {
            ValidCommand() with { FullName = "" },
            nameof(CreateUser.Command.FullName),
            FullNameErrors.Required
        },
        {
            ValidCommand() with { BirthDate = Tomorrow },
            nameof(CreateUser.Command.BirthDate),
            BirthDateErrors.InFuture(Tomorrow, Today)
        },
        {
            ValidCommand() with { Gender = Gender.Unknown },
            nameof(CreateUser.Command.Gender),
            UserErrors.GenderRequired
        },
        {
            ValidCommand() with { CallingCode = "" },
            nameof(CreateUser.Command.CallingCode),
            TelephoneNumberErrors.CallingCodeRequired
        },
        {
            ValidCommand() with { RegionCode = "BRA" },
            nameof(CreateUser.Command.RegionCode),
            TelephoneNumberErrors.RegionCodeInvalid
        },
        {
            ValidCommand() with { AreaCode = "4A" },
            nameof(CreateUser.Command.AreaCode),
            TelephoneNumberErrors.AreaCodeInvalid
        },
        {
            ValidCommand() with { PhoneType = TelephoneType.Unknown },
            nameof(CreateUser.Command.PhoneType),
            TelephoneNumberErrors.PhoneTypeRequired
        },
        {
            ValidCommand() with { PhoneNumber = "" },
            nameof(CreateUser.Command.PhoneNumber),
            TelephoneNumberErrors.NationalNumberRequired
        },
        {
            ValidCommand() with { PhoneNumber = new string('1', 31) },
            nameof(CreateUser.Command.PhoneNumber),
            TelephoneNumberErrors.NationalNumberTooLong(31, 30)
        }
    };

    private static DateOnly Today => DateOnly.FromDateTime(DateTime.UtcNow);
    private static DateOnly Tomorrow => Today.AddDays(1);

    [Theory]
    [MemberData(nameof(InvalidCommands))]
    public async Task Validate_WhenCommandFieldIsInvalid_ShouldUseItsDomainErrorCatalog(
        CreateUser.Command command,
        string expectedProperty,
        Error expectedError)
    {
        // Arrange

        var validator = new CreateUser.CommandValidator();

        // Act

        var result = await validator.ValidateAsync(command);

        // Assert

        var failure = result.Errors.Should().ContainSingle().Subject;
        failure.PropertyName.Should().Be(expectedProperty);
        failure.ErrorCode.Should().Be(expectedError.Code);
        failure.ErrorMessage.Should().Be(expectedError.Message);
        failure.CustomState.Should().Be(expectedError);
    }

    [Fact]
    public void AddApplication_ShouldRegisterCreateCommandValidator()
    {
        // Arrange

        var services = new ServiceCollection();

        // Act

        services.AddApplication();

        // Assert

        services
            .Where(descriptor =>
                descriptor.ServiceType == typeof(IValidator<CreateUser.Command>))
            .Should()
            .ContainSingle(descriptor =>
                descriptor.ImplementationType == typeof(CreateUser.CommandValidator));
    }

    [Fact]
    public async Task Handle_WhenCommandIsInvalid_ShouldReturnFailureWithoutCallingHandler()
    {
        // Arrange

        var command = ValidCommand() with { EmailAddress = "" };
        var validator = new CreateUser.CommandValidator();
        var behavior = new ValidationBehavior<
            CreateUser.Command,
            Result<CreateUser.Response>>([validator]);
        var handlerWasCalled = false;

        // Act

        var result = await behavior.Handle(
            command,
            cancellationToken =>
            {
                handlerWasCalled = true;
                return Task.FromResult(
                    Result<CreateUser.Response>.Success(
                        new CreateUser.Response(
                            Guid.NewGuid(),
                            "Example Person",
                            "Created")));
            },
            CancellationToken.None);

        // Assert

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(EmailErrors.Required);
        handlerWasCalled.Should().BeFalse();
    }

    private static CreateUser.Command ValidCommand() => new(
        EmailAddress: "person@example.com",
        Password: "Secure@123",
        FullName: "Example Person",
        BirthDate: new DateOnly(2000, 1, 1),
        Gender: Gender.Prefer_Not_To_Say,
        CallingCode: "+55",
        RegionCode: "BR",
        AreaCode: "48",
        PhoneType: TelephoneType.Mobile,
        PhoneNumber: "999999999");
}

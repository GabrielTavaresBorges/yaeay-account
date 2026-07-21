using YaeaY.Account.Domain.Abstraction.Records;

namespace YaeaY.Account.Domain.Errors.Users;

public static class UserErrors
{
    public static readonly Error EmailRequired = new(
        Identifier: "user.email.required",
        Message: "A user must have an email address.");   

    public static readonly Error PasswordRequired = new(
        Identifier: "user.password.required",
        Message: "A user must have a password.");

    public static readonly Error NameRequired = new(
        Identifier: "user.name.required",
        Message: "A user must have an name.");

    public static readonly Error BirthDateRequired = new(
        Identifier: "user.birth-date.required",
        Message: "A user must have a birth date.");

    public static readonly Error GenderRequired = new(
        Identifier: "user.gender.required",
        Message: "A user must have a defined gender.");

    public static readonly Error GenderInvalid = new(
        Identifier: "user.gender.invalid",
        Message: "The informed gender is invalid.");

    public static readonly Error PhoneRequired = new(
        Identifier: "user.phone.required",
        Message: "A phone must be informed.");

    public static readonly Error PhoneAlreadyExists = new(
        Identifier: "user.phone.already-exists",
        Message: "The phone is already associated with this user.");

    public static readonly Error AtLeastOnePhoneRequired = new(
        Identifier: "user.phone.at-least-one-required",
        Message: "A user must have at least one phone.");

    public static readonly Error PrimaryPhoneRequired = new(
        Identifier: "user.phone.primary-required",
        Message: "A user must have exactly one primary phone.");

    public static readonly Error PrimaryPhoneCannotBeRemoved = new(
        Identifier: "user.phone.primary-cannot-be-removed",
        Message: "The primary phone cannot be removed before another phone is set as primary.");

    public static readonly Error PhoneNotFound = new(
        Identifier: "user.phone.not-found",
        Message: "The phone is not associated with this user.");
}
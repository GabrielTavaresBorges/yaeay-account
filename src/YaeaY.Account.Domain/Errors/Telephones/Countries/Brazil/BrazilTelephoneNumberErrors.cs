using YaeaY.Account.Domain.Abstraction.Errors;
using YaeaY.Account.Domain.Abstraction.Errors.Enumerators;

namespace YaeaY.Account.Domain.Errors.Telephones.Countries.Brazil;

public static class BrazilTelephoneNumberErrors
{
    public static readonly Error CallingCodeInvalid = new(
        Code: "phone-number.brazil.calling-code.invalid",
        Message: "The country calling code for Brazil must be 55.",
        Category: ErrorCategory.Validation,
        Rule: ErrorRule.InvalidValue);

    public static readonly Error AreaCodeRequired = new(
        Code: "phone-number.brazil.area-code.required",
        Message: "The area code is required for Brazilian geographic telephone numbers.",
        Category: ErrorCategory.Validation,
        Rule: ErrorRule.Required);

    public static readonly Error AreaCodeInvalid = new(
        Code: "phone-number.brazil.area-code.invalid",
        Message: "The area code is not assigned to a Brazilian numbering area.",
        Category: ErrorCategory.Validation,
        Rule: ErrorRule.InvalidValue);

    public static readonly Error NationalNumberInvalid = new(
        Code: "phone-number.brazil.national-number.invalid",
        Message: "A Brazilian subscriber number must contain eight or nine digits.",
        Category: ErrorCategory.Validation,
        Rule: ErrorRule.InvalidFormat);

    public static readonly Error LandlineNumberInvalid = new(
        Code: "phone-number.brazil.landline-number.invalid",
        Message: "A Brazilian landline subscriber number must contain eight digits.",
        Category: ErrorCategory.Validation,
        Rule: ErrorRule.InvalidFormat);

    public static readonly Error MobileNumberInvalid = new(
        Code: "phone-number.brazil.mobile-number.invalid",
        Message: "A Brazilian mobile subscriber number must contain nine digits and start with 9.",
        Category: ErrorCategory.Validation,
        Rule: ErrorRule.InvalidFormat);
}

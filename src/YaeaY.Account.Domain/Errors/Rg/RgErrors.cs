using YaeaY.Account.Domain.Abstraction.Errors;
using YaeaY.Account.Domain.Abstraction.Errors.Enumerators;

namespace YaeaY.Account.Domain.Errors.Rg;

public static class RgErrors
{
    public static readonly Error NumberRequired = new("rg.number.required", "RG number is required.", ErrorCategory.Validation, ErrorRule.Required);
    public static readonly Error IssueDateRequired = new("rg.issue-date.required", "RG issue date is required.", ErrorCategory.Validation, ErrorRule.Required);
    public static readonly Error IssuingAuthorityRequired = new("rg.issuing-authority.required", "RG issuing authority is required.", ErrorCategory.Validation, ErrorRule.Required);
    public static readonly Error IssuingStateInvalid = new("rg.issuing-state.invalid", "RG issuing state must be a valid Brazilian federative unit.", ErrorCategory.Validation, ErrorRule.InvalidValue);
}

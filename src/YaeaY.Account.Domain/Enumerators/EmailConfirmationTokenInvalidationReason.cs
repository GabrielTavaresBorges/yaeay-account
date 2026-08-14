namespace YaeaY.Account.Domain.Enumerators;

public enum EmailConfirmationTokenInvalidationReason
{
    Unknown,
    Superseded,
    EmailChanged,
    AdminRevoked,
    AccountDisabled
}

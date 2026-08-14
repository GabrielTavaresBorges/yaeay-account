namespace YaeaY.Account.Domain.Enumerators;

public enum EmailConfirmationTokenRequestReason
{
    Unknown,
    AccountCreated,
    AccountSuspended,
    UserRequestedResend,
    AdminRequestedResend,
    ExpiredLink
}

namespace YaeaY.Account.Domain.Enumerators;

public enum PasswordRecoveryChallengeInvalidationReason
{
    Unknown = 0,
    Superseded = 1,
    AttemptsExceeded = 2
}

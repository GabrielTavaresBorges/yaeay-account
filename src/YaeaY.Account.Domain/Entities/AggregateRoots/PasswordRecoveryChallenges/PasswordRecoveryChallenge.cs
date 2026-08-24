using YaeaY.Account.Domain.Abstraction.Entities;
using YaeaY.Account.Domain.Abstraction.Exceptions;
using YaeaY.Account.Domain.Abstraction.Interfaces;
using YaeaY.Account.Domain.Enumerators;
using YaeaY.Account.Domain.Errors.PasswordRecoveryChallenges;
using YaeaY.Account.Domain.Events.PasswordRecoveries;
using YaeaY.Account.Domain.ValueObjects.Emails;
using YaeaY.Account.Domain.ValueObjects.Securities;

namespace YaeaY.Account.Domain.Entities.AggregateRoots.PasswordRecoveryChallenges;

public sealed class PasswordRecoveryChallenge : Entity, IAggregateRoot
{
    private readonly Guid _userId;
    private readonly Email _email = null!;
    private readonly DateTimeOffset _requestedAt;
    private PasswordRecoveryCodeHash? _codeHash;
    private DateTimeOffset? _issuedAt;
    private DateTimeOffset? _expiresAt;
    private int _failedAttempts;
    private DateTimeOffset? _verifiedAt;
    private DateTimeOffset? _authorizationExpiresAt;
    private DateTimeOffset? _consumedAt;
    private DateTimeOffset? _invalidatedAt;
    private PasswordRecoveryChallengeInvalidationReason? _invalidationReason;

    public Guid UserId => _userId;
    public Email Email => _email;
    public DateTimeOffset RequestedAt => _requestedAt;
    public PasswordRecoveryCodeHash? CodeHash => _codeHash;
    public DateTimeOffset? IssuedAt => _issuedAt;
    public DateTimeOffset? ExpiresAt => _expiresAt;
    public int FailedAttempts => _failedAttempts;
    public DateTimeOffset? VerifiedAt => _verifiedAt;
    public DateTimeOffset? AuthorizationExpiresAt => _authorizationExpiresAt;
    public DateTimeOffset? ConsumedAt => _consumedAt;
    public DateTimeOffset? InvalidatedAt => _invalidatedAt;
    public PasswordRecoveryChallengeInvalidationReason? InvalidationReason => _invalidationReason;

    private PasswordRecoveryChallenge() { }

    private PasswordRecoveryChallenge(Guid userId, Email email, DateTimeOffset requestedAt)
    {
        _userId = userId;
        _email = email;
        _requestedAt = requestedAt;
    }

    public static PasswordRecoveryChallenge Create(Guid userId, Email email, DateTimeOffset requestedAt)
    {
        if (userId == Guid.Empty)
            throw new DomainException(PasswordRecoveryChallengeErrors.UserIdRequired);

        if (email is null)
            throw new DomainException(PasswordRecoveryChallengeErrors.EmailRequired);

        if (requestedAt == default)
            throw new DomainException(PasswordRecoveryChallengeErrors.RequestedAtRequired);

        var challenge = new PasswordRecoveryChallenge(userId, email, requestedAt);
        challenge.AddDomainEvent(new PasswordRecoveryRequestedDomainEvent(challenge.Id));

        return challenge;
    }

    public bool IsOpen() => !_consumedAt.HasValue && !_invalidatedAt.HasValue;
    public bool IsAwaitingIssuance() => IsOpen() && _codeHash is null;

    public bool IsCodeUsable(DateTimeOffset nowUtc) =>
        IsOpen()
        && _codeHash is not null
        && _expiresAt.HasValue
        && nowUtc < _expiresAt.Value
        && !_verifiedAt.HasValue;

    public bool IsResetAuthorized(DateTimeOffset nowUtc) =>
        IsOpen()
        && _verifiedAt.HasValue
        && _authorizationExpiresAt.HasValue
        && nowUtc < _authorizationExpiresAt.Value;

    public void Issue(PasswordRecoveryCodeHash codeHash, DateTimeOffset issuedAtUtc, DateTimeOffset expiresAtUtc)
    {
        if (!IsOpen() || _codeHash is not null)
            throw new DomainException(PasswordRecoveryChallengeErrors.AlreadyIssued);

        if (codeHash is null)
            throw new DomainException(PasswordRecoveryChallengeErrors.CodeHashRequired);

        if (issuedAtUtc == default || issuedAtUtc < _requestedAt)
            throw new DomainException(PasswordRecoveryChallengeErrors.IssuedAtRequired);

        if (expiresAtUtc <= issuedAtUtc)
            throw new DomainException(PasswordRecoveryChallengeErrors.ExpirationNotAfterIssue);

        _codeHash = codeHash;
        _issuedAt = issuedAtUtc;
        _expiresAt = expiresAtUtc;
    }

    public void RegisterFailedAttempt(DateTimeOffset attemptedAtUtc, int maximumAttempts)
    {
        if (maximumAttempts <= 0)
            throw new DomainException(PasswordRecoveryChallengeErrors.MaximumAttemptsInvalid);

        if (!IsCodeUsable(attemptedAtUtc))
            throw new DomainException(PasswordRecoveryChallengeErrors.InvalidOrExpired);

        _failedAttempts++;
        if (_failedAttempts >= maximumAttempts)
            Invalidate(PasswordRecoveryChallengeInvalidationReason.AttemptsExceeded, attemptedAtUtc);
    }

    public void Verify(DateTimeOffset verifiedAtUtc, DateTimeOffset authorizationExpiresAtUtc)
    {
        if (!IsCodeUsable(verifiedAtUtc))
            throw new DomainException(PasswordRecoveryChallengeErrors.InvalidOrExpired);

        if (authorizationExpiresAtUtc <= verifiedAtUtc)
            throw new DomainException(PasswordRecoveryChallengeErrors.AuthorizationExpirationInvalid);

        _verifiedAt = verifiedAtUtc;
        _authorizationExpiresAt = authorizationExpiresAtUtc;
    }

    public void Consume(DateTimeOffset consumedAtUtc)
    {
        if (!IsResetAuthorized(consumedAtUtc))
            throw new DomainException(PasswordRecoveryChallengeErrors.ResetNotAuthorized);

        _consumedAt = consumedAtUtc;
        AddDomainEvent(new PasswordRecoveryCompletedDomainEvent(Id));
    }

    public void Invalidate(PasswordRecoveryChallengeInvalidationReason reason, DateTimeOffset invalidatedAtUtc)
    {
        if (!IsOpen())
            throw new DomainException(PasswordRecoveryChallengeErrors.AlreadyInvalidated);

        if (reason == PasswordRecoveryChallengeInvalidationReason.Unknown)
            throw new DomainException(PasswordRecoveryChallengeErrors.InvalidationReasonRequired);

        if (!Enum.IsDefined(reason))
            throw new DomainException(PasswordRecoveryChallengeErrors.InvalidationReasonInvalid);

        if (invalidatedAtUtc == default || invalidatedAtUtc < _requestedAt)
            throw new DomainException(PasswordRecoveryChallengeErrors.InvalidatedAtRequired);

        _invalidatedAt = invalidatedAtUtc;
        _invalidationReason = reason;
    }
}

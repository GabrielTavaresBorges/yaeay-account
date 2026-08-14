using YaeaY.Account.Domain.Abstraction.Entities;
using YaeaY.Account.Domain.Abstraction.Exceptions;
using YaeaY.Account.Domain.Abstraction.Interfaces;
using YaeaY.Account.Domain.Enumerators;
using YaeaY.Account.Domain.Errors.EmailConfirmationTokens;
using YaeaY.Account.Domain.ValueObjects.Emails;
using YaeaY.Account.Domain.ValueObjects.Securities;

namespace YaeaY.Account.Domain.Entities.AggregateRoots.EmailConfirmationTokens;

public sealed class EmailConfirmationToken : Entity, IAggregateRoot
{
    private readonly Guid _userId;
    private readonly Email _email = null!;
    private readonly TokenHash _tokenHash = null!;
    private readonly DateTimeOffset _createdAt;
    private readonly DateTimeOffset _expiresAt;
    private readonly EmailConfirmationTokenRequestedBy _requestedBy;
    private readonly EmailConfirmationTokenRequestReason _requestReason;
    private DateTimeOffset? _usedAt;
    private DateTimeOffset? _invalidatedAt;
    private EmailConfirmationTokenInvalidationReason? _invalidationReason;

    public Guid UserId => _userId;
    public Email Email => _email;
    public TokenHash TokenHash => _tokenHash;
    public DateTimeOffset CreatedAt => _createdAt;
    public DateTimeOffset ExpiresAt => _expiresAt;
    public DateTimeOffset? UsedAt => _usedAt;
    public DateTimeOffset? InvalidatedAt => _invalidatedAt;
    public EmailConfirmationTokenInvalidationReason? InvalidationReason => _invalidationReason;
    public EmailConfirmationTokenRequestedBy RequestedBy => _requestedBy;
    public EmailConfirmationTokenRequestReason RequestReason => _requestReason;

    private EmailConfirmationToken() { }

    private EmailConfirmationToken(
        Guid userId,
        Email email,
        TokenHash tokenHash,
        DateTimeOffset createdAt,
        DateTimeOffset expiresAt,
        EmailConfirmationTokenRequestedBy requestedBy,
        EmailConfirmationTokenRequestReason requestReason)
    {
        _userId = userId;
        _email = email;
        _tokenHash = tokenHash;
        _createdAt = createdAt;
        _expiresAt = expiresAt;
        _requestedBy = requestedBy;
        _requestReason = requestReason;
    }

    public static EmailConfirmationToken Create(
        Guid userId,
        Email email,
        TokenHash tokenHash,
        DateTimeOffset expiresAt,
        EmailConfirmationTokenRequestedBy requestedBy,
        EmailConfirmationTokenRequestReason requestReason)
    {
        var createdAt = DateTimeOffset.UtcNow;

        ValidateCreation(userId, email, tokenHash, createdAt, expiresAt, requestedBy, requestReason);

        return new EmailConfirmationToken(
            userId,
            email,
            tokenHash,
            createdAt,
            expiresAt,
            requestedBy,
            requestReason);
    }

    private static void ValidateCreation(
        Guid userId,
        Email email,
        TokenHash tokenHash,
        DateTimeOffset createdAt,
        DateTimeOffset expiresAt,
        EmailConfirmationTokenRequestedBy requestedBy,
        EmailConfirmationTokenRequestReason requestReason)
    {
        if (userId == Guid.Empty)
            throw new DomainException(EmailConfirmationTokenErrors.UserIdRequired);

        if (email is null)
            throw new DomainException(EmailConfirmationTokenErrors.EmailRequired);

        if (tokenHash is null)
            throw new DomainException(EmailConfirmationTokenErrors.TokenHashRequired);

        if (expiresAt <= createdAt)
            throw new DomainException(EmailConfirmationTokenErrors.ExpirationNotAfterCreation);

        if (requestedBy == EmailConfirmationTokenRequestedBy.Unknown)
            throw new DomainException(EmailConfirmationTokenErrors.RequestedByRequired);

        if (!Enum.IsDefined(requestedBy))
            throw new DomainException(EmailConfirmationTokenErrors.RequestedByInvalid);

        if (requestReason == EmailConfirmationTokenRequestReason.Unknown)
            throw new DomainException(EmailConfirmationTokenErrors.RequestReasonRequired);

        if (!Enum.IsDefined(requestReason))
            throw new DomainException(EmailConfirmationTokenErrors.RequestReasonInvalid);
    }

    public bool IsExpired(DateTimeOffset nowUtc)
        => nowUtc >= _expiresAt;

    public bool IsUsed()
        => _usedAt.HasValue;

    public bool IsInvalidated()
        => _invalidatedAt.HasValue;

    public bool IsUsable(DateTimeOffset nowUtc)
        => !IsUsed() && !IsInvalidated() && !IsExpired(nowUtc);

    public void MarkAsUsed(DateTimeOffset usedAtUtc)
    {
        if (usedAtUtc == default)
            throw new DomainException(EmailConfirmationTokenErrors.UsedAtRequired);

        if (_usedAt.HasValue)
            throw new DomainException(EmailConfirmationTokenErrors.AlreadyUsed);

        if (_invalidatedAt.HasValue)
            throw new DomainException(EmailConfirmationTokenErrors.Invalidated);

        if (usedAtUtc < _createdAt)
            throw new DomainException(EmailConfirmationTokenErrors.UsedBeforeCreation);

        if (IsExpired(usedAtUtc))
            throw new DomainException(EmailConfirmationTokenErrors.Expired);

        _usedAt = usedAtUtc;
    }

    public void Invalidate(
        EmailConfirmationTokenInvalidationReason reason,
        DateTimeOffset invalidatedAtUtc)
    {
        if (reason == EmailConfirmationTokenInvalidationReason.Unknown)
            throw new DomainException(EmailConfirmationTokenErrors.InvalidationReasonRequired);

        if (!Enum.IsDefined(reason))
            throw new DomainException(EmailConfirmationTokenErrors.InvalidationReasonInvalid);

        if (invalidatedAtUtc == default)
            throw new DomainException(EmailConfirmationTokenErrors.InvalidatedAtRequired);

        if (_usedAt.HasValue)
            throw new DomainException(EmailConfirmationTokenErrors.UsedTokenCannotBeInvalidated);

        if (_invalidatedAt.HasValue)
            throw new DomainException(EmailConfirmationTokenErrors.AlreadyInvalidated);

        if (invalidatedAtUtc < _createdAt)
            throw new DomainException(EmailConfirmationTokenErrors.InvalidatedBeforeCreation);

        _invalidationReason = reason;
        _invalidatedAt = invalidatedAtUtc;
    }
}

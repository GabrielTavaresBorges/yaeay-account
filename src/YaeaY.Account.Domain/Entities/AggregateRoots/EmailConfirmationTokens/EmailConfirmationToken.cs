using YaeaY.Account.Domain.Abstraction.Entities;
using YaeaY.Account.Domain.Abstraction.Exceptions;
using YaeaY.Account.Domain.Abstraction.Interfaces;
using YaeaY.Account.Domain.ValueObjects.Securities;

namespace YaeaY.Account.Domain.Entities.AggregateRoots.EmailConfirmationTokens;

public sealed class EmailConfirmationToken : Entity, IAggregateRoot
{
    private readonly Guid _userId;
    private readonly TokenHash _tokenHash = null!;
    private readonly DateTimeOffset _createdAt;
    private readonly DateTimeOffset _expiresAt;
    private DateTimeOffset? _usedAt;

    public Guid UserId => _userId;
    public TokenHash TokenHash => _tokenHash;
    public DateTimeOffset CreatedAt => _createdAt;
    public DateTimeOffset ExpiresAt => _expiresAt;
    public DateTimeOffset? UsedAt => _usedAt;

    private EmailConfirmationToken() { }

    private EmailConfirmationToken(Guid userId, TokenHash tokenHash, DateTimeOffset expiresAt)
    {
        _userId = userId;
        _tokenHash = tokenHash;
        _createdAt = DateTimeOffset.UtcNow;
        _expiresAt = expiresAt;
    }

    public static EmailConfirmationToken Create(Guid userId, TokenHash tokenHash, DateTimeOffset expiresAt)
    {
        Validate(userId, tokenHash, expiresAt);

        var emailConfirmationToken = new EmailConfirmationToken(userId, tokenHash, expiresAt);

        return emailConfirmationToken;
    }

    private static void Validate(Guid userId, TokenHash tokenHash, DateTimeOffset expiresAt)
    {
        if (userId == Guid.Empty)
            throw new DomainException(
                identifier: "USER_ID_INVALID",
                message: "UserId cannot be empty.");

        if (tokenHash is null)
            throw new DomainException(
                identifier: "TOKEN_HASH_NULL",
                message: "Token hash cannot be null.");

        if (expiresAt <= DateTimeOffset.UtcNow)
            throw new DomainException(
                identifier: "TOKEN_EXPIRATION_INVALID",
                message: "Expiration date must be in the future.");
    }

    public bool IsExpired(DateTimeOffset nowUtc)
        => nowUtc >= _expiresAt;

    public bool IsUsed()
        => _usedAt.HasValue;

    public void MarkAsUsed(DateTimeOffset usedAtUtc)
    {
        if (_usedAt.HasValue)
            throw new DomainException(
                identifier: "TOKEN_ALREADY_USED",
                message: "Token has already been used.");

        if (IsExpired(usedAtUtc))
            throw new DomainException(
                identifier: "TOKEN_EXPIRED",
                message: "Token has expired.");

        _usedAt = usedAtUtc;
    }
}

using YaeaY.Account.Domain.Abstraction.Entities;
using YaeaY.Account.Domain.Abstraction.Exceptions;
using YaeaY.Account.Domain.Abstraction.Interfaces;
using YaeaY.Account.Domain.Entities.UserDocuments;
using YaeaY.Account.Domain.Entities.UserPhones;
using YaeaY.Account.Domain.Enumerators;
using YaeaY.Account.Domain.Errors.Users;
using YaeaY.Account.Domain.Events.Users;
using YaeaY.Account.Domain.ValueObjects.Accounts;
using YaeaY.Account.Domain.ValueObjects.Dates;
using YaeaY.Account.Domain.ValueObjects.Emails;
using YaeaY.Account.Domain.ValueObjects.Names;
using YaeaY.Account.Domain.ValueObjects.Securities;

namespace YaeaY.Account.Domain.Entities.AggregateRoots.Users;

public class User : Entity, IAggregateRoot
{
    private Email _email = null!;
    private UserName _userName = null!;
    private PasswordHash _passwordHash = null!;
    private BirthDate _birthDate = null!;
    private readonly AccountStatus _status;
    private Gender _gender;
    private readonly SuspensionInfo? _suspension;
    private readonly DateTimeOffset _createdAt;
    private readonly DateTimeOffset? _emailConfirmedAt;
    private readonly DateTimeOffset? _firstLoginAt;
    private readonly DateTimeOffset? _lastLoginAt;

    private readonly List<UserDocument> _documents = new();
    private readonly List<UserPhone> _phones = new();

    public Email Email => _email;
    public PasswordHash PasswordHash => _passwordHash;
    public UserName UserName => _userName;
    public BirthDate BirthDate => _birthDate;
    public AccountStatus Status => _status;
    public Gender Gender => _gender;
    public SuspensionInfo? SuspensionInfo => _suspension;
    public DateTimeOffset CreatedAt => _createdAt;
    public DateTimeOffset? EmailConfirmedAt => _emailConfirmedAt;
    public DateTimeOffset? FirstLoginAt => _firstLoginAt;
    public DateTimeOffset? LastLoginAt => _lastLoginAt;

    public IReadOnlyCollection<UserDocument> Documents => _documents.AsReadOnly();
    public IReadOnlyCollection<UserPhone> Phones => _phones.AsReadOnly();

    private User() { }

    private User(
        Email email,
        PasswordHash passwordHash,
        UserName userName,
        BirthDate birthDate,
        Gender gender)
    {
        _email = email;
        _passwordHash = passwordHash;
        _userName = userName;
        _birthDate = birthDate;
        _gender = gender;

        _status = AccountStatus.PendingEmailConfirmation;
        _createdAt = DateTimeOffset.UtcNow;
    }

    public static User Create(
        Email emailAddress,
        PasswordHash passwordHash,
        UserName userName,
        BirthDate birthDate,
        Gender gender,
        UserPhone initialPhone)
    {
        Validate(emailAddress, passwordHash, userName, birthDate, gender, initialPhone);

        var user = new User(emailAddress, passwordHash, userName, birthDate, gender);

        // Regra: no cadastro, esse telefone é obrigatório e deve ser primário
        user.AddPhone(initialPhone);

        // Dispara evento de usuário registrado
        var userRegisteredEvent = new UserRegisteredDomainEvent(
            UserId: user.Id,
            Email: user.Email.EmailAddress,
            UserName: user.UserName.Name);

        user.AddDomainEvent(userRegisteredEvent);

        return user;
    }

    private static void Validate(
        Email emailAddress,
        PasswordHash passwordHash,
        UserName userName,
        BirthDate birthDate,
        Gender gender,
        UserPhone initialPhone)
    {
        if (emailAddress is null)
            throw new DomainException(UserErrors.EmailRequired);

        if (passwordHash is null)
            throw new DomainException(UserErrors.PasswordRequired);

        if (userName is null)
            throw new DomainException(UserErrors.NameRequired);

        if (birthDate is null)
            throw new DomainException(UserErrors.BirthDateRequired);

        if (gender == Gender.Unknown)
            throw new DomainException(UserErrors.GenderRequired);

        if (!Enum.IsDefined(typeof(Gender), gender))
            throw new DomainException(UserErrors.GenderInvalid);
    }

    #region Phones (Aggregate rules)

    public void AddPhone(UserPhone phone)
    {
        if (phone is null)
            throw new DomainException(UserErrors.PhoneRequired);

        // Dedup pelo seu índice (E164)
        if (_phones.Any(p => p.E164 == phone.E164))
            throw new DomainException(UserErrors.PhoneAlreadyExists);

        // Se já existe primário e o novo vem como primário, desmarca o atual
        if (phone.IsPrimary)
        {
            foreach (var p in _phones.Where(p => p.IsPrimary))
                p.SetPrimary(false);
        }

        _phones.Add(phone);

        // Invariante: sempre precisa existir 1 primário
        if (!_phones.Any(p => p.IsPrimary))
            phone.SetPrimary(true);
    }

    #endregion

    public void ChangeEmail(Email email)
    {
        if (email is null)
            throw new DomainException(
                message: "Email cannot be null.",
                identifier: "EMAIL_NULL");

        _email = email;
    }

    public void ChangePasswordHash(PasswordHash passwordHash)
    {
        if (passwordHash is null)
            throw new DomainException(
                message: "Password hash cannot be null.",
                identifier: "PASSWORD_HASH_NULL");

        _passwordHash = passwordHash;
    }

    public void ChangeUserName(UserName userName)
    {
        if (userName is null)
            throw new DomainException(
                message: "UserName cannot be null.",
                identifier: "USER_NAME_NULL");

        _userName = userName;
    }

    public void ChangeBirthDate(BirthDate birthDate)
    {
        if (birthDate is null)
            throw new DomainException(
                message: "Birth date cannot be null.",
                identifier: "BIRTH_DATE_NULL");

        _birthDate = birthDate;
    }

    public void ChangeGender(Gender gender)
    {
        if (gender == Gender.Unknown)
            throw new DomainException(
                message: "Gender cannot be unknown.",
                identifier: "GENDER_UNKNOWN");

        if (_gender == gender)
            return;

        _gender = gender;
    }
}
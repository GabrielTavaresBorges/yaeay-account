using YaeaY.Account.Domain.Abstraction.Entities;
using YaeaY.Account.Domain.Abstraction.Exceptions;
using YaeaY.Account.Domain.Abstraction.Interfaces;
using YaeaY.Account.Domain.Entities.UserDocuments;
using YaeaY.Account.Domain.Entities.UserPhones;
using YaeaY.Account.Domain.Enumerators;
using YaeaY.Account.Domain.Errors.Users;
using YaeaY.Account.Domain.Errors.UserDocuments;
using YaeaY.Account.Domain.Events.Users;
using YaeaY.Account.Domain.ValueObjects.Accounts;
using YaeaY.Account.Domain.ValueObjects.Dates;
using YaeaY.Account.Domain.ValueObjects.Documents;
using YaeaY.Account.Domain.ValueObjects.Emails;
using YaeaY.Account.Domain.ValueObjects.Names;
using YaeaY.Account.Domain.ValueObjects.Telephones;

namespace YaeaY.Account.Domain.Entities.AggregateRoots.Users;

public class User : Entity, IAggregateRoot
{
    private Email _email = null!;
    private FullName _fullName = null!;
    private BirthDate _birthDate = null!;
    private AccountStatus _status;
    private Gender _gender;
    private DateTimeOffset _createdAt;
    private SuspensionInfo? _suspension;
    private DateTimeOffset? _emailConfirmedAt;
    private DateTimeOffset? _firstLoginAt;
    private DateTimeOffset? _lastLoginAt;

    private readonly List<UserDocument> _documents = new();
    private readonly List<UserPhone> _phones = new();

    public Email Email => _email;
    public FullName FullName => _fullName;
    public BirthDate BirthDate => _birthDate;
    public AccountStatus Status => _status;
    public Gender Gender => _gender;
    public DateTimeOffset CreatedAt => _createdAt;
    public SuspensionInfo? SuspensionInfo => _suspension;
    public DateTimeOffset? EmailConfirmedAt => _emailConfirmedAt;
    public DateTimeOffset? FirstLoginAt => _firstLoginAt;
    public DateTimeOffset? LastLoginAt => _lastLoginAt;

    public IReadOnlyCollection<UserDocument> Documents => _documents.AsReadOnly();
    public IReadOnlyCollection<UserPhone> Phones => _phones.AsReadOnly();

    private User() { }

    private User(
        Email email,
        FullName fullName,
        BirthDate birthDate,
        Gender gender,
        TelephoneNumber initialPhoneNumber)
    {
        _email = email;
        _fullName = fullName;
        _birthDate = birthDate;
        _gender = gender;

        _phones.Add(UserPhone.Create(initialPhoneNumber, isPrimary: true));

        _status = AccountStatus.PendingEmailConfirmation;
        _createdAt = DateTimeOffset.UtcNow;
    }

    public static User Create(
        Email emailAddress,
        FullName fullName,
        BirthDate birthDate,
        Gender gender,
        TelephoneNumber initialPhoneNumber)
    {
        Validate(emailAddress, fullName, birthDate, gender, initialPhoneNumber);

        var user = new User(
            emailAddress,
            fullName,
            birthDate,
            gender,
            initialPhoneNumber);

        // Dispara evento de usuário registrado
        var userRegisteredEvent = new UserRegisteredDomainEvent(
            UserId: user.Id,
            Email: user.Email.EmailAddress,
            FullName: user.FullName.Name);

        user.AddDomainEvent(userRegisteredEvent);

        return user;
    }

    private static void Validate(
        Email emailAddress,
        FullName fullName,
        BirthDate birthDate,
        Gender gender,
        TelephoneNumber initialPhoneNumber)
    {
        if (emailAddress is null)
            throw new DomainException(UserErrors.EmailRequired);

        if (fullName is null)
            throw new DomainException(UserErrors.FullNameRequired);

        if (birthDate is null)
            throw new DomainException(UserErrors.BirthDateRequired);

        if (gender == Gender.Unknown)
            throw new DomainException(UserErrors.GenderRequired);

        if (!Enum.IsDefined(typeof(Gender), gender))
            throw new DomainException(UserErrors.GenderInvalid);

        if (initialPhoneNumber is null)
            throw new DomainException(UserErrors.PhoneRequired);
    }

    public void AddPhone(TelephoneNumber phoneNumber, bool isPrimary = false)
    {
        if (phoneNumber is null)
            throw new DomainException(UserErrors.PhoneRequired);

        if (_phones.Any(existing => existing.E164 == phoneNumber.E164))
            throw new DomainException(UserErrors.PhoneAlreadyExists);

        if (isPrimary)
        {
            foreach (var currentPrimary in _phones.Where(p => p.IsPrimary))
                currentPrimary.SetPrimary(false);
        }

        _phones.Add(UserPhone.Create(phoneNumber, isPrimary));
    }

    public UserDocument AddCpfDocument(Cpf cpf, IEnumerable<UserDocumentImage>? images = null)
    {
        if (cpf is null)
            throw new DomainException(UserDocumentErrors.CpfRequired);

        var documentImages = images?.ToArray() ?? [];
        var existingStorageKeys = _documents
            .SelectMany(document => document.Images)
            .Select(image => image.StorageObjectKey)
            .ToHashSet(StringComparer.Ordinal);

        if (documentImages.Any(image => image is not null && existingStorageKeys.Contains(image.StorageObjectKey)))
            throw new DomainException(UserDocumentErrors.ImageStorageObjectKeyAlreadyExists);

        var document = UserDocument.CreateFromCpf(cpf, documentImages);
        _documents.Add(document);
        return document;
    }

    public void ConfirmEmail(DateTimeOffset confirmedAtUtc)
    {
        if (confirmedAtUtc == default)
            throw new DomainException(UserErrors.EmailConfirmationDateRequired);

        if (confirmedAtUtc < _createdAt)
            throw new DomainException(UserErrors.EmailConfirmationBeforeAccountCreation);

        if (_emailConfirmedAt.HasValue || _status == AccountStatus.Active)
            throw new DomainException(UserErrors.EmailAlreadyConfirmed);

        if (_status == AccountStatus.Suspended)
        {
            if (_suspension is null ||
                _suspension.SuspensionBy != SuspensionBy.System ||
                _suspension.Reason != SuspensionReason.Inactivity)
            {
                throw new DomainException(UserErrors.SuspensionPreventsEmailConfirmation);
            }
        }
        else if (_status != AccountStatus.PendingEmailConfirmation)
        {
            throw new DomainException(UserErrors.AccountCannotBeEmailConfirmed);
        }

        _emailConfirmedAt = confirmedAtUtc;
        _suspension = null;
        _status = AccountStatus.Active;
    }

    public void ChangeEmail(Email email)
    {
        if (email is null)
            throw new DomainException(
                message: "Email cannot be null.",
                code: "EMAIL_NULL");

        _email = email;
    }

    public void RegisterSuccessfulLogin(DateTimeOffset occurredAtUtc)
    {
        if (occurredAtUtc == default)
            throw new DomainException(UserErrors.LoginDateRequired);

        if (_status != AccountStatus.Active || !_emailConfirmedAt.HasValue)
            throw new DomainException(UserErrors.AccountCannotLogin);

        if (occurredAtUtc < _emailConfirmedAt.Value ||
            (_lastLoginAt.HasValue && occurredAtUtc < _lastLoginAt.Value))
        {
            throw new DomainException(UserErrors.LoginBeforePreviousAccountActivity);
        }

        _firstLoginAt ??= occurredAtUtc;
        _lastLoginAt = occurredAtUtc;
    }

    public void ChangeFullName(FullName fullName)
    {
        if (fullName is null)
            throw new DomainException(
                message: "Full name cannot be null.",
                code: "FULL_NAME_NULL");

        _fullName = fullName;
    }

    public void ChangeBirthDate(BirthDate birthDate)
    {
        if (birthDate is null)
            throw new DomainException(
                message: "Birth date cannot be null.",
                code: "BIRTH_DATE_NULL");

        _birthDate = birthDate;
    }

    public void ChangeGender(Gender gender)
    {
        if (gender == Gender.Unknown)
            throw new DomainException(
                message: "Gender cannot be unknown.",
                code: "GENDER_UNKNOWN");

        if (_gender == gender)
            return;

        _gender = gender;
    }
}

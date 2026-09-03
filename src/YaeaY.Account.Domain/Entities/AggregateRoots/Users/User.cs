using YaeaY.Account.Domain.Abstraction.Entities;
using YaeaY.Account.Domain.Abstraction.Exceptions;
using YaeaY.Account.Domain.Abstraction.Interfaces;
using YaeaY.Account.Domain.Entities.UserDocuments;
using YaeaY.Account.Domain.Entities.UserPhones;
using YaeaY.Account.Domain.Enumerators;
using YaeaY.Account.Domain.Errors.UserDocuments;
using YaeaY.Account.Domain.Errors.Users;
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

    #region Email
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
        AddDomainEvent(new UserEmailConfirmedDomainEvent(Id));
    }

    public void ChangeEmail(Email email)
    {
        if (email is null)
            throw new DomainException(UserErrors.EmailRequired);

        _email = email;
        AddDomainEvent(new UserProfileChangedDomainEvent(Id));
    }
    #endregion

    #region FullName
    public void ChangeFullName(FullName fullName)
    {
        if (fullName is null)
            throw new DomainException(
                message: "Full name cannot be null.",
                code: "FULL_NAME_NULL");

        _fullName = fullName;
        AddDomainEvent(new UserProfileChangedDomainEvent(Id));
    }
    #endregion

    #region Gender
    public void ChangeGender(Gender gender)
    {
        if (gender == Gender.Unknown)
            throw new DomainException(
                message: "Gender cannot be unknown.",
                code: "GENDER_UNKNOWN");

        if (_gender == gender)
            return;

        _gender = gender;
        AddDomainEvent(new UserProfileChangedDomainEvent(Id));
    }
    #endregion

    #region BirthDate
    public void ChangeBirthDate(BirthDate birthDate)
    {
        if (birthDate is null)
            throw new DomainException(
                message: "Birth date cannot be null.",
                code: "BIRTH_DATE_NULL");

        _birthDate = birthDate;
        AddDomainEvent(new UserProfileChangedDomainEvent(Id));
    }
    #endregion

    #region Telephones
    public UserPhone AddPhone(TelephoneNumber phoneNumber, bool isPrimary = false)
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

        var phone = UserPhone.Create(phoneNumber, isPrimary);
        _phones.Add(phone);
        AddDomainEvent(new UserProfileChangedDomainEvent(Id));
        return phone;
    }

    public bool SetPrimaryPhone(Guid phoneId)
    {
        var primaryPhone = _phones.SingleOrDefault(phone => phone.Id == phoneId);
        if (primaryPhone is null)
            throw new DomainException(UserErrors.PhoneNotFound);

        var changed = false;
        foreach (var phone in _phones)
            changed |= phone.SetPrimary(phone.Id == phoneId);

        if (changed)
            AddDomainEvent(new UserProfileChangedDomainEvent(Id));

        return changed;
    }

    public bool ChangePhone(Guid phoneId, TelephoneNumber phoneNumber)
    {
        if (phoneNumber is null)
            throw new DomainException(UserErrors.PhoneRequired);

        var phone = _phones.SingleOrDefault(existing => existing.Id == phoneId);
        if (phone is null)
            throw new DomainException(UserErrors.PhoneNotFound);

        if (_phones.Any(existing => existing.Id != phoneId && existing.E164 == phoneNumber.E164))
            throw new DomainException(UserErrors.PhoneAlreadyExists);

        if (!phone.Update(phoneNumber))
            return false;

        AddDomainEvent(new UserProfileChangedDomainEvent(Id));
        return true;
    }

    public void RemovePhone(Guid phoneId)
    {
        if (_phones.Count <= 1)
            throw new DomainException(UserErrors.AtLeastOnePhoneRequired);

        var phone = _phones.SingleOrDefault(existing => existing.Id == phoneId);
        if (phone is null)
            throw new DomainException(UserErrors.PhoneNotFound);

        if (phone.IsPrimary)
            throw new DomainException(UserErrors.PrimaryPhoneCannotBeRemoved);

        _phones.Remove(phone);
        AddDomainEvent(new UserProfileChangedDomainEvent(Id));
    }

    #endregion

    #region Documents
    public UserDocument UpsertCpfDocument(Cpf cpf, IEnumerable<UserDocumentImage>? images, out bool changed)
    {
        if (cpf is null)
            throw new DomainException(UserDocumentErrors.CpfRequired);

        var documentImages = images?.ToArray() ?? [];
        var existingCpfDocument = _documents
            .Where(document => document.DocumentType == DocumentType.Cpf)
            .OrderByDescending(document => document.CreatedAt)
            .FirstOrDefault();

        var existingStorageKeys = _documents
            .Where(document => document != existingCpfDocument)
            .SelectMany(document => document.Images)
            .Select(image => image.StorageObjectKey)
            .ToHashSet(StringComparer.Ordinal);

        if (documentImages.Any(image => image is not null && existingStorageKeys.Contains(image.StorageObjectKey)))
            throw new DomainException(UserDocumentErrors.ImageStorageObjectKeyAlreadyExists);

        if (existingCpfDocument is not null)
        {
            var sameCpf = existingCpfDocument.Cpf?.Cpf.Number == cpf.Number;
            var sameImages = existingCpfDocument.Images.Count == documentImages.Length
                && existingCpfDocument.Images.OrderBy(image => image.Position).Select(image => image.StorageObjectKey)
                    .SequenceEqual(documentImages.OrderBy(image => image.Position).Select(image => image.StorageObjectKey), StringComparer.Ordinal);
            if (sameCpf && sameImages)
            {
                changed = false;
                return existingCpfDocument;
            }

            // CPF possui somente um estado atual. Substituir o agregado-filho evita
            // depender de identidades legadas inconsistentes dos detalhes e imagens.
            _documents.Remove(existingCpfDocument);
            var replacement = UserDocument.CreateFromCpf(cpf, documentImages);
            _documents.Add(replacement);
            changed = true;
            AddDomainEvent(new UserProfileChangedDomainEvent(Id));
            return replacement;
        }

        var document = UserDocument.CreateFromCpf(cpf, documentImages);
        _documents.Add(document);
        changed = true;
        AddDomainEvent(new UserProfileChangedDomainEvent(Id));
        return document;
    }

    public UserDocument UpsertRgDocument(Rg rg, IEnumerable<UserDocumentImage>? images, out bool changed)
    {
        if (rg is null)
            throw new DomainException(UserDocumentErrors.RgRequired);

        var documentImages = images?.ToArray() ?? [];
        var existingRgDocument = _documents
            .Where(document => document.DocumentType == DocumentType.Rg)
            .OrderByDescending(document => document.CreatedAt)
            .FirstOrDefault();

        if (existingRgDocument is not null)
        {
            changed = existingRgDocument.UpdateRg(rg, documentImages);
            if (changed)
                AddDomainEvent(new UserProfileChangedDomainEvent(Id));

            return existingRgDocument;
        }

        var document = UserDocument.CreateFromRg(rg, documentImages);
        _documents.Add(document);
        changed = true;
        AddDomainEvent(new UserProfileChangedDomainEvent(Id));
        return document;
    }
    #endregion

    public void RegisterDocumentChanged() => AddDomainEvent(new UserProfileChangedDomainEvent(Id));

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
        AddDomainEvent(new UserLoginRegisteredDomainEvent(Id));
    }

    public void Suspend(SuspensionReason reason, string justification, DateTimeOffset occurredAtUtc, DateTimeOffset? untilUtc = null)
    {
        if (string.IsNullOrWhiteSpace(justification))
            throw new DomainException(UserErrors.AdministrativeJustificationRequired);

        if (_status == AccountStatus.Disabled)
            throw new DomainException(UserErrors.DisabledAccountCannotBeSuspended);

        var suspension = SuspensionInfo.Create(reason, SuspensionBy.Admin, occurredAtUtc, untilUtc, justification);
        if (suspension.IsFailure)
            throw new DomainException(suspension.Error);

        _suspension = suspension.Value;
        _status = AccountStatus.Suspended;
        AddDomainEvent(new UserProfileChangedDomainEvent(Id));
    }

    public void Disable(string justification)
    {
        if (string.IsNullOrWhiteSpace(justification))
            throw new DomainException(UserErrors.AdministrativeJustificationRequired);

        _suspension = null;
        _status = AccountStatus.Disabled;
        AddDomainEvent(new UserProfileChangedDomainEvent(Id));
    }

    public void Reactivate(string justification)
    {
        if (string.IsNullOrWhiteSpace(justification))
            throw new DomainException(UserErrors.AdministrativeJustificationRequired);

        if (!_emailConfirmedAt.HasValue)
            throw new DomainException(UserErrors.UnconfirmedAccountCannotBeReactivated);

        _suspension = null;
        _status = AccountStatus.Active;
        AddDomainEvent(new UserProfileChangedDomainEvent(Id));
    }
}

using YaeaY.Account.Domain.Abstraction.Entities;
using YaeaY.Account.Domain.Abstraction.Exceptions;
using YaeaY.Account.Domain.Enumerators;
using PhoneNumberValue = YaeaY.Account.Domain.ValueObjects.Telephones.TelephoneNumber;

namespace YaeaY.Account.Domain.Entities.UserPhones;

public sealed class UserPhone : Entity
{
    private PhoneNumberValue? _number;
    private string _callingCode = string.Empty;
    private string _regionCode = string.Empty;
    private string? _areaCode;
    private TelephoneType _phoneType;
    private string _phoneNumber = string.Empty;
    private string _e164 = string.Empty;
    private bool _isVerified;
    private DateTimeOffset? _verifiedAt;
    private bool _isPrimary;

    public PhoneNumberValue Number => _number ??= RestorePhoneNumber();
    public string CallingCode => _callingCode;
    public string RegionCode => _regionCode;
    public string? AreaCode => _areaCode;
    public TelephoneType PhoneType => _phoneType;
    public string PhoneNumber => _phoneNumber;
    public string E164 => _e164;
    public DateTimeOffset? VerifiedAt => _verifiedAt;
    public bool IsVerified => _isVerified;
    public bool IsPrimary => _isPrimary;
    public DateTimeOffset CreatedAt { get; private set; }

    private UserPhone() { }

    private UserPhone(PhoneNumberValue number, bool isPrimary)
    {
        SetNumber(number);
        _isPrimary = isPrimary;

        CreatedAt = DateTimeOffset.UtcNow;
    }

    public static UserPhone Create(PhoneNumberValue number, bool isPrimary)
    {
        if (number is null)
            throw new DomainException(
                code: "user-phone.number.required",
                message: "A phone number is required.");

        return new UserPhone(number, isPrimary);
    }

    internal void Update(PhoneNumberValue number)
    {
        if (number is null)
            throw new DomainException(
                code: "user-phone.number.required",
                message: "A phone number is required.");

        if (Number == number)
            return;

        SetNumber(number);

        _isVerified = false;
        _verifiedAt = null;
    }

    private void SetNumber(PhoneNumberValue number)
    {
        _number = number;
        _callingCode = number.CallingCode;
        _regionCode = number.RegionCode;
        _areaCode = number.AreaCode;
        _phoneType = number.PhoneType;
        _phoneNumber = number.NationalNumber;
        _e164 = number.E164;
    }

    private PhoneNumberValue RestorePhoneNumber() =>
        PhoneNumberValue.Create(
            _callingCode,
            _regionCode,
            _areaCode,
            _phoneType,
            _phoneNumber,
            _e164).Value;

    internal void MarkVerified(DateTimeOffset verifiedAtUtc)
    {
        if (_isVerified)
            return;

        if (verifiedAtUtc == default)
            throw new DomainException(
                code: "PHONE_VERIFIED_AT_INVALID",
                message: "VerifiedAt cannot be default.");

        _isVerified = true;
        _verifiedAt = verifiedAtUtc;
    }

    internal void SetPrimary(bool isPrimary)
    {
        if (_isPrimary == isPrimary)
            return;

        _isPrimary = isPrimary;
    }
}

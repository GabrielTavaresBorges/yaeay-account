using YaeaY.Account.Domain.Abstraction.Entities;
using YaeaY.Account.Domain.Abstraction.Exceptions;
using YaeaY.Account.Domain.Enumerators;
using YaeaY.Account.Domain.Errors.UserPhones;
using YaeaY.Account.Domain.ValueObjects.Telephones;

namespace YaeaY.Account.Domain.Entities.UserPhones;

public sealed class UserPhone : Entity
{
    private TelephoneNumber? _telephoneNumber = null!;
    private string _callingCode = string.Empty;
    private string _regionCode = string.Empty;
    private string? _areaCode;
    private TelephoneType _phoneType;
    private string _phoneNumber = string.Empty;
    private string _e164 = string.Empty;
    private DateTimeOffset _createdAt;
    private bool _isPrimary;
    private bool _isVerified;
    private DateTimeOffset? _verifiedAt;    

    public TelephoneNumber TelephoneNumber => _telephoneNumber!;
    public string CallingCode => _callingCode;
    public string RegionCode => _regionCode;
    public string? AreaCode => _areaCode;
    public TelephoneType PhoneType => _phoneType;
    public string PhoneNumber => _phoneNumber;
    public string E164 => _e164;
    public DateTimeOffset CreatedAt => _createdAt;
    public bool IsPrimary => _isPrimary;
    public DateTimeOffset? VerifiedAt => _verifiedAt;
    public bool IsVerified => _isVerified;

    private UserPhone() { }

    private UserPhone(TelephoneNumber telephoneNumber, bool isPrimary)
    {
        _telephoneNumber = telephoneNumber;
        _callingCode = telephoneNumber.CallingCode;
        _regionCode = telephoneNumber.RegionCode;
        _areaCode = telephoneNumber.AreaCode;
        _phoneType = telephoneNumber.PhoneType;
        _phoneNumber = telephoneNumber.NationalNumber;
        _e164 = telephoneNumber.E164;
        _isPrimary = isPrimary;
        _createdAt = DateTimeOffset.UtcNow;
    }

    public static UserPhone Create(TelephoneNumber telephoneNumber, bool isPrimary)
    {
        Validate(telephoneNumber);

        var userPhone = new UserPhone(telephoneNumber, isPrimary);

        return userPhone;
    }

    internal void Update(TelephoneNumber number)
    {
        Validate(number);

        if (TelephoneNumber == number)
            return;

        _telephoneNumber = number;
        _callingCode = number.CallingCode;
        _regionCode = number.RegionCode;
        _areaCode = number.AreaCode;
        _phoneType = number.PhoneType;
        _phoneNumber = number.NationalNumber;
        _e164 = number.E164;
        _isVerified = false;
        _verifiedAt = null;
    }

    private static void Validate(TelephoneNumber telehpneNumber)
    {
        if (telehpneNumber is null)
            throw new DomainException(UserPhoneErrors.NumberRequired);
    }

    internal void SetPrimary(bool isPrimary)
    {
        if (_isPrimary == isPrimary)
            return;

        _isPrimary = isPrimary;
    }
}

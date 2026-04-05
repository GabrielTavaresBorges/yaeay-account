using YaeaY.Account.Domain.Abstraction.Entities;
using YaeaY.Account.Domain.Abstraction.Exceptions;
using YaeaY.Account.Domain.Enumerators;

namespace YaeaY.Account.Domain.Entities.UserPhones;

public sealed class UserPhone : Entity
{
    private string _callingCode = string.Empty;
    private string _regionCode = string.Empty;
    private string? _areaCode = string.Empty;
    private PhoneType _phoneType;
    private string _phoneNumber = string.Empty;
    private string _e164 = string.Empty;
    private bool _isVerified;
    private DateTimeOffset? _verifiedAt;
    private bool _isPrimary;

    public string CallingCode => _callingCode;
    public string RegionCode => _regionCode;
    public string? AreaCode => _areaCode;
    public PhoneType PhoneType => _phoneType;
    public string PhoneNumber => _phoneNumber;
    public string E164 => _e164;
    public DateTimeOffset? VerifiedAt => _verifiedAt;
    public bool IsVerified => _isVerified;
    public bool IsPrimary => _isPrimary;
    public DateTimeOffset CreatedAt { get; private set; }

    private UserPhone() { }

    private UserPhone(
        string callingCode,
        string regionCode,
        string? areaCode,
        PhoneType phoneType,
        string phoneNumber,
        string e164,
        bool isPrimary)
    {
        _callingCode = callingCode;
        _regionCode = regionCode;
        _areaCode = areaCode;
        _phoneType = phoneType;
        _phoneNumber = phoneNumber;
        _e164 = e164;
        _isPrimary = isPrimary;

        CreatedAt = DateTimeOffset.UtcNow;
    }

    public static UserPhone Create(
        string callingCode,
        string regionCode,
        string? areaCode,
        PhoneType phoneType,
        string phoneNumber,
        string e164,
        bool isPrimary)
    {
        var normalized = NormalizeParameters(callingCode, regionCode, areaCode, phoneNumber, e164);

        Validate(
            normalized.CallingCode,
            normalized.RegionCode,
            normalized.AreaCode,
            phoneType,
            normalized.PhoneNumber,
            normalized.E164,
            isPrimary);

        var userPhone = new UserPhone(
            normalized.CallingCode,
            normalized.RegionCode,
            normalized.AreaCode,
            phoneType,
            normalized.PhoneNumber,
            normalized.E164,
            isPrimary);

        return userPhone;
    }

    internal void Update(
         string callingCode,
         string regionCode,
         string? areaCode,
         PhoneType phoneType,
         string phoneNumber,
         string e164)
    {
        Validate(callingCode, regionCode, areaCode, phoneType, phoneNumber, e164, isPrimary: _isPrimary);

        var changed =
            _callingCode != callingCode ||
            _regionCode != regionCode ||
            _areaCode != areaCode ||
            _phoneType != phoneType ||
            _phoneNumber != phoneNumber ||
            _e164 != e164;

        if (!changed)
            return;

        _callingCode = callingCode;
        _regionCode = regionCode;
        _areaCode = areaCode;
        _phoneType = phoneType;
        _phoneNumber = phoneNumber;
        _e164 = e164;

        // mudou telefone => invalida verificação
        _isVerified = false;
        _verifiedAt = null;
    }

    private static (
        string CallingCode,
        string RegionCode,
        string? AreaCode,
        string PhoneNumber,
        string E164)
        NormalizeParameters(string callingCode, string regionCode, string? areaCode, string phoneNumber, string e164)
    {
        return (
            CallingCode: (callingCode ?? string.Empty).Trim(),
            RegionCode: (regionCode ?? string.Empty).Trim().ToUpperInvariant(),
            AreaCode: string.IsNullOrWhiteSpace(areaCode) ? null : areaCode.Trim(),
            PhoneNumber: (phoneNumber ?? string.Empty).Trim(),
            E164: (e164 ?? string.Empty).Trim()
        );
    }

    private static void Validate(
        string callingCode,
        string regionCode,
        string? areaCode,
        PhoneType phoneType,
        string phoneNumber,
        string e164,
        bool isPrimary)
    {
        if (string.IsNullOrWhiteSpace(callingCode))
            throw new DomainException(
                identifier: "PHONE_CALLING_CODE_NULL_EMPTY_WHITE_SPACE",
                message: "CallingCode cannot be null, empty or white space.");

        if (!callingCode.StartsWith("+") || callingCode.Length < 2 || callingCode.Skip(1).Any(ch => !char.IsDigit(ch)))
            throw new DomainException(
                identifier: "PHONE_CALLING_CODE_INVALID",
                message: "CallingCode must be in format +<digits> (e.g., +55, +1).");

        if (string.IsNullOrWhiteSpace(regionCode))
            throw new DomainException(
                identifier: "PHONE_REGION_CODE_NULL_EMPTY_WHITE_SPACE",
                message: "RegionCode cannot be null, empty or white space.");

        // ISO2 básico: 2 letras (BR/US/CA)
        if (regionCode.Length != 2 || regionCode.Any(ch => ch < 'A' || ch > 'Z'))
            throw new DomainException(
                identifier: "PHONE_REGION_CODE_INVALID",
                message: "RegionCode must be a valid ISO2 code (e.g., BR, US, CA).");

        // AreaCode é opcional (nullable). Se vier preenchido, valida básico: só dígitos.
        if (areaCode is not null && areaCode.Any(ch => !char.IsDigit(ch)))
            throw new DomainException(
                identifier: "PHONE_AREA_CODE_INVALID",
                message: "AreaCode must contain digits only.");

        if (phoneType == PhoneType.Unknown)
            throw new DomainException(
                identifier: "PHONE_TYPE_UNKNOWN",
                message: "Phone type cannot be unknown.");

        if (!Enum.IsDefined(typeof(PhoneType), phoneType))
            throw new DomainException(
                identifier: "PHONE_TYPE_INVALID",
                message: "Phone type is invalid.");

        if (string.IsNullOrWhiteSpace(phoneNumber))
            throw new DomainException(
                identifier: "PHONE_NUMBER_NULL_EMPTY_WHITE_SPACE",
                message: "Phone number cannot be null, empty or white space.");

        if (phoneNumber.Any(ch => !char.IsDigit(ch)))
            throw new DomainException(
                identifier: "PHONE_NUMBER_INVALID",
                message: "Phone number must contain digits only.");

        if (string.IsNullOrWhiteSpace(e164))
            throw new DomainException(
                identifier: "PHONE_E164_NULL_EMPTY_WHITE_SPACE",
                message: "E164 cannot be null, empty or white space.");

        // E.164 básico: '+' seguido de dígitos (validação oficial fica na libphonenumber)
        if (!e164.StartsWith("+") || e164.Length < 2 || e164.Skip(1).Any(ch => !char.IsDigit(ch)))
            throw new DomainException(
                identifier: "PHONE_E164_INVALID",
                message: "E164 must be in format +<digits>.");
    }

    internal void MarkVerified(DateTimeOffset verifiedAtUtc)
    {
        if (_isVerified)
            return;

        if (verifiedAtUtc == default)
            throw new DomainException(
                identifier: "PHONE_VERIFIED_AT_INVALID",
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

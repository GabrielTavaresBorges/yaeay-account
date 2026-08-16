using YaeaY.Account.Domain.ValueObjects.Emails;

namespace YaeaY.Account.Application.Services.Emails;

public sealed class EmailAddressMasker
{
    private const int VisibleLocalPartCharacters = 2;
    private const int FixedMaskLength = 6;

    public string Mask(Email email)
    {
        ArgumentNullException.ThrowIfNull(email);

        var emailAddress = email.EmailAddress;
        var separatorIndex = emailAddress.LastIndexOf('@');
        var localPart = emailAddress[..separatorIndex];
        var domain = emailAddress[separatorIndex..];
        var visibleLength = Math.Min(VisibleLocalPartCharacters, localPart.Length);

        return string.Concat(
            localPart.AsSpan(0, visibleLength),
            new string('*', FixedMaskLength),
            domain);
    }
}

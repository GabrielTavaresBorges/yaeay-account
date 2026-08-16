namespace YaeaY.Account.Application.Services.Emails.Interfaces;

public interface IEmailConfirmationLinkBuilder
{
    string Build(string rawToken);
}

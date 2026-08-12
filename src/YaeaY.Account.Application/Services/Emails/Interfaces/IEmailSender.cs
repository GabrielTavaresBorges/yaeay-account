using YaeaY.Account.Application.Services.Emails.Models;

namespace YaeaY.Account.Application.Services.Emails.Interfaces;

public interface IEmailSender
{
    Task SendAsync(EmailMessage message, CancellationToken cancellationToken = default);
}

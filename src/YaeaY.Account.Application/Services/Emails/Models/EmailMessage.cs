namespace YaeaY.Account.Application.Services.Emails.Models;

public sealed record EmailMessage(
    string FromEmail,
    string FromName,
    string ToEmail,
    string Subject,
    string BodyHtml);

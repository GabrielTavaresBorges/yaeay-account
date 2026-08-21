using MediatR;
using YaeaY.Account.Application.Events.Notifications;
using YaeaY.Account.Application.Services.Emails;
using YaeaY.Account.Application.Services.Emails.Interfaces;
using YaeaY.Account.Application.Services.Emails.Models;
using YaeaY.Account.Domain.Enumerators;
using YaeaY.Account.Domain.Events.PasswordRecoveries;
using YaeaY.Account.Domain.Repositories.PasswordRecoveryTemplates;
using IssueCode = YaeaY.Account.Application.UseCases.PasswordRecoveries.Commands.IssuePasswordRecoveryCode;

namespace YaeaY.Account.Application.UseCases.PasswordRecoveries.EventHandlers;

public sealed class PasswordRecoveryRequestedDomainEventHandler(
    ISender sender,
    IPasswordRecoveryTemplateRepository templateRepository,
    PasswordRecoveryMessageComposer composer,
    IEmailSender emailSender)
    : INotificationHandler<DomainEventNotification<PasswordRecoveryRequestedDomainEvent>>
{
    public async Task Handle(DomainEventNotification<PasswordRecoveryRequestedDomainEvent> notification, CancellationToken cancellationToken)
    {
        var issueResult = await sender.Send(new IssueCode.Command(notification.DomainEvent.ChallengeId), cancellationToken);
        if (issueResult.IsFailure)
            throw new InvalidOperationException($"Password recovery code issuance failed. Error code: '{issueResult.Error.Code}'.");
        if (!issueResult.Value.ShouldDeliver)
            return;

        var template = await templateRepository.GetActiveAsync(PasswordRecoveryTemplatePurpose.RecoveryCode, cancellationToken)
            ?? throw new InvalidOperationException("No active password recovery code template is configured.");

        var issued = issueResult.Value;
        var context = new PasswordRecoveryMessageContext(issued.ToEmail, issued.FullName, issued.RevealRawCode(), null);
        var messageResult = composer.Compose(template, context);
        if (messageResult.IsFailure)
            throw new InvalidOperationException($"Password recovery message composition failed. Error code: '{messageResult.Error.Code}'.");

        await emailSender.SendAsync(messageResult.Value, cancellationToken);
    }
}

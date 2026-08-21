using MediatR;
using YaeaY.Account.Application.Events.Notifications;
using YaeaY.Account.Application.Services.Emails;
using YaeaY.Account.Application.Services.Emails.Interfaces;
using YaeaY.Account.Application.Services.Emails.Models;
using YaeaY.Account.Domain.Enumerators;
using YaeaY.Account.Domain.Events.PasswordRecoveries;
using YaeaY.Account.Domain.Repositories.PasswordRecoveryChallenges;
using YaeaY.Account.Domain.Repositories.PasswordRecoveryTemplates;
using YaeaY.Account.Domain.Repositories.Users;

namespace YaeaY.Account.Application.UseCases.PasswordRecoveries.EventHandlers;

public sealed class PasswordRecoveryCompletedDomainEventHandler(
    IPasswordRecoveryChallengeRepository challengeRepository,
    IUserRepository userRepository,
    IPasswordRecoveryTemplateRepository templateRepository,
    PasswordRecoveryMessageComposer composer,
    IEmailSender emailSender)
    : INotificationHandler<DomainEventNotification<PasswordRecoveryCompletedDomainEvent>>
{
    public async Task Handle(DomainEventNotification<PasswordRecoveryCompletedDomainEvent> notification, CancellationToken cancellationToken)
    {
        var challenge = await challengeRepository.GetByIdAsync(notification.DomainEvent.ChallengeId, cancellationToken)
            ?? throw new InvalidOperationException("Password recovery challenge was not found for completion notification.");
        var user = await userRepository.GetByIdAsync(challenge.UserId, cancellationToken)
            ?? throw new InvalidOperationException("Password recovery user was not found for completion notification.");
        var template = await templateRepository.GetActiveAsync(PasswordRecoveryTemplatePurpose.PasswordChanged, cancellationToken)
            ?? throw new InvalidOperationException("No active password changed template is configured.");

        var context = new PasswordRecoveryMessageContext(
            user.Email.EmailAddress, user.FullName.Name, null, challenge.ConsumedAt);
        var messageResult = composer.Compose(template, context);
        if (messageResult.IsFailure)
            throw new InvalidOperationException($"Password changed message composition failed. Error code: '{messageResult.Error.Code}'.");

        await emailSender.SendAsync(messageResult.Value, cancellationToken);
    }
}

using MediatR;
using YaeaY.Account.Application.Events.Notifications;
using YaeaY.Account.Application.Services.Emails;
using YaeaY.Account.Application.Services.Emails.Interfaces;
using YaeaY.Account.Application.Services.Emails.Models;
using YaeaY.Account.Domain.Events.Users;
using YaeaY.Account.Domain.Repositories.EmailConfirmationTemplates;
using IssueInitialToken = YaeaY.Account.Application.UseCases.EmailConfirmations.Commands.IssueInitialToken;

namespace YaeaY.Account.Application.UseCases.EmailConfirmations.EventHandlers;

public sealed class UserRegisteredDomainEventHandler
    : INotificationHandler<DomainEventNotification<UserRegisteredDomainEvent>>
{
    private readonly ISender _sender;
    private readonly IEmailConfirmationTemplateRepository _templateRepository;
    private readonly EmailConfirmationMessageComposer _messageComposer;
    private readonly IEmailSender _emailSender;

    public UserRegisteredDomainEventHandler(
        ISender sender,
        IEmailConfirmationTemplateRepository templateRepository,
        EmailConfirmationMessageComposer messageComposer,
        IEmailSender emailSender)
    {
        _sender = sender;
        _templateRepository = templateRepository;
        _messageComposer = messageComposer;
        _emailSender = emailSender;
    }

    public async Task Handle(
        DomainEventNotification<UserRegisteredDomainEvent> notification,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(notification);

        var tokenResult = await _sender.Send(
            new IssueInitialToken.Command(notification.DomainEvent),
            cancellationToken);

        if (tokenResult.IsFailure)
        {
            throw CreateProcessingException(
                stage: "token issuance",
                errorCode: tokenResult.Error.Code);
        }

        var template = await _templateRepository.GetActiveTemplateAsync(cancellationToken);
        if (template is null)
        {
            throw new InvalidOperationException(
                "Email confirmation delivery failed because no active template is configured.");
        }

        var token = tokenResult.Value;
        var messageContext = new EmailConfirmationMessageContext(
            toEmail: token.ToEmail,
            fullName: token.FullName,
            rawToken: token.RevealRawToken());

        var messageResult = _messageComposer.Compose(template, messageContext);
        if (messageResult.IsFailure)
        {
            throw CreateProcessingException(
                stage: "message composition",
                errorCode: messageResult.Error.Code);
        }

        await _emailSender.SendAsync(messageResult.Value, cancellationToken);
    }

    private static InvalidOperationException CreateProcessingException(
        string stage,
        string errorCode)
        => new(
            $"Email confirmation delivery failed during {stage}. " +
            $"Error code: '{errorCode}'.");
}

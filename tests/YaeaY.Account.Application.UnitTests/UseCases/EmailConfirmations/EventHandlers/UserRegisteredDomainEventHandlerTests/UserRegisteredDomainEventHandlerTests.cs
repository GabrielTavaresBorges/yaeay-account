using System.Runtime.CompilerServices;
using FluentAssertions;
using MediatR;
using YaeaY.Account.Application.Events.Notifications;
using YaeaY.Account.Application.Services.Emails;
using YaeaY.Account.Application.Services.Emails.Interfaces;
using YaeaY.Account.Application.Services.Emails.Models;
using YaeaY.Account.Application.UseCases.EmailConfirmations.EventHandlers;
using YaeaY.Account.Domain.Abstraction.Result;
using YaeaY.Account.Domain.Entities.AggregateRoots.EmailConfirmationTemplates;
using YaeaY.Account.Domain.Errors.EmailConfirmationTokens;
using YaeaY.Account.Domain.Events.Users;
using YaeaY.Account.Domain.Repositories.EmailConfirmationTemplates;
using YaeaY.Account.Domain.ValueObjects.Emails;
using IssueInitialToken = YaeaY.Account.Application.UseCases.EmailConfirmations.Commands.IssueInitialToken;

namespace YaeaY.Account.Application.UnitTests.UseCases.EmailConfirmations.EventHandlers.UserRegisteredDomainEventHandlerTests;

public sealed class UserRegisteredDomainEventHandlerTests
{
    private const string RawToken = "raw-token-only-in-memory";

    [Fact]
    public async Task Handle_WhenDependenciesSucceed_ShouldSendComposedEmail()
    {
        // Arrange

        var emailSender = new RecordingEmailSender();
        var handler = CreateHandler(
            tokenResult: CreateSuccessfulTokenResult(),
            template: CreateTemplate(),
            emailSender: emailSender);
        var notification = CreateNotification();

        // Act

        await handler.Handle(notification, CancellationToken.None);

        // Assert

        emailSender.Message.Should().NotBeNull();
        emailSender.Message!.ToEmail.Should().Be("current@example.com");
        emailSender.Message.GetBodyHtml().Should().Contain(
            $"https://account.example.com/confirm-email#token={RawToken}");
        emailSender.Message.GetBodyHtml().Should().Contain("Current Person");
    }

    [Fact]
    public async Task Handle_WhenTokenIssuanceFails_ShouldPropagateSafeFailure()
    {
        // Arrange

        var emailSender = new RecordingEmailSender();
        var handler = CreateHandler(
            tokenResult: Result<IssueInitialToken.Response>.Failure(
                EmailConfirmationTokenErrors.InitialStageExpired),
            template: CreateTemplate(),
            emailSender: emailSender);
        var notification = CreateNotification();

        // Act

        Func<Task> act = () => handler.Handle(notification, CancellationToken.None);

        // Assert

        var exception = await act.Should().ThrowAsync<InvalidOperationException>();
        exception.Which.Message.Should().Contain(EmailConfirmationTokenErrors.InitialStageExpired.Code);
        exception.Which.Message.Should().NotContain(RawToken);
        emailSender.Message.Should().BeNull();
    }

    [Fact]
    public async Task Handle_WhenActiveTemplateDoesNotExist_ShouldNotSendEmailOrRevealToken()
    {
        // Arrange

        var emailSender = new RecordingEmailSender();
        var handler = CreateHandler(
            tokenResult: CreateSuccessfulTokenResult(),
            template: null,
            emailSender: emailSender);
        var notification = CreateNotification();

        // Act

        Func<Task> act = () => handler.Handle(notification, CancellationToken.None);

        // Assert

        var exception = await act.Should().ThrowAsync<InvalidOperationException>();
        exception.Which.Message.Should().Contain("no active template");
        exception.Which.Message.Should().NotContain(RawToken);
        emailSender.Message.Should().BeNull();
    }

    [Fact]
    public async Task Handle_WhenEmailSenderFails_ShouldPropagateFailureForOutboxRetry()
    {
        // Arrange

        var emailSender = new FailingEmailSender();
        var handler = CreateHandler(
            tokenResult: CreateSuccessfulTokenResult(),
            template: CreateTemplate(),
            emailSender: emailSender);
        var notification = CreateNotification();

        // Act

        Func<Task> act = () => handler.Handle(notification, CancellationToken.None);

        // Assert

        var exception = await act.Should().ThrowAsync<InvalidOperationException>();
        exception.Which.Message.Should().Be("Simulated delivery failure.");
        exception.Which.Message.Should().NotContain(RawToken);
    }

    private static UserRegisteredDomainEventHandler CreateHandler(
        Result<IssueInitialToken.Response> tokenResult,
        EmailConfirmationTemplate? template,
        IEmailSender emailSender)
        => new(
            sender: new StubSender(tokenResult),
            templateRepository: new StubTemplateRepository(template),
            messageComposer: new EmailConfirmationMessageComposer(
                new StubEmailConfirmationLinkBuilder()),
            emailSender: emailSender);

    private static Result<IssueInitialToken.Response> CreateSuccessfulTokenResult()
        => Result<IssueInitialToken.Response>.Success(
            new IssueInitialToken.Response(
                tokenId: Guid.NewGuid(),
                toEmail: "current@example.com",
                fullName: "Current Person",
                rawToken: RawToken,
                expiresAt: new DateTimeOffset(2026, 11, 16, 12, 0, 0, TimeSpan.Zero)));

    private static EmailConfirmationTemplate CreateTemplate()
    {
        var fromEmail = Email.Create("account@yaeay.com");

        return EmailConfirmationTemplate.Create(
            fromEmail: fromEmail.Value,
            fromName: "YaeaY Account",
            subject: "Confirm your account",
            bodyHtml: "<p>{{FullName}}</p><a href=\"{{ConfirmationUrl}}\">Confirm</a>",
            isActive: true);
    }

    private static DomainEventNotification<UserRegisteredDomainEvent> CreateNotification()
        => new(
            new UserRegisteredDomainEvent(
                UserId: Guid.NewGuid(),
                Email: "original@example.com",
                FullName: "Original Person"));

    private sealed class StubSender : ISender
    {
        private readonly Result<IssueInitialToken.Response> _result;

        public StubSender(Result<IssueInitialToken.Response> result)
        {
            _result = result;
        }

        public Task<TResponse> Send<TResponse>(
            IRequest<TResponse> request,
            CancellationToken cancellationToken = default)
        {
            if (request is IssueInitialToken.Command &&
                _result is TResponse typedResult)
            {
                return Task.FromResult(typedResult);
            }

            throw new InvalidOperationException("Unexpected request.");
        }

        public Task Send<TRequest>(
            TRequest request,
            CancellationToken cancellationToken = default)
            where TRequest : IRequest
            => throw new NotSupportedException();

        public Task<object?> Send(
            object request,
            CancellationToken cancellationToken = default)
            => throw new NotSupportedException();

        public IAsyncEnumerable<TResponse> CreateStream<TResponse>(
            IStreamRequest<TResponse> request,
            CancellationToken cancellationToken = default)
            => EmptyStream<TResponse>(cancellationToken);

        public IAsyncEnumerable<object?> CreateStream(
            object request,
            CancellationToken cancellationToken = default)
            => EmptyStream<object?>(cancellationToken);

        private static async IAsyncEnumerable<T> EmptyStream<T>(
            [EnumeratorCancellation] CancellationToken cancellationToken)
        {
            await Task.CompletedTask;
            cancellationToken.ThrowIfCancellationRequested();
            yield break;
        }
    }

    private sealed class StubTemplateRepository : IEmailConfirmationTemplateRepository
    {
        private readonly EmailConfirmationTemplate? _template;

        public StubTemplateRepository(EmailConfirmationTemplate? template)
        {
            _template = template;
        }

        public Task CreateEmailConfirmationTemplateAsync(
            EmailConfirmationTemplate emailConfirmationTemplate,
            CancellationToken cancellationToken)
            => throw new NotSupportedException();

        public Task<EmailConfirmationTemplate?> GetActiveTemplateAsync(
            CancellationToken cancellationToken)
            => Task.FromResult(_template);
    }

    private sealed class RecordingEmailSender : IEmailSender
    {
        public EmailMessage? Message { get; private set; }

        public Task SendAsync(
            EmailMessage message,
            CancellationToken cancellationToken = default)
        {
            Message = message;
            return Task.CompletedTask;
        }
    }

    private sealed class StubEmailConfirmationLinkBuilder
        : IEmailConfirmationLinkBuilder
    {
        public string Build(string rawToken)
            => $"https://account.example.com/confirm-email#token={Uri.EscapeDataString(rawToken)}";
    }

    private sealed class FailingEmailSender : IEmailSender
    {
        public Task SendAsync(
            EmailMessage message,
            CancellationToken cancellationToken = default)
            => throw new InvalidOperationException("Simulated delivery failure.");
    }
}

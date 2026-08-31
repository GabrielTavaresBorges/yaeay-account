using MediatR;
using YaeaY.Account.Domain.Abstraction.Exceptions;
using YaeaY.Account.Domain.Abstraction.Result;
using YaeaY.Account.Domain.Abstraction.Errors.Enumerators;
using YaeaY.Account.Domain.Abstraction.Interfaces;
using YaeaY.Account.Domain.Entities.AggregateRoots.Administration;
using YaeaY.Account.Domain.Repositories.Administration;
using YaeaY.Account.Domain.Repositories.EmailConfirmationTemplates;
namespace YaeaY.Account.Application.UseCases.Administration.Commands.UpdateEmailConfirmationTemplate;
public sealed class Handler(IEmailConfirmationTemplateRepository templates, IAdministrationAuditRepository audit, IUnitOfWork unitOfWork, TimeProvider timeProvider) : IRequestHandler<Command, Result<Response>>
{ public async Task<Result<Response>> Handle(Command command, CancellationToken cancellationToken) { var template = await templates.GetActiveTemplateAsync(cancellationToken); if (template is null) return Result<Response>.Failure(new("administration.template.not-found", "Template ativo não encontrado.", ErrorCategory.NotFound, ErrorRule.NotFound)); try { template.UpdateContent(command.Subject, command.BodyHtml); await audit.AddAsync(AdministrationAuditEntry.Create(command.AdministratorId, null, "EmailConfirmationTemplate.Updated", command.Justification, timeProvider.GetUtcNow()), cancellationToken); await unitOfWork.CommitAsync(cancellationToken); return Result<Response>.Success(new(template.UpdatedAt)); } catch (DomainException exception) { return Result<Response>.Failure(exception.Error); } } }

using MediatR;
using YaeaY.Account.Application.Services.Administration.Interfaces;

namespace YaeaY.Account.Application.UseCases.Administration.Queries.GetAdministrationConfiguration;
public sealed record Query : IRequest<Response>;
public sealed record Response(EmailConfirmationTemplateSettings? EmailConfirmationTemplate, IReadOnlyList<IdentityRoleSummary> Roles);

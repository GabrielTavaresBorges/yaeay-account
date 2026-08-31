using MediatR;
using YaeaY.Account.Application.Services.Administration.Interfaces;
namespace YaeaY.Account.Application.UseCases.Administration.Queries.GetAdministrationConfiguration;
public sealed class Handler(IAdministrationConfigurationService configuration) : IRequestHandler<Query, Response>
{ public async Task<Response> Handle(Query request, CancellationToken cancellationToken) => new(await configuration.GetEmailConfirmationTemplateAsync(cancellationToken), await configuration.GetRolesAsync(cancellationToken)); }

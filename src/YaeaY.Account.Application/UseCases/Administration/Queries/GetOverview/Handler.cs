using MediatR;
using YaeaY.Account.Application.Services.Administration.Interfaces;
namespace YaeaY.Account.Application.UseCases.Administration.Queries.GetOverview;
public sealed class Handler(IAdministrationReader reader) : IRequestHandler<Query, Overview> { public Task<Overview> Handle(Query request, CancellationToken cancellationToken) => reader.GetOverviewAsync(cancellationToken); }

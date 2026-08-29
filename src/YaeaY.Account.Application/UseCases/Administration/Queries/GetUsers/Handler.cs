using MediatR;
using YaeaY.Account.Application.Services.Administration.Interfaces;
namespace YaeaY.Account.Application.UseCases.Administration.Queries.GetUsers;
public sealed class Handler(IAdministrationReader reader) : IRequestHandler<Query, IReadOnlyList<UserSummary>> { public Task<IReadOnlyList<UserSummary>> Handle(Query request, CancellationToken cancellationToken) => reader.GetUsersAsync(cancellationToken); }

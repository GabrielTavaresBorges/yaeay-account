using MediatR;
using YaeaY.Account.Application.Services.Administration.Interfaces;
namespace YaeaY.Account.Application.UseCases.Administration.Queries.GetUsers;
public sealed record Query : IRequest<IReadOnlyList<UserSummary>>;

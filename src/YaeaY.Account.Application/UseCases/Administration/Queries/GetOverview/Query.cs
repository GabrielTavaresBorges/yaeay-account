using MediatR;
using YaeaY.Account.Application.Services.Administration.Interfaces;
namespace YaeaY.Account.Application.UseCases.Administration.Queries.GetOverview;
public sealed record Query : IRequest<Overview>;

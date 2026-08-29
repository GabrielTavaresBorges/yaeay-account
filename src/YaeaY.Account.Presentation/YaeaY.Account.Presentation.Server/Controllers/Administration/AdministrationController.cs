using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using GetAudit = YaeaY.Account.Application.UseCases.Administration.Queries.GetAudit;
using GetOverview = YaeaY.Account.Application.UseCases.Administration.Queries.GetOverview;
using GetUsers = YaeaY.Account.Application.UseCases.Administration.Queries.GetUsers;

namespace YaeaY.Account.Presentation.Server.Controllers.Administration;

[ApiController]
[Route("api/administration")]
[Authorize(Policy = "AccountAdministration")]
public sealed class AdministrationController(IMediator mediator) : ControllerBase
{
    [HttpGet("overview")]
    public Task<YaeaY.Account.Application.Services.Administration.Interfaces.Overview> GetOverview(CancellationToken cancellationToken)
        => mediator.Send(new GetOverview.Query(), cancellationToken);

    [HttpGet("users")]
    public Task<IReadOnlyList<YaeaY.Account.Application.Services.Administration.Interfaces.UserSummary>> GetUsers(CancellationToken cancellationToken)
        => mediator.Send(new GetUsers.Query(), cancellationToken);

    [HttpGet("audit")]
    public Task<IReadOnlyList<YaeaY.Account.Application.Services.Administration.Interfaces.AuditEntry>> GetAudit(CancellationToken cancellationToken)
        => mediator.Send(new GetAudit.Query(), cancellationToken);
}

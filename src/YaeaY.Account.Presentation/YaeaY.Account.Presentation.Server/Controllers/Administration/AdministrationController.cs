using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using YaeaY.Account.Presentation.Server.Contracts.Administration;
using CreateIdentityRole = YaeaY.Account.Application.UseCases.Administration.Commands.CreateIdentityRole;
using GetConfiguration = YaeaY.Account.Application.UseCases.Administration.Queries.GetAdministrationConfiguration;
using UpdateTemplate = YaeaY.Account.Application.UseCases.Administration.Commands.UpdateEmailConfirmationTemplate;
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

    [HttpGet("configuration")]
    public Task<GetConfiguration.Response> GetConfiguration(CancellationToken cancellationToken) => mediator.Send(new GetConfiguration.Query(), cancellationToken);

    [HttpPut("email-confirmation-template")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateEmailConfirmationTemplate(UpdateEmailConfirmationTemplateRequest request, CancellationToken cancellationToken)
    {
        if (!Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var administratorId)) return Unauthorized();
        var result = await mediator.Send(new UpdateTemplate.Command(administratorId, request.Subject, request.BodyHtml, request.Justification), cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : UnprocessableEntity(new { code = result.Error.Code, message = result.Error.Message });
    }

    [HttpPost("roles")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateRole(CreateIdentityRoleRequest request, CancellationToken cancellationToken)
    {
        if (!Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var administratorId)) return Unauthorized();
        var result = await mediator.Send(new CreateIdentityRole.Command(administratorId, request.Name, request.Justification), cancellationToken);
        return result.IsSuccess ? Created(string.Empty, result.Value) : UnprocessableEntity(new { code = result.Error.Code, message = result.Error.Message });
    }
}

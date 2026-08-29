using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using YaeaY.Account.Presentation.Server.Contracts.Administration;
using ChangeUserStatus = YaeaY.Account.Application.UseCases.Administration.Commands.ChangeUserStatus;

namespace YaeaY.Account.Presentation.Server.Controllers.Administration;

[ApiController]
[Route("api/administration/users")]
[Authorize(Policy = "AccountAdministration")]
public sealed class AdministrationUsersController(IMediator mediator) : ControllerBase
{
    [HttpPut("{userId:guid}/status")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ChangeStatus(Guid userId, ChangeUserStatusRequest request, CancellationToken cancellationToken)
    {
        if (!Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var administratorId))
            return Unauthorized();
        var result = await mediator.Send(new ChangeUserStatus.Command(administratorId, userId, request.Status, request.SuspensionReason, request.SuspendedUntilUtc, request.Justification), cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : UnprocessableEntity(new { code = result.Error.Code, message = result.Error.Message });
    }
}

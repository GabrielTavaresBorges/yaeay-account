using MediatR;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using YaeaY.Account.Domain.Abstraction.Errors;
using YaeaY.Account.Domain.Abstraction.Errors.Enumerators;
using Login = YaeaY.Account.Application.UseCases.Authentication.Commands.Login;
using Logout = YaeaY.Account.Application.UseCases.Authentication.Commands.Logout;
using GetCurrentSession = YaeaY.Account.Application.UseCases.Authentication.Queries.GetCurrentSession;

namespace YaeaY.Account.Presentation.Server.Controllers.Authentication;

[ApiController]
[Route("api/authentication")]
public sealed class AuthenticationController(IMediator mediator, IAntiforgery antiforgery)
    : ControllerBase
{
    [AllowAnonymous]
    [HttpGet("antiforgery-token")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult GetAntiforgeryToken()
    {
        var tokens = antiforgery.GetAndStoreTokens(HttpContext);
        return Ok(new { token = tokens.RequestToken });
    }

    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    [HttpPost("login")]
    [ProducesResponseType(typeof(Login.Response), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> Login(
        [FromBody] Login.Command command,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(command, cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : ToAuthenticationError(result.Error);
    }

    [Authorize]
    [HttpGet("session")]
    [ProducesResponseType(typeof(GetCurrentSession.Response), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetCurrentSession(CancellationToken cancellationToken)
    {
        var userIdValue = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(userIdValue, out var userId))
            return Unauthorized();

        var result = await mediator.Send(
            new GetCurrentSession.Query(userId, User.IsInRole("Admin")),
            cancellationToken);

        return result.IsSuccess ? Ok(result.Value) : Unauthorized();
    }

    [Authorize]
    [ValidateAntiForgeryToken]
    [HttpPost("logout")]
    [ProducesResponseType(typeof(Logout.Response), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Logout(CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new Logout.Command(), cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : ToAuthenticationError(result.Error);
    }

    private IActionResult ToAuthenticationError(Error error)
    {
        var response = new
        {
            code = error.Code,
            message = error.Message,
            category = error.Category.ToString(),
            rule = error.Rule.ToString()
        };

        return error.Code switch
        {
            "identity.credentials.invalid" => Unauthorized(response),
            "identity.account.locked-out" => StatusCode(StatusCodes.Status423Locked, response),
            "user.login.email-confirmation-required" or
            "user.login.account-suspended" or
            "user.login.account-disabled" or
            "user.account.cannot-login" => StatusCode(StatusCodes.Status403Forbidden, response),
            _ when error.Category == ErrorCategory.Validation => UnprocessableEntity(response),
            _ => StatusCode(StatusCodes.Status500InternalServerError, response)
        };
    }
}

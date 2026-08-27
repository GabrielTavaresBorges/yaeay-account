using System.Security.Cryptography;
using MediatR;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using YaeaY.Account.Domain.Abstraction.Errors;
using YaeaY.Account.Domain.Abstraction.Errors.Enumerators;
using YaeaY.Account.Domain.Policies.PasswordRecoveries;
using RequestRecovery = YaeaY.Account.Application.UseCases.PasswordRecoveries.Commands.RequestPasswordRecovery;
using VerifyCode = YaeaY.Account.Application.UseCases.PasswordRecoveries.Commands.VerifyPasswordRecoveryCode;
using ResetPassword = YaeaY.Account.Application.UseCases.PasswordRecoveries.Commands.ResetPassword;

namespace YaeaY.Account.Presentation.Server.Controllers.PasswordRecoveries;

[ApiController]
[AllowAnonymous]
[EnableRateLimiting("password-recovery")]
[Route("api/password-recoveries")]
public sealed class PasswordRecoveryController(
    IMediator mediator,
    IAntiforgery antiforgery,
    IDataProtectionProvider dataProtectionProvider,
    IPasswordRecoveryPolicy policy) : ControllerBase
{
    private const string AuthorizationCookieName = "__Host-YaeaY.PasswordRecovery";
    private readonly ITimeLimitedDataProtector _protector = dataProtectionProvider
        .CreateProtector("YaeaY.Account.PasswordRecovery.Authorization.v1")
        .ToTimeLimitedDataProtector();

    [HttpGet("antiforgery-token")]
    public IActionResult GetAntiforgeryToken()
    {
        var tokens = antiforgery.GetAndStoreTokens(HttpContext);
        return Ok(new { token = tokens.RequestToken });
    }

    [ValidateAntiForgeryToken]
    [HttpPost("request")]
    public async Task<IActionResult> RequestRecoveryCode([FromBody] RequestRecovery.Command command, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(command, cancellationToken);
        return result.IsSuccess ? Accepted(result.Value) : ToErrorResponse(result.Error);
    }

    [ValidateAntiForgeryToken]
    [HttpPost("verify")]
    public async Task<IActionResult> Verify([FromBody] VerifyCode.Command command, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(command, cancellationToken);
        if (result.IsFailure)
            return ToErrorResponse(result.Error);

        var protectedChallengeId = _protector.Protect(
            result.Value.ChallengeId.ToString("N"),
            policy.ResetAuthorizationLifetime);

        Response.Cookies.Append(AuthorizationCookieName, protectedChallengeId, new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Strict,
            Path = "/",
            MaxAge = policy.ResetAuthorizationLifetime,
            IsEssential = true
        });

        return Ok(new { verified = true });
    }

    [ValidateAntiForgeryToken]
    [HttpPost("reset")]
    public async Task<IActionResult> Reset([FromBody] ResetRequest request, CancellationToken cancellationToken)
    {
        if (!TryGetAuthorizedChallengeId(out var challengeId))
            return UnprocessableEntity(GenericInvalidAuthorizationResponse());

        var result = await mediator.Send(
            new ResetPassword.Command(challengeId, request.NewPassword, request.ConfirmPassword),
            cancellationToken);

        if (result.IsFailure)
            return ToErrorResponse(result.Error);

        Response.Cookies.Delete(AuthorizationCookieName, new CookieOptions
        {
            Secure = true,
            SameSite = SameSiteMode.Strict,
            Path = "/"
        });

        return Ok(result.Value);
    }

    private bool TryGetAuthorizedChallengeId(out Guid challengeId)
    {
        challengeId = Guid.Empty;
        if (!HttpContext.Request.Cookies.TryGetValue(AuthorizationCookieName, out var protectedValue))
            return false;

        try
        {
            var value = _protector.Unprotect(protectedValue, out _);
            return Guid.TryParseExact(value, "N", out challengeId);
        }
        catch (CryptographicException)
        {
            return false;
        }
    }

    private IActionResult ToErrorResponse(Error error)
    {
        if (error.Code.StartsWith("password-recovery-challenge.", StringComparison.Ordinal))
            return UnprocessableEntity(GenericInvalidAuthorizationResponse());

        var response = new { code = error.Code, message = error.Message, category = error.Category.ToString(), rule = error.Rule.ToString() };
        return error.Category switch
        {
            ErrorCategory.Validation or ErrorCategory.BusinessRule => UnprocessableEntity(response),
            ErrorCategory.Conflict => Conflict(response),
            _ => StatusCode(StatusCodes.Status500InternalServerError, response)
        };
    }

    private static object GenericInvalidAuthorizationResponse() => new
    {
        code = "password-recovery.invalid-or-expired",
        message = "A autorização para alterar a senha expirou ou não foi encontrada. Solicite um novo código.",
        category = ErrorCategory.BusinessRule.ToString(),
        rule = ErrorRule.InvariantViolation.ToString()
    };

    public sealed record ResetRequest(string NewPassword, string ConfirmPassword);
}

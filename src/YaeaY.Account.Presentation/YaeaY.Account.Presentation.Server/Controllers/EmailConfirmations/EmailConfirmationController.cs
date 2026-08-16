using MediatR;
using Microsoft.AspNetCore.Mvc;
using YaeaY.Account.Domain.Abstraction.Errors;
using YaeaY.Account.Domain.Abstraction.Errors.Enumerators;
using ConfirmEmail = YaeaY.Account.Application.UseCases.EmailConfirmations.Commands.ConfirmEmail;
using GetConfirmationPreview = YaeaY.Account.Application.UseCases.EmailConfirmations.Queries.GetConfirmationPreview;

namespace YaeaY.Account.Presentation.Server.Controllers.EmailConfirmations;

[ApiController]
[Route("api/email-confirmations")]
public sealed class EmailConfirmationController : ControllerBase
{
    private readonly IMediator _mediator;

    public EmailConfirmationController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost("confirm")]
    [ProducesResponseType(typeof(ConfirmEmail.Response), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Confirm(
        [FromBody] ConfirmEmail.Command command,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(command, cancellationToken);

        if (result.IsFailure)
            return ToErrorResponse(result.Error);

        return Ok(result.Value);
    }

    [HttpPost("preview")]
    [ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
    [ProducesResponseType(typeof(GetConfirmationPreview.Response), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Preview(
        [FromBody] GetConfirmationPreview.Query query,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(query, cancellationToken);

        if (result.IsFailure)
            return ToErrorResponse(result.Error);

        return Ok(result.Value);
    }

    private IActionResult ToErrorResponse(Error error)
    {
        var response = new
        {
            code = error.Code,
            message = error.Message,
            category = error.Category.ToString(),
            rule = error.Rule.ToString()
        };

        return error.Category switch
        {
            ErrorCategory.Validation or ErrorCategory.BusinessRule => UnprocessableEntity(response),
            ErrorCategory.Conflict => Conflict(response),
            ErrorCategory.NotFound => NotFound(response),
            ErrorCategory.Unexpected => StatusCode(StatusCodes.Status500InternalServerError, response),
            _ => StatusCode(StatusCodes.Status500InternalServerError, response)
        };
    }
}

using MediatR;
using Microsoft.AspNetCore.Mvc;
using YaeaY.Account.Domain.Abstraction.Errors;
using YaeaY.Account.Domain.Abstraction.Errors.Enumerators;
using CreateUser = YaeaY.Account.Application.UseCases.Users.Commands.Create;
using UpdateUser = YaeaY.Account.Application.UseCases.Users.Commands.Update;

namespace YaeaY.Account.Presentation.Server.Controllers.Users;

[ApiController]
[Route("api/[controller]")]
public class UserController : ControllerBase
{
    private readonly IMediator _mediator;

    public UserController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> CreateUser([FromBody] CreateUser.Command command, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(command, cancellationToken);

        if (result.IsFailure)
            return ToErrorResponse(result.Error);

        return CreatedAtAction(
            actionName: nameof(GetById),
            routeValues: new { id = result.Value.Id },
            value: result.Value
        );
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> UpdateUser(Guid id, [FromBody] UpdateUser.Command command, CancellationToken cancellationToken)
    {
        // garante que o Id vem do route (evita cliente mandar id diferente no body)
        command = command with { Id = id };

        var result = await _mediator.Send(command, cancellationToken);

        if (result.IsFailure)
            return ToErrorResponse(result.Error);

        // Pode ser Ok(result.Value) (200 com body) ou NoContent() (204 sem body).
        return Ok(result.Value);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult GetById(Guid id) => Ok();

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

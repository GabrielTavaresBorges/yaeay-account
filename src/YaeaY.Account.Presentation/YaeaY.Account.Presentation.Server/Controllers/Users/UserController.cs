using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using YaeaY.Account.Domain.Abstraction.Errors;
using YaeaY.Account.Domain.Abstraction.Errors.Enumerators;
using YaeaY.Account.Presentation.Server.Contracts.Users;
using CreateUser = YaeaY.Account.Application.UseCases.Users.Commands.Create;
using UpdateUser = YaeaY.Account.Application.UseCases.Users.Commands.Update;
using GetMyData = YaeaY.Account.Application.UseCases.Users.Queries.GetMyData;
using GetDocumentImage = YaeaY.Account.Application.UseCases.Users.Queries.GetDocumentImage;
using UploadCpfDocumentImage = YaeaY.Account.Application.UseCases.Users.Commands.UploadCpfDocumentImage;

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

    [Authorize]
    [ValidateAntiForgeryToken]
    [HttpPut]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> UpdateUser([FromBody] UpdateUserRequest request, CancellationToken cancellationToken)
    {
        var userIdValue = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(userIdValue, out var userId))
            return Unauthorized();

        var command = new UpdateUser.Command(
            userId,
            request.FullName,
            request.BirthDate,
            request.Gender,
            request.Phones?.Select(phone => new UpdateUser.PhoneInput(
                phone.Id,
                phone.CallingCode,
                phone.RegionCode,
                phone.AreaCode,
                phone.PhoneType,
                phone.PhoneNumber,
                phone.IsPrimary)).ToArray(),
            request.CpfDocumentsToAdd?.Select(document => new UpdateUser.CpfDocumentInput(
                document.Number,
                document.Images?.Select(image => new UpdateUser.DocumentImageInput(
                    image.Position,
                    image.StorageObjectKey,
                    image.OriginalFileName,
                    image.ContentType,
                    image.FileSizeBytes,
                    image.Sha256Hash)).ToArray())).ToArray());

        var result = await _mediator.Send(command, cancellationToken);

        if (result.IsFailure)
            return ToErrorResponse(result.Error);

        return Ok(result.Value);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult GetById(Guid id) => Ok();

    [Authorize]
    [HttpGet("me")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetMyData(CancellationToken cancellationToken)
    {
        var userIdValue = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(userIdValue, out var userId))
            return Unauthorized();

        var result = await _mediator.Send(new GetMyData.Query(userId), cancellationToken);
        return result.IsFailure ? ToErrorResponse(result.Error) : Ok(result.Value);
    }

    [Authorize]
    [ValidateAntiForgeryToken]
    [HttpPost("documents/cpf/images")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> UploadCpfDocumentImage(IFormFile? image, CancellationToken cancellationToken)
    {
        var userIdValue = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(userIdValue, out var userId))
            return Unauthorized();
        if (image is null)
            return UnprocessableEntity(new { code = "document_image.required", message = "Selecione uma imagem para enviar." });

        await using var content = image.OpenReadStream();
        var result = await _mediator.Send(new UploadCpfDocumentImage.Command(
            userId, content, image.FileName, image.ContentType, image.Length), cancellationToken);
        return result.IsFailure ? ToErrorResponse(result.Error) : Ok(result.Value);
    }

    [Authorize]
    [HttpGet("documents/images/{imageId:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetDocumentImage(Guid imageId, CancellationToken cancellationToken)
    {
        var userIdValue = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(userIdValue, out var userId))
            return Unauthorized();

        var result = await _mediator.Send(new GetDocumentImage.Query(userId, imageId), cancellationToken);
        return result.IsFailure
            ? ToErrorResponse(result.Error)
            : File(result.Value.Content, result.Value.ContentType, result.Value.OriginalFileName, enableRangeProcessing: true);
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

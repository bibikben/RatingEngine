using BTSS.Rating.Application.Abstractions;
using Microsoft.AspNetCore.Mvc;

namespace BTSS.Rating.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class ContractsController : ControllerBase
{
    /// <summary>
    /// Publish a contract version (Draft -> Published) and write audit history.
    /// </summary>
    [HttpPost("versions/{contractVersionId:long}/publish")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> PublishVersion(
        [FromRoute] long contractVersionId,
        [FromQuery] string? userId,
        [FromQuery] string? note,
        [FromServices] IContractVersionPublisher publisher,
        CancellationToken ct)
    {
        try
        {
        await publisher.PublishAsync(contractVersionId, userId, note, ct);
        return NoContent();
        }
        catch (Exception ex)
        {
            return Problem(detail: ex.Message, statusCode: StatusCodes.Status400BadRequest);
        }
    }
}

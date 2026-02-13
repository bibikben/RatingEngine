using BTSS.Rating.Application.Abstractions;
using BTSS.Rating.Shared.Contracts;
using Microsoft.AspNetCore.Mvc;

namespace BTSS.Rating.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class RatingController : ControllerBase
{
    /// <summary>
    /// Get a rating quote for a shipment based on contract rules in RatingDb.
    /// </summary>
    [HttpPost("quote")]
    [ProducesResponseType(typeof(RatingQuoteResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<RatingQuoteResponse>> Quote(
        [FromBody] RatingQuoteRequest request,
        [FromServices] IRatingService ratingService,
        CancellationToken ct)
    {
        var result = await ratingService.QuoteAsync(request, ct);
        return Ok(result);
/// <summary>
/// Compute a rating quote and persist it to RatingDb for reporting/audit.
/// </summary>
[HttpPost("commit")]
[ProducesResponseType(typeof(RatingCommitResponse), StatusCodes.Status200OK)]
public async Task<ActionResult<RatingCommitResponse>> Commit(
    [FromBody] RatingQuoteRequest request,
    [FromServices] IRatingCommitService commitService,
    CancellationToken ct)
{
    var result = await commitService.CommitAsync(request, ct);
    return Ok(result);
}
}
/// <summary>
/// Compute a rating quote and persist it to RatingDb for reporting/audit.
/// </summary>
[HttpPost("commit")]
[ProducesResponseType(typeof(RatingCommitResponse), StatusCodes.Status200OK)]
public async Task<ActionResult<RatingCommitResponse>> Commit(
    [FromBody] RatingQuoteRequest request,
    [FromServices] IRatingCommitService commitService,
    CancellationToken ct)
{
    var result = await commitService.CommitAsync(request, ct);
    return Ok(result);
}

}

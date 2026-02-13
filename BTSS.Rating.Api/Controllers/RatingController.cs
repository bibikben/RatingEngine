using BTSS.Rating.Application.Abstractions;
using BTSS.Rating.Shared.Contracts;
using BTSS.Rating.Shared.Enums;
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
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<RatingQuoteResponse>> Quote(
        [FromBody] RatingQuoteRequest request,
        [FromServices] IRatingService ratingService,
        CancellationToken ct)
    {
        var errors = ValidateRequest(request);
        if (errors.Count > 0)
            return ValidationProblem(errors);

        var result = await ratingService.QuoteAsync(request, ct);
        return Ok(result);
    }

    /// <summary>
    /// Commits (persists) the rating quote, results, and charge lines for reporting/audit.
    /// Supports idempotency via request.RequestId (GUID string).
    /// </summary>
    [HttpPost("commit")]
    [ProducesResponseType(typeof(RatingCommitResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<RatingCommitResponse>> Commit(
        [FromBody] RatingQuoteRequest request,
        [FromServices] IRatingCommitService commitService,
        CancellationToken ct)
    {
        var errors = ValidateRequest(request);
        if (errors.Count > 0)
            return ValidationProblem(errors);

        var result = await commitService.CommitAsync(request, ct);
        return Ok(result);
    }

    private static Dictionary<string, string[]> ValidateRequest(RatingQuoteRequest request)
    {
        var errors = new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase);
        void Add(string key, string message)
        {
            if (!errors.TryGetValue(key, out var arr))
                errors[key] = [message];
            else
                errors[key] = arr.Concat([message]).ToArray();
        }

        if (request.Lines is null || request.Lines.Count == 0)
            Add(nameof(request.Lines), "At least one shipment line is required.");

        for (var i = 0; i < request.Lines.Count; i++)
        {
            var line = request.Lines[i];
            if (line.Weight <= 0)
                Add($"Lines[{i}].Weight", "Weight must be > 0.");
            if (line.Pieces <= 0)
                Add($"Lines[{i}].Pieces", "Pieces must be > 0.");

            var hasAnyDim = line.LengthIn.HasValue || line.WidthIn.HasValue || line.HeightIn.HasValue;
            var hasAllDims = line.LengthIn.HasValue && line.WidthIn.HasValue && line.HeightIn.HasValue;

            if (hasAnyDim && !hasAllDims)
                Add($"Lines[{i}].Dimensions", "Provide all 3 dimensions (LengthIn/WidthIn/HeightIn) or none.");

            if (hasAllDims && (line.LengthIn!.Value <= 0 || line.WidthIn!.Value <= 0 || line.HeightIn!.Value <= 0))
                Add($"Lines[{i}].Dimensions", "Dimensions must be > 0.");

            if (request.Mode == ShipmentMode.LTL && string.IsNullOrWhiteSpace(line.FreightClass))
                Add($"Lines[{i}].FreightClass", "FreightClass is required for LTL.");
        }

        if (!string.IsNullOrWhiteSpace(request.RequestId) && !Guid.TryParse(request.RequestId, out _))
            Add(nameof(request.RequestId), "RequestId must be a GUID string when provided.");

        return errors;
    }
}

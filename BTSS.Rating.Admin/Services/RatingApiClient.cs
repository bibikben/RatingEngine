using System.Net.Http.Json;
using BTSS.Rating.Shared.Contracts;

namespace BTSS.Rating.Admin.Services;

public sealed class RatingApiClient
{
    private readonly HttpClient _http;

    public RatingApiClient(IHttpClientFactory factory)
    {
        _http = factory.CreateClient("RatingApi");
    }

    public async Task<RatingQuoteResponse> QuoteAsync(RatingQuoteRequest request, CancellationToken ct = default)
    {
        var resp = await _http.PostAsJsonAsync("api/rating/quote", request, ct);
        resp.EnsureSuccessStatusCode();
        return (await resp.Content.ReadFromJsonAsync<RatingQuoteResponse>(cancellationToken: ct))!;
    }
    public async Task<RatingCommitResponse> CommitAsync(RatingQuoteRequest request, CancellationToken ct = default)
    {
        var resp = await _http.PostAsJsonAsync("api/rating/commit", request, ct);
        resp.EnsureSuccessStatusCode();
        return (await resp.Content.ReadFromJsonAsync<RatingCommitResponse>(cancellationToken: ct))!;
    }
    public async Task PublishContractVersionAsync(long contractVersionId, string? userId = null, string? note = null, CancellationToken ct = default)
    {
        var qs = new List<string>();
        if (!string.IsNullOrWhiteSpace(userId)) qs.Add($"userId={Uri.EscapeDataString(userId)}");
        if (!string.IsNullOrWhiteSpace(note)) qs.Add($"note={Uri.EscapeDataString(note)}");
        var url = $"api/contracts/versions/{contractVersionId}/publish" + (qs.Count > 0 ? "?" + string.Join("&", qs) : "");
        var resp = await _http.PostAsync(url, content: null, ct);
        resp.EnsureSuccessStatusCode();
    }
}

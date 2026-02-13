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
}

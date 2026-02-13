using BTSS.Rating.Application.Services;
using BTSS.Rating.Shared.Contracts;
using BTSS.Rating.Shared.Enums;

namespace BTSS.Rating.Tests;

public sealed class RatingServiceTests
{
    [Fact]
    public async Task QuoteAsync_Returns_Total()
    {
        var svc = new RatingService();
        var req = new RatingQuoteRequest(
            Mode: ShipmentMode.LTL,
            CustomerId: "CUST1",
            ContractId: "CON1",
            Origin: new Address("US", "NY", "New York", "10001"),
            Destination: new Address("US", "IL", "Chicago", "60601"),
            ShipDate: DateOnly.FromDateTime(DateTime.UtcNow),
            Lines: new[] { new ShipmentLine(Weight: 1000m, Pieces: 1, LengthIn: null, WidthIn: null, HeightIn: null, FreightClass: "55", Nmfc: null) },
            AccessorialCodes: null
        );

        var resp = await svc.QuoteAsync(req);
        Assert.True(resp.Total > 0);
        Assert.NotEmpty(resp.QuoteId);
        Assert.NotEmpty(resp.Charges);
    }
}

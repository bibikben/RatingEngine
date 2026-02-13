using BTSS.Rating.Shared.Enums;

namespace BTSS.Rating.Shared.Contracts;

public sealed record RatingQuoteRequest(
    ShipmentMode Mode,
    string? CustomerId,
    string? ContractId,
    Address Origin,
    Address Destination,
    DateOnly ShipDate,
    IReadOnlyList<ShipmentLine> Lines,
    IReadOnlyList<string>? AccessorialCodes,
    Guid? RequestId = null,
    string? OriginPort = null,
    string? DestinationPort = null,
    string? EquipmentType = null,
    string? ContainerType = null,
    string? ServiceLevel = null
);

public sealed record Address(
    string Country,
    string? StateOrProvince,
    string? City,
    string? PostalCode
);

public sealed record ShipmentLine(
    decimal Weight,
    int Pieces,
    decimal? LengthIn,
    decimal? WidthIn,
    decimal? HeightIn,
    string? FreightClass,   // LTL
    string? Nmfc           // LTL
);

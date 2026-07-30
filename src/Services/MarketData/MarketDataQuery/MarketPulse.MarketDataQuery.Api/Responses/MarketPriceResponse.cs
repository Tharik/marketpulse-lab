namespace MarketPulse.MarketDataQuery.Api.Responses;

public sealed record MarketPriceResponse(
    Guid EventId,
    string Exchange,
    string Symbol,
    decimal Price,
    string QuoteCurrency,
    DateTimeOffset ExchangeTimestamp,
    DateTimeOffset OccurredAt,
    DateTimeOffset ProducedAt,
    DateTimeOffset StoredAt);

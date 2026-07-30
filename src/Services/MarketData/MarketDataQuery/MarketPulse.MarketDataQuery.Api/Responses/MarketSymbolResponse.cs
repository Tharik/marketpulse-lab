namespace MarketPulse.MarketDataQuery.Api.Responses;

public sealed record MarketSymbolResponse(
    string Exchange,
    string Symbol,
    string QuoteCurrency);
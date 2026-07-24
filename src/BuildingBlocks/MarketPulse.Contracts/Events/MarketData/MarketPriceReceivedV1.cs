namespace MarketPulse.Contracts.Events.MarketData;

public sealed record MarketPriceReceivedV1
{
    public required string Exchange { get; init; }

    public required string Symbol { get; init; }

    public required decimal Price { get; init; }

    public required string QuoteCurrency { get; init; }

    public required DateTimeOffset ExchangeTimestamp { get; init; }
}

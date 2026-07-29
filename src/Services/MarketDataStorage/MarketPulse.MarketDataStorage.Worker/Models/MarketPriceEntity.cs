namespace MarketPulse.MarketDataStorage.Worker.Models;

public sealed class MarketPriceEntity
{
    public long Id { get; set; }

    public Guid EventId { get; set; }

    public required string Exchange { get; set; }

    public required string Symbol { get; set; }

    public decimal Price { get; set; }

    public required string QuoteCurrency { get; set; }

    public DateTimeOffset ExchangeTimestamp { get; set; }

    public DateTimeOffset OccurredAt { get; set; }

    public DateTimeOffset ProducedAt { get; set; }

    public DateTimeOffset StoredAt { get; set; }
}

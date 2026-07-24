namespace MarketPulse.MarketDataCollector.Worker.Configuration;

public sealed class EventOptions
{
    public const string SectionName = "Events";

    public required string Producer { get; init; }
}

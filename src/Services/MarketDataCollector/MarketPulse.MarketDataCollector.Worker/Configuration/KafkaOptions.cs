namespace MarketPulse.MarketDataCollector.Worker.Configuration;

public sealed class KafkaOptions
{
    public const string SectionName = "Kafka";

    public required string BootstrapServers { get; init; }

    public required string ClientId { get; init; }

    public required string MarketPriceTopic { get; init; }
}

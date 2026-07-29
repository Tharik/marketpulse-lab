namespace MarketPulse.MarketDataStorage.Worker.Configuration;

public sealed class KafkaOptions
{
    public const string SectionName = "Kafka";

    public required string BootstrapServers { get; init; }

    public required string GroupId { get; init; }

    public required string MarketPriceTopic { get; init; }
}

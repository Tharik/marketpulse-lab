namespace MarketPulse.Contracts.Events;

public sealed record EventMetadata
{
    public required Guid EventId { get; init; }

    public required string EventType { get; init; }

    public required int EventVersion { get; init; }

    public required DateTimeOffset OccurredAt { get; init; }

    public required DateTimeOffset ProducedAt { get; init; }

    public required string Producer { get; init; }

    public Guid? CorrelationId { get; init; }

    public Guid? CausationId { get; init; }

    public string? TraceId { get; init; }

    public required EventIngestionMode IngestionMode { get; init; }
}

public enum EventIngestionMode
{
    Live = 1,
    Replay = 2,
    Manual = 3
}

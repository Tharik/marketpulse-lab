namespace MarketPulse.Contracts.Events;

public sealed record EventEnvelope<TPayload>
{
    public required EventMetadata Metadata { get; init; }

    public required TPayload Payload { get; init; }
}

using Microsoft.Extensions.Options;
using MarketPulse.Contracts.Events;
using MarketPulse.MarketDataCollector.Worker.Configuration;

namespace MarketPulse.MarketDataCollector.Worker.Events;

public sealed class EventFactory : IEventFactory
{
    private readonly EventOptions _options;
    private readonly TimeProvider _timeProvider;

    public EventFactory(
        IOptions<EventOptions> options,
        TimeProvider timeProvider)
    {
        _options = options.Value;
        _timeProvider = timeProvider;
    }

    public EventEnvelope<TPayload> Create<TPayload>(
        TPayload payload,
        string eventType,
        int eventVersion,
        DateTimeOffset occurredAt,
        Guid? correlationId = null,
        Guid? causationId = null)
    {
        ArgumentNullException.ThrowIfNull(payload);
        ArgumentException.ThrowIfNullOrWhiteSpace(eventType);

        if (eventVersion <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(eventVersion),
                "Event version must be greater than zero.");
        }

        return new EventEnvelope<TPayload>
        {
            Metadata = new EventMetadata
            {
                EventId = Guid.NewGuid(),
                EventType = eventType,
                EventVersion = eventVersion,
                OccurredAt = occurredAt,
                ProducedAt = _timeProvider.GetUtcNow(),
                Producer = _options.Producer,
                CorrelationId = correlationId,
                CausationId = causationId,
                TraceId = null,
                IngestionMode = EventIngestionMode.Live
            },
            Payload = payload
        };
    }
}

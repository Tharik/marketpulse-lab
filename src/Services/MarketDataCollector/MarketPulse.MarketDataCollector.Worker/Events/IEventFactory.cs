using MarketPulse.Contracts.Events;

namespace MarketPulse.MarketDataCollector.Worker.Events;

public interface IEventFactory
{
    EventEnvelope<TPayload> Create<TPayload>(
        TPayload payload,
        string eventType,
        int eventVersion,
        DateTimeOffset occurredAt,
        Guid? correlationId = null,
        Guid? causationId = null);
}

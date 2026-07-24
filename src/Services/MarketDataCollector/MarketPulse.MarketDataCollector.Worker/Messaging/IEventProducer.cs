using MarketPulse.Contracts.Events;

namespace MarketPulse.MarketDataCollector.Worker.Messaging;

public interface IEventProducer
{
    Task ProduceAsync<TPayload>(
        string topic,
        string key,
        EventEnvelope<TPayload> envelope,
        CancellationToken cancellationToken);
}

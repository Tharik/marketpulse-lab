using MarketPulse.Contracts.Events;
using MarketPulse.Contracts.Events.MarketData;

namespace MarketPulse.MarketDataStorage.Worker.Processing;

public interface IMarketPriceProcessor
{
    Task ProcessAsync(
        EventEnvelope<MarketPriceReceivedV1> envelope,
        CancellationToken cancellationToken);
}

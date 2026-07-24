using MarketPulse.Contracts.Events.MarketData;

namespace MarketPulse.MarketDataCollector.Worker.MarketData;

public interface IPriceGenerator
{
    MarketPriceReceivedV1 Generate();
}

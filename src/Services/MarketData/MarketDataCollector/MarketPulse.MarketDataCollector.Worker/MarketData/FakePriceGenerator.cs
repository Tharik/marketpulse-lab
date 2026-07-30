using MarketPulse.Contracts.Events.MarketData;

namespace MarketPulse.MarketDataCollector.Worker.MarketData;

public sealed class FakePriceGenerator : IPriceGenerator
{
    private decimal _currentPrice = 118_500m;

    public MarketPriceReceivedV1 Generate()
    {
        var variation = Random.Shared.Next(-10_000, 10_001) / 100m;

        _currentPrice = Math.Max(
            1m,
            _currentPrice + variation);

        return new MarketPriceReceivedV1
        {
            Exchange = "fake-exchange",
            Symbol = "BTCUSDT",
            Price = decimal.Round(_currentPrice, 2),
            QuoteCurrency = "USDT",
            ExchangeTimestamp = DateTimeOffset.UtcNow
        };
    }
}

using System.Text.Json;
using MarketPulse.Contracts.Events;
using MarketPulse.Contracts.Events.MarketData;
using MarketPulse.Contracts.Serialization;

namespace MarketPulse.Contracts.Tests;

public sealed class MarketPriceReceivedV1SerializationTests
{
    [Fact]
    public void Should_serialize_market_price_event_using_contract_conventions()
    {
        var exchangeTimestamp =
            new DateTimeOffset(2026, 7, 23, 20, 30, 0, TimeSpan.Zero);

        var envelope = new EventEnvelope<MarketPriceReceivedV1>
        {
            Metadata = new EventMetadata
            {
                EventId = Guid.Parse("6ea5ac8c-f763-4cb4-8379-632da8d39028"),
                EventType = "market-price-received",
                EventVersion = 1,
                OccurredAt = exchangeTimestamp,
                ProducedAt = exchangeTimestamp.AddMilliseconds(200),
                Producer = "market-data-collector",
                CorrelationId = null,
                CausationId = null,
                TraceId = "9f734f2aabec4ca5a476293fb16ab66b",
                IngestionMode = EventIngestionMode.Live
            },
            Payload = new MarketPriceReceivedV1
            {
                Exchange = "binance",
                Symbol = "BTCUSDT",
                Price = 118532.42m,
                QuoteCurrency = "USDT",
                ExchangeTimestamp = exchangeTimestamp
            }
        };

        var json = JsonSerializer.Serialize(
            envelope,
            EventJsonSerializerOptions.Default);

        using var document = JsonDocument.Parse(json);

        var root = document.RootElement;
        var metadata = root.GetProperty("metadata");
        var payload = root.GetProperty("payload");

        Assert.Equal(
            "market-price-received",
            metadata.GetProperty("eventType").GetString());

        Assert.Equal(
            "live",
            metadata.GetProperty("ingestionMode").GetString());

        Assert.Equal(
            "BTCUSDT",
            payload.GetProperty("symbol").GetString());

        Assert.Equal(
            118532.42m,
            payload.GetProperty("price").GetDecimal());

        Assert.False(metadata.TryGetProperty("correlationId", out _));
        Assert.False(metadata.TryGetProperty("causationId", out _));
    }
}

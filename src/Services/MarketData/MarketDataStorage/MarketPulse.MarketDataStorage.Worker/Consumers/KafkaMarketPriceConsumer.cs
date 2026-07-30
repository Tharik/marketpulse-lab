using System.Text.Json;
using Confluent.Kafka;
using Microsoft.Extensions.Options;
using MarketPulse.Contracts.Events;
using MarketPulse.Contracts.Events.MarketData;
using MarketPulse.Contracts.Serialization;
using MarketPulse.MarketDataStorage.Worker.Configuration;

namespace MarketPulse.MarketDataStorage.Worker.Consumers;

public sealed class KafkaMarketPriceConsumer : IDisposable
{
    private readonly IConsumer<string, string> _consumer;
    private readonly KafkaOptions _options;
    private readonly ILogger<KafkaMarketPriceConsumer> _logger;

    public KafkaMarketPriceConsumer(
        IOptions<KafkaOptions> options,
        ILogger<KafkaMarketPriceConsumer> logger)
    {
        _options = options.Value;
        _logger = logger;

        var config = new ConsumerConfig
        {
            BootstrapServers = _options.BootstrapServers,
            GroupId = _options.GroupId,

            AutoOffsetReset = AutoOffsetReset.Earliest,

            EnableAutoCommit = false,
            EnableAutoOffsetStore = false,

            AllowAutoCreateTopics = false
        };

        _consumer = new ConsumerBuilder<string, string>(config)
            .Build();
    }

    public void Subscribe()
    {
        _consumer.Subscribe(_options.MarketPriceTopic);

        _logger.LogInformation(
            "Subscribed to topic {Topic} using consumer group {GroupId}",
            _options.MarketPriceTopic,
            _options.GroupId);
    }

    public ConsumeResult<string, string> Consume(
        CancellationToken cancellationToken)
    {
        return _consumer.Consume(cancellationToken);
    }

    public EventEnvelope<MarketPriceReceivedV1> Deserialize(
        string json)
    {
        var envelope =
            JsonSerializer.Deserialize<EventEnvelope<MarketPriceReceivedV1>>(
                json,
                EventJsonSerializerOptions.Default);

        return envelope
            ?? throw new JsonException(
                "Kafka message could not be deserialized.");
    }

    public void Commit(
        ConsumeResult<string, string> result)
    {
        _consumer.StoreOffset(result);
        _consumer.Commit(result);
    }

    public void Close()
    {
        _consumer.Close();
    }

    public void Dispose()
    {
        _consumer.Dispose();
    }
}

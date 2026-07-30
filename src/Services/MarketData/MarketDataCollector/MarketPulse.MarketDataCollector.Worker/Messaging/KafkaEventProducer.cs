using System.Text.Json;
using Confluent.Kafka;
using Microsoft.Extensions.Options;
using MarketPulse.Contracts.Events;
using MarketPulse.Contracts.Serialization;
using MarketPulse.MarketDataCollector.Worker.Configuration;

namespace MarketPulse.MarketDataCollector.Worker.Messaging;

public sealed class KafkaEventProducer : IEventProducer, IDisposable
{
    private readonly IProducer<string, string> _producer;
    private readonly ILogger<KafkaEventProducer> _logger;

    public KafkaEventProducer(
        IOptions<KafkaOptions> options,
        ILogger<KafkaEventProducer> logger)
    {
        _logger = logger;

        var kafkaOptions = options.Value;

        var producerConfig = new ProducerConfig
        {
            BootstrapServers = kafkaOptions.BootstrapServers,
            ClientId = kafkaOptions.ClientId,

            Acks = Acks.All,
            EnableIdempotence = true
        };

        _producer = new ProducerBuilder<string, string>(producerConfig)
            .Build();
    }

    public async Task ProduceAsync<TPayload>(
        string topic,
        string key,
        EventEnvelope<TPayload> envelope,
        CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(topic);
        ArgumentException.ThrowIfNullOrWhiteSpace(key);
        ArgumentNullException.ThrowIfNull(envelope);

        var json = JsonSerializer.Serialize(
            envelope,
            EventJsonSerializerOptions.Default);

        var message = new Message<string, string>
        {
            Key = key,
            Value = json
        };

        var result = await _producer.ProduceAsync(
            topic,
            message,
            cancellationToken);

        _logger.LogInformation(
            "Event {EventId} published to {Topic} partition {Partition} offset {Offset}",
            envelope.Metadata.EventId,
            result.Topic,
            result.Partition.Value,
            result.Offset.Value);
    }

    public void Dispose()
    {
        _producer.Flush(TimeSpan.FromSeconds(10));
        _producer.Dispose();
    }
}

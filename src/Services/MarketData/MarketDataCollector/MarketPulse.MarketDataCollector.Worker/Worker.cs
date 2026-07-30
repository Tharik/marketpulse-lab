using Microsoft.Extensions.Options;
using MarketPulse.MarketDataCollector.Worker.Configuration;
using MarketPulse.MarketDataCollector.Worker.Events;
using MarketPulse.MarketDataCollector.Worker.MarketData;
using MarketPulse.MarketDataCollector.Worker.Messaging;

namespace MarketPulse.MarketDataCollector.Worker;

public sealed class Worker : BackgroundService
{
    private const string MarketPriceEventType =
        "market-price-received";

    private const int MarketPriceEventVersion = 1;

    private readonly IPriceGenerator _priceGenerator;
    private readonly IEventFactory _eventFactory;
    private readonly IEventProducer _eventProducer;
    private readonly KafkaOptions _kafkaOptions;
    private readonly ILogger<Worker> _logger;

    public Worker(
        IPriceGenerator priceGenerator,
        IEventFactory eventFactory,
        IEventProducer eventProducer,
        IOptions<KafkaOptions> kafkaOptions,
        ILogger<Worker> logger)
    {
        _priceGenerator = priceGenerator;
        _eventFactory = eventFactory;
        _eventProducer = eventProducer;
        _kafkaOptions = kafkaOptions.Value;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(
        CancellationToken stoppingToken)
    {
        _logger.LogInformation(
            "Market Data Collector started.");

        using var timer = new PeriodicTimer(
            TimeSpan.FromSeconds(1));

        try
        {
            while (await timer.WaitForNextTickAsync(stoppingToken))
            {
                var price = _priceGenerator.Generate();

                var envelope = _eventFactory.Create(
                    payload: price,
                    eventType: MarketPriceEventType,
                    eventVersion: MarketPriceEventVersion,
                    occurredAt: price.ExchangeTimestamp);

                await _eventProducer.ProduceAsync(
                    topic: _kafkaOptions.MarketPriceTopic,
                    key: price.Symbol,
                    envelope: envelope,
                    cancellationToken: stoppingToken);

                _logger.LogInformation(
                    "Generated {Symbol} price {Price} from {Exchange}",
                    price.Symbol,
                    price.Price,
                    price.Exchange);
            }
        }
        catch (OperationCanceledException)
            when (stoppingToken.IsCancellationRequested)
        {
            _logger.LogInformation(
                "Market Data Collector is stopping.");
        }
    }
}
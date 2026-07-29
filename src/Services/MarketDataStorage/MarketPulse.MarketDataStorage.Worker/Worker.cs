using MarketPulse.MarketDataStorage.Worker.Consumers;
using MarketPulse.MarketDataStorage.Worker.Processing;

namespace MarketPulse.MarketDataStorage.Worker;

public sealed class Worker : BackgroundService
{
    private readonly KafkaMarketPriceConsumer _consumer;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<Worker> _logger;

    public Worker(
        KafkaMarketPriceConsumer consumer,
        IServiceScopeFactory scopeFactory,
        ILogger<Worker> logger)
    {
        _consumer = consumer;
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(
        CancellationToken stoppingToken)
    {
        _consumer.Subscribe();

        try
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                var result = _consumer.Consume(stoppingToken);

                try
                {
                    var envelope =
                        _consumer.Deserialize(result.Message.Value);

                    await using var scope =
                        _scopeFactory.CreateAsyncScope();

                    var processor =
                        scope.ServiceProvider
                            .GetRequiredService<IMarketPriceProcessor>();

                    await processor.ProcessAsync(
                        envelope,
                        stoppingToken);

                    _consumer.Commit(result);

                    _logger.LogInformation(
                        "Processed partition {Partition}, offset {Offset}",
                        result.Partition.Value,
                        result.Offset.Value);
                }
                catch (Exception exception)
                    when (exception is not OperationCanceledException)
                {
                    _logger.LogError(
                        exception,
                        "Failed to process message from partition {Partition}, offset {Offset}",
                        result.Partition.Value,
                        result.Offset.Value);

                    await Task.Delay(
                        TimeSpan.FromSeconds(2),
                        stoppingToken);
                }
            }
        }
        catch (OperationCanceledException)
            when (stoppingToken.IsCancellationRequested)
        {
            _logger.LogInformation(
                "Market Data Storage Worker is stopping.");
        }
        finally
        {
            _consumer.Close();
        }
    }
}

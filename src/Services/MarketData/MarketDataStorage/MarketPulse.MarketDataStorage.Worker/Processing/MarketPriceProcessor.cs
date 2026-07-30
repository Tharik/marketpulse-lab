using Microsoft.EntityFrameworkCore;
using MarketPulse.Contracts.Events;
using MarketPulse.Contracts.Events.MarketData;
using MarketPulse.MarketData.Persistence.Models;
using MarketPulse.MarketData.Persistence;

namespace MarketPulse.MarketDataStorage.Worker.Processing;

public sealed class MarketPriceProcessor : IMarketPriceProcessor
{
    private readonly MarketDataDbContext _dbContext;
    private readonly TimeProvider _timeProvider;
    private readonly ILogger<MarketPriceProcessor> _logger;

    public MarketPriceProcessor(
        MarketDataDbContext dbContext,
        TimeProvider timeProvider,
        ILogger<MarketPriceProcessor> logger)
    {
        _dbContext = dbContext;
        _timeProvider = timeProvider;
        _logger = logger;
    }

    public async Task ProcessAsync(
        EventEnvelope<MarketPriceReceivedV1> envelope,
        CancellationToken cancellationToken)
    {
        var alreadyProcessed = await _dbContext.MarketPrices
            .AnyAsync(
                entity => entity.EventId == envelope.Metadata.EventId,
                cancellationToken);

        if (alreadyProcessed)
        {
            _logger.LogWarning(
                "Event {EventId} has already been processed",
                envelope.Metadata.EventId);

            return;
        }

        var entity = new MarketPriceEntity
        {
            EventId = envelope.Metadata.EventId,
            Exchange = envelope.Payload.Exchange,
            Symbol = envelope.Payload.Symbol,
            Price = envelope.Payload.Price,
            QuoteCurrency = envelope.Payload.QuoteCurrency,
            ExchangeTimestamp = envelope.Payload.ExchangeTimestamp,
            OccurredAt = envelope.Metadata.OccurredAt,
            ProducedAt = envelope.Metadata.ProducedAt,
            StoredAt = _timeProvider.GetUtcNow()
        };

        _dbContext.MarketPrices.Add(entity);

        await _dbContext.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Stored event {EventId} for {Symbol} at price {Price}",
            envelope.Metadata.EventId,
            envelope.Payload.Symbol,
            envelope.Payload.Price);
    }
}

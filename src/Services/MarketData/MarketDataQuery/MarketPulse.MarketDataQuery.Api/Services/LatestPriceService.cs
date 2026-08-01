using System.Diagnostics;
using System.Text.Json;
using MarketPulse.MarketData.Persistence;
using MarketPulse.MarketDataQuery.Api.Configuration;
using MarketPulse.MarketDataQuery.Api.Observability;
using MarketPulse.MarketDataQuery.Api.Responses;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using StackExchange.Redis;

namespace MarketPulse.MarketDataQuery.Api.Services;

public sealed class LatestPriceService(
    MarketDataDbContext dbContext,
    IConnectionMultiplexer multiplexer,
    IOptions<RedisOptions> redisOptions,
    ILogger<LatestPriceService> logger,
    MarketDataMetrics metrics)
{
    private static readonly JsonSerializerOptions JsonOptions =
        new(JsonSerializerDefaults.Web);

    private readonly MarketDataDbContext _dbContext = dbContext;
    private readonly IDatabase _redis = multiplexer.GetDatabase();
    private readonly RedisOptions _redisOptions = redisOptions.Value;
    private readonly ILogger<LatestPriceService> _logger = logger;
    private readonly MarketDataMetrics _metrics = metrics;

    public async Task<MarketPriceResponse?> GetAsync(
        string exchange,
        string symbol,
        CancellationToken cancellationToken)
    {
        var requestedExchange = exchange.Trim();
        var requestedSymbol = symbol.Trim();

        var cacheKey = BuildCacheKey(
            requestedExchange,
            requestedSymbol);

        var cacheResult =
            await TryGetFromCacheAsync(cacheKey);

        if (cacheResult.Price is not null)
        {
            _metrics.RecordCacheHit();

            _logger.LogInformation(
                "Latest price cache hit for {Exchange}/{Symbol}",
                requestedExchange,
                requestedSymbol);

            return cacheResult.Price;
        }

        _metrics.RecordCacheMiss();

        _logger.LogInformation(
            "Latest price cache miss for {Exchange}/{Symbol}",
            requestedExchange,
            requestedSymbol);

        var price = await GetFromPostgreSqlAsync(
            requestedExchange,
            requestedSymbol,
            cancellationToken);

        if (price is null)
        {
            return null;
        }

        if (cacheResult.RedisAvailable)
        {
            await TrySetCacheAsync(
                cacheKey,
                price);
        }

        return price;
    }

    private async Task<MarketPriceResponse?> GetFromPostgreSqlAsync(
        string exchange,
        string symbol,
        CancellationToken cancellationToken)
    {
        var stopwatch = Stopwatch.StartNew();

        try
        {
            return await _dbContext.MarketPrices
                .AsNoTracking()
                .Where(price =>
                    EF.Functions.ILike(
                        price.Exchange,
                        exchange) &&
                    EF.Functions.ILike(
                        price.Symbol,
                        symbol))
                .OrderByDescending(
                    price => price.ExchangeTimestamp)
                .Select(price => new MarketPriceResponse(
                    price.EventId,
                    price.Exchange,
                    price.Symbol,
                    price.Price,
                    price.QuoteCurrency,
                    price.ExchangeTimestamp,
                    price.OccurredAt,
                    price.ProducedAt,
                    price.StoredAt))
                .FirstOrDefaultAsync(cancellationToken);
        }
        finally
        {
            stopwatch.Stop();

            _metrics.RecordDatabaseReadDuration(
                stopwatch.Elapsed.TotalMilliseconds);
        }
    }

    private async Task<(
        MarketPriceResponse? Price,
        bool RedisAvailable)> TryGetFromCacheAsync(
        string cacheKey)
    {
        var stopwatch = Stopwatch.StartNew();

        try
        {
            var cachedValue =
                await _redis.StringGetAsync(cacheKey);

            if (cachedValue.IsNullOrEmpty)
            {
                return (null, true);
            }

            var price =
                JsonSerializer.Deserialize<MarketPriceResponse>(
                    cachedValue.ToString(),
                    JsonOptions);

            return (price, true);
        }
        catch (RedisException exception)
        {
            _metrics.RecordCacheFailure("read");

            _logger.LogWarning(
                exception,
                "Redis read failed for key {CacheKey}. Falling back to PostgreSQL.",
                cacheKey);

            return (null, false);
        }
        catch (JsonException exception)
        {
            _metrics.RecordCacheFailure("deserialization");

            _logger.LogWarning(
                exception,
                "Cached value for key {CacheKey} is invalid. Falling back to PostgreSQL.",
                cacheKey);

            return (null, true);
        }
        finally
        {
            stopwatch.Stop();

            _metrics.RecordCacheReadDuration(
                stopwatch.Elapsed.TotalMilliseconds);
        }
    }

    private async Task TrySetCacheAsync(
        string cacheKey,
        MarketPriceResponse price)
    {
        try
        {
            var json = JsonSerializer.Serialize(
                price,
                JsonOptions);

            var expiration = TimeSpan.FromSeconds(
                _redisOptions.LatestPriceTtlSeconds);

            await _redis.StringSetAsync(
                cacheKey,
                json,
                expiration);

            _logger.LogInformation(
                "Latest price cached under {CacheKey} for {TtlSeconds} seconds",
                cacheKey,
                _redisOptions.LatestPriceTtlSeconds);
        }
        catch (RedisException exception)
        {
            _metrics.RecordCacheFailure("write");

            _logger.LogWarning(
                exception,
                "Redis write failed for key {CacheKey}. Returning PostgreSQL result.",
                cacheKey);
        }
        catch (JsonException exception)
        {
            _metrics.RecordCacheFailure("serialization");

            _logger.LogWarning(
                exception,
                "Could not serialize latest price for cache key {CacheKey}.",
                cacheKey);
        }
    }

    private static string BuildCacheKey(
        string exchange,
        string symbol)
    {
        return
            $"market-data:latest-price:" +
            $"{exchange.ToLowerInvariant()}:" +
            $"{symbol.ToUpperInvariant()}";
    }
}
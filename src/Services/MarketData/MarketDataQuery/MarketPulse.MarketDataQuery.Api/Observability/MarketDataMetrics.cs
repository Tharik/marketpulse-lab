using System.Diagnostics.Metrics;

namespace MarketPulse.MarketDataQuery.Api.Observability;

public sealed class MarketDataMetrics
{
    public const string MeterName =
        "MarketPulse.MarketDataQuery";

    private readonly Counter<long> _cacheHits;
    private readonly Counter<long> _cacheMisses;
    private readonly Counter<long> _cacheFailures;
    private readonly Histogram<double> _cacheReadDuration;
    private readonly Histogram<double> _databaseReadDuration;

    public MarketDataMetrics(
        IMeterFactory meterFactory)
    {
        var meter = meterFactory.Create(MeterName);

        _cacheHits = meter.CreateCounter<long>(
            "marketpulse.cache.hits",
            unit: "{hit}",
            description: "Number of successful cache reads.");

        _cacheMisses = meter.CreateCounter<long>(
            "marketpulse.cache.misses",
            unit: "{miss}",
            description: "Number of cache reads with no value.");

        _cacheFailures = meter.CreateCounter<long>(
            "marketpulse.cache.failures",
            unit: "{failure}",
            description: "Number of cache operations that failed.");

        _cacheReadDuration = meter.CreateHistogram<double>(
            "marketpulse.cache.read.duration",
            unit: "ms",
            description: "Redis cache read duration.");

        _databaseReadDuration = meter.CreateHistogram<double>(
            "marketpulse.database.read.duration",
            unit: "ms",
            description: "PostgreSQL latest-price query duration.");
    }

    public void RecordCacheHit()
    {
        _cacheHits.Add(1);
    }

    public void RecordCacheMiss()
    {
        _cacheMisses.Add(1);
    }

    public void RecordCacheFailure(
        string operation)
    {
        _cacheFailures.Add(
            1,
            new KeyValuePair<string, object?>(
                "operation",
                operation));
    }

    public void RecordCacheReadDuration(
        double elapsedMilliseconds)
    {
        _cacheReadDuration.Record(
            elapsedMilliseconds);
    }

    public void RecordDatabaseReadDuration(
        double elapsedMilliseconds)
    {
        _databaseReadDuration.Record(
            elapsedMilliseconds);
    }
}

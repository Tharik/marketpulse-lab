namespace MarketPulse.MarketDataQuery.Api.Configuration;

public sealed class RedisOptions
{
    public const string SectionName = "Redis";

    public required string ConnectionString { get; init; }

    public int LatestPriceTtlSeconds { get; init; } = 5;
}
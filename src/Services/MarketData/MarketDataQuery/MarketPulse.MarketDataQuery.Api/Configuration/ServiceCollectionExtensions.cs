using MarketPulse.MarketData.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using StackExchange.Redis;

namespace MarketPulse.MarketDataQuery.Api.Configuration;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddRedis(this IServiceCollection services, IConfiguration configuration)
    {
        services
            .AddOptions<RedisOptions>()
            .Bind(
                configuration.GetSection(
                    RedisOptions.SectionName))
            .Validate(
                options =>
                    !string.IsNullOrWhiteSpace(
                        options.ConnectionString),
                "Redis ConnectionString must be configured.")
            .Validate(
                options =>
                    options.LatestPriceTtlSeconds > 0,
                "Redis LatestPriceTtlSeconds must be greater than zero.")
            .ValidateOnStart();

        services.AddSingleton<IConnectionMultiplexer>(
            serviceProvider =>
            {
                var redisOptions = serviceProvider
                    .GetRequiredService<IOptions<RedisOptions>>()
                    .Value;

                var configurationOptions =
                    ConfigurationOptions.Parse(
                        redisOptions.ConnectionString);
                configurationOptions.AbortOnConnectFail = false;
                configurationOptions.ConnectTimeout = 500;
                configurationOptions.SyncTimeout = 500;
                configurationOptions.AsyncTimeout = 500;

                configurationOptions.AbortOnConnectFail = false;

                return ConnectionMultiplexer.Connect(
                    configurationOptions);
            });

        return services;
    }

    public static IServiceCollection AddPersistence(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString =
            configuration.GetConnectionString("MarketData")
            ?? throw new InvalidOperationException(
                "Connection string 'MarketData' was not configured.");

        services.AddDbContext<MarketDataDbContext>(
            options =>
                options.UseNpgsql(connectionString));

        return services;
    }

    public static IServiceCollection AddMarketDataHealthChecks(
        this IServiceCollection services)
    {
        services
            .AddHealthChecks()
            .AddDbContextCheck<MarketDataDbContext>(
                name: "market-data-postgresql",
                tags: ["ready"]);

        return services;
    }

    public static IServiceCollection AddMarketDataOpenApi(
        this IServiceCollection services)
    {
        services.AddOpenApi(options =>
        {
            options.AddDocumentTransformer(
                (document, _, _) =>
                {
                    document.Info.Title =
                        "MarketPulse Market Data API";

                    document.Info.Version = "v1";

                    document.Info.Description =
                        "Read API for querying persisted market-price data.";

                    return Task.CompletedTask;
                });
        });

        return services;
    }
}

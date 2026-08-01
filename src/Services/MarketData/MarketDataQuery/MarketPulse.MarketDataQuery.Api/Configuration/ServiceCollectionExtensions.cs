using MarketPulse.MarketData.Persistence;
using Microsoft.EntityFrameworkCore;

namespace MarketPulse.MarketDataQuery.Api.Configuration;

public static class ServiceCollectionExtensions
{
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

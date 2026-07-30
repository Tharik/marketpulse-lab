using MarketPulse.MarketData.Persistence;
using MarketPulse.MarketDataQuery.Api.Endpoints;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

var connectionString =
    builder.Configuration.GetConnectionString("MarketData")
    ?? throw new InvalidOperationException(
        "Connection string 'MarketData' was not configured.");

builder.Services.AddDbContext<MarketDataDbContext>(
    options => options.UseNpgsql(connectionString));

builder.Services
    .AddHealthChecks()
    .AddDbContextCheck<MarketDataDbContext>(
        name: "market-data-postgresql",
        tags: ["ready"]);

builder.Services.AddOpenApi(options =>
{
    options.AddDocumentTransformer(
        (document, context, cancellationToken) =>
        {
            document.Info.Title =
                "MarketPulse Market Data API";

            document.Info.Version = "v1";

            document.Info.Description =
                "Read API for querying persisted market-price data.";

            return Task.CompletedTask;
        });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.MapMarketDataEndpoints();

app.MapHealthChecks(
    "/health/live",
    new HealthCheckOptions
    {
        Predicate = _ => false
    });

app.MapHealthChecks(
    "/health/ready",
    new HealthCheckOptions
    {
        Predicate = healthCheck =>
            healthCheck.Tags.Contains("ready")
    });

app.Run();
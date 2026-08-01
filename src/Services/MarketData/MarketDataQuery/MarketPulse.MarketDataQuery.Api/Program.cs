using MarketPulse.MarketDataQuery.Api.Configuration;
using MarketPulse.MarketDataQuery.Api.Endpoints;
using MarketPulse.MarketDataQuery.Api.Services;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddPersistence(builder.Configuration)
    .AddRedis(builder.Configuration)
    .AddMarketDataHealthChecks()
    .AddMarketDataOpenApi();
builder.Services.AddScoped<LatestPriceService>();

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
        Predicate = registration =>
            registration.Tags.Contains("ready")
    });

app.Run();
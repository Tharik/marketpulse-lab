using MarketPulse.MarketData.Persistence;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

var connectionString =
    builder.Configuration.GetConnectionString("MarketData")
    ?? throw new InvalidOperationException(
        "Connection string 'MarketData' was not configured.");

builder.Services.AddDbContext<MarketDataDbContext>(
    options => options.UseNpgsql(connectionString));

builder.Services.AddOpenApi();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.MapGet(
    "/prices/{exchange}/{symbol}/latest",
    async (
        string exchange,
        string symbol,
        MarketDataDbContext dbContext,
        CancellationToken cancellationToken) =>
    {
        var requestedExchange = exchange.Trim();
        var requestedSymbol = symbol.Trim();

        var result = await dbContext.MarketPrices
            .AsNoTracking()
            .Where(price =>
                EF.Functions.ILike(
                    price.Exchange,
                    requestedExchange) &&
                EF.Functions.ILike(
                    price.Symbol,
                    requestedSymbol))
            .OrderByDescending(
                price => price.ExchangeTimestamp)
            .Select(price => new
            {
                price.EventId,
                price.Exchange,
                price.Symbol,
                price.Price,
                price.QuoteCurrency,
                price.ExchangeTimestamp,
                price.OccurredAt,
                price.ProducedAt,
                price.StoredAt
            })
            .FirstOrDefaultAsync(cancellationToken);

        return result is null
            ? Results.NotFound()
            : Results.Ok(result);
    });

app.MapGet(
    "/prices/{exchange}/{symbol}",
    async (
        string exchange,
        string symbol,
        int? limit,
        MarketDataDbContext dbContext,
        CancellationToken cancellationToken) =>
    {
        var requestedExchange = exchange.Trim();
        var requestedSymbol = symbol.Trim();

        var resultLimit = Math.Clamp(limit ?? 100, 1, 500);

        var results = await dbContext.MarketPrices
            .AsNoTracking()
            .Where(price =>
                EF.Functions.ILike(
                    price.Exchange,
                    requestedExchange) &&
                EF.Functions.ILike(
                    price.Symbol,
                    requestedSymbol))
            .OrderByDescending(price => price.ExchangeTimestamp)
            .Take(resultLimit)
            .Select(price => new
            {
                price.EventId,
                price.Exchange,
                price.Symbol,
                price.Price,
                price.QuoteCurrency,
                price.ExchangeTimestamp,
                price.OccurredAt,
                price.ProducedAt,
                price.StoredAt
            })
            .ToListAsync(cancellationToken);

        return Results.Ok(results);
    });

app.MapGet(
    "/symbols",
    async (
        MarketDataDbContext dbContext,
        CancellationToken cancellationToken) =>
    {
        var symbols = await dbContext.MarketPrices
            .AsNoTracking()
            .Select(price => new
            {
                price.Exchange,
                price.Symbol,
                price.QuoteCurrency
            })
            .Distinct()
            .OrderBy(item => item.Exchange)
            .ThenBy(item => item.Symbol)
            .ToListAsync(cancellationToken);

        return Results.Ok(symbols);
    });

app.Run();
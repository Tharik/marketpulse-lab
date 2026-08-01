using MarketPulse.MarketData.Persistence;
using MarketPulse.MarketDataQuery.Api.Responses;
using MarketPulse.MarketDataQuery.Api.Services;
using Microsoft.EntityFrameworkCore;

namespace MarketPulse.MarketDataQuery.Api.Endpoints;

public static class MarketDataEndpoints
{
    public static IEndpointRouteBuilder MapMarketDataEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints
            .MapGroup("/api/v1")
            .WithTags("Market Data");

        group.MapGet(
                "/prices/{exchange}/{symbol}/latest",
                GetLatestPriceAsync)
            .WithName("GetLatestMarketPrice")
            .WithSummary("Returns the latest market price")
            .WithDescription(
                "Returns the most recent persisted price for a trading pair.")
            .Produces<MarketPriceResponse>(
                StatusCodes.Status200OK)
            .Produces(
                StatusCodes.Status404NotFound);

        group.MapGet(
                "/prices/{exchange}/{symbol}",
                GetPriceHistoryAsync)
            .WithName("GetMarketPriceHistory")
            .WithSummary("Returns market price history")
            .WithDescription(
                "Returns persisted prices ordered from newest to oldest. " +
                "The limit must be between 1 and 500.")
            .Produces<List<MarketPriceResponse>>(
                StatusCodes.Status200OK);

        group.MapGet(
                "/symbols",
                GetSymbolsAsync)
            .WithName("GetMarketSymbols")
            .WithSummary("Returns available market symbols")
            .WithDescription(
                "Returns the distinct exchange, symbol and quote-currency " +
                "combinations currently stored.")
            .Produces<List<MarketSymbolResponse>>(
                StatusCodes.Status200OK);

        return endpoints;
    }

    private static async Task<IResult> GetLatestPriceAsync(
    string exchange,
    string symbol,
    LatestPriceService latestPriceService,
    CancellationToken cancellationToken)
    {
        var result =
            await latestPriceService.GetAsync(
                exchange,
                symbol,
                cancellationToken);

        return result is null
            ? Results.NotFound()
            : Results.Ok(result);
    }

    private static async Task<IResult> GetPriceHistoryAsync(
        string exchange,
        string symbol,
        int? limit,
        MarketDataDbContext dbContext,
        CancellationToken cancellationToken)
    {
        var requestedExchange = exchange.Trim();
        var requestedSymbol = symbol.Trim();

        var resultLimit = Math.Clamp(
            limit ?? 100,
            1,
            500);

        var results = await dbContext.MarketPrices
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
            .Take(resultLimit)
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
            .ToListAsync(cancellationToken);

        return Results.Ok(results);
    }

    private static async Task<IResult> GetSymbolsAsync(
        MarketDataDbContext dbContext,
        CancellationToken cancellationToken)
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

        var response = symbols
            .Select(item => new MarketSymbolResponse(
                item.Exchange,
                item.Symbol,
                item.QuoteCurrency))
            .ToList();

        return Results.Ok(response);
    }
}
using Microsoft.EntityFrameworkCore;
using MarketPulse.MarketData.Persistence.Models;

namespace MarketPulse.MarketData.Persistence;

public sealed class MarketDataDbContext : DbContext
{
    public MarketDataDbContext(
        DbContextOptions<MarketDataDbContext> options)
        : base(options)
    {
    }

    public DbSet<MarketPriceEntity> MarketPrices =>
        Set<MarketPriceEntity>();

    protected override void OnModelCreating(
        ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(
            typeof(MarketDataDbContext).Assembly);
    }
}

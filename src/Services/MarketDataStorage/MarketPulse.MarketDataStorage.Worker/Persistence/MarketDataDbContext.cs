using Microsoft.EntityFrameworkCore;
using MarketPulse.MarketDataStorage.Worker.Models;

namespace MarketPulse.MarketDataStorage.Worker.Persistence;

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

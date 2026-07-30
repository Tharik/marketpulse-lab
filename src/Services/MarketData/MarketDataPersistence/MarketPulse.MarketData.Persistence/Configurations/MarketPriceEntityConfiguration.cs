using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MarketPulse.MarketData.Persistence.Models;

namespace MarketPulse.MarketData.Persistence.Configurations;

public sealed class MarketPriceEntityConfiguration
    : IEntityTypeConfiguration<MarketPriceEntity>
{
    public void Configure(
        EntityTypeBuilder<MarketPriceEntity> builder)
    {
        builder.ToTable("market_prices");

        builder.HasKey(entity => entity.Id);

        builder.Property(entity => entity.Id)
            .UseIdentityByDefaultColumn();

        builder.Property(entity => entity.EventId)
            .IsRequired();

        builder.HasIndex(entity => entity.EventId)
            .IsUnique();

        builder.Property(entity => entity.Exchange)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(entity => entity.Symbol)
            .HasMaxLength(30)
            .IsRequired();

        builder.Property(entity => entity.Price)
            .HasPrecision(28, 10)
            .IsRequired();

        builder.Property(entity => entity.QuoteCurrency)
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(entity => entity.ExchangeTimestamp)
            .IsRequired();

        builder.Property(entity => entity.OccurredAt)
            .IsRequired();

        builder.Property(entity => entity.ProducedAt)
            .IsRequired();

        builder.Property(entity => entity.StoredAt)
            .IsRequired();

        builder.HasIndex(entity => new
        {
            entity.Exchange,
            entity.Symbol,
            entity.ExchangeTimestamp
        });
    }
}

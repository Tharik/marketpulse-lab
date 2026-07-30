using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace MarketPulse.MarketDataStorage.Worker.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialMarketDataStorage : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "market_prices",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    EventId = table.Column<Guid>(type: "uuid", nullable: false),
                    Exchange = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Symbol = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    Price = table.Column<decimal>(type: "numeric(28,10)", precision: 28, scale: 10, nullable: false),
                    QuoteCurrency = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    ExchangeTimestamp = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    OccurredAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    ProducedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    StoredAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_market_prices", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_market_prices_EventId",
                table: "market_prices",
                column: "EventId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_market_prices_Exchange_Symbol_ExchangeTimestamp",
                table: "market_prices",
                columns: new[] { "Exchange", "Symbol", "ExchangeTimestamp" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "market_prices");
        }
    }
}

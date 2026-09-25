using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CosmicWiki.Payments.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialPayments : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "COSMICWIKI_PAYMENTS");

            migrationBuilder.CreateTable(
                name: "PAYMENTS",
                schema: "COSMICWIKI_PAYMENTS",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "RAW(16)", nullable: false),
                    OrderId = table.Column<Guid>(type: "RAW(16)", nullable: false),
                    PaymentIntentId = table.Column<string>(type: "NVARCHAR2(200)", maxLength: 200, nullable: false),
                    Amount = table.Column<decimal>(type: "DECIMAL(18,2)", precision: 18, scale: 2, nullable: false),
                    Currency = table.Column<string>(type: "NVARCHAR2(3)", maxLength: 3, nullable: false),
                    Status = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: false),
                    CompletedAtUtc = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PAYMENTS", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PROCESSED_PROVIDER_EVENTS",
                schema: "COSMICWIKI_PAYMENTS",
                columns: table => new
                {
                    ProviderEventId = table.Column<string>(type: "NVARCHAR2(200)", maxLength: 200, nullable: false),
                    ProcessedAtUtc = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PROCESSED_PROVIDER_EVENTS", x => x.ProviderEventId);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PAYMENTS_PaymentIntentId",
                schema: "COSMICWIKI_PAYMENTS",
                table: "PAYMENTS",
                column: "PaymentIntentId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PAYMENTS",
                schema: "COSMICWIKI_PAYMENTS");

            migrationBuilder.DropTable(
                name: "PROCESSED_PROVIDER_EVENTS",
                schema: "COSMICWIKI_PAYMENTS");
        }
    }
}

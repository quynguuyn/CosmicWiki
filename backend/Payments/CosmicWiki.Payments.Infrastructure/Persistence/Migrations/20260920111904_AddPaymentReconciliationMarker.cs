using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CosmicWiki.Payments.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddPaymentReconciliationMarker : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "ReconciledAtUtc",
                schema: "COSMICWIKI_PAYMENTS",
                table: "PAYMENTS",
                type: "TIMESTAMP(7)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_PAYMENTS_ReconciledAtUtc",
                schema: "COSMICWIKI_PAYMENTS",
                table: "PAYMENTS",
                column: "ReconciledAtUtc");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_PAYMENTS_ReconciledAtUtc",
                schema: "COSMICWIKI_PAYMENTS",
                table: "PAYMENTS");

            migrationBuilder.DropColumn(
                name: "ReconciledAtUtc",
                schema: "COSMICWIKI_PAYMENTS",
                table: "PAYMENTS");
        }
    }
}

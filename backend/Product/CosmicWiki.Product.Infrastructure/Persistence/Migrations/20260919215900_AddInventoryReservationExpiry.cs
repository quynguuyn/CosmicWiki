using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CosmicWiki.Product.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddInventoryReservationExpiry : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "EXPIRES_AT_UTC",
                schema: "COSMICWIKI_PRODUCT",
                table: "INVENTORY_RESERVATIONS",
                type: "TIMESTAMP(7)",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EXPIRES_AT_UTC",
                schema: "COSMICWIKI_PRODUCT",
                table: "INVENTORY_RESERVATIONS");
        }
    }
}

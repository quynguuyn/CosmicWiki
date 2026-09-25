using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CosmicWiki.Product.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddInventoryReservations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "INVENTORY_RESERVATIONS",
                schema: "COSMICWIKI_PRODUCT",
                columns: table => new
                {
                    ID = table.Column<Guid>(type: "RAW(16)", nullable: false),
                    ORDER_ID = table.Column<Guid>(type: "RAW(16)", nullable: false),
                    PRODUCT_ID = table.Column<Guid>(type: "RAW(16)", nullable: false),
                    QUANTITY = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    STATUS = table.Column<string>(type: "NVARCHAR2(20)", maxLength: 20, nullable: false),
                    CREATED_AT_UTC = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: false),
                    RELEASED_AT_UTC = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_INVENTORY_RESERVATIONS", x => x.ID);
                });

            migrationBuilder.CreateIndex(
                name: "IX_INVENTORY_RESERVATIONS_ORDER_ID",
                schema: "COSMICWIKI_PRODUCT",
                table: "INVENTORY_RESERVATIONS",
                column: "ORDER_ID");

            migrationBuilder.CreateIndex(
                name: "IX_INVENTORY_RESERVATIONS_ORDER_ID_PRODUCT_ID",
                schema: "COSMICWIKI_PRODUCT",
                table: "INVENTORY_RESERVATIONS",
                columns: new[] { "ORDER_ID", "PRODUCT_ID" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "INVENTORY_RESERVATIONS",
                schema: "COSMICWIKI_PRODUCT");
        }
    }
}

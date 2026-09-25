using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CosmicWiki.Product.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialProduct : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "COSMICWIKI_PRODUCT");

            migrationBuilder.CreateTable(
                name: "CATEGORIES",
                schema: "COSMICWIKI_PRODUCT",
                columns: table => new
                {
                    ID = table.Column<Guid>(type: "RAW(16)", nullable: false),
                    NAME = table.Column<string>(type: "NVARCHAR2(200)", maxLength: 200, nullable: false),
                    SLUG = table.Column<string>(type: "NVARCHAR2(200)", maxLength: 200, nullable: false),
                    DESCRIPTION = table.Column<string>(type: "NVARCHAR2(1000)", maxLength: 1000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CATEGORIES", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "PRODUCTS",
                schema: "COSMICWIKI_PRODUCT",
                columns: table => new
                {
                    ID = table.Column<Guid>(type: "RAW(16)", nullable: false),
                    SKU = table.Column<string>(type: "NVARCHAR2(100)", maxLength: 100, nullable: false),
                    NAME = table.Column<string>(type: "NVARCHAR2(200)", maxLength: 200, nullable: false),
                    SLUG = table.Column<string>(type: "NVARCHAR2(200)", maxLength: 200, nullable: false),
                    DESCRIPTION = table.Column<string>(type: "NCLOB", maxLength: 4000, nullable: true),
                    ORIGINAL_PRICE = table.Column<decimal>(type: "DECIMAL(18,2)", precision: 18, scale: 2, nullable: false),
                    SALE_PRICE = table.Column<decimal>(type: "DECIMAL(18,2)", precision: 18, scale: 2, nullable: false),
                    STOCK_QUANTITY = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    STATUS = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    CATEGORY_ID = table.Column<Guid>(type: "RAW(16)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PRODUCTS", x => x.ID);
                    table.ForeignKey(
                        name: "FK_PRODUCTS_CATEGORIES_CATEGORY_ID",
                        column: x => x.CATEGORY_ID,
                        principalSchema: "COSMICWIKI_PRODUCT",
                        principalTable: "CATEGORIES",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PRODUCT_IMAGES",
                schema: "COSMICWIKI_PRODUCT",
                columns: table => new
                {
                    ID = table.Column<Guid>(type: "RAW(16)", nullable: false),
                    PRODUCT_ID = table.Column<Guid>(type: "RAW(16)", nullable: false),
                    URL = table.Column<string>(type: "NVARCHAR2(2000)", maxLength: 2000, nullable: false),
                    DISPLAY_ORDER = table.Column<int>(type: "NUMBER(10)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PRODUCT_IMAGES", x => x.ID);
                    table.ForeignKey(
                        name: "FK_PRODUCT_IMAGES_PRODUCTS_PRODUCT_ID",
                        column: x => x.PRODUCT_ID,
                        principalSchema: "COSMICWIKI_PRODUCT",
                        principalTable: "PRODUCTS",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CATEGORIES_NAME",
                schema: "COSMICWIKI_PRODUCT",
                table: "CATEGORIES",
                column: "NAME",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CATEGORIES_SLUG",
                schema: "COSMICWIKI_PRODUCT",
                table: "CATEGORIES",
                column: "SLUG",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PRODUCT_IMAGES_PRODUCT_ID_DISPLAY_ORDER",
                schema: "COSMICWIKI_PRODUCT",
                table: "PRODUCT_IMAGES",
                columns: new[] { "PRODUCT_ID", "DISPLAY_ORDER" });

            migrationBuilder.CreateIndex(
                name: "IX_PRODUCTS_CATEGORY_ID",
                schema: "COSMICWIKI_PRODUCT",
                table: "PRODUCTS",
                column: "CATEGORY_ID");

            migrationBuilder.CreateIndex(
                name: "IX_PRODUCTS_SKU",
                schema: "COSMICWIKI_PRODUCT",
                table: "PRODUCTS",
                column: "SKU",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PRODUCTS_SLUG",
                schema: "COSMICWIKI_PRODUCT",
                table: "PRODUCTS",
                column: "SLUG",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PRODUCT_IMAGES",
                schema: "COSMICWIKI_PRODUCT");

            migrationBuilder.DropTable(
                name: "PRODUCTS",
                schema: "COSMICWIKI_PRODUCT");

            migrationBuilder.DropTable(
                name: "CATEGORIES",
                schema: "COSMICWIKI_PRODUCT");
        }
    }
}

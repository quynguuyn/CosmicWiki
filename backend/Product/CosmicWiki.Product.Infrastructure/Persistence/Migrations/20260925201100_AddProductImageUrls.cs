using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CosmicWiki.Product.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddProductImageUrls : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "URLS",
                schema: "COSMICWIKI_PRODUCT",
                table: "PRODUCT_IMAGES",
                type: "CLOB",
                nullable: false,
                defaultValue: "[]");

            migrationBuilder.Sql(
                "UPDATE \"COSMICWIKI_PRODUCT\".\"PRODUCT_IMAGES\" " +
                "SET \"URLS\" = '[\"' || REPLACE(\"URL\", '\"', '\\\"') || '\"]'");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "URLS",
                schema: "COSMICWIKI_PRODUCT",
                table: "PRODUCT_IMAGES");
        }
    }
}

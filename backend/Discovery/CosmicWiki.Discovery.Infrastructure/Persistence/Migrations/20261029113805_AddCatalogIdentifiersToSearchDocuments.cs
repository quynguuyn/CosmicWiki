using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CosmicWiki.Discovery.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddCatalogIdentifiersToSearchDocuments : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CATALOG",
                schema: "COSMICWIKI_DISCOVERY",
                table: "SEARCH_DOCUMENTS",
                type: "NVARCHAR2(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CATALOG_VALUE",
                schema: "COSMICWIKI_DISCOVERY",
                table: "SEARCH_DOCUMENTS",
                type: "NVARCHAR2(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_SEARCH_DOCUMENTS_CATALOG_VALUE",
                schema: "COSMICWIKI_DISCOVERY",
                table: "SEARCH_DOCUMENTS",
                column: "CATALOG_VALUE");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_SEARCH_DOCUMENTS_CATALOG_VALUE",
                schema: "COSMICWIKI_DISCOVERY",
                table: "SEARCH_DOCUMENTS");

            migrationBuilder.DropColumn(
                name: "CATALOG",
                schema: "COSMICWIKI_DISCOVERY",
                table: "SEARCH_DOCUMENTS");

            migrationBuilder.DropColumn(
                name: "CATALOG_VALUE",
                schema: "COSMICWIKI_DISCOVERY",
                table: "SEARCH_DOCUMENTS");
        }
    }
}

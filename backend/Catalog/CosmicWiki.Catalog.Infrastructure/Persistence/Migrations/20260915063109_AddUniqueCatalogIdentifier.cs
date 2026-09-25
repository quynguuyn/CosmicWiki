using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CosmicWiki.Catalog.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddUniqueCatalogIdentifier : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_STAR_CATALOG_IDENTIFIERS_CATALOG_VALUE",
                schema: "COSMICWIKI_CATALOG",
                table: "STAR_CATALOG_IDENTIFIERS",
                columns: new[] { "CATALOG", "VALUE" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_STAR_CATALOG_IDENTIFIERS_CATALOG_VALUE",
                schema: "COSMICWIKI_CATALOG",
                table: "STAR_CATALOG_IDENTIFIERS");
        }
    }
}

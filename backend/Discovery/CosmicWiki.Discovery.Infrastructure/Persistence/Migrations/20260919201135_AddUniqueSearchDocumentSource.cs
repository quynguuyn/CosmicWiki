using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CosmicWiki.Discovery.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddUniqueSearchDocumentSource : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_SEARCH_DOCUMENTS_SOURCE_ID",
                schema: "COSMICWIKI_DISCOVERY",
                table: "SEARCH_DOCUMENTS");

            migrationBuilder.CreateIndex(
                name: "IX_SEARCH_DOCUMENTS_SOURCE_TYPE_SOURCE_ID",
                schema: "COSMICWIKI_DISCOVERY",
                table: "SEARCH_DOCUMENTS",
                columns: new[] { "SOURCE_TYPE", "SOURCE_ID" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_SEARCH_DOCUMENTS_SOURCE_TYPE_SOURCE_ID",
                schema: "COSMICWIKI_DISCOVERY",
                table: "SEARCH_DOCUMENTS");

            migrationBuilder.CreateIndex(
                name: "IX_SEARCH_DOCUMENTS_SOURCE_ID",
                schema: "COSMICWIKI_DISCOVERY",
                table: "SEARCH_DOCUMENTS",
                column: "SOURCE_ID");
        }
    }
}

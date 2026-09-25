using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CosmicWiki.Discovery.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialDiscovery : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "COSMICWIKI_DISCOVERY");

            migrationBuilder.CreateTable(
                name: "SEARCH_DOCUMENTS",
                schema: "COSMICWIKI_DISCOVERY",
                columns: table => new
                {
                    ID = table.Column<Guid>(type: "RAW(16)", nullable: false),
                    SOURCE_ID = table.Column<Guid>(type: "RAW(16)", nullable: false),
                    SOURCE_TYPE = table.Column<string>(type: "NVARCHAR2(100)", maxLength: 100, nullable: false),
                    NAME = table.Column<string>(type: "NVARCHAR2(500)", maxLength: 500, nullable: false),
                    DESCRIPTION = table.Column<string>(type: "NCLOB", maxLength: 4000, nullable: true),
                    IMAGE_URL = table.Column<string>(type: "NVARCHAR2(2000)", maxLength: 2000, nullable: true),
                    UPDATED_AT = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SEARCH_DOCUMENTS", x => x.ID);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SEARCH_DOCUMENTS_NAME",
                schema: "COSMICWIKI_DISCOVERY",
                table: "SEARCH_DOCUMENTS",
                column: "NAME");

            migrationBuilder.CreateIndex(
                name: "IX_SEARCH_DOCUMENTS_SOURCE_ID",
                schema: "COSMICWIKI_DISCOVERY",
                table: "SEARCH_DOCUMENTS",
                column: "SOURCE_ID");

            migrationBuilder.CreateIndex(
                name: "IX_SEARCH_DOCUMENTS_SOURCE_TYPE",
                schema: "COSMICWIKI_DISCOVERY",
                table: "SEARCH_DOCUMENTS",
                column: "SOURCE_TYPE");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SEARCH_DOCUMENTS",
                schema: "COSMICWIKI_DISCOVERY");
        }
    }
}

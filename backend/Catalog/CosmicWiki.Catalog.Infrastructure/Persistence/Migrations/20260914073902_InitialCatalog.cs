using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CosmicWiki.Catalog.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialCatalog : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "COSMICWIKI_CATALOG");

            migrationBuilder.CreateTable(
                name: "STARS",
                schema: "COSMICWIKI_CATALOG",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "RAW(16)", nullable: false),
                    Name = table.Column<string>(type: "NVARCHAR2(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_STARS", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "STAR_CATALOG_IDENTIFIERS",
                schema: "COSMICWIKI_CATALOG",
                columns: table => new
                {
                    Catalog = table.Column<string>(type: "NVARCHAR2(50)", maxLength: 50, nullable: false),
                    Value = table.Column<string>(type: "NVARCHAR2(200)", maxLength: 200, nullable: false),
                    STAR_ID = table.Column<Guid>(type: "RAW(16)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_STAR_CATALOG_IDENTIFIERS", x => new { x.STAR_ID, x.Catalog, x.Value });
                    table.ForeignKey(
                        name: "FK_STAR_CATALOG_IDENTIFIERS_STARS_STAR_ID",
                        column: x => x.STAR_ID,
                        principalSchema: "COSMICWIKI_CATALOG",
                        principalTable: "STARS",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "STAR_PARALLAXES",
                schema: "COSMICWIKI_CATALOG",
                columns: table => new
                {
                    ID = table.Column<Guid>(type: "RAW(16)", nullable: false),
                    VALUE_MAS = table.Column<double>(type: "BINARY_DOUBLE", nullable: false),
                    QUALITY = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    REFERENCE_ID = table.Column<Guid>(type: "RAW(16)", nullable: true),
                    STAR_ID = table.Column<Guid>(type: "RAW(16)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_STAR_PARALLAXES", x => x.ID);
                    table.ForeignKey(
                        name: "FK_STAR_PARALLAXES_STARS_STAR_ID",
                        column: x => x.STAR_ID,
                        principalSchema: "COSMICWIKI_CATALOG",
                        principalTable: "STARS",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_STAR_PARALLAXES_REFERENCE_ID",
                schema: "COSMICWIKI_CATALOG",
                table: "STAR_PARALLAXES",
                column: "REFERENCE_ID");

            migrationBuilder.CreateIndex(
                name: "IX_STAR_PARALLAXES_STAR_ID",
                schema: "COSMICWIKI_CATALOG",
                table: "STAR_PARALLAXES",
                column: "STAR_ID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "STAR_CATALOG_IDENTIFIERS",
                schema: "COSMICWIKI_CATALOG");

            migrationBuilder.DropTable(
                name: "STAR_PARALLAXES",
                schema: "COSMICWIKI_CATALOG");

            migrationBuilder.DropTable(
                name: "STARS",
                schema: "COSMICWIKI_CATALOG");
        }
    }
}

using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CosmicWiki.Catalog.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddStarCoordinates : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "STAR_COORDINATES",
                schema: "COSMICWIKI_CATALOG",
                columns: table => new
                {
                    ID = table.Column<Guid>(type: "RAW(16)", nullable: false),
                    RIGHT_ASCENSION = table.Column<double>(type: "BINARY_DOUBLE", nullable: false),
                    DECLINATION = table.Column<double>(type: "BINARY_DOUBLE", nullable: false),
                    COORDINATE_SYSTEM = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    EPOCH = table.Column<double>(type: "BINARY_DOUBLE", nullable: false),
                    WAVELENGTH_BAND = table.Column<int>(type: "NUMBER(10)", nullable: true),
                    QUALITY = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    REFERENCE_ID = table.Column<Guid>(type: "RAW(16)", nullable: true),
                    STAR_ID = table.Column<Guid>(type: "RAW(16)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_STAR_COORDINATES", x => x.ID);
                    table.ForeignKey(
                        name: "FK_STAR_COORDINATES_STARS_STAR_ID",
                        column: x => x.STAR_ID,
                        principalSchema: "COSMICWIKI_CATALOG",
                        principalTable: "STARS",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_STAR_COORDINATES_REFERENCE_ID",
                schema: "COSMICWIKI_CATALOG",
                table: "STAR_COORDINATES",
                column: "REFERENCE_ID");

            migrationBuilder.CreateIndex(
                name: "IX_STAR_COORDINATES_STAR_ID",
                schema: "COSMICWIKI_CATALOG",
                table: "STAR_COORDINATES",
                column: "STAR_ID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "STAR_COORDINATES",
                schema: "COSMICWIKI_CATALOG");
        }
    }
}

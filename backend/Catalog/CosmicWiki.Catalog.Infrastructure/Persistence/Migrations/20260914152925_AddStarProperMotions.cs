using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CosmicWiki.Catalog.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddStarProperMotions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "STAR_PROPER_MOTIONS",
                schema: "COSMICWIKI_CATALOG",
                columns: table => new
                {
                    ID = table.Column<Guid>(type: "RAW(16)", nullable: false),
                    RIGHT_ASCENSION = table.Column<double>(type: "BINARY_DOUBLE", nullable: false),
                    DECLINATION = table.Column<double>(type: "BINARY_DOUBLE", nullable: false),
                    UNCERTAINTY_MAJOR_AXIS = table.Column<double>(type: "BINARY_DOUBLE", nullable: true),
                    UNCERTAINTY_MINOR_AXIS = table.Column<double>(type: "BINARY_DOUBLE", nullable: true),
                    UNCERTAINTY_POSITION_ANGLE = table.Column<double>(type: "BINARY_DOUBLE", nullable: true),
                    QUALITY = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    REFERENCE_ID = table.Column<Guid>(type: "RAW(16)", nullable: true),
                    STAR_ID = table.Column<Guid>(type: "RAW(16)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_STAR_PROPER_MOTIONS", x => x.ID);
                    table.ForeignKey(
                        name: "FK_STAR_PROPER_MOTIONS_STARS_STAR_ID",
                        column: x => x.STAR_ID,
                        principalSchema: "COSMICWIKI_CATALOG",
                        principalTable: "STARS",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_STAR_PROPER_MOTIONS_REFERENCE_ID",
                schema: "COSMICWIKI_CATALOG",
                table: "STAR_PROPER_MOTIONS",
                column: "REFERENCE_ID");

            migrationBuilder.CreateIndex(
                name: "IX_STAR_PROPER_MOTIONS_STAR_ID",
                schema: "COSMICWIKI_CATALOG",
                table: "STAR_PROPER_MOTIONS",
                column: "STAR_ID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "STAR_PROPER_MOTIONS",
                schema: "COSMICWIKI_CATALOG");
        }
    }
}

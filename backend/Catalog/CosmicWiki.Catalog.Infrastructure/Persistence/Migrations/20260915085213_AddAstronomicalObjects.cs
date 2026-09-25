using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CosmicWiki.Catalog.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddAstronomicalObjects : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ASTRONOMICAL_OBJECTS",
                schema: "COSMICWIKI_CATALOG",
                columns: table => new
                {
                    ID = table.Column<Guid>(type: "RAW(16)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ASTRONOMICAL_OBJECTS", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "ASTRONOMICAL_IMAGES",
                schema: "COSMICWIKI_CATALOG",
                columns: table => new
                {
                    ID = table.Column<Guid>(type: "RAW(16)", nullable: false),
                    ASTRONOMICAL_OBJECT_ID = table.Column<Guid>(type: "RAW(16)", nullable: false),
                    URL = table.Column<string>(type: "NVARCHAR2(2000)", maxLength: 2000, nullable: false),
                    URLS = table.Column<string>(type: "CLOB", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ASTRONOMICAL_IMAGES", x => x.ID);
                    table.ForeignKey(
                        name: "FK_ASTRONOMICAL_IMAGES_ASTRONOMICAL_OBJECTS_ASTRONOMICAL_OBJECT_ID",
                        column: x => x.ASTRONOMICAL_OBJECT_ID,
                        principalSchema: "COSMICWIKI_CATALOG",
                        principalTable: "ASTRONOMICAL_OBJECTS",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ASTRONOMICAL_IMAGES_ASTRONOMICAL_OBJECT_ID",
                schema: "COSMICWIKI_CATALOG",
                table: "ASTRONOMICAL_IMAGES",
                column: "ASTRONOMICAL_OBJECT_ID");

            migrationBuilder.AddForeignKey(
                name: "FK_STARS_ASTRONOMICAL_OBJECTS_Id",
                schema: "COSMICWIKI_CATALOG",
                table: "STARS",
                column: "Id",
                principalSchema: "COSMICWIKI_CATALOG",
                principalTable: "ASTRONOMICAL_OBJECTS",
                principalColumn: "ID",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_STARS_ASTRONOMICAL_OBJECTS_Id",
                schema: "COSMICWIKI_CATALOG",
                table: "STARS");

            migrationBuilder.DropTable(
                name: "ASTRONOMICAL_IMAGES",
                schema: "COSMICWIKI_CATALOG");

            migrationBuilder.DropTable(
                name: "ASTRONOMICAL_OBJECTS",
                schema: "COSMICWIKI_CATALOG");
        }
    }
}

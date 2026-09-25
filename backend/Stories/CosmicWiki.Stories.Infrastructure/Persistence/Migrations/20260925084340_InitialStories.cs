using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CosmicWiki.Stories.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialStories : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "COSMICWIKI_STORIES");

            migrationBuilder.CreateTable(
                name: "STORIES",
                schema: "COSMICWIKI_STORIES",
                columns: table => new
                {
                    ID = table.Column<Guid>(type: "RAW(16)", nullable: false),
                    CATEGORY = table.Column<string>(type: "NVARCHAR2(100)", maxLength: 100, nullable: false),
                    ERA = table.Column<string>(type: "NVARCHAR2(200)", maxLength: 200, nullable: false),
                    CODE = table.Column<string>(type: "NVARCHAR2(100)", maxLength: 100, nullable: false),
                    TITLE = table.Column<string>(type: "NVARCHAR2(500)", maxLength: 500, nullable: false),
                    SLUG = table.Column<string>(type: "NVARCHAR2(500)", maxLength: 500, nullable: false),
                    DESCRIPTION = table.Column<string>(type: "NCLOB", maxLength: 4000, nullable: false),
                    CONTENT = table.Column<string>(type: "NCLOB", maxLength: 20000, nullable: false),
                    AUTHOR = table.Column<string>(type: "NVARCHAR2(300)", maxLength: 300, nullable: false),
                    READ_TIME_MINUTES = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    IMAGE_URL = table.Column<string>(type: "NVARCHAR2(2000)", maxLength: 2000, nullable: true),
                    IMAGE_ALT = table.Column<string>(type: "NVARCHAR2(1000)", maxLength: 1000, nullable: true),
                    STATUS = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    CREATED_AT_UTC = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: false),
                    UPDATED_AT_UTC = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_STORIES", x => x.ID);
                });

            migrationBuilder.CreateIndex(
                name: "IX_STORIES_CATEGORY",
                schema: "COSMICWIKI_STORIES",
                table: "STORIES",
                column: "CATEGORY");

            migrationBuilder.CreateIndex(
                name: "IX_STORIES_CODE",
                schema: "COSMICWIKI_STORIES",
                table: "STORIES",
                column: "CODE",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_STORIES_CREATED_AT_UTC",
                schema: "COSMICWIKI_STORIES",
                table: "STORIES",
                column: "CREATED_AT_UTC");

            migrationBuilder.CreateIndex(
                name: "IX_STORIES_SLUG",
                schema: "COSMICWIKI_STORIES",
                table: "STORIES",
                column: "SLUG",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "STORIES",
                schema: "COSMICWIKI_STORIES");
        }
    }
}

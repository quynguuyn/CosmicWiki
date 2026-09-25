using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CosmicWiki.Visit.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddVisitContent : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "EVENTS",
                schema: "COSMICWIKI_VISIT",
                columns: table => new
                {
                    ID = table.Column<Guid>(type: "RAW(16)", nullable: false),
                    EVENT_DATE_UTC = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: false),
                    NAME = table.Column<string>(type: "NVARCHAR2(300)", maxLength: 300, nullable: false),
                    DESCRIPTION = table.Column<string>(type: "NCLOB", maxLength: 4000, nullable: false),
                    SPECIFICATION = table.Column<string>(type: "NVARCHAR2(500)", maxLength: 500, nullable: true),
                    LOCATION = table.Column<string>(type: "NVARCHAR2(500)", maxLength: 500, nullable: true),
                    STATUS = table.Column<string>(type: "NVARCHAR2(100)", maxLength: 100, nullable: false),
                    STATUS_TYPE = table.Column<string>(type: "NVARCHAR2(50)", maxLength: 50, nullable: true),
                    ACTION = table.Column<string>(type: "NVARCHAR2(100)", maxLength: 100, nullable: true),
                    DISPLAY_ORDER = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    IS_PUBLISHED = table.Column<bool>(type: "BOOLEAN", nullable: false),
                    CREATED_AT_UTC = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: false),
                    UPDATED_AT_UTC = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EVENTS", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "EXHIBITIONS",
                schema: "COSMICWIKI_VISIT",
                columns: table => new
                {
                    ID = table.Column<Guid>(type: "RAW(16)", nullable: false),
                    NUMBER = table.Column<string>(type: "NVARCHAR2(32)", maxLength: 32, nullable: false),
                    CATEGORY = table.Column<string>(type: "NVARCHAR2(200)", maxLength: 200, nullable: false),
                    TITLE = table.Column<string>(type: "NVARCHAR2(300)", maxLength: 300, nullable: false),
                    DESCRIPTION = table.Column<string>(type: "NCLOB", maxLength: 4000, nullable: false),
                    STATUS = table.Column<string>(type: "NVARCHAR2(100)", maxLength: 100, nullable: false),
                    IMAGE_URL = table.Column<string>(type: "NVARCHAR2(2000)", maxLength: 2000, nullable: true),
                    DISPLAY_ORDER = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    IS_PUBLISHED = table.Column<bool>(type: "BOOLEAN", nullable: false),
                    CREATED_AT_UTC = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: false),
                    UPDATED_AT_UTC = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EXHIBITIONS", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "FAQS",
                schema: "COSMICWIKI_VISIT",
                columns: table => new
                {
                    ID = table.Column<Guid>(type: "RAW(16)", nullable: false),
                    QUESTION = table.Column<string>(type: "NVARCHAR2(1000)", maxLength: 1000, nullable: false),
                    ANSWER = table.Column<string>(type: "NCLOB", maxLength: 10000, nullable: false),
                    DISPLAY_ORDER = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    IS_PUBLISHED = table.Column<bool>(type: "BOOLEAN", nullable: false),
                    CREATED_AT_UTC = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: false),
                    UPDATED_AT_UTC = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FAQS", x => x.ID);
                });

            migrationBuilder.CreateIndex(
                name: "IX_EVENTS_IS_PUBLISHED_EVENT_DATE_UTC",
                schema: "COSMICWIKI_VISIT",
                table: "EVENTS",
                columns: new[] { "IS_PUBLISHED", "EVENT_DATE_UTC" });

            migrationBuilder.CreateIndex(
                name: "IX_EXHIBITIONS_IS_PUBLISHED_DISPLAY_ORDER",
                schema: "COSMICWIKI_VISIT",
                table: "EXHIBITIONS",
                columns: new[] { "IS_PUBLISHED", "DISPLAY_ORDER" });

            migrationBuilder.CreateIndex(
                name: "IX_FAQS_IS_PUBLISHED_DISPLAY_ORDER",
                schema: "COSMICWIKI_VISIT",
                table: "FAQS",
                columns: new[] { "IS_PUBLISHED", "DISPLAY_ORDER" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EVENTS",
                schema: "COSMICWIKI_VISIT");

            migrationBuilder.DropTable(
                name: "EXHIBITIONS",
                schema: "COSMICWIKI_VISIT");

            migrationBuilder.DropTable(
                name: "FAQS",
                schema: "COSMICWIKI_VISIT");
        }
    }
}

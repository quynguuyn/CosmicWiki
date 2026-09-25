using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CosmicWiki.Catalog.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddCatalogOutbox : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "OUTBOX_MESSAGES",
                schema: "COSMICWIKI_CATALOG",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "RAW(16)", nullable: false),
                    EventId = table.Column<Guid>(type: "RAW(16)", nullable: false),
                    Topic = table.Column<string>(type: "NVARCHAR2(200)", maxLength: 200, nullable: false),
                    Payload = table.Column<string>(type: "NVARCHAR2(2000)", nullable: false),
                    OccurredAtUtc = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: false),
                    PublishedAtUtc = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OUTBOX_MESSAGES", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_OUTBOX_MESSAGES_EventId",
                schema: "COSMICWIKI_CATALOG",
                table: "OUTBOX_MESSAGES",
                column: "EventId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_OUTBOX_MESSAGES_PublishedAtUtc",
                schema: "COSMICWIKI_CATALOG",
                table: "OUTBOX_MESSAGES",
                column: "PublishedAtUtc");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "OUTBOX_MESSAGES",
                schema: "COSMICWIKI_CATALOG");
        }
    }
}

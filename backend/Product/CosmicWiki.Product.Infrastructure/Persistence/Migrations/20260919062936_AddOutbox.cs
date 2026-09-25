using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CosmicWiki.Product.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddOutbox : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "OUTBOX_MESSAGES",
                schema: "COSMICWIKI_PRODUCT",
                columns: table => new
                {
                    ID = table.Column<Guid>(type: "RAW(16)", nullable: false),
                    EVENT_ID = table.Column<Guid>(type: "RAW(16)", nullable: false),
                    TOPIC = table.Column<string>(type: "NVARCHAR2(200)", maxLength: 200, nullable: false),
                    TYPE = table.Column<string>(type: "NVARCHAR2(500)", maxLength: 500, nullable: false),
                    PAYLOAD = table.Column<string>(type: "NVARCHAR2(2000)", nullable: false),
                    OCCURRED_AT_UTC = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: false),
                    PUBLISHED_AT_UTC = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: true),
                    ATTEMPTS = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    ERROR = table.Column<string>(type: "NCLOB", maxLength: 4000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OUTBOX_MESSAGES", x => x.ID);
                });

            migrationBuilder.CreateIndex(
                name: "IX_OUTBOX_MESSAGES_EVENT_ID",
                schema: "COSMICWIKI_PRODUCT",
                table: "OUTBOX_MESSAGES",
                column: "EVENT_ID",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_OUTBOX_MESSAGES_PUBLISHED_AT_UTC",
                schema: "COSMICWIKI_PRODUCT",
                table: "OUTBOX_MESSAGES",
                column: "PUBLISHED_AT_UTC");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "OUTBOX_MESSAGES",
                schema: "COSMICWIKI_PRODUCT");
        }
    }
}

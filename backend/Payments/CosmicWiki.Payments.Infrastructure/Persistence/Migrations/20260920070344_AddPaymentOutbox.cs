using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CosmicWiki.Payments.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddPaymentOutbox : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "OUTBOX_MESSAGES",
                schema: "COSMICWIKI_PAYMENTS",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "RAW(16)", nullable: false),
                    EventId = table.Column<Guid>(type: "RAW(16)", nullable: false),
                    AggregateId = table.Column<Guid>(type: "RAW(16)", nullable: false),
                    Topic = table.Column<string>(type: "NVARCHAR2(200)", maxLength: 200, nullable: false),
                    Type = table.Column<string>(type: "NVARCHAR2(500)", maxLength: 500, nullable: false),
                    Payload = table.Column<string>(type: "NVARCHAR2(2000)", nullable: false),
                    OccurredAtUtc = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: false),
                    PublishedAtUtc = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: true),
                    Attempts = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    Error = table.Column<string>(type: "NCLOB", maxLength: 4000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OUTBOX_MESSAGES", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_OUTBOX_MESSAGES_EventId",
                schema: "COSMICWIKI_PAYMENTS",
                table: "OUTBOX_MESSAGES",
                column: "EventId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_OUTBOX_MESSAGES_PublishedAtUtc",
                schema: "COSMICWIKI_PAYMENTS",
                table: "OUTBOX_MESSAGES",
                column: "PublishedAtUtc");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "OUTBOX_MESSAGES",
                schema: "COSMICWIKI_PAYMENTS");
        }
    }
}

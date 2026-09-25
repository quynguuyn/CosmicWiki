using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CosmicWiki.Ordering.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddPaymentInbox : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "INBOX_MESSAGES",
                schema: "COSMICWIKI_ORDERING",
                columns: table => new
                {
                    EventId = table.Column<Guid>(type: "RAW(16)", nullable: false),
                    ReceivedAtUtc = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_INBOX_MESSAGES", x => x.EventId);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "INBOX_MESSAGES",
                schema: "COSMICWIKI_ORDERING");
        }
    }
}

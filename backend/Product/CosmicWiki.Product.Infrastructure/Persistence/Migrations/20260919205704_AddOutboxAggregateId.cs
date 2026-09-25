using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CosmicWiki.Product.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddOutboxAggregateId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "AGGREGATE_ID",
                schema: "COSMICWIKI_PRODUCT",
                table: "OUTBOX_MESSAGES",
                type: "RAW(16)",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AGGREGATE_ID",
                schema: "COSMICWIKI_PRODUCT",
                table: "OUTBOX_MESSAGES");
        }
    }
}

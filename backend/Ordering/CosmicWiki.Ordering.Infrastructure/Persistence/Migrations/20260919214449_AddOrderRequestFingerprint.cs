using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CosmicWiki.Ordering.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddOrderRequestFingerprint : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "RequestFingerprint",
                schema: "COSMICWIKI_ORDERING",
                table: "ORDERS",
                type: "NVARCHAR2(64)",
                maxLength: 64,
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RequestFingerprint",
                schema: "COSMICWIKI_ORDERING",
                table: "ORDERS");
        }
    }
}

using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CosmicWiki.Ordering.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddCustomerEmailToOrders : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CustomerEmail",
                schema: "COSMICWIKI_ORDERING",
                table: "ORDERS",
                type: "NVARCHAR2(320)",
                maxLength: 320,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CustomerEmail",
                schema: "COSMICWIKI_ORDERING",
                table: "ORDERS");
        }
    }
}

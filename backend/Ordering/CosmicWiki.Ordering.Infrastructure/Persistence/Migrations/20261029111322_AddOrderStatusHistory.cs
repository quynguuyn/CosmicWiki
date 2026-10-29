using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CosmicWiki.Ordering.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddOrderStatusHistory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ORDER_STATUS_HISTORY",
                schema: "COSMICWIKI_ORDERING",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "RAW(16)", nullable: false),
                    OrderId = table.Column<Guid>(type: "RAW(16)", nullable: false),
                    Status = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    ChangedAtUtc = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ORDER_STATUS_HISTORY", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ORDER_STATUS_HISTORY_ORDERS_OrderId",
                        column: x => x.OrderId,
                        principalSchema: "COSMICWIKI_ORDERING",
                        principalTable: "ORDERS",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ORDER_STATUS_HISTORY_OrderId_ChangedAtUtc",
                schema: "COSMICWIKI_ORDERING",
                table: "ORDER_STATUS_HISTORY",
                columns: new[] { "OrderId", "ChangedAtUtc" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ORDER_STATUS_HISTORY",
                schema: "COSMICWIKI_ORDERING");
        }
    }
}

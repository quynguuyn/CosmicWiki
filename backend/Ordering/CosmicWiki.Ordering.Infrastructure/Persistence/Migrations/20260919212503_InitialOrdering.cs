using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CosmicWiki.Ordering.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialOrdering : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "COSMICWIKI_ORDERING");

            migrationBuilder.CreateTable(
                name: "ORDERS",
                schema: "COSMICWIKI_ORDERING",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "RAW(16)", nullable: false),
                    CustomerId = table.Column<Guid>(type: "RAW(16)", nullable: false),
                    Currency = table.Column<string>(type: "NVARCHAR2(3)", maxLength: 3, nullable: false),
                    IdempotencyKey = table.Column<string>(type: "NVARCHAR2(200)", maxLength: 200, nullable: false),
                    Status = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    TotalAmount = table.Column<decimal>(type: "DECIMAL(18,2)", precision: 18, scale: 2, nullable: false),
                    PaymentIntentId = table.Column<string>(type: "NVARCHAR2(200)", maxLength: 200, nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: false),
                    PaidAtUtc = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ORDERS", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "OUTBOX_MESSAGES",
                schema: "COSMICWIKI_ORDERING",
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

            migrationBuilder.CreateTable(
                name: "ORDER_LINES",
                schema: "COSMICWIKI_ORDERING",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "RAW(16)", nullable: false),
                    OrderId = table.Column<Guid>(type: "RAW(16)", nullable: false),
                    ProductId = table.Column<Guid>(type: "RAW(16)", nullable: false),
                    SKU = table.Column<string>(type: "NVARCHAR2(100)", maxLength: 100, nullable: false),
                    ProductName = table.Column<string>(type: "NVARCHAR2(300)", maxLength: 300, nullable: false),
                    UnitPrice = table.Column<decimal>(type: "DECIMAL(18,2)", precision: 18, scale: 2, nullable: false),
                    Quantity = table.Column<int>(type: "NUMBER(10)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ORDER_LINES", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ORDER_LINES_ORDERS_OrderId",
                        column: x => x.OrderId,
                        principalSchema: "COSMICWIKI_ORDERING",
                        principalTable: "ORDERS",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ORDER_LINES_OrderId",
                schema: "COSMICWIKI_ORDERING",
                table: "ORDER_LINES",
                column: "OrderId");

            migrationBuilder.CreateIndex(
                name: "IX_ORDERS_IdempotencyKey",
                schema: "COSMICWIKI_ORDERING",
                table: "ORDERS",
                column: "IdempotencyKey",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_OUTBOX_MESSAGES_EventId",
                schema: "COSMICWIKI_ORDERING",
                table: "OUTBOX_MESSAGES",
                column: "EventId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_OUTBOX_MESSAGES_PublishedAtUtc",
                schema: "COSMICWIKI_ORDERING",
                table: "OUTBOX_MESSAGES",
                column: "PublishedAtUtc");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ORDER_LINES",
                schema: "COSMICWIKI_ORDERING");

            migrationBuilder.DropTable(
                name: "OUTBOX_MESSAGES",
                schema: "COSMICWIKI_ORDERING");

            migrationBuilder.DropTable(
                name: "ORDERS",
                schema: "COSMICWIKI_ORDERING");
        }
    }
}

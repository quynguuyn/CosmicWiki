using CosmicWiki.Ordering.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore;

#nullable disable

namespace CosmicWiki.Ordering.Infrastructure.Persistence.Migrations;

[DbContext(typeof(OrderingDbContext))]
[Migration("20261030050000_AddOrderItemDetailsAndNotifications")]
public partial class AddOrderItemDetailsAndNotifications : Migration
{
    protected override void BuildTargetModel(ModelBuilder modelBuilder) =>
        new MigrationSnapshotBuilder().Build(modelBuilder);

    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<string>(
            name: "ProductImageUrl",
            schema: "COSMICWIKI_ORDERING",
            table: "ORDER_LINES",
            type: "NVARCHAR2(2000)",
            maxLength: 2000,
            nullable: true);

        migrationBuilder.CreateTable(
            name: "ORDER_NOTIFICATIONS",
            schema: "COSMICWIKI_ORDERING",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "RAW(16)", nullable: false),
                CustomerId = table.Column<Guid>(type: "RAW(16)", nullable: false),
                OrderId = table.Column<Guid>(type: "RAW(16)", nullable: false),
                Title = table.Column<string>(type: "NVARCHAR2(200)", maxLength: 200, nullable: false),
                Message = table.Column<string>(type: "NVARCHAR2(1000)", maxLength: 1000, nullable: false),
                Status = table.Column<string>(type: "NVARCHAR2(32)", maxLength: 32, nullable: false),
                CreatedAtUtc = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: false),
                ReadAtUtc = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_ORDER_NOTIFICATIONS", x => x.Id);
            });

        migrationBuilder.CreateIndex(
            name: "IX_ORDER_NOTIFICATIONS_CustomerId_CreatedAtUtc",
            schema: "COSMICWIKI_ORDERING",
            table: "ORDER_NOTIFICATIONS",
            columns: new[] { "CustomerId", "CreatedAtUtc" });

        migrationBuilder.CreateIndex(
            name: "IX_ORDER_NOTIFICATIONS_CustomerId_ReadAtUtc",
            schema: "COSMICWIKI_ORDERING",
            table: "ORDER_NOTIFICATIONS",
            columns: new[] { "CustomerId", "ReadAtUtc" });
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "ORDER_NOTIFICATIONS",
            schema: "COSMICWIKI_ORDERING");

        migrationBuilder.DropColumn(
            name: "ProductImageUrl",
            schema: "COSMICWIKI_ORDERING",
            table: "ORDER_LINES");
    }

    private sealed class MigrationSnapshotBuilder : OrderingDbContextModelSnapshot
    {
        public void Build(ModelBuilder modelBuilder) => BuildModel(modelBuilder);
    }
}

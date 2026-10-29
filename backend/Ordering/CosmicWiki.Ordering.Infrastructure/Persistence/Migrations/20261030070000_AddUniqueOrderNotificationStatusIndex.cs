using CosmicWiki.Ordering.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CosmicWiki.Ordering.Infrastructure.Persistence.Migrations;

[DbContext(typeof(OrderingDbContext))]
[Migration("20261030070000_AddUniqueOrderNotificationStatusIndex")]
public partial class AddUniqueOrderNotificationStatusIndex : Migration
{
    protected override void BuildTargetModel(ModelBuilder modelBuilder) =>
        new MigrationSnapshotBuilder().Build(modelBuilder);

    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateIndex(
            name: "IX_ORDER_NOTIFICATIONS_CustomerId_OrderId_Status",
            schema: "COSMICWIKI_ORDERING",
            table: "ORDER_NOTIFICATIONS",
            columns: new[] { "CustomerId", "OrderId", "Status" },
            unique: true);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropIndex(
            name: "IX_ORDER_NOTIFICATIONS_CustomerId_OrderId_Status",
            schema: "COSMICWIKI_ORDERING",
            table: "ORDER_NOTIFICATIONS");
    }

    private sealed class MigrationSnapshotBuilder : OrderingDbContextModelSnapshot
    {
        public void Build(ModelBuilder modelBuilder) => BuildModel(modelBuilder);
    }
}

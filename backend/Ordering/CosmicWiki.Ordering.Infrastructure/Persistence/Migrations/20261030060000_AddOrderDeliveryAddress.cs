using CosmicWiki.Ordering.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CosmicWiki.Ordering.Infrastructure.Persistence.Migrations;

[DbContext(typeof(OrderingDbContext))]
[Migration("20261030060000_AddOrderDeliveryAddress")]
public partial class AddOrderDeliveryAddress : Migration
{
    protected override void BuildTargetModel(ModelBuilder modelBuilder) =>
        new MigrationSnapshotBuilder().Build(modelBuilder);

    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<string>(
            name: "DELIVERY_ADDRESS",
            schema: "COSMICWIKI_ORDERING",
            table: "ORDERS",
            type: "NVARCHAR2(500)",
            maxLength: 500,
            nullable: true);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "DELIVERY_ADDRESS",
            schema: "COSMICWIKI_ORDERING",
            table: "ORDERS");
    }

    private sealed class MigrationSnapshotBuilder : OrderingDbContextModelSnapshot
    {
        public void Build(ModelBuilder modelBuilder) => BuildModel(modelBuilder);
    }
}

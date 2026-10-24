using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CosmicWiki.Ordering.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddDeliveryTracking : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<double>(
                name: "DELIVERY_LATITUDE",
                schema: "COSMICWIKI_ORDERING",
                table: "ORDERS",
                type: "BINARY_DOUBLE",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DELIVERY_LOCATION_UPDATED_AT_UTC",
                schema: "COSMICWIKI_ORDERING",
                table: "ORDERS",
                type: "TIMESTAMP(7)",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "DELIVERY_LONGITUDE",
                schema: "COSMICWIKI_ORDERING",
                table: "ORDERS",
                type: "BINARY_DOUBLE",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "DELIVERY_PERSON_ID",
                schema: "COSMICWIKI_ORDERING",
                table: "ORDERS",
                type: "RAW(16)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DELIVERY_PERSON_NAME",
                schema: "COSMICWIKI_ORDERING",
                table: "ORDERS",
                type: "NVARCHAR2(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_ORDERS_DELIVERY_PERSON_ID_Status",
                schema: "COSMICWIKI_ORDERING",
                table: "ORDERS",
                columns: new[] { "DELIVERY_PERSON_ID", "Status" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ORDERS_DELIVERY_PERSON_ID_Status",
                schema: "COSMICWIKI_ORDERING",
                table: "ORDERS");

            migrationBuilder.DropColumn(
                name: "DELIVERY_LATITUDE",
                schema: "COSMICWIKI_ORDERING",
                table: "ORDERS");

            migrationBuilder.DropColumn(
                name: "DELIVERY_LOCATION_UPDATED_AT_UTC",
                schema: "COSMICWIKI_ORDERING",
                table: "ORDERS");

            migrationBuilder.DropColumn(
                name: "DELIVERY_LONGITUDE",
                schema: "COSMICWIKI_ORDERING",
                table: "ORDERS");

            migrationBuilder.DropColumn(
                name: "DELIVERY_PERSON_ID",
                schema: "COSMICWIKI_ORDERING",
                table: "ORDERS");

            migrationBuilder.DropColumn(
                name: "DELIVERY_PERSON_NAME",
                schema: "COSMICWIKI_ORDERING",
                table: "ORDERS");
        }
    }
}

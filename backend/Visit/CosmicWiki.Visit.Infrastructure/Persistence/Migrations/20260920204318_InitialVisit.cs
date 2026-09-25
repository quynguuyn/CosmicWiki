using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CosmicWiki.Visit.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialVisit : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "COSMICWIKI_VISIT");

            migrationBuilder.CreateTable(
                name: "VISIT_REQUESTS",
                schema: "COSMICWIKI_VISIT",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "RAW(16)", nullable: false),
                    ReferenceCode = table.Column<string>(type: "NVARCHAR2(16)", maxLength: 16, nullable: false),
                    Name = table.Column<string>(type: "NVARCHAR2(200)", maxLength: 200, nullable: false),
                    Email = table.Column<string>(type: "NVARCHAR2(320)", maxLength: 320, nullable: false),
                    RequestedDateUtc = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: false),
                    PartySize = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    Notes = table.Column<string>(type: "NVARCHAR2(2000)", maxLength: 2000, nullable: true),
                    Status = table.Column<string>(type: "NVARCHAR2(32)", maxLength: 32, nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VISIT_REQUESTS", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_VISIT_REQUESTS_ReferenceCode",
                schema: "COSMICWIKI_VISIT",
                table: "VISIT_REQUESTS",
                column: "ReferenceCode",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "VISIT_REQUESTS",
                schema: "COSMICWIKI_VISIT");
        }
    }
}

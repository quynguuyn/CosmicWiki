using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CosmicWiki.Auth.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialAuth : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "USERS",
                columns: table => new
                {
                    ID = table.Column<Guid>(type: "RAW(16)", nullable: false),
                    USERNAME = table.Column<string>(type: "NVARCHAR2(50)", maxLength: 50, nullable: false),
                    EMAIL = table.Column<string>(type: "NVARCHAR2(254)", maxLength: 254, nullable: false),
                    PASSWORD_HASH = table.Column<string>(type: "NVARCHAR2(2000)", nullable: false),
                    STATUS = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    CREATED_AT = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: false),
                    UPDATED_AT = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_USERS", x => x.ID);
                });

            migrationBuilder.CreateIndex(
                name: "IX_USERS_EMAIL",
                table: "USERS",
                column: "EMAIL",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_USERS_USERNAME",
                table: "USERS",
                column: "USERNAME",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "USERS");
        }
    }
}

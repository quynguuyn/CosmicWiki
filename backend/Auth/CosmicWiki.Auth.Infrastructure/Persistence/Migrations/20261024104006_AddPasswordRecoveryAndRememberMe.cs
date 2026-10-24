using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CosmicWiki.Auth.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddPasswordRecoveryAndRememberMe : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "REMEMBER_ME",
                table: "REFRESH_TOKENS",
                type: "BOOLEAN",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "PASSWORD_RESET_TOKENS",
                columns: table => new
                {
                    ID = table.Column<Guid>(type: "RAW(16)", nullable: false),
                    USER_ID = table.Column<Guid>(type: "RAW(16)", nullable: false),
                    TOKEN_HASH = table.Column<string>(type: "NVARCHAR2(64)", maxLength: 64, nullable: false),
                    EXPIRES_AT_UTC = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: false),
                    USED_AT_UTC = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PASSWORD_RESET_TOKENS", x => x.ID);
                    table.ForeignKey(
                        name: "FK_PASSWORD_RESET_TOKENS_USERS_USER_ID",
                        column: x => x.USER_ID,
                        principalTable: "USERS",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PASSWORD_RESET_TOKENS_TOKEN_HASH",
                table: "PASSWORD_RESET_TOKENS",
                column: "TOKEN_HASH",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PASSWORD_RESET_TOKENS_USER_ID",
                table: "PASSWORD_RESET_TOKENS",
                column: "USER_ID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PASSWORD_RESET_TOKENS");

            migrationBuilder.DropColumn(
                name: "REMEMBER_ME",
                table: "REFRESH_TOKENS");
        }
    }
}

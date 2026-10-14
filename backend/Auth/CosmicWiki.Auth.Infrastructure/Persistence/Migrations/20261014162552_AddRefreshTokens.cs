using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CosmicWiki.Auth.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddRefreshTokens : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "REFRESH_TOKENS",
                columns: table => new
                {
                    ID = table.Column<Guid>(type: "RAW(16)", nullable: false),
                    USER_ID = table.Column<Guid>(type: "RAW(16)", nullable: false),
                    FAMILY_ID = table.Column<Guid>(type: "RAW(16)", nullable: false),
                    TOKEN_HASH = table.Column<string>(type: "NVARCHAR2(64)", maxLength: 64, nullable: false),
                    CREATED_AT_UTC = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: false),
                    EXPIRES_AT_UTC = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: false),
                    REVOKED_AT_UTC = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: true),
                    REPLACED_BY_TOKEN_HASH = table.Column<string>(type: "NVARCHAR2(64)", maxLength: 64, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_REFRESH_TOKENS", x => x.ID);
                    table.ForeignKey(
                        name: "FK_REFRESH_TOKENS_USERS_USER_ID",
                        column: x => x.USER_ID,
                        principalTable: "USERS",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_REFRESH_TOKENS_TOKEN_HASH",
                table: "REFRESH_TOKENS",
                column: "TOKEN_HASH",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_REFRESH_TOKENS_USER_ID_FAMILY_ID",
                table: "REFRESH_TOKENS",
                columns: new[] { "USER_ID", "FAMILY_ID" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "REFRESH_TOKENS");
        }
    }
}

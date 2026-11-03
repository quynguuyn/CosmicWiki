using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CosmicWiki.Stories.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddSupportChat : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SUPPORT_AGENT_PRESENCE",
                schema: "COSMICWIKI_STORIES",
                columns: table => new
                {
                    USER_ID = table.Column<Guid>(type: "RAW(16)", nullable: false),
                    ACCEPTING_CHATS = table.Column<bool>(type: "BOOLEAN", nullable: false),
                    LAST_SEEN_AT_UTC = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: false),
                    LAST_ASSIGNED_AT_UTC = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SUPPORT_AGENT_PRESENCE", x => x.USER_ID);
                });

            migrationBuilder.CreateTable(
                name: "SUPPORT_CONVERSATIONS",
                schema: "COSMICWIKI_STORIES",
                columns: table => new
                {
                    ID = table.Column<Guid>(type: "RAW(16)", nullable: false),
                    CUSTOMER_USER_ID = table.Column<Guid>(type: "RAW(16)", nullable: true),
                    GUEST_TOKEN_HASH = table.Column<string>(type: "NVARCHAR2(64)", maxLength: 64, nullable: true),
                    ASSIGNED_SUPPORT_ID = table.Column<Guid>(type: "RAW(16)", nullable: true),
                    STATUS = table.Column<string>(type: "NVARCHAR2(20)", maxLength: 20, nullable: false),
                    CREATED_AT_UTC = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: false),
                    UPDATED_AT_UTC = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: false),
                    LAST_MESSAGE_AT_UTC = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SUPPORT_CONVERSATIONS", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "SUPPORT_MESSAGES",
                schema: "COSMICWIKI_STORIES",
                columns: table => new
                {
                    ID = table.Column<Guid>(type: "RAW(16)", nullable: false),
                    CONVERSATION_ID = table.Column<Guid>(type: "RAW(16)", nullable: false),
                    SENDER_TYPE = table.Column<string>(type: "NVARCHAR2(20)", maxLength: 20, nullable: false),
                    SENDER_USER_ID = table.Column<Guid>(type: "RAW(16)", nullable: true),
                    BODY = table.Column<string>(type: "NCLOB", maxLength: 4000, nullable: false),
                    CREATED_AT_UTC = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SUPPORT_MESSAGES", x => x.ID);
                    table.ForeignKey(
                        name: "FK_SUPPORT_MESSAGES_SUPPORT_CONVERSATIONS_CONVERSATION_ID",
                        column: x => x.CONVERSATION_ID,
                        principalSchema: "COSMICWIKI_STORIES",
                        principalTable: "SUPPORT_CONVERSATIONS",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SUPPORT_CONVERSATIONS_ASSIGNED_SUPPORT_ID_STATUS",
                schema: "COSMICWIKI_STORIES",
                table: "SUPPORT_CONVERSATIONS",
                columns: new[] { "ASSIGNED_SUPPORT_ID", "STATUS" });

            migrationBuilder.CreateIndex(
                name: "IX_SUPPORT_CONVERSATIONS_CUSTOMER_USER_ID_STATUS",
                schema: "COSMICWIKI_STORIES",
                table: "SUPPORT_CONVERSATIONS",
                columns: new[] { "CUSTOMER_USER_ID", "STATUS" });

            migrationBuilder.CreateIndex(
                name: "IX_SUPPORT_CONVERSATIONS_GUEST_TOKEN_HASH",
                schema: "COSMICWIKI_STORIES",
                table: "SUPPORT_CONVERSATIONS",
                column: "GUEST_TOKEN_HASH",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SUPPORT_CONVERSATIONS_STATUS_CREATED_AT_UTC",
                schema: "COSMICWIKI_STORIES",
                table: "SUPPORT_CONVERSATIONS",
                columns: new[] { "STATUS", "CREATED_AT_UTC" });

            migrationBuilder.CreateIndex(
                name: "IX_SUPPORT_MESSAGES_CONVERSATION_ID_CREATED_AT_UTC",
                schema: "COSMICWIKI_STORIES",
                table: "SUPPORT_MESSAGES",
                columns: new[] { "CONVERSATION_ID", "CREATED_AT_UTC" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SUPPORT_AGENT_PRESENCE",
                schema: "COSMICWIKI_STORIES");

            migrationBuilder.DropTable(
                name: "SUPPORT_MESSAGES",
                schema: "COSMICWIKI_STORIES");

            migrationBuilder.DropTable(
                name: "SUPPORT_CONVERSATIONS",
                schema: "COSMICWIKI_STORIES");
        }
    }
}

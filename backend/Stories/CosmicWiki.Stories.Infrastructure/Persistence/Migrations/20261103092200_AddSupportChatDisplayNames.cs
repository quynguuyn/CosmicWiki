using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CosmicWiki.Stories.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddSupportChatDisplayNames : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "SENDER_DISPLAY_NAME",
                schema: "COSMICWIKI_STORIES",
                table: "SUPPORT_MESSAGES",
                type: "NVARCHAR2(256)",
                maxLength: 256,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ASSIGNED_SUPPORT_DISPLAY_NAME",
                schema: "COSMICWIKI_STORIES",
                table: "SUPPORT_CONVERSATIONS",
                type: "NVARCHAR2(256)",
                maxLength: 256,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CUSTOMER_DISPLAY_NAME",
                schema: "COSMICWIKI_STORIES",
                table: "SUPPORT_CONVERSATIONS",
                type: "NVARCHAR2(256)",
                maxLength: 256,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DISPLAY_NAME",
                schema: "COSMICWIKI_STORIES",
                table: "SUPPORT_AGENT_PRESENCE",
                type: "NVARCHAR2(256)",
                maxLength: 256,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SENDER_DISPLAY_NAME",
                schema: "COSMICWIKI_STORIES",
                table: "SUPPORT_MESSAGES");

            migrationBuilder.DropColumn(
                name: "ASSIGNED_SUPPORT_DISPLAY_NAME",
                schema: "COSMICWIKI_STORIES",
                table: "SUPPORT_CONVERSATIONS");

            migrationBuilder.DropColumn(
                name: "CUSTOMER_DISPLAY_NAME",
                schema: "COSMICWIKI_STORIES",
                table: "SUPPORT_CONVERSATIONS");

            migrationBuilder.DropColumn(
                name: "DISPLAY_NAME",
                schema: "COSMICWIKI_STORIES",
                table: "SUPPORT_AGENT_PRESENCE");
        }
    }
}

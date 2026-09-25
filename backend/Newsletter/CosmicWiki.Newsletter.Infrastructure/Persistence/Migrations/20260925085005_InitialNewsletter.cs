using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CosmicWiki.Newsletter.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialNewsletter : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "COSMICWIKI_NEWSLETTER");

            migrationBuilder.CreateTable(
                name: "NEWSLETTER_SUBSCRIPTIONS",
                schema: "COSMICWIKI_NEWSLETTER",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "RAW(16)", nullable: false),
                    Email = table.Column<string>(type: "NVARCHAR2(320)", maxLength: 320, nullable: false),
                    Status = table.Column<string>(type: "NVARCHAR2(32)", maxLength: 32, nullable: false),
                    ConfirmationTokenHash = table.Column<string>(type: "NVARCHAR2(128)", maxLength: 128, nullable: false),
                    ConfirmationTokenExpiresAtUtc = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: false),
                    ConfirmedAtUtc = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: true),
                    UnsubscribedAtUtc = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NEWSLETTER_SUBSCRIPTIONS", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "NEWSLETTER_DELIVERIES",
                schema: "COSMICWIKI_NEWSLETTER",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "RAW(16)", nullable: false),
                    SubscriptionId = table.Column<Guid>(type: "RAW(16)", nullable: false),
                    Status = table.Column<string>(type: "NVARCHAR2(32)", maxLength: 32, nullable: false),
                    RecipientEmail = table.Column<string>(type: "NVARCHAR2(320)", maxLength: 320, nullable: false),
                    Subject = table.Column<string>(type: "NVARCHAR2(500)", maxLength: 500, nullable: false),
                    Body = table.Column<string>(type: "NCLOB", maxLength: 4000, nullable: false),
                    Attempts = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    LastAttemptAtUtc = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: true),
                    NextAttemptAtUtc = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: true),
                    SentAtUtc = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: true),
                    LastError = table.Column<string>(type: "NVARCHAR2(2000)", maxLength: 2000, nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NEWSLETTER_DELIVERIES", x => x.Id);
                    table.ForeignKey(
                        name: "FK_NEWSLETTER_DELIVERIES_NEWSLETTER_SUBSCRIPTIONS_SubscriptionId",
                        column: x => x.SubscriptionId,
                        principalSchema: "COSMICWIKI_NEWSLETTER",
                        principalTable: "NEWSLETTER_SUBSCRIPTIONS",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_NEWSLETTER_DELIVERIES_Status_NextAttemptAtUtc",
                schema: "COSMICWIKI_NEWSLETTER",
                table: "NEWSLETTER_DELIVERIES",
                columns: new[] { "Status", "NextAttemptAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_NEWSLETTER_DELIVERIES_SubscriptionId",
                schema: "COSMICWIKI_NEWSLETTER",
                table: "NEWSLETTER_DELIVERIES",
                column: "SubscriptionId");

            migrationBuilder.CreateIndex(
                name: "IX_NEWSLETTER_SUBSCRIPTIONS_ConfirmationTokenHash",
                schema: "COSMICWIKI_NEWSLETTER",
                table: "NEWSLETTER_SUBSCRIPTIONS",
                column: "ConfirmationTokenHash",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_NEWSLETTER_SUBSCRIPTIONS_Email",
                schema: "COSMICWIKI_NEWSLETTER",
                table: "NEWSLETTER_SUBSCRIPTIONS",
                column: "Email",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "NEWSLETTER_DELIVERIES",
                schema: "COSMICWIKI_NEWSLETTER");

            migrationBuilder.DropTable(
                name: "NEWSLETTER_SUBSCRIPTIONS",
                schema: "COSMICWIKI_NEWSLETTER");
        }
    }
}

using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CosmicWiki.Auth.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddRolePermissionAuthorization : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PERMISSIONS",
                columns: table => new
                {
                    ID = table.Column<Guid>(type: "RAW(16)", nullable: false),
                    CODE = table.Column<string>(type: "NVARCHAR2(200)", maxLength: 200, nullable: false),
                    NAME = table.Column<string>(type: "NVARCHAR2(200)", maxLength: 200, nullable: false),
                    DESCRIPTION = table.Column<string>(type: "NVARCHAR2(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PERMISSIONS", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "ROLES",
                columns: table => new
                {
                    ID = table.Column<Guid>(type: "RAW(16)", nullable: false),
                    NAME = table.Column<string>(type: "NVARCHAR2(100)", maxLength: 100, nullable: false),
                    DESCRIPTION = table.Column<string>(type: "NVARCHAR2(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ROLES", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "ROLE_PERMISSIONS",
                columns: table => new
                {
                    ID = table.Column<Guid>(type: "RAW(16)", nullable: false),
                    ROLE_ID = table.Column<Guid>(type: "RAW(16)", nullable: false),
                    PERMISSION_ID = table.Column<Guid>(type: "RAW(16)", nullable: false),
                    RoleId1 = table.Column<Guid>(type: "RAW(16)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ROLE_PERMISSIONS", x => x.ID);
                    table.ForeignKey(
                        name: "FK_ROLE_PERMISSIONS_PERMISSIONS_PERMISSION_ID",
                        column: x => x.PERMISSION_ID,
                        principalTable: "PERMISSIONS",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ROLE_PERMISSIONS_ROLES_ROLE_ID",
                        column: x => x.ROLE_ID,
                        principalTable: "ROLES",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ROLE_PERMISSIONS_ROLES_RoleId1",
                        column: x => x.RoleId1,
                        principalTable: "ROLES",
                        principalColumn: "ID");
                });

            migrationBuilder.CreateTable(
                name: "USER_ROLES",
                columns: table => new
                {
                    ID = table.Column<Guid>(type: "RAW(16)", nullable: false),
                    USER_ID = table.Column<Guid>(type: "RAW(16)", nullable: false),
                    ROLE_ID = table.Column<Guid>(type: "RAW(16)", nullable: false),
                    UserId1 = table.Column<Guid>(type: "RAW(16)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_USER_ROLES", x => x.ID);
                    table.ForeignKey(
                        name: "FK_USER_ROLES_ROLES_ROLE_ID",
                        column: x => x.ROLE_ID,
                        principalTable: "ROLES",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_USER_ROLES_USERS_USER_ID",
                        column: x => x.USER_ID,
                        principalTable: "USERS",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_USER_ROLES_USERS_UserId1",
                        column: x => x.UserId1,
                        principalTable: "USERS",
                        principalColumn: "ID");
                });

            migrationBuilder.CreateIndex(
                name: "IX_PERMISSIONS_CODE",
                table: "PERMISSIONS",
                column: "CODE",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ROLE_PERMISSIONS_PERMISSION_ID",
                table: "ROLE_PERMISSIONS",
                column: "PERMISSION_ID");

            migrationBuilder.CreateIndex(
                name: "IX_ROLE_PERMISSIONS_ROLE_ID_PERMISSION_ID",
                table: "ROLE_PERMISSIONS",
                columns: new[] { "ROLE_ID", "PERMISSION_ID" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ROLE_PERMISSIONS_RoleId1",
                table: "ROLE_PERMISSIONS",
                column: "RoleId1");

            migrationBuilder.CreateIndex(
                name: "IX_ROLES_NAME",
                table: "ROLES",
                column: "NAME",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_USER_ROLES_ROLE_ID",
                table: "USER_ROLES",
                column: "ROLE_ID");

            migrationBuilder.CreateIndex(
                name: "IX_USER_ROLES_USER_ID_ROLE_ID",
                table: "USER_ROLES",
                columns: new[] { "USER_ID", "ROLE_ID" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_USER_ROLES_UserId1",
                table: "USER_ROLES",
                column: "UserId1");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ROLE_PERMISSIONS");

            migrationBuilder.DropTable(
                name: "USER_ROLES");

            migrationBuilder.DropTable(
                name: "PERMISSIONS");

            migrationBuilder.DropTable(
                name: "ROLES");
        }
    }
}

using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace DotAPI.Migrations
{
    public partial class CreateAccessModels : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "access_resources",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    key_code = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    archived_stamp = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_access_resources", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "access_roles",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    label = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    name_canonical = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    archived_stamp = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_access_roles", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "access_users",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    username = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    display_name = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    first = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    last = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    email = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    phone = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    first_login_stamp = table.Column<DateTime>(type: "datetime2", nullable: true),
                    last_login_stamp = table.Column<DateTime>(type: "datetime2", nullable: true),
                    external_id = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    archived_stamp = table.Column<DateTime>(type: "datetime2", nullable: true),
                    user_password_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    needs_password_change = table.Column<bool>(type: "bit", nullable: false),
                    enabled = table.Column<short>(type: "smallint", nullable: false),
                    requested_password_reset = table.Column<bool>(type: "bit", nullable: false),
                    password_reset_token = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    created_stamp = table.Column<DateTime>(type: "datetime2", nullable: true),
                    last_updated_stamp = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_access_users", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "refresh_token",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    refresh_token = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    username = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    valid = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_refresh_token", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "access_resource_access",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    resource_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    key_code = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    archived_stamp = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_access_resource_access", x => x.id);
                    table.ForeignKey(
                        name: "FK_access_resource_access_access_resources_resource_id",
                        column: x => x.resource_id,
                        principalTable: "access_resources",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "access_user_passwords",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    password = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    user_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    created_stamp = table.Column<DateTime>(type: "datetime2", nullable: true),
                    archived_stamp = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_access_user_passwords", x => x.id);
                    table.ForeignKey(
                        name: "FK_access_user_passwords_access_users_user_id",
                        column: x => x.user_id,
                        principalTable: "access_users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "access_user_roles",
                columns: table => new
                {
                    role_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    user_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    archived_stamp = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_access_user_roles", x => new { x.user_id, x.role_id });
                    table.ForeignKey(
                        name: "FK_access_user_roles_access_roles_role_id",
                        column: x => x.role_id,
                        principalTable: "access_roles",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_access_user_roles_access_users_user_id",
                        column: x => x.user_id,
                        principalTable: "access_users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "access_role_resource_access",
                columns: table => new
                {
                    role_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    resource_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    resource_access_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    archived_stamp = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_access_role_resource_access", x => new { x.resource_id, x.role_id, x.resource_access_id });
                    table.ForeignKey(
                        name: "FK_access_role_resource_access_access_resource_access_resource_access_id",
                        column: x => x.resource_access_id,
                        principalTable: "access_resource_access",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_access_role_resource_access_access_resources_resource_id",
                        column: x => x.resource_id,
                        principalTable: "access_resources",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_access_role_resource_access_access_roles_role_id",
                        column: x => x.role_id,
                        principalTable: "access_roles",
                        principalColumn: "id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_access_resource_access_resource_id",
                table: "access_resource_access",
                column: "resource_id");

            migrationBuilder.CreateIndex(
                name: "IX_access_role_resource_access_resource_access_id",
                table: "access_role_resource_access",
                column: "resource_access_id");

            migrationBuilder.CreateIndex(
                name: "IX_access_role_resource_access_role_id",
                table: "access_role_resource_access",
                column: "role_id");

            migrationBuilder.CreateIndex(
                name: "IX_access_user_passwords_user_id",
                table: "access_user_passwords",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_access_user_roles_role_id",
                table: "access_user_roles",
                column: "role_id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "access_role_resource_access");

            migrationBuilder.DropTable(
                name: "access_user_passwords");

            migrationBuilder.DropTable(
                name: "access_user_roles");

            migrationBuilder.DropTable(
                name: "refresh_token");

            migrationBuilder.DropTable(
                name: "access_resource_access");

            migrationBuilder.DropTable(
                name: "access_roles");

            migrationBuilder.DropTable(
                name: "access_users");

            migrationBuilder.DropTable(
                name: "access_resources");
        }
    }
}

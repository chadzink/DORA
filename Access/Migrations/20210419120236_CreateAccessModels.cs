using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Access.Migrations
{
    public partial class CreateAccessModels : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
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
                name: "resources",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    key_code = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    sql_object_name = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    archived_stamp = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_resources", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "users",
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
                    requested_password_reset = table.Column<DateTime>(type: "datetime2", nullable: true),
                    password_reset_token = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    created_stamp = table.Column<DateTime>(type: "datetime2", nullable: true),
                    last_updated_stamp = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_users", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "included_resources",
                columns: table => new
                {
                    resource_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    included_recource_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    collection_name = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    description = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_included_resources", x => new { x.resource_id, x.included_recource_id });
                    table.ForeignKey(
                        name: "FK_included_resources_resources_resource_id",
                        column: x => x.resource_id,
                        principalTable: "resources",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "resource_accesses",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    resource_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    key_code = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    archived_stamp = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_resource_accesses", x => x.id);
                    table.ForeignKey(
                        name: "FK_resource_accesses_resources_resource_id",
                        column: x => x.resource_id,
                        principalTable: "resources",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "roles",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    label = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    key_code = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    archived_stamp = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RoleId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_roles", x => x.id);
                    table.ForeignKey(
                        name: "FK_roles_roles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "roles",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_roles_users_UserId",
                        column: x => x.UserId,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "user_passwords",
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
                    table.PrimaryKey("PK_user_passwords", x => x.id);
                    table.ForeignKey(
                        name: "FK_user_passwords_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "role_resource_accesses",
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
                    table.PrimaryKey("PK_role_resource_accesses", x => new { x.resource_id, x.role_id, x.resource_access_id });
                    table.ForeignKey(
                        name: "FK_role_resource_accesses_resource_accesses_resource_access_id",
                        column: x => x.resource_access_id,
                        principalTable: "resource_accesses",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_role_resource_accesses_resources_resource_id",
                        column: x => x.resource_id,
                        principalTable: "resources",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_role_resource_accesses_roles_role_id",
                        column: x => x.role_id,
                        principalTable: "roles",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "user_roles",
                columns: table => new
                {
                    role_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    user_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    archived_stamp = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_user_roles", x => new { x.user_id, x.role_id });
                    table.ForeignKey(
                        name: "FK_user_roles_roles_role_id",
                        column: x => x.role_id,
                        principalTable: "roles",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_user_roles_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_resource_accesses_resource_id",
                table: "resource_accesses",
                column: "resource_id");

            migrationBuilder.CreateIndex(
                name: "IX_role_resource_accesses_resource_access_id",
                table: "role_resource_accesses",
                column: "resource_access_id");

            migrationBuilder.CreateIndex(
                name: "IX_role_resource_accesses_role_id",
                table: "role_resource_accesses",
                column: "role_id");

            migrationBuilder.CreateIndex(
                name: "IX_roles_RoleId",
                table: "roles",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "IX_roles_UserId",
                table: "roles",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_user_passwords_user_id",
                table: "user_passwords",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_user_roles_role_id",
                table: "user_roles",
                column: "role_id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "included_resources");

            migrationBuilder.DropTable(
                name: "refresh_token");

            migrationBuilder.DropTable(
                name: "role_resource_accesses");

            migrationBuilder.DropTable(
                name: "user_passwords");

            migrationBuilder.DropTable(
                name: "user_roles");

            migrationBuilder.DropTable(
                name: "resource_accesses");

            migrationBuilder.DropTable(
                name: "roles");

            migrationBuilder.DropTable(
                name: "resources");

            migrationBuilder.DropTable(
                name: "users");
        }
    }
}

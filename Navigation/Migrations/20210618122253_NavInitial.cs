using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Navigation.Migrations
{
    public partial class NavInitial : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "navigation_item",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    nav_group_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    parent_nav_item_id = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    label = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    key = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    url = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    target = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    on_click = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_navigation_item", x => x.id);
                    table.ForeignKey(
                        name: "FK_navigation_item_navigation_item_parent_nav_item_id",
                        column: x => x.parent_nav_item_id,
                        principalTable: "navigation_item",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "navigation_group",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    group_type = table.Column<int>(type: "int", nullable: false),
                    label = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    key = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    default_nav_item_id = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_navigation_group", x => x.id);
                    table.ForeignKey(
                        name: "FK_navigation_group_navigation_item_default_nav_item_id",
                        column: x => x.default_nav_item_id,
                        principalTable: "navigation_item",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "role_navigation_item",
                columns: table => new
                {
                    nav_item_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    role_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_role_navigation_item", x => new { x.nav_item_id, x.role_id });
                    table.ForeignKey(
                        name: "FK_role_navigation_item_navigation_item_nav_item_id",
                        column: x => x.nav_item_id,
                        principalTable: "navigation_item",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_role_navigation_item_roles_role_id",
                        column: x => x.role_id,
                        principalTable: "roles",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "role_navigation_group",
                columns: table => new
                {
                    nav_group_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    role_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_role_navigation_group", x => new { x.nav_group_id, x.role_id });
                    table.ForeignKey(
                        name: "FK_role_navigation_group_navigation_group_nav_group_id",
                        column: x => x.nav_group_id,
                        principalTable: "navigation_group",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_role_navigation_group_roles_role_id",
                        column: x => x.role_id,
                        principalTable: "roles",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_navigation_group_default_nav_item_id",
                table: "navigation_group",
                column: "default_nav_item_id");

            migrationBuilder.CreateIndex(
                name: "IX_navigation_item_nav_group_id",
                table: "navigation_item",
                column: "nav_group_id");

            migrationBuilder.CreateIndex(
                name: "IX_navigation_item_parent_nav_item_id",
                table: "navigation_item",
                column: "parent_nav_item_id");

            migrationBuilder.CreateIndex(
                name: "IX_role_navigation_group_role_id",
                table: "role_navigation_group",
                column: "role_id");

            migrationBuilder.CreateIndex(
                name: "IX_role_navigation_item_role_id",
                table: "role_navigation_item",
                column: "role_id");

            migrationBuilder.AddForeignKey(
                name: "FK_navigation_item_navigation_group_nav_group_id",
                table: "navigation_item",
                column: "nav_group_id",
                principalTable: "navigation_group",
                principalColumn: "id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_navigation_group_navigation_item_default_nav_item_id",
                table: "navigation_group");

            migrationBuilder.DropTable(
                name: "role_navigation_group");

            migrationBuilder.DropTable(
                name: "role_navigation_item");

            migrationBuilder.DropTable(
                name: "navigation_item");

            migrationBuilder.DropTable(
                name: "navigation_group");
        }
    }
}

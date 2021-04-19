using Microsoft.EntityFrameworkCore.Migrations;

namespace Access.Migrations
{
    public partial class SeedDefaultData : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            SeedInitialData.Up();
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            SeedInitialData.Down();
        }
    }
}

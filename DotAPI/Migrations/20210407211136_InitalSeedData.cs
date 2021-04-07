using Microsoft.EntityFrameworkCore.Migrations;

namespace DotAPI.Migrations
{
    public partial class InitalSeedData : Migration
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

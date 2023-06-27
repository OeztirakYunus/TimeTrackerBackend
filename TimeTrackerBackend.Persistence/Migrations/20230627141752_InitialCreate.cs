using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TimeTrackerBackend.Persistence.Migrations
{
    public partial class InitialCreate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "NumberOfKids",
                table: "AspNetUsers",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "SocialSecurityNumber",
                table: "AspNetUsers",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "NumberOfKids",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "SocialSecurityNumber",
                table: "AspNetUsers");
        }
    }
}

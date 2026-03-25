using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ArıtmaEnvanter.Migrations
{
   
    public partial class AddAktifMiToRafTanim : Migration
    {
        
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "AktifMi",
                table: "RafTanimlar",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

       
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AktifMi",
                table: "RafTanimlar");
        }
    }
}

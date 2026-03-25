using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ArıtmaEnvanter.Migrations
{
    
    public partial class AddPersonelToDepoHareke : Migration
    {
        
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "PersonelId",
                table: "DepoHareketler",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_DepoHareketler_PersonelId",
                table: "DepoHareketler",
                column: "PersonelId");

            migrationBuilder.AddForeignKey(
                name: "FK_DepoHareketler_Personeller_PersonelId",
                table: "DepoHareketler",
                column: "PersonelId",
                principalTable: "Personeller",
                principalColumn: "Id");
        }

      
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DepoHareketler_Personeller_PersonelId",
                table: "DepoHareketler");

            migrationBuilder.DropIndex(
                name: "IX_DepoHareketler_PersonelId",
                table: "DepoHareketler");

            migrationBuilder.DropColumn(
                name: "PersonelId",
                table: "DepoHareketler");
        }
    }
}

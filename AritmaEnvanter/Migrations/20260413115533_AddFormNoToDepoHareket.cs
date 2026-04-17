using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AritmaEnvanter.Migrations
{
    public partial class AddFormNoToDepoHareket : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Veritabanýnda olmadýðý için hata veren silme iþlemlerini (DropForeignKey/DropIndex) kaldýrdýk.
            // Sadece kolonlarý ekliyoruz:

            migrationBuilder.AddColumn<string>(
                name: "FormNo",
                table: "DepoHareketler",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "IslemTuru",
                table: "DepoHareketler",
                type: "text",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FormNo",
                table: "DepoHareketler");

            migrationBuilder.DropColumn(
                name: "IslemTuru",
                table: "DepoHareketler");
        }
    }
}

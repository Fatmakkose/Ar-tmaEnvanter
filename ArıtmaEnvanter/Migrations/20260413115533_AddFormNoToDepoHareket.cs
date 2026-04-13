using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ArıtmaEnvanter.Migrations
{
    public partial class AddFormNoToDepoHareket : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Veritabanında olmadığı için hata veren silme işlemlerini (DropForeignKey/DropIndex) kaldırdık.
            // Sadece kolonları ekliyoruz:

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
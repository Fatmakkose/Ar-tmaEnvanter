using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ArıtmaEnvanter.Migrations
{
    /// <inheritdoc />
    public partial class AddFormKayitToDepoHareket : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "FormKayitId",
                table: "DepoHareketler",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_DepoHareketler_FormKayitId",
                table: "DepoHareketler",
                column: "FormKayitId");

            migrationBuilder.AddForeignKey(
                name: "FK_DepoHareketler_FormKayitlar_FormKayitId",
                table: "DepoHareketler",
                column: "FormKayitId",
                principalTable: "FormKayitlar",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DepoHareketler_FormKayitlar_FormKayitId",
                table: "DepoHareketler");

            migrationBuilder.DropIndex(
                name: "IX_DepoHareketler_FormKayitId",
                table: "DepoHareketler");

            migrationBuilder.DropColumn(
                name: "FormKayitId",
                table: "DepoHareketler");
        }
    }
}

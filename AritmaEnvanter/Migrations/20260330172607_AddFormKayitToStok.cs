using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AritmaEnvanter.Migrations
{
    /// <inheritdoc />
    public partial class AddFormKayitToStok : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "FormKayitId",
                table: "DepoStoklar",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_DepoStoklar_FormKayitId",
                table: "DepoStoklar",
                column: "FormKayitId");

            migrationBuilder.AddForeignKey(
                name: "FK_DepoStoklar_FormKayitlar_FormKayitId",
                table: "DepoStoklar",
                column: "FormKayitId",
                principalTable: "FormKayitlar",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DepoStoklar_FormKayitlar_FormKayitId",
                table: "DepoStoklar");

            migrationBuilder.DropIndex(
                name: "IX_DepoStoklar_FormKayitId",
                table: "DepoStoklar");

            migrationBuilder.DropColumn(
                name: "FormKayitId",
                table: "DepoStoklar");
        }
    }
}


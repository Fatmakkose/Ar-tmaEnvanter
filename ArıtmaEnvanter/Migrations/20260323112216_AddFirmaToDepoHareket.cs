using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ArıtmaEnvanter.Migrations
{
    /// <inheritdoc />
    public partial class AddFirmaToDepoHareket : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "FirmaId",
                table: "DepoHareketler",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_DepoHareketler_FirmaId",
                table: "DepoHareketler",
                column: "FirmaId");

            migrationBuilder.AddForeignKey(
                name: "FK_DepoHareketler_Firmalar_FirmaId",
                table: "DepoHareketler",
                column: "FirmaId",
                principalTable: "Firmalar",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DepoHareketler_Firmalar_FirmaId",
                table: "DepoHareketler");

            migrationBuilder.DropIndex(
                name: "IX_DepoHareketler_FirmaId",
                table: "DepoHareketler");

            migrationBuilder.DropColumn(
                name: "FirmaId",
                table: "DepoHareketler");
        }
    }
}

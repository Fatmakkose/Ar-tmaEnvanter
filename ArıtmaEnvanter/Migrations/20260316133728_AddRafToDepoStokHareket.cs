using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ArıtmaEnvanter.Migrations
{
    /// <inheritdoc />
    public partial class AddRafToDepoStokHareket : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "RafTanimId",
                table: "DepoStoklar",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "RafTanimId",
                table: "DepoHareketler",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_DepoStoklar_RafTanimId",
                table: "DepoStoklar",
                column: "RafTanimId");

            migrationBuilder.CreateIndex(
                name: "IX_DepoHareketler_RafTanimId",
                table: "DepoHareketler",
                column: "RafTanimId");

            migrationBuilder.AddForeignKey(
                name: "FK_DepoHareketler_RafTanimlar_RafTanimId",
                table: "DepoHareketler",
                column: "RafTanimId",
                principalTable: "RafTanimlar",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_DepoStoklar_RafTanimlar_RafTanimId",
                table: "DepoStoklar",
                column: "RafTanimId",
                principalTable: "RafTanimlar",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DepoHareketler_RafTanimlar_RafTanimId",
                table: "DepoHareketler");

            migrationBuilder.DropForeignKey(
                name: "FK_DepoStoklar_RafTanimlar_RafTanimId",
                table: "DepoStoklar");

            migrationBuilder.DropIndex(
                name: "IX_DepoStoklar_RafTanimId",
                table: "DepoStoklar");

            migrationBuilder.DropIndex(
                name: "IX_DepoHareketler_RafTanimId",
                table: "DepoHareketler");

            migrationBuilder.DropColumn(
                name: "RafTanimId",
                table: "DepoStoklar");

            migrationBuilder.DropColumn(
                name: "RafTanimId",
                table: "DepoHareketler");
        }
    }
}

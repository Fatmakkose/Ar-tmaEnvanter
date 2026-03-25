using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ArıtmaEnvanter.Migrations
{
    /// <inheritdoc />
    public partial class MakeKaynakDepoIdNullable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DepoHareketler_Depolar_KaynakDepoId",
                table: "DepoHareketler");

            migrationBuilder.AlterColumn<int>(
                name: "KaynakDepoId",
                table: "DepoHareketler",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AddForeignKey(
                name: "FK_DepoHareketler_Depolar_KaynakDepoId",
                table: "DepoHareketler",
                column: "KaynakDepoId",
                principalTable: "Depolar",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DepoHareketler_Depolar_KaynakDepoId",
                table: "DepoHareketler");

            migrationBuilder.AlterColumn<int>(
                name: "KaynakDepoId",
                table: "DepoHareketler",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_DepoHareketler_Depolar_KaynakDepoId",
                table: "DepoHareketler",
                column: "KaynakDepoId",
                principalTable: "Depolar",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}

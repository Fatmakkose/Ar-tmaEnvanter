using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AritmaEnvanter.Migrations
{
    /// <inheritdoc />
    public partial class AddRafNoToStokAndHareket : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "RafNo",
                table: "DepoStoklar",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RafNo",
                table: "DepoHareketler",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RafNo",
                table: "DepoStoklar");

            migrationBuilder.DropColumn(
                name: "RafNo",
                table: "DepoHareketler");
        }
    }
}


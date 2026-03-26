using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ArıtmaEnvanter.Migrations
{
    /// <inheritdoc />
    public partial class BidonAlanlariEklendi : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "BidonKg",
                table: "DepoStoklar",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "BidonSayisi",
                table: "DepoStoklar",
                type: "integer",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BidonKg",
                table: "DepoStoklar");

            migrationBuilder.DropColumn(
                name: "BidonSayisi",
                table: "DepoStoklar");
        }
    }
}

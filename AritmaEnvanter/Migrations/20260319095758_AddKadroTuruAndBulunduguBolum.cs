using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AritmaEnvanter.Migrations
{
    /// <inheritdoc />
    public partial class AddKadroTuruAndBulunduguBolum : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "BulunduguBolum",
                table: "Zimmetler",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "KadroTuru",
                table: "Personeller",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BulunduguBolum",
                table: "Zimmetler");

            migrationBuilder.DropColumn(
                name: "KadroTuru",
                table: "Personeller");
        }
    }
}


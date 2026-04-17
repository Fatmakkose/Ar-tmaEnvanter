using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AritmaEnvanter.Migrations
{
    /// <inheritdoc />
    public partial class AddAciklamaToDepoHareketler : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Aciklama",
                table: "DepoHareketler",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Aciklama",
                table: "DepoHareketler");
        }
    }
}


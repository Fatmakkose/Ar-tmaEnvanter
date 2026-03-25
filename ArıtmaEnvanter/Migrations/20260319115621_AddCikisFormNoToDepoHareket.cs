using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ArıtmaEnvanter.Migrations
{
    /// <inheritdoc />
    public partial class AddCikisFormNoToDepoHareket : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CikisFormNo",
                table: "DepoHareketler",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CikisFormNo",
                table: "DepoHareketler");
        }
    }
}

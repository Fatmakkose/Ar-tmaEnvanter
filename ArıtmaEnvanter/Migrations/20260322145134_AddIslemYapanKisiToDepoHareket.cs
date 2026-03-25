using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ArıtmaEnvanter.Migrations
{
    /// <inheritdoc />
    public partial class AddIslemYapanKisiToDepoHareket : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "IslemYapanKisi",
                table: "DepoHareketler",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IslemYapanKisi",
                table: "DepoHareketler");
        }
    }
}

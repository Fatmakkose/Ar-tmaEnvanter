using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AritmaEnvanter.Migrations
{
    /// <inheritdoc />
    public partial class AddIslemYapanKisiToDepoStok : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "IslemYapanKisi",
                table: "DepoStoklar",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IslemYapanKisi",
                table: "DepoStoklar");
        }
    }
}


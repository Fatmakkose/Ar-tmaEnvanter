using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AritmaEnvanter.Migrations
{
    /// <inheritdoc />
    public partial class AddKadroTuruToPersonel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BulunduguBolum",
                table: "Zimmetler");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "BulunduguBolum",
                table: "Zimmetler",
                type: "text",
                nullable: true);
        }
    }
}


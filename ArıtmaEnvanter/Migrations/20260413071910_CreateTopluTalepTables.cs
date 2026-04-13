using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace ArıtmaEnvanter.Migrations
{
    /// <inheritdoc />
    public partial class CreateTopluTalepTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MalzemeTalepler");

            migrationBuilder.CreateTable(
                name: "MalzemeTalepFormlar",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    FormSiraNo = table.Column<int>(type: "integer", nullable: false),
                    TalepEdenPersonelId = table.Column<string>(type: "text", nullable: false),
                    TalepTarihi = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    OnayTarihi = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    GenelAciklama = table.Column<string>(type: "text", nullable: true),
                    Durum = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MalzemeTalepFormlar", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MalzemeTalepFormlar_AspNetUsers_TalepEdenPersonelId",
                        column: x => x.TalepEdenPersonelId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "MalzemeTalepSatirlar",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    MalzemeTalepFormId = table.Column<int>(type: "integer", nullable: false),
                    StokId = table.Column<int>(type: "integer", nullable: false),
                    Miktar = table.Column<decimal>(type: "numeric", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MalzemeTalepSatirlar", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MalzemeTalepSatirlar_DepoStoklar_StokId",
                        column: x => x.StokId,
                        principalTable: "DepoStoklar",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MalzemeTalepSatirlar_MalzemeTalepFormlar_MalzemeTalepFormId",
                        column: x => x.MalzemeTalepFormId,
                        principalTable: "MalzemeTalepFormlar",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MalzemeTalepFormlar_TalepEdenPersonelId",
                table: "MalzemeTalepFormlar",
                column: "TalepEdenPersonelId");

            migrationBuilder.CreateIndex(
                name: "IX_MalzemeTalepSatirlar_MalzemeTalepFormId",
                table: "MalzemeTalepSatirlar",
                column: "MalzemeTalepFormId");

            migrationBuilder.CreateIndex(
                name: "IX_MalzemeTalepSatirlar_StokId",
                table: "MalzemeTalepSatirlar",
                column: "StokId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MalzemeTalepSatirlar");

            migrationBuilder.DropTable(
                name: "MalzemeTalepFormlar");

            migrationBuilder.CreateTable(
                name: "MalzemeTalepler",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    MalzemeId = table.Column<int>(type: "integer", nullable: false),
                    StokId = table.Column<int>(type: "integer", nullable: false),
                    TalepEdenPersonelId = table.Column<string>(type: "text", nullable: false),
                    Aciklama = table.Column<string>(type: "text", nullable: false),
                    Durum = table.Column<int>(type: "integer", nullable: false),
                    FormSiraNo = table.Column<int>(type: "integer", nullable: false),
                    Miktar = table.Column<decimal>(type: "numeric", nullable: false),
                    OnayTarihi = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    TalepTarihi = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MalzemeTalepler", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MalzemeTalepler_AspNetUsers_TalepEdenPersonelId",
                        column: x => x.TalepEdenPersonelId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MalzemeTalepler_DepoStoklar_StokId",
                        column: x => x.StokId,
                        principalTable: "DepoStoklar",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MalzemeTalepler_Malzemeler_MalzemeId",
                        column: x => x.MalzemeId,
                        principalTable: "Malzemeler",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MalzemeTalepler_MalzemeId",
                table: "MalzemeTalepler",
                column: "MalzemeId");

            migrationBuilder.CreateIndex(
                name: "IX_MalzemeTalepler_StokId",
                table: "MalzemeTalepler",
                column: "StokId");

            migrationBuilder.CreateIndex(
                name: "IX_MalzemeTalepler_TalepEdenPersonelId",
                table: "MalzemeTalepler",
                column: "TalepEdenPersonelId");
        }
    }
}

using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace AritmaEnvanter.Migrations
{

    public partial class AddEkipmanTutanakTable : Migration
    {
        
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "EkipmanTutanaklar",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Tarih = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    MalzemeAdi = table.Column<string>(type: "text", nullable: false),
                    Adet = table.Column<int>(type: "integer", nullable: false),
                    TeknikOzellikler = table.Column<string>(type: "text", nullable: true),
                    TeslimEden = table.Column<string>(type: "text", nullable: false),
                    TeslimAlan = table.Column<string>(type: "text", nullable: false),
                    KontrolAmiri = table.Column<string>(type: "text", nullable: false),
                    IletisimBilgisi = table.Column<string>(type: "text", nullable: true),
                    OnayDurumu = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EkipmanTutanaklar", x => x.Id);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EkipmanTutanaklar");
        }
    }
}


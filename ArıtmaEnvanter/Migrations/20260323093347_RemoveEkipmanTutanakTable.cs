using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace ArıtmaEnvanter.Migrations
{
   
    public partial class RemoveEkipmanTutanakTable : Migration
    {
       
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EkipmanTutanaklar");
        }

       
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "EkipmanTutanaklar",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Adet = table.Column<int>(type: "integer", nullable: false),
                    IletisimBilgisi = table.Column<string>(type: "text", nullable: true),
                    KontrolAmiri = table.Column<string>(type: "text", nullable: false),
                    MalzemeAdi = table.Column<string>(type: "text", nullable: false),
                    OnayDurumu = table.Column<int>(type: "integer", nullable: false),
                    Tarih = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    TeknikOzellikler = table.Column<string>(type: "text", nullable: true),
                    TeslimAlan = table.Column<string>(type: "text", nullable: false),
                    TeslimEden = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EkipmanTutanaklar", x => x.Id);
                });
        }
    }
}

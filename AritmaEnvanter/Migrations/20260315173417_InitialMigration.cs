using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace AritmaEnvanter.Migrations
{
    
    public partial class InitialMigration : Migration
    {
       
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AspNetRoles",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    Name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    NormalizedName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUsers",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    AdSoyad = table.Column<string>(type: "text", nullable: false),
                    UserName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    NormalizedUserName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    Email = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    NormalizedEmail = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    EmailConfirmed = table.Column<bool>(type: "boolean", nullable: false),
                    PasswordHash = table.Column<string>(type: "text", nullable: true),
                    SecurityStamp = table.Column<string>(type: "text", nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "text", nullable: true),
                    PhoneNumber = table.Column<string>(type: "text", nullable: true),
                    PhoneNumberConfirmed = table.Column<bool>(type: "boolean", nullable: false),
                    TwoFactorEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    LockoutEnd = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    LockoutEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    AccessFailedCount = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUsers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Birimler",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Ad = table.Column<string>(type: "text", nullable: false),
                    UstBirimId = table.Column<int>(type: "integer", nullable: true),
                    OlusturmaTarihi = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Birimler", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Birimler_Birimler_UstBirimId",
                        column: x => x.UstBirimId,
                        principalTable: "Birimler",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Kategoriler",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Ad = table.Column<string>(type: "text", nullable: false),
                    HiyerarsikId = table.Column<string>(type: "text", nullable: true),
                    OlusturmaTarihi = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UstKategoriId = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Kategoriler", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Kategoriler_Kategoriler_UstKategoriId",
                        column: x => x.UstKategoriId,
                        principalTable: "Kategoriler",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "TamirAtasmanlar",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    DosyaAdi = table.Column<string>(type: "text", nullable: true),
                    DosyaYolu = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TamirAtasmanlar", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AspNetRoleClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    RoleId = table.Column<string>(type: "text", nullable: false),
                    ClaimType = table.Column<string>(type: "text", nullable: true),
                    ClaimValue = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoleClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetRoleClaims_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<string>(type: "text", nullable: false),
                    ClaimType = table.Column<string>(type: "text", nullable: true),
                    ClaimValue = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetUserClaims_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserLogins",
                columns: table => new
                {
                    LoginProvider = table.Column<string>(type: "text", nullable: false),
                    ProviderKey = table.Column<string>(type: "text", nullable: false),
                    ProviderDisplayName = table.Column<string>(type: "text", nullable: true),
                    UserId = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserLogins", x => new { x.LoginProvider, x.ProviderKey });
                    table.ForeignKey(
                        name: "FK_AspNetUserLogins_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserRoles",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "text", nullable: false),
                    RoleId = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserRoles", x => new { x.UserId, x.RoleId });
                    table.ForeignKey(
                        name: "FK_AspNetUserRoles_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AspNetUserRoles_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserTokens",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "text", nullable: false),
                    LoginProvider = table.Column<string>(type: "text", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Value = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserTokens", x => new { x.UserId, x.LoginProvider, x.Name });
                    table.ForeignKey(
                        name: "FK_AspNetUserTokens_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AssetTransactions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UrunId = table.Column<int>(type: "integer", nullable: false),
                    EskiLokasyonId = table.Column<int>(type: "integer", nullable: true),
                    YeniLokasyonId = table.Column<int>(type: "integer", nullable: true),
                    IslemTarihi = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IslemYapanKullaniciId = table.Column<string>(type: "text", nullable: true),
                    Aciklama = table.Column<string>(type: "text", nullable: true),
                    IslemTipi = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AssetTransactions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AssetTransactions_AspNetUsers_IslemYapanKullaniciId",
                        column: x => x.IslemYapanKullaniciId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Depolar",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Ad = table.Column<string>(type: "text", nullable: false),
                    BirimId = table.Column<int>(type: "integer", nullable: true),
                    UstDepoId = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Depolar", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Depolar_Birimler_BirimId",
                        column: x => x.BirimId,
                        principalTable: "Birimler",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Depolar_Depolar_UstDepoId",
                        column: x => x.UstDepoId,
                        principalTable: "Depolar",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Lokasyonlar",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    BirimId = table.Column<int>(type: "integer", nullable: true),
                    Kat = table.Column<string>(type: "text", nullable: true),
                    OdaNo = table.Column<string>(type: "text", nullable: true),
                    ParentId = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Lokasyonlar", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Lokasyonlar_Birimler_BirimId",
                        column: x => x.BirimId,
                        principalTable: "Birimler",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Lokasyonlar_Lokasyonlar_ParentId",
                        column: x => x.ParentId,
                        principalTable: "Lokasyonlar",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Personeller",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    AdSoyad = table.Column<string>(type: "text", nullable: false),
                    Unvan = table.Column<string>(type: "text", nullable: true),
                    Telefon = table.Column<string>(type: "text", nullable: true),
                    Email = table.Column<string>(type: "text", nullable: true),
                    BirimId = table.Column<int>(type: "integer", nullable: true),
                    OlusturmaTarihi = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Personeller", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Personeller_Birimler_BirimId",
                        column: x => x.BirimId,
                        principalTable: "Birimler",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "FormSablonlar",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Baslik = table.Column<string>(type: "text", nullable: false),
                    KategoriId = table.Column<int>(type: "integer", nullable: true),
                    Aciklama = table.Column<string>(type: "text", nullable: true),
                    UstFormId = table.Column<int>(type: "integer", nullable: true),
                    HiyerarsikId = table.Column<string>(type: "text", nullable: true),
                    OlusturmaTarihi = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FormSablonlar", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FormSablonlar_FormSablonlar_UstFormId",
                        column: x => x.UstFormId,
                        principalTable: "FormSablonlar",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_FormSablonlar_Kategoriler_KategoriId",
                        column: x => x.KategoriId,
                        principalTable: "Kategoriler",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "TransferTalepler",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    KaynakDepoId = table.Column<int>(type: "integer", nullable: true),
                    HedefDepoId = table.Column<int>(type: "integer", nullable: true),
                    TalepEdenUserId = table.Column<string>(type: "text", nullable: true),
                    OnaylayanUserId = table.Column<string>(type: "text", nullable: true),
                    TalepTarihi = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Aciklama = table.Column<string>(type: "text", nullable: true),
                    Durum = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TransferTalepler", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TransferTalepler_AspNetUsers_OnaylayanUserId",
                        column: x => x.OnaylayanUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TransferTalepler_AspNetUsers_TalepEdenUserId",
                        column: x => x.TalepEdenUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TransferTalepler_Depolar_HedefDepoId",
                        column: x => x.HedefDepoId,
                        principalTable: "Depolar",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TransferTalepler_Depolar_KaynakDepoId",
                        column: x => x.KaynakDepoId,
                        principalTable: "Depolar",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Urunler",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    DynamicProperties = table.Column<string>(type: "jsonb", nullable: true),
                    UrunTuru = table.Column<string>(type: "text", nullable: true),
                    KategoriId = table.Column<int>(type: "integer", nullable: true),
                    BirimId = table.Column<int>(type: "integer", nullable: true),
                    SeriNo = table.Column<string>(type: "text", nullable: true),
                    Durum = table.Column<string>(type: "text", nullable: true),
                    Miktar = table.Column<decimal>(type: "numeric", nullable: true),
                    LokasyonId = table.Column<int>(type: "integer", nullable: true),
                    DepoId = table.Column<int>(type: "integer", nullable: true),
                    OlusturmaTarihi = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Urunler", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Urunler_Birimler_BirimId",
                        column: x => x.BirimId,
                        principalTable: "Birimler",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Urunler_Kategoriler_KategoriId",
                        column: x => x.KategoriId,
                        principalTable: "Kategoriler",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Urunler_Lokasyonlar_LokasyonId",
                        column: x => x.LokasyonId,
                        principalTable: "Lokasyonlar",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "FormAlanlar",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    FormSablonId = table.Column<int>(type: "integer", nullable: false),
                    AlanAdi = table.Column<string>(type: "text", nullable: false),
                    AlanTipi = table.Column<string>(type: "text", nullable: false),
                    Secenekler = table.Column<string>(type: "text", nullable: true),
                    Gerekli = table.Column<bool>(type: "boolean", nullable: false),
                    Sira = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FormAlanlar", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FormAlanlar_FormSablonlar_FormSablonId",
                        column: x => x.FormSablonId,
                        principalTable: "FormSablonlar",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "FormKayitlar",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    FormSablonId = table.Column<int>(type: "integer", nullable: false),
                    KayitNo = table.Column<string>(type: "text", nullable: true),
                    Barkod = table.Column<string>(type: "text", nullable: true),
                    OlusturmaTarihi = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    KaydedenKullanici = table.Column<string>(type: "text", nullable: true),
                    DepoId = table.Column<int>(type: "integer", nullable: true),
                    BirimId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FormKayitlar", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FormKayitlar_Birimler_BirimId",
                        column: x => x.BirimId,
                        principalTable: "Birimler",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_FormKayitlar_Depolar_DepoId",
                        column: x => x.DepoId,
                        principalTable: "Depolar",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_FormKayitlar_FormSablonlar_FormSablonId",
                        column: x => x.FormSablonId,
                        principalTable: "FormSablonlar",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Malzemeler",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Ad = table.Column<string>(type: "text", nullable: false),
                    AlternatifAd = table.Column<string>(type: "text", nullable: true),
                    KategoriId = table.Column<int>(type: "integer", nullable: true),
                    BirimId = table.Column<int>(type: "integer", nullable: true),
                    UrunTuru = table.Column<string>(type: "text", nullable: true),
                    Aciklama = table.Column<string>(type: "text", nullable: true),
                    Durum = table.Column<string>(type: "text", nullable: true),
                    OlusturmaTarihi = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    RafId = table.Column<int>(type: "integer", nullable: true),
                    Raf = table.Column<string>(type: "text", nullable: true),
                    Miktar = table.Column<decimal>(type: "numeric", nullable: true),
                    SeriNo = table.Column<string>(type: "text", nullable: true),
                    MalzemeTuru = table.Column<string>(type: "text", nullable: true),
                    FormSablonId = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Malzemeler", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Malzemeler_Birimler_BirimId",
                        column: x => x.BirimId,
                        principalTable: "Birimler",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Malzemeler_FormSablonlar_FormSablonId",
                        column: x => x.FormSablonId,
                        principalTable: "FormSablonlar",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Malzemeler_Kategoriler_KategoriId",
                        column: x => x.KategoriId,
                        principalTable: "Kategoriler",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "ArizaFormlari",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UrunId = table.Column<int>(type: "integer", nullable: false),
                    ArizaTanimi = table.Column<string>(type: "text", nullable: false),
                    Durum = table.Column<string>(type: "text", nullable: false),
                    OlusturmaTarihi = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ArizaFormlari", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ArizaFormlari_Urunler_UrunId",
                        column: x => x.UrunId,
                        principalTable: "Urunler",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SatinAlmalar",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UrunId = table.Column<int>(type: "integer", nullable: false),
                    Miktar = table.Column<decimal>(type: "numeric", nullable: false),
                    BirimFiyat = table.Column<decimal>(type: "numeric", nullable: false),
                    Tarih = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SatinAlmalar", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SatinAlmalar_Urunler_UrunId",
                        column: x => x.UrunId,
                        principalTable: "Urunler",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Transferler",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UrunId = table.Column<int>(type: "integer", nullable: false),
                    KaynakBirimId = table.Column<int>(type: "integer", nullable: false),
                    HedefBirimId = table.Column<int>(type: "integer", nullable: false),
                    TransferTarihi = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Aciklama = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Transferler", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Transferler_Birimler_HedefBirimId",
                        column: x => x.HedefBirimId,
                        principalTable: "Birimler",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Transferler_Birimler_KaynakBirimId",
                        column: x => x.KaynakBirimId,
                        principalTable: "Birimler",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Transferler_Urunler_UrunId",
                        column: x => x.UrunId,
                        principalTable: "Urunler",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "FormKayitDegerler",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    FormKayitId = table.Column<int>(type: "integer", nullable: false),
                    FormAlanId = table.Column<int>(type: "integer", nullable: false),
                    Deger = table.Column<string>(type: "text", nullable: true),
                    GorselUrl = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FormKayitDegerler", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FormKayitDegerler_FormAlanlar_FormAlanId",
                        column: x => x.FormAlanId,
                        principalTable: "FormAlanlar",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_FormKayitDegerler_FormKayitlar_FormKayitId",
                        column: x => x.FormKayitId,
                        principalTable: "FormKayitlar",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Bakimlar",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UrunId = table.Column<int>(type: "integer", nullable: false),
                    BakimTarihi = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    SonrakiBakimTarihi = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    MalzemeId = table.Column<int>(type: "integer", nullable: true),
                    Aciklama = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Bakimlar", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Bakimlar_Malzemeler_MalzemeId",
                        column: x => x.MalzemeId,
                        principalTable: "Malzemeler",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Bakimlar_Urunler_UrunId",
                        column: x => x.UrunId,
                        principalTable: "Urunler",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DepoHareketler",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    MalzemeId = table.Column<int>(type: "integer", nullable: false),
                    KaynakDepoId = table.Column<int>(type: "integer", nullable: false),
                    HedefDepoId = table.Column<int>(type: "integer", nullable: true),
                    Miktar = table.Column<decimal>(type: "numeric", nullable: false),
                    Tarih = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    TamirAtasmanId = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DepoHareketler", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DepoHareketler_Depolar_HedefDepoId",
                        column: x => x.HedefDepoId,
                        principalTable: "Depolar",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DepoHareketler_Depolar_KaynakDepoId",
                        column: x => x.KaynakDepoId,
                        principalTable: "Depolar",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DepoHareketler_Malzemeler_MalzemeId",
                        column: x => x.MalzemeId,
                        principalTable: "Malzemeler",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DepoHareketler_TamirAtasmanlar_TamirAtasmanId",
                        column: x => x.TamirAtasmanId,
                        principalTable: "TamirAtasmanlar",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "DepoStoklar",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    MalzemeId = table.Column<int>(type: "integer", nullable: false),
                    DepoId = table.Column<int>(type: "integer", nullable: false),
                    Miktar = table.Column<decimal>(type: "numeric", nullable: false),
                    UrunAdi = table.Column<string>(type: "text", nullable: true),
                    Birim = table.Column<string>(type: "text", nullable: true),
                    GuncellemeTarihi = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DepoStoklar", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DepoStoklar_Depolar_DepoId",
                        column: x => x.DepoId,
                        principalTable: "Depolar",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DepoStoklar_Malzemeler_MalzemeId",
                        column: x => x.MalzemeId,
                        principalTable: "Malzemeler",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Zimmetler",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    PersonelId = table.Column<int>(type: "integer", nullable: false),
                    MalzemeId = table.Column<int>(type: "integer", nullable: false),
                    UrunId = table.Column<int>(type: "integer", nullable: true),
                    ZimmetTarihi = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IadeTarihi = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Notlar = table.Column<string>(type: "text", nullable: true),
                    Durum = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Zimmetler", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Zimmetler_Malzemeler_MalzemeId",
                        column: x => x.MalzemeId,
                        principalTable: "Malzemeler",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Zimmetler_Personeller_PersonelId",
                        column: x => x.PersonelId,
                        principalTable: "Personeller",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Zimmetler_Urunler_UrunId",
                        column: x => x.UrunId,
                        principalTable: "Urunler",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_ArizaFormlari_UrunId",
                table: "ArizaFormlari",
                column: "UrunId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetRoleClaims_RoleId",
                table: "AspNetRoleClaims",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "RoleNameIndex",
                table: "AspNetRoles",
                column: "NormalizedName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserClaims_UserId",
                table: "AspNetUserClaims",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserLogins_UserId",
                table: "AspNetUserLogins",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserRoles_RoleId",
                table: "AspNetUserRoles",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "EmailIndex",
                table: "AspNetUsers",
                column: "NormalizedEmail");

            migrationBuilder.CreateIndex(
                name: "UserNameIndex",
                table: "AspNetUsers",
                column: "NormalizedUserName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AssetTransactions_IslemYapanKullaniciId",
                table: "AssetTransactions",
                column: "IslemYapanKullaniciId");

            migrationBuilder.CreateIndex(
                name: "IX_Bakimlar_MalzemeId",
                table: "Bakimlar",
                column: "MalzemeId");

            migrationBuilder.CreateIndex(
                name: "IX_Bakimlar_UrunId",
                table: "Bakimlar",
                column: "UrunId");

            migrationBuilder.CreateIndex(
                name: "IX_Birimler_UstBirimId",
                table: "Birimler",
                column: "UstBirimId");

            migrationBuilder.CreateIndex(
                name: "IX_DepoHareketler_HedefDepoId",
                table: "DepoHareketler",
                column: "HedefDepoId");

            migrationBuilder.CreateIndex(
                name: "IX_DepoHareketler_KaynakDepoId",
                table: "DepoHareketler",
                column: "KaynakDepoId");

            migrationBuilder.CreateIndex(
                name: "IX_DepoHareketler_MalzemeId",
                table: "DepoHareketler",
                column: "MalzemeId");

            migrationBuilder.CreateIndex(
                name: "IX_DepoHareketler_TamirAtasmanId",
                table: "DepoHareketler",
                column: "TamirAtasmanId");

            migrationBuilder.CreateIndex(
                name: "IX_Depolar_BirimId",
                table: "Depolar",
                column: "BirimId");

            migrationBuilder.CreateIndex(
                name: "IX_Depolar_UstDepoId",
                table: "Depolar",
                column: "UstDepoId");

            migrationBuilder.CreateIndex(
                name: "IX_DepoStoklar_DepoId",
                table: "DepoStoklar",
                column: "DepoId");

            migrationBuilder.CreateIndex(
                name: "IX_DepoStoklar_MalzemeId",
                table: "DepoStoklar",
                column: "MalzemeId");

            migrationBuilder.CreateIndex(
                name: "IX_FormAlanlar_FormSablonId",
                table: "FormAlanlar",
                column: "FormSablonId");

            migrationBuilder.CreateIndex(
                name: "IX_FormKayitDegerler_FormAlanId",
                table: "FormKayitDegerler",
                column: "FormAlanId");

            migrationBuilder.CreateIndex(
                name: "IX_FormKayitDegerler_FormKayitId",
                table: "FormKayitDegerler",
                column: "FormKayitId");

            migrationBuilder.CreateIndex(
                name: "IX_FormKayitlar_BirimId",
                table: "FormKayitlar",
                column: "BirimId");

            migrationBuilder.CreateIndex(
                name: "IX_FormKayitlar_DepoId",
                table: "FormKayitlar",
                column: "DepoId");

            migrationBuilder.CreateIndex(
                name: "IX_FormKayitlar_FormSablonId",
                table: "FormKayitlar",
                column: "FormSablonId");

            migrationBuilder.CreateIndex(
                name: "IX_FormSablonlar_KategoriId",
                table: "FormSablonlar",
                column: "KategoriId");

            migrationBuilder.CreateIndex(
                name: "IX_FormSablonlar_UstFormId",
                table: "FormSablonlar",
                column: "UstFormId");

            migrationBuilder.CreateIndex(
                name: "IX_Kategoriler_UstKategoriId",
                table: "Kategoriler",
                column: "UstKategoriId");

            migrationBuilder.CreateIndex(
                name: "IX_Lokasyonlar_BirimId",
                table: "Lokasyonlar",
                column: "BirimId");

            migrationBuilder.CreateIndex(
                name: "IX_Lokasyonlar_ParentId",
                table: "Lokasyonlar",
                column: "ParentId");

            migrationBuilder.CreateIndex(
                name: "IX_Malzemeler_BirimId",
                table: "Malzemeler",
                column: "BirimId");

            migrationBuilder.CreateIndex(
                name: "IX_Malzemeler_FormSablonId",
                table: "Malzemeler",
                column: "FormSablonId");

            migrationBuilder.CreateIndex(
                name: "IX_Malzemeler_KategoriId",
                table: "Malzemeler",
                column: "KategoriId");

            migrationBuilder.CreateIndex(
                name: "IX_Personeller_BirimId",
                table: "Personeller",
                column: "BirimId");

            migrationBuilder.CreateIndex(
                name: "IX_SatinAlmalar_UrunId",
                table: "SatinAlmalar",
                column: "UrunId");

            migrationBuilder.CreateIndex(
                name: "IX_Transferler_HedefBirimId",
                table: "Transferler",
                column: "HedefBirimId");

            migrationBuilder.CreateIndex(
                name: "IX_Transferler_KaynakBirimId",
                table: "Transferler",
                column: "KaynakBirimId");

            migrationBuilder.CreateIndex(
                name: "IX_Transferler_UrunId",
                table: "Transferler",
                column: "UrunId");

            migrationBuilder.CreateIndex(
                name: "IX_TransferTalepler_HedefDepoId",
                table: "TransferTalepler",
                column: "HedefDepoId");

            migrationBuilder.CreateIndex(
                name: "IX_TransferTalepler_KaynakDepoId",
                table: "TransferTalepler",
                column: "KaynakDepoId");

            migrationBuilder.CreateIndex(
                name: "IX_TransferTalepler_OnaylayanUserId",
                table: "TransferTalepler",
                column: "OnaylayanUserId");

            migrationBuilder.CreateIndex(
                name: "IX_TransferTalepler_TalepEdenUserId",
                table: "TransferTalepler",
                column: "TalepEdenUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Urunler_BirimId",
                table: "Urunler",
                column: "BirimId");

            migrationBuilder.CreateIndex(
                name: "IX_Urunler_KategoriId",
                table: "Urunler",
                column: "KategoriId");

            migrationBuilder.CreateIndex(
                name: "IX_Urunler_LokasyonId",
                table: "Urunler",
                column: "LokasyonId");

            migrationBuilder.CreateIndex(
                name: "IX_Zimmetler_MalzemeId",
                table: "Zimmetler",
                column: "MalzemeId");

            migrationBuilder.CreateIndex(
                name: "IX_Zimmetler_PersonelId",
                table: "Zimmetler",
                column: "PersonelId");

            migrationBuilder.CreateIndex(
                name: "IX_Zimmetler_UrunId",
                table: "Zimmetler",
                column: "UrunId");
        }

        
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ArizaFormlari");

            migrationBuilder.DropTable(
                name: "AspNetRoleClaims");

            migrationBuilder.DropTable(
                name: "AspNetUserClaims");

            migrationBuilder.DropTable(
                name: "AspNetUserLogins");

            migrationBuilder.DropTable(
                name: "AspNetUserRoles");

            migrationBuilder.DropTable(
                name: "AspNetUserTokens");

            migrationBuilder.DropTable(
                name: "AssetTransactions");

            migrationBuilder.DropTable(
                name: "Bakimlar");

            migrationBuilder.DropTable(
                name: "DepoHareketler");

            migrationBuilder.DropTable(
                name: "DepoStoklar");

            migrationBuilder.DropTable(
                name: "FormKayitDegerler");

            migrationBuilder.DropTable(
                name: "SatinAlmalar");

            migrationBuilder.DropTable(
                name: "Transferler");

            migrationBuilder.DropTable(
                name: "TransferTalepler");

            migrationBuilder.DropTable(
                name: "Zimmetler");

            migrationBuilder.DropTable(
                name: "AspNetRoles");

            migrationBuilder.DropTable(
                name: "TamirAtasmanlar");

            migrationBuilder.DropTable(
                name: "FormAlanlar");

            migrationBuilder.DropTable(
                name: "FormKayitlar");

            migrationBuilder.DropTable(
                name: "AspNetUsers");

            migrationBuilder.DropTable(
                name: "Malzemeler");

            migrationBuilder.DropTable(
                name: "Personeller");

            migrationBuilder.DropTable(
                name: "Urunler");

            migrationBuilder.DropTable(
                name: "Depolar");

            migrationBuilder.DropTable(
                name: "FormSablonlar");

            migrationBuilder.DropTable(
                name: "Lokasyonlar");

            migrationBuilder.DropTable(
                name: "Kategoriler");

            migrationBuilder.DropTable(
                name: "Birimler");
        }
    }
}


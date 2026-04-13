using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using ArıtmaEnvanter.Models.Entities;
using System.Security.Cryptography.Xml;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;
using System.Linq;
using ArıtmaEnvanter.Models.Entities;
using System.Collections.Generic;
using System.Reflection.Emit;

namespace ArıtmaEnvanter.Data
{
    public class AppDbContext : IdentityDbContext<ApplicationUser>
    {
        private readonly IHttpContextAccessor? _httpContextAccessor;

        public AppDbContext(DbContextOptions<AppDbContext> options, IHttpContextAccessor? httpContextAccessor = null) : base(options)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public bool IsAdmin => _httpContextAccessor?.HttpContext?.User?.IsInRole("Admin") ?? false;
        public bool IsIlceSorumlusu => (_httpContextAccessor?.HttpContext?.User?.IsInRole("IlceSorumlusu") ?? false) || (_httpContextAccessor?.HttpContext?.User?.IsInRole("İlçe Sorumlusu") ?? false);
        public int? CurrentDistrictId
        {
            get
            {
                var claim = _httpContextAccessor?.HttpContext?.User?.Claims.FirstOrDefault(c => c.Type == "DistrictId")?.Value;
                if (int.TryParse(claim, out int id)) return id;
                return null;
            }
        }
        public int? CurrentWarehouseId
        {
            get
            {
                var claim = _httpContextAccessor?.HttpContext?.User?.Claims.FirstOrDefault(c => c.Type == "DefaultWarehouseId")?.Value;
                if (int.TryParse(claim, out int id)) return id;
                return null;
            }
        }
        public string? CurrentUserId => _httpContextAccessor?.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);

        public DbSet<AssetTransaction> AssetTransactions { get; set; }
        public DbSet<Birim> Birimler { get; set; }
        public DbSet<Kategori> Kategoriler { get; set; }
        public DbSet<Urun> Urunler { get; set; }
        public DbSet<Personel> Personeller { get; set; }
        public DbSet<Zimmet> Zimmetler { get; set; }
        public DbSet<Bakim> Bakimlar { get; set; }
        public DbSet<SatinAlma> SatinAlmalar { get; set; }
        public DbSet<Transfer> Transferler { get; set; }
        public DbSet<FormSablon> FormSablonlar { get; set; }
        public DbSet<FormAlan> FormAlanlar { get; set; }
        public DbSet<FormKayit> FormKayitlar { get; set; }
        public DbSet<FormKayitDeger> FormKayitDegerler { get; set; }
        public DbSet<Lokasyon> Lokasyonlar { get; set; }
        public DbSet<ArizaFormu> ArizaFormlari { get; set; }
        public DbSet<Depo> Depolar { get; set; }
        public DbSet<DepoStok> DepoStoklar { get; set; }
        public DbSet<DepoHareket> DepoHareketler { get; set; }
        public DbSet<TamirAtasman> TamirAtasmanlar { get; set; }
        public DbSet<Malzeme> Malzemeler { get; set; }
        public DbSet<TransferTalep> TransferTalepler { get; set; }
        public DbSet<RafTanim> RafTanimlar { get; set; }
        public DbSet<KimyasalTuketim> KimyasalTuketimler { get; set; }
        public DbSet<KimyasalGiris> KimyasalGirisler { get; set; }
        public DbSet<KimyasalDevir> KimyasalDevirler { get; set; }
        public DbSet<Firma> Firmalar { get; set; }
        public DbSet<MalzemeTalepForm> MalzemeTalepFormlar { get; set; }
        public DbSet<MalzemeTalepSatir> MalzemeTalepSatirlar { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<Lokasyon>()
                .HasOne(l => l.ParentLokasyon)
                .WithMany(l => l.AltLokasyonlar)
                .HasForeignKey(l => l.ParentId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Lokasyon>().HasQueryFilter(l =>
                IsAdmin ||
                (CurrentDistrictId != null && l.BirimId == CurrentDistrictId)
            );

            builder.Entity<Birim>().HasQueryFilter(b =>
                IsAdmin || IsIlceSorumlusu ||
                (CurrentDistrictId != null && b.Id == CurrentDistrictId)
            );

            builder.Entity<Personel>().HasQueryFilter(p =>
                IsAdmin ||
                (CurrentDistrictId != null && p.BirimId == CurrentDistrictId)
            );


            builder.Entity<Transfer>().HasQueryFilter(t =>
                IsAdmin ||
                (t.KaynakBirimId == CurrentDistrictId || t.HedefBirimId == CurrentDistrictId)
            );


            builder.Entity<Urun>()
                .Property(u => u.DynamicProperties)
                .HasColumnType("jsonb");


            builder.Entity<Birim>()
                .HasOne(b => b.UstBirim)
                .WithMany(b => b.AltBirimler)
                .HasForeignKey(b => b.UstBirimId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Kategori>()
                .HasOne(k => k.UstKategori)
                .WithMany(k => k.AltKategoriler)
                .HasForeignKey(k => k.UstKategoriId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Depo>()
            .HasOne(d => d.UstDepo)
             .WithMany(d => d.AltDepolar)
                  .HasForeignKey(d => d.UstDepoId)
              .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Depo>()
                .HasOne(d => d.Birim)
                .WithMany()
                .HasForeignKey(d => d.BirimId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<DepoHareket>()
                .HasOne(h => h.HedefDepo)
                .WithMany()
                .HasForeignKey(h => h.HedefDepoId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<DepoHareket>()
                .HasOne(h => h.Firma)
                .WithMany()
                .HasForeignKey(h => h.FirmaId)
                .OnDelete(DeleteBehavior.SetNull);

            builder.Entity<Transfer>()
                .HasOne(t => t.KaynakBirim)
                .WithMany()
                .HasForeignKey(t => t.KaynakBirimId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Transfer>()
                .HasOne(t => t.HedefBirim)
                .WithMany()
                .HasForeignKey(t => t.HedefBirimId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<TamirAtasman>()
                .HasMany(a => a.Hareketler)
                .WithOne(h => h.TamirAtasman)
                .HasForeignKey(h => h.TamirAtasmanId)
                .OnDelete(DeleteBehavior.SetNull);

            builder.Entity<TransferTalep>()
                .HasOne(t => t.KaynakDepo)
                .WithMany()
                .HasForeignKey(t => t.KaynakDepoId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<TransferTalep>()
                .HasOne(t => t.HedefDepo)
                .WithMany()
                .HasForeignKey(t => t.HedefDepoId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<TransferTalep>()
                .HasOne(t => t.TalepEdenUser)
                .WithMany()
                .HasForeignKey(t => t.TalepEdenUserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<TransferTalep>()
                .HasOne(t => t.OnaylayanUser)
                .WithMany()
                .HasForeignKey(t => t.OnaylayanUserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<MalzemeTalepForm>()
              .HasOne(t => t.TalepEdenPersonel)
              .WithMany()
              .HasForeignKey(t => t.TalepEdenPersonelId)
              .OnDelete(DeleteBehavior.Restrict);


            builder.Entity<MalzemeTalepSatir>()
                .HasOne(s => s.MalzemeTalepForm)
                .WithMany(f => f.Satirlar)
                .HasForeignKey(s => s.MalzemeTalepFormId)
                .OnDelete(DeleteBehavior.Cascade);


            builder.Entity<MalzemeTalepSatir>()
                .HasOne(s => s.Stok)
                .WithMany()
                .HasForeignKey(s => s.StokId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
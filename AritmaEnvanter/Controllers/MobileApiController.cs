using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using AritmaEnvanter.Data;
using AritmaEnvanter.Models.Entities;
using AritmaEnvanter.Models.DTOs;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace AritmaEnvanter.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [AllowAnonymous]
    public class MobileApiController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;

        public MobileApiController(
            AppDbContext context,
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager)
        {
            _context = context;
            _userManager = userManager;
            _signInManager = signInManager;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] MobileLoginRequest request)
        {
            if (request == null || string.IsNullOrEmpty(request.Email) || string.IsNullOrEmpty(request.Password))
                return BadRequest(new { success = false, message = "Email ve şifre gereklidir." });

            var user = await _userManager.FindByEmailAsync(request.Email);
            if (user == null)
                return Unauthorized(new { success = false, message = "Kullanıcı bulunamadı." });

            var result = await _signInManager.CheckPasswordSignInAsync(user, request.Password, false);
            if (!result.Succeeded)
                return Unauthorized(new { success = false, message = "Hatalı şifre." });

            var roles = await _userManager.GetRolesAsync(user);

            return Ok(new MobileLoginResponse
            {
                Success = true,
                Message = "Giriş başarılı.",
                UserEmail = user.Email,
                FullName = user.AdSoyad,
                Role = string.Join(", ", roles)
            });
        }

        [HttpGet("stocks")]
        public async Task<IActionResult> GetStockList()
        {
            var stocks = await _context.DepoStoklar
                .Include(s => s.Malzeme)
                .Include(s => s.Depo)
                .Include(s => s.RafTanim)
                .Select(s => new StockItemDto
                {
                    Id = s.Id,
                    MalzemeId = s.MalzemeId,
                    MaterialName = !string.IsNullOrEmpty(s.UrunAdi) ? s.UrunAdi : (s.Malzeme != null ? s.Malzeme.Ad : "Bilinmeyen Malzeme"),
                    WarehouseName = s.Depo != null ? s.Depo.Ad : "Bilinmeyen Depo",
                    ShelfName = !string.IsNullOrEmpty(s.RafNo) ? s.RafNo : (s.RafTanim != null ? s.RafTanim.Ad : "-"),
                    Quantity = s.Miktar,
                    Unit = s.Birim ?? (s.Malzeme != null && s.Malzeme.Birim != null ? s.Malzeme.Birim.Ad : "Adet"),
                    LastUpdatedBy = s.IslemYapanKisi ?? "Sistem",
                    LastUpdatedDate = s.GuncellemeTarihi.ToString("yyyy-MM-dd HH:mm"),
                    MaterialType = MapMaterialType(s.Malzeme)
                })
                .ToListAsync();

            return Ok(stocks);
        }

        [HttpGet("summary")]
        public async Task<IActionResult> GetDashboardSummary()
        {
            var today = DateTime.UtcNow.Date;
            
            var summary = new MobileDashboardSummary
            {
                CriticalStockCount = await _context.DepoStoklar.CountAsync(s => s.Miktar < 10), // Eşik: 10
                TodayEntryCount = await _context.DepoHareketler.CountAsync(h => h.Tarih >= today && h.IslemTuru == "GİR"),
                TodayExitCount = await _context.DepoHareketler.CountAsync(h => h.Tarih >= today && h.IslemTuru == "ÇIK")
            };

            return Ok(summary);
        }

        private static string MapMaterialType(Malzeme? m)
        {
            if (m == null) return "Part";
            
            var name = m.Ad.ToLower();
            var category = m.UrunTuru?.ToLower() ?? "";

            if (name.Contains("sıvı") || name.Contains("likit") || category.Contains("kimyasal"))
                return "Liquid";
            if (name.Contains("toz") || name.Contains("katı") || name.Contains("çuval"))
                return "Solid";
            if (name.Contains("bidon") || name.Contains("varil"))
                return "Barrel";
            
            return "Part";
        }

        [HttpPost("movement")]
        public async Task<IActionResult> PostStockMovement([FromBody] StockMovementRequest request)
        {
            if (request == null || request.Amount <= 0)
                return BadRequest(new { success = false, message = "Geçersiz değerler." });

            var stock = await _context.DepoStoklar
                .Include(s => s.Malzeme)
                .FirstOrDefaultAsync(s => s.Id == request.StokId);

            if (stock == null)
                return NotFound(new { success = false, message = "Stok kaydı bulunamadı." });

            // İleride Login olan kullanıcıdan alınacak, şimdilik sabit
            string userName = "Mobil Kullanıcı"; 

            if (request.Type == "Giris")
            {
                stock.Miktar += request.Amount;
            }
            else if (request.Type == "Cikis")
            {
                if (stock.Miktar < request.Amount)
                    return BadRequest(new { success = false, message = "Yetersiz stok." });

                stock.Miktar -= request.Amount;
            }
            else
            {
                return BadRequest(new { success = false, message = "Geçersiz işlem tipi." });
            }

            stock.IslemYapanKisi = userName;
            stock.GuncellemeTarihi = DateTime.UtcNow;

            // Hareket kaydı oluştur
            var hareket = new DepoHareket
            {
                MalzemeId = stock.MalzemeId,
                KaynakDepoId = stock.DepoId,
                Miktar = request.Amount,
                Tarih = DateTime.UtcNow,
                IslemYapanKisi = userName,
                Aciklama = request.Description ?? $"Mobil {request.Type} işlemi",
                FormNo = request.FormNo,
                IslemTuru = request.Type == "Giris" ? "GİR" : "ÇIK",
                RafTanimId = stock.RafTanimId,
                RafNo = stock.RafNo,
                FormKayitId = stock.FormKayitId
            };

            _context.DepoHareketler.Add(hareket);
            await _context.SaveChangesAsync();

            return Ok(new { success = true, message = "İşlem başarılı.", currentQuantity = stock.Miktar });
        }

        [HttpPost("create-talep")]
        public async Task<IActionResult> CreateTalep([FromBody] MobileTalepRequest request)
        {
            if (request == null || request.Items == null || !request.Items.Any())
                return BadRequest(new { success = false, message = "Talep listesi boş olamaz." });

            // TODO: Mevcut kullanıcıyı çek (şimdilik ilk admin'i alalım veya email'e göre bulalım)
            var user = await _context.Users.FirstOrDefaultAsync(); // Demo amaçlı

            int sonSiraNo = await _context.MalzemeTalepFormlar.MaxAsync(t => (int?)t.FormSiraNo) ?? 0;

            var yeniForm = new MalzemeTalepForm
            {
                FormSiraNo = sonSiraNo + 1,
                TalepEdenPersonelId = user?.Id,
                TalepTarihi = DateTime.UtcNow,
                GenelAciklama = request.Description ?? "Mobil Talep",
                Durum = TalepDurumu.Beklemede,
                Satirlar = request.Items.Select(x => new MalzemeTalepSatir
                {
                    StokId = x.StokId,
                    Miktar = x.Quantity
                }).ToList()
            };

            _context.MalzemeTalepFormlar.Add(yeniForm);
            await _context.SaveChangesAsync();

            return Ok(new { success = true, message = $"Talep oluşturuldu. Form No: MASKİ-{yeniForm.FormSiraNo:D4}", formNo = yeniForm.FormSiraNo });
        }

        [HttpGet("talepler")]
        public async Task<IActionResult> GetTalepler()
        {
            var talepler = await _context.MalzemeTalepFormlar
                .Include(t => t.TalepEdenPersonel)
                .Include(t => t.Satirlar)
                    .ThenInclude(s => s.Stok)
                        .ThenInclude(st => st.Malzeme)
                .OrderByDescending(t => t.TalepTarihi)
                .Select(t => new TalepDetailDto
                {
                    Id = t.Id,
                    FormNo = t.FormSiraNo,
                    RequesterName = t.TalepEdenPersonel != null ? t.TalepEdenPersonel.AdSoyad : "Bilinmeyen",
                    Date = t.TalepTarihi.ToString("dd.MM.yyyy HH:mm"),
                    Description = t.GenelAciklama ?? "",
                    Status = t.Durum.ToString(),
                    Items = t.Satirlar.Select(s => new TalepSatirDetailDto
                    {
                        Id = s.Id,
                        MaterialName = s.Stok != null ? (s.Stok.UrunAdi ?? s.Stok.Malzeme.Ad) : "Bilinmeyen",
                        Specification = s.Stok != null ? (s.Stok.FormKayitId.HasValue ? "Var" : "") : "", // Basitleştirilmiş
                        Quantity = s.Miktar,
                        Unit = s.Stok != null ? s.Stok.Birim : "Adet"
                    }).ToList()
                })
                .ToListAsync();

            return Ok(talepler);
        }

        [HttpPost("approve-talep/{id}")]
        public async Task<IActionResult> ApproveTalep(int id)
        {
            var talep = await _context.MalzemeTalepFormlar.FindAsync(id);
            if (talep == null) return NotFound();

            talep.Durum = TalepDurumu.Onaylandi;
            talep.OnayTarihi = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return Ok(new { success = true, message = "Talep onaylandı." });
        }

        [HttpPost("reject-talep/{id}")]
        public async Task<IActionResult> RejectTalep(int id)
        {
            var talep = await _context.MalzemeTalepFormlar.FindAsync(id);
            if (talep == null) return NotFound();

            talep.Durum = TalepDurumu.Reddedildi;
            await _context.SaveChangesAsync();

            return Ok(new { success = true, message = "Talep reddedildi." });
        }

        [HttpGet("movements")]
        public async Task<IActionResult> GetStockMovements([FromQuery] string? type, [FromQuery] string? startDate, [FromQuery] string? endDate)
        {
            var query = _context.DepoHareketler
                .Include(h => h.Malzeme)
                .Include(h => h.KaynakDepo)
                .Include(h => h.HedefDepo)
                .Include(h => h.RafTanim)
                .Include(h => h.FormKayit)
                .ThenInclude(fk => fk.Degerler)
                .AsQueryable();

            if (!string.IsNullOrEmpty(type))
            {
                query = query.Where(h => h.IslemTuru == type);
            }

            if (!string.IsNullOrEmpty(startDate) && DateTime.TryParse(startDate, out var start))
            {
                query = query.Where(h => h.Tarih >= start.ToUniversalTime());
            }

            if (!string.IsNullOrEmpty(endDate) && DateTime.TryParse(endDate, out var end))
            {
                query = query.Where(h => h.Tarih <= end.ToUniversalTime().AddDays(1));
            }

            var movementsData = await query
                .OrderByDescending(h => h.Tarih)
                .Take(100)
                .ToListAsync();

            var movements = movementsData.Select(h => new MovementItemDto
            {
                Id = h.Id,
                MaterialName = h.Malzeme != null ? h.Malzeme.Ad : "Bilinmeyen",
                Amount = h.Miktar,
                Unit = h.Malzeme != null && h.Malzeme.Birim != null ? h.Malzeme.Birim.Ad : "Adet",
                TransactionType = h.IslemTuru != null ? h.IslemTuru : (h.HedefDepoId == null ? "ÇIK" : "GİR"),
                Date = h.Tarih.ToString("dd.MM.yyyy HH:mm"),
                User = h.IslemYapanKisi ?? "Sistem",
                WarehouseName = h.HedefDepo != null ? h.HedefDepo.Ad : (h.KaynakDepo != null ? h.KaynakDepo.Ad : "-"),
                ShelfName = h.RafTanim != null ? h.RafTanim.Ad : (h.RafNo != null ? h.RafNo : "-"),
                Specification = h.FormKayit != null ? string.Join(" ", h.FormKayit.Degerler.Select(d => d.Deger)) : ""
            }).ToList();

            return Ok(movements);
        }

        [HttpGet("metadata")]
        public async Task<IActionResult> GetOperationMetadata()
        {
            var warehouses = await _context.Depolar.Select(d => new { d.Id, d.Ad }).ToListAsync();
            var personnel = await _context.Personeller.Select(p => new { p.Id, p.AdSoyad }).ToListAsync();
            var companies = await _context.Firmalar.Select(f => new { f.Id, f.FirmaAdi }).ToListAsync();
            var materials = await _context.Malzemeler.Select(m => new { m.Id, m.Ad }).ToListAsync();

            return Ok(new { warehouses, personnel, companies, materials });
        }

        [HttpGet("material-details/{id}")]
        public async Task<IActionResult> GetMaterialDetails(int id)
        {
            var material = await _context.Malzemeler
                .Include(m => m.FormSablon)
                .ThenInclude(fs => fs.Alanlar)
                .Include(m => m.Birim)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (material == null) return NotFound();

            var details = new MaterialDetailsDto
            {
                Id = material.Id,
                Name = material.Ad,
                Unit = material.Birim?.Ad ?? "Adet",
                DynamicFields = material.FormSablon?.Alanlar.OrderBy(a => a.Sira).Select(a => new FormAlanDto
                {
                    Id = a.Id,
                    AlanAdi = a.AlanAdi,
                    AlanTipi = a.AlanTipi,
                    Gerekli = a.Gerekli,
                    Secenekler = a.Secenekler ?? ""
                }).ToList() ?? new List<FormAlanDto>()
            };

            return Ok(details);
        }

        [HttpGet("shelves/{warehouseId}")]
        public async Task<IActionResult> GetShelves(int warehouseId)
        {
            var shelves = await _context.RafTanimlar
                .Where(r => r.AktifMi)
                .Select(r => new { r.Id, r.Ad })
                .ToListAsync();

            return Ok(shelves);
        }
        [HttpPost("perform-operation")]
        public async Task<IActionResult> PerformStockOperation([FromBody] MobileStockOperationRequest request)
        {
            if (request == null || request.Amount <= 0)
                return BadRequest(new { success = false, message = "Geçersiz miktar." });

            string userName = "Mobil Kullanıcı"; // İleride Identity'den alınacak

            if (request.OperationType == "GIRIS")
            {
                if (request.MalzemeId == null || request.WarehouseId == null)
                    return BadRequest(new { success = false, message = "Malzeme ve Depo seçimi zorunludur." });

                var malzeme = await _context.Malzemeler.FindAsync(request.MalzemeId);
                if (malzeme == null) return NotFound(new { success = false, message = "Malzeme bulunamadı." });

                // 1. Dinamik Form Kaydı
                FormKayit? yeniFormKayit = null;
                if (request.DynamicFields != null && request.DynamicFields.Any() && malzeme.FormSablonId.HasValue)
                {
                    // BirimId'yi belirle:
                    // 1. Malzemenin kendi birimi
                    // 2. Depo'nun bağlı olduğu birim (İlçe/Birim)
                    // 3. Veritabanındaki ilk birim (Filtreleri atla)
                    var depo = await _context.Depolar.FindAsync(request.WarehouseId);
                    int birimId = malzeme.BirimId ?? depo?.BirimId ?? await _context.Birimler.IgnoreQueryFilters().Select(b => b.Id).FirstOrDefaultAsync();

                    yeniFormKayit = new FormKayit
                    {
                        FormSablonId = malzeme.FormSablonId.Value,
                        BirimId = birimId, 
                        DepoId = request.WarehouseId,
                        OlusturmaTarihi = DateTime.UtcNow,
                        Degerler = request.DynamicFields.Select(d => new FormKayitDeger
                        {
                            FormAlanId = d.FieldId,
                            Deger = d.Value
                        }).ToList()
                    };
                    _context.FormKayitlar.Add(yeniFormKayit);
                    await _context.SaveChangesAsync();
                }

                // 2. Stok Kaydı
                string ekOzellikler = "";
                if (yeniFormKayit != null)
                    ekOzellikler = " " + string.Join(" ", yeniFormKayit.Degerler.Select(d => d.Deger));
                
                string temizUrunAdi = (malzeme.Ad + ekOzellikler).Trim();

                var stok = await _context.DepoStoklar.FirstOrDefaultAsync(s => 
                    s.MalzemeId == request.MalzemeId && 
                    s.DepoId == request.WarehouseId && 
                    s.RafTanimId == request.ShelfId &&
                    s.UrunAdi == temizUrunAdi);

                if (stok == null)
                {
                    stok = new DepoStok
                    {
                        MalzemeId = request.MalzemeId.Value,
                        DepoId = request.WarehouseId.Value,
                        RafTanimId = request.ShelfId,
                        RafNo = request.ShelfNo,
                        Miktar = request.Amount,
                        UrunAdi = temizUrunAdi,
                        Birim = malzeme.Birim?.Ad ?? "Adet",
                        IslemYapanKisi = userName,
                        GuncellemeTarihi = DateTime.UtcNow,
                        FormKayitId = yeniFormKayit?.Id
                    };
                    _context.DepoStoklar.Add(stok);
                }
                else
                {
                    stok.Miktar += request.Amount;
                    stok.IslemYapanKisi = userName;
                    stok.GuncellemeTarihi = DateTime.UtcNow;
                    stok.FormKayitId = yeniFormKayit?.Id;
                }

                // 3. Hareket Kaydı
                var hareket = new DepoHareket
                {
                    MalzemeId = malzeme.Id,
                    HedefDepoId = request.WarehouseId,
                    KaynakDepoId = request.WarehouseId,
                    RafTanimId = request.ShelfId,
                    Miktar = request.Amount,
                    Tarih = DateTime.UtcNow,
                    IslemYapanKisi = userName,
                    IslemTuru = "GİR",
                    Aciklama = request.Note ?? "Mobil Giriş",
                    FormKayitId = yeniFormKayit?.Id,
                    FirmaId = request.CompanyId
                };
                _context.DepoHareketler.Add(hareket);
                
                await _context.SaveChangesAsync();
                return Ok(new { success = true, message = "Stok girişi başarılı." });
            }
            else if (request.OperationType == "CIKIS")
            {
                if (request.StokId == null)
                    return BadRequest(new { success = false, message = "Stok seçimi zorunludur." });

                var stok = await _context.DepoStoklar
                    .Include(s => s.Malzeme)
                    .FirstOrDefaultAsync(s => s.Id == request.StokId);

                if (stok == null || stok.Miktar < request.Amount)
                    return BadRequest(new { success = false, message = "Yetersiz stok." });

                stok.Miktar -= request.Amount;
                stok.GuncellemeTarihi = DateTime.UtcNow;

                var hareket = new DepoHareket
                {
                    MalzemeId = stok.MalzemeId,
                    KaynakDepoId = stok.DepoId,
                    RafTanimId = stok.RafTanimId,
                    Miktar = request.Amount,
                    Tarih = DateTime.UtcNow,
                    IslemYapanKisi = userName,
                    IslemTuru = "ÇIK",
                    Aciklama = request.Note ?? "Mobil Çıkış",
                    PersonelId = request.PersonnelId,
                    FormKayitId = stok.FormKayitId
                };
                _context.DepoHareketler.Add(hareket);

                if (request.ExitType == "Demirbaş" && request.PersonnelId.HasValue)
                {
                    _context.Zimmetler.Add(new Zimmet
                    {
                        MalzemeId = stok.MalzemeId,
                        PersonelId = request.PersonnelId.Value,
                        ZimmetTarihi = DateTime.UtcNow,
                        ZimmetMiktari = request.Amount,
                        Durum = "Aktif"
                    });
                }

                await _context.SaveChangesAsync();
                return Ok(new { success = true, message = "Stok çıkışı başarılı." });
            }
            else if (request.OperationType == "IADE")
            {
                if (request.StokId == null)
                    return BadRequest(new { success = false, message = "Stok seçimi zorunludur." });

                var stok = await _context.DepoStoklar
                    .Include(s => s.Malzeme)
                    .FirstOrDefaultAsync(s => s.Id == request.StokId);

                if (stok == null)
                    return NotFound(new { success = false, message = "Stok kaydı bulunamadı." });

                stok.Miktar += request.Amount;
                stok.GuncellemeTarihi = DateTime.UtcNow;
                stok.IslemYapanKisi = userName;

                var hareket = new DepoHareket
                {
                    MalzemeId = stok.MalzemeId,
                    HedefDepoId = stok.DepoId,
                    KaynakDepoId = stok.DepoId,
                    RafTanimId = stok.RafTanimId,
                    Miktar = request.Amount,
                    Tarih = DateTime.UtcNow,
                    IslemYapanKisi = userName,
                    IslemTuru = "IAD",
                    Aciklama = request.Note ?? "Mobil İade",
                    FormKayitId = stok.FormKayitId
                };
                _context.DepoHareketler.Add(hareket);

                await _context.SaveChangesAsync();
                return Ok(new { success = true, message = "Ürün iadesi başarılı." });
            }

            return BadRequest(new { success = false, message = "Geçersiz işlem tipi." });
        }
    }
}

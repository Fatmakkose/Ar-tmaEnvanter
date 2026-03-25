using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ArıtmaEnvanter.Data;
using ArıtmaEnvanter.Models.Entities;
using System;

namespace ArıtmaEnvanter.Controllers
{
    [Authorize]
    public class MalzemeController : Controller
    {
        private readonly AppDbContext _db;
        public MalzemeController(AppDbContext db) => _db = db;

        public async Task<IActionResult> Index(string? arama)
        {
            var query = _db.Malzemeler
                .Include(m => m.FormSablon)
                .AsQueryable();

            if (!string.IsNullOrEmpty(arama))
            {
                query = query.Where(m => m.Ad.Contains(arama) || (m.AlternatifAd != null && m.AlternatifAd.Contains(arama)));
                ViewBag.Arama = arama;
            }
            return View(await query.OrderBy(m => m.Ad).ToListAsync());
        }


        public async Task<IActionResult> Olustur()
        {
            ViewBag.Sablonlar = await _db.FormSablonlar.OrderBy(s => s.Baslik).ToListAsync();
            ViewBag.Raflar = new List<dynamic>();
            return View();
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Olustur(Malzeme model)
        {
            try
            {
                model.OlusturmaTarihi = DateTime.UtcNow;
                _db.Malzemeler.Add(model);
                await _db.SaveChangesAsync();
                TempData["Basarili"] = "Ürün tanımı başarıyla oluşturuldu.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["Hata"] = ex.Message;
                ViewBag.Sablonlar = await _db.FormSablonlar.OrderBy(s => s.Baslik).ToListAsync();
                return View(model);
            }
        }

        public async Task<IActionResult> Duzenle(int id)
        {
            var malzeme = await _db.Malzemeler.FindAsync(id);
            if (malzeme == null) return NotFound();
            ViewBag.Sablonlar = await _db.FormSablonlar.OrderBy(s => s.Baslik).ToListAsync();
            ViewBag.Raflar = new List<dynamic>();
            return View(malzeme);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Duzenle(Malzeme model)
        {
            try
            {
                _db.Malzemeler.Update(model);
                await _db.SaveChangesAsync();
                TempData["Basarili"] = "Ürün tanımı güncellendi.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["Hata"] = ex.Message;
                ViewBag.Sablonlar = await _db.FormSablonlar.OrderBy(s => s.Baslik).ToListAsync();
                return View(model);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Sil(int id)
        {
            var malzeme = await _db.Malzemeler
                .Include(m => m.FormSablon)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (malzeme == null) return NotFound();

           
            var stocks = await _db.DepoStoklar.Where(s => s.MalzemeId == id).ToListAsync();
            if (stocks.Any(s => s.Miktar > 0))
            {
                TempData["Hata"] = "Bu ürüne ait aktif stok bulunduğundan silinemez. Önce stokları boşaltın.";
                return RedirectToAction(nameof(Index));
            }

            
            if (stocks.Any())
            {
                _db.DepoStoklar.RemoveRange(stocks);
            }

            
            if (malzeme.FormSablonId.HasValue)
            {
                var isUsedElsewhere = await _db.Malzemeler.AnyAsync(m => m.Id != id && m.FormSablonId == malzeme.FormSablonId);
                if (!isUsedElsewhere && malzeme.FormSablon != null)
                {
                    _db.FormSablonlar.Remove(malzeme.FormSablon);
                }
            }

            _db.Malzemeler.Remove(malzeme);
            await _db.SaveChangesAsync();
            TempData["Basarili"] = "Ürün tanımı ve ilgili boş kayıtlar başarıyla silindi.";
            return RedirectToAction(nameof(Index));
        }
        [HttpPost]
        public async Task<IActionResult> OlusturWithBuilder([FromBody] ArıtmaEnvanter.Models.DTOs.MalzemeCreateDto payload)
        {
            try
            {
                if (payload == null || string.IsNullOrEmpty(payload.Ad))
                    return Json(new { success = false, message = "Ürün adı boş olamaz." });

                
                int? formSablonId = null;
                if (payload.Fields != null && payload.Fields.Any())
                {
                    var sablon = new FormSablon
                    {
                        Baslik = payload.Ad + "",
                        Aciklama = payload.Ad + " için özel tanımlanmış özellikler.",
                        OlusturmaTarihi = DateTime.UtcNow,
                        Alanlar = payload.Fields.OrderBy(f => f.DisplayOrder).Select(f => new FormAlan
                        {
                            AlanAdi = f.Label,
                            AlanTipi = IntToAlanTipi(f.FieldType),
                            Gerekli = f.IsRequired,
                            Sira = f.DisplayOrder,
                            Secenekler = f.FieldOptions != null && f.FieldOptions.Any()
                                ? string.Join(",", f.FieldOptions.Select(o => o.Value))
                                : null
                        }).ToList()
                    };
                    _db.FormSablonlar.Add(sablon);
                    await _db.SaveChangesAsync();
                    formSablonId = sablon.Id;
                }

                
                var malzeme = new Malzeme
                {
                    Ad = payload.Ad,
                    AlternatifAd = payload.AlternatifAd,
                    Raf = payload.Raf,
                    Aciklama = payload.Aciklama,
                    FormSablonId = formSablonId,
                    OlusturmaTarihi = DateTime.UtcNow
                };


                _db.Malzemeler.Add(malzeme);
                await _db.SaveChangesAsync();

                TempData["Basarili"] = "Ürün tanımı ve özel formu başarıyla oluşturuldu.";
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        private string IntToAlanTipi(int tip) => tip switch
        {
            0 => "Metin",
            1 => "Sayı",
            2 => "Tarih",
            3 => "Checkbox",
            4 => "Seçenek",
            5 => "Uzun Metin",
            6 => "Görsel",
            7 => "Kategori",
            8 => "Personel",
            9 => "Durum",
            10 => "Birim",
            11 => "Demirbaş",
            12 => "Lokasyon",
            _ => "Metin"
        };
    }
}


using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AritmaEnvanter.Data;
using AritmaEnvanter.Models.Entities;
using System;

namespace AritmaEnvanter.Controllers
{
    [Authorize]
    public class ZimmetController : Controller
    {
        private readonly AppDbContext _db;
        public ZimmetController(AppDbContext db) => _db = db;

        public async Task<IActionResult> Index(string? adSoyad, string? kadroTuru, string? unvan, string? malzemeAdi)
        {
            ViewBag.AdSoyad = adSoyad;
            ViewBag.KadroTuru = kadroTuru;
            ViewBag.Unvan = unvan;
            ViewBag.MalzemeAdi = malzemeAdi;

            ViewBag.PersonelAdlari = await _db.Personeller.Select(p => p.AdSoyad).Distinct().OrderBy(a => a).ToListAsync();
            ViewBag.MalzemeAdlari = await _db.Malzemeler.Select(m => m.Ad).Distinct().OrderBy(a => a).ToListAsync();

            var query = _db.Zimmetler
                .Include(z => z.Malzeme)
                .Include(z => z.Personel)
                    .ThenInclude(p => p.Birim)
                .AsQueryable();

            if (!string.IsNullOrEmpty(adSoyad))
                query = query.Where(z => z.Personel.AdSoyad.Contains(adSoyad));

            if (!string.IsNullOrEmpty(kadroTuru))
                query = query.Where(z => z.Personel.KadroTuru != null && z.Personel.KadroTuru.Contains(kadroTuru));

            if (!string.IsNullOrEmpty(unvan))
                query = query.Where(z => z.Personel.Unvan != null && z.Personel.Unvan.Contains(unvan));

            if (!string.IsNullOrEmpty(malzemeAdi))
                query = query.Where(z => z.Malzeme.Ad.Contains(malzemeAdi));

            return View(await query.OrderByDescending(z => z.ZimmetTarihi).ToListAsync());
        }

        public async Task<IActionResult> Olustur(int? personelId)
        {
            ViewBag.Personeller = await _db.Personeller.OrderBy(p => p.AdSoyad).ToListAsync();
            ViewBag.Malzemeler = await _db.Malzemeler.OrderBy(u => u.Ad).ToListAsync();
            ViewBag.SeciliPersonelId = personelId;
            return View();
        }
        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Olustur(Zimmet model)
        {
            try
            {
                model.ZimmetTarihi = DateTime.UtcNow;
                model.Durum = "Aktif";
                _db.Zimmetler.Add(model);
                await _db.SaveChangesAsync();
                TempData["Basarili"] = "Zimmet başarıyla oluşturuldu.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["Hata"] = ex.Message;
                ViewBag.Personeller = await _db.Personeller.OrderBy(p => p.AdSoyad).ToListAsync();
                ViewBag.Malzemeler = await _db.Malzemeler.OrderBy(u => u.MalzemeTuru).ToListAsync();
                return View(model);
            }
        }
        public async Task<IActionResult> ExcelExport()
        {
            var zimmetler = await _db.Zimmetler
                .Include(z => z.Malzeme)
                .Include(z => z.Personel)
                .OrderByDescending(z => z.ZimmetTarihi)
                .ToListAsync();

            using var workbook = new ClosedXML.Excel.XLWorkbook();
            var ws = workbook.Worksheets.Add("Zimmetler");

            ws.Cell(1, 1).Value = "Ürün";
            ws.Cell(1, 2).Value = "Miktar";
            ws.Cell(1, 3).Value = "Personel";
            ws.Cell(1, 4).Value = "Zimmet Tarihi";
            ws.Cell(1, 5).Value = "İade Tarihi";
            ws.Cell(1, 6).Value = "Durum";
            ws.Cell(1, 7).Value = "Notlar";

            var baslik = ws.Range("A1:G1");
            baslik.Style.Font.Bold = true;
            baslik.Style.Fill.BackgroundColor = ClosedXML.Excel.XLColor.FromHtml("#003d7a");
            baslik.Style.Font.FontColor = ClosedXML.Excel.XLColor.White;
            baslik.Style.Alignment.Horizontal = ClosedXML.Excel.XLAlignmentHorizontalValues.Center;

            int satir = 2;
            foreach (var z in zimmetler)
            {
                ws.Cell(satir, 1).Value = z.Malzeme?.Ad ?? "—";
                ws.Cell(satir, 2).Value = z.ZimmetMiktari;
                ws.Cell(satir, 3).Value = z.Personel?.AdSoyad ?? "—";
                ws.Cell(satir, 4).Value = z.ZimmetTarihi.ToLocalTime().ToString("dd.MM.yyyy");
                ws.Cell(satir, 5).Value = z.IadeTarihi?.ToLocalTime().ToString("dd.MM.yyyy") ?? "—";
                ws.Cell(satir, 6).Value = z.Durum;
                ws.Cell(satir, 7).Value = z.Notlar ?? "—";
                if (satir % 2 == 0)
                    ws.Row(satir).Style.Fill.BackgroundColor = ClosedXML.Excel.XLColor.FromHtml("#f8faff");
                satir++;
            }

            ws.Columns().AdjustToContents();
            ws.RangeUsed()!.Style.Border.OutsideBorder = ClosedXML.Excel.XLBorderStyleValues.Thin;
            ws.RangeUsed()!.Style.Border.InsideBorder = ClosedXML.Excel.XLBorderStyleValues.Thin;

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            return File(stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                $"MaSKI_Zimmetler_{DateTime.Now:yyyyMMdd}.xlsx");
        }
        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ExcelImport(IFormFile excelFile)
        {
            if (excelFile == null || excelFile.Length <= 0)
            {
                TempData["Hata"] = "Lütfen bir excel dosyası seçin.";
                return RedirectToAction(nameof(Index));
            }

            try
            {
                using var stream = new MemoryStream();
                await excelFile.CopyToAsync(stream);
                using var workbook = new ClosedXML.Excel.XLWorkbook(stream);
                var ws = workbook.Worksheets.First();
                int maxRow = ws.LastRowUsed()?.RowNumber() ?? 0;

                int eklenenAdet = 0;
                int atlananAdet = 0;

                
                var units = await _db.Birimler.ToListAsync();
                var people = await _db.Personeller.ToListAsync();
                var categories = await _db.Kategoriler.ToListAsync();
                var products = await _db.Malzemeler.ToListAsync();
                var existingZimmets = await _db.Zimmetler.ToListAsync();

                
                var fallBackCategory = categories.FirstOrDefault(c => c.Ad.Equals("Genel Cihazlar", StringComparison.OrdinalIgnoreCase));
                if (fallBackCategory == null)
                {
                    fallBackCategory = new Kategori { Ad = "Genel Cihazlar" };
                    _db.Kategoriler.Add(fallBackCategory);
                    await _db.SaveChangesAsync();
                    categories.Add(fallBackCategory);
                }

                for (int r = 6; r <= maxRow; r++)
                {
                    var row = ws.Row(r);
                    
                    
                    string valAdSoyad = row.Cell(2).GetString().Trim();
                    string valKadroTuru = row.Cell(3).GetString().Trim();
                    string valUnvan = row.Cell(4).GetString().Trim();
                    string valMalzemeAdi = row.Cell(5).GetString().Trim();
                    string valTeslimTarihiStr = row.Cell(9).GetString().Trim();
                    string valDurumu = row.Cell(10).GetString().Trim();
                    string valDaire = row.Cell(11).GetString().Trim();

                    if (string.IsNullOrWhiteSpace(valAdSoyad) || string.IsNullOrWhiteSpace(valMalzemeAdi))
                        continue;

                    
                    DateTime teslimTarihi = DateTime.UtcNow;
                    if (row.Cell(9).TryGetValue<DateTime>(out DateTime dtCell)) {
                        teslimTarihi = dtCell.ToUniversalTime();
                    } else if (DateTime.TryParse(valTeslimTarihiStr, out DateTime parsedDate)) {
                        teslimTarihi = parsedDate.ToUniversalTime();
                    }

                    
                    string nDaire = NormalizeAd(valDaire);
                    var birim = units.FirstOrDefault(b => NormalizeAd(b.Ad) == nDaire);
                    if (birim == null && !string.IsNullOrWhiteSpace(valDaire))
                    {
                        var targetParent = units.FirstOrDefault(u => u.Ad.Contains("BAŞKANLI")); 
                        birim = new Birim { Ad = valDaire, UstBirimId = targetParent?.Id };
                        _db.Birimler.Add(birim);
                        await _db.SaveChangesAsync();
                        units.Add(birim);
                    }

                    
                    var personel = people.FirstOrDefault(p => p.AdSoyad.Equals(valAdSoyad, StringComparison.OrdinalIgnoreCase));
                    if (personel == null)
                    {
                        personel = new Personel { 
                            AdSoyad = valAdSoyad, 
                            BirimId = birim?.Id,
                            KadroTuru = string.IsNullOrWhiteSpace(valKadroTuru) ? null : valKadroTuru,
                            Unvan = string.IsNullOrWhiteSpace(valUnvan) ? null : valUnvan
                        };
                        _db.Personeller.Add(personel);
                        await _db.SaveChangesAsync();
                        people.Add(personel);
                    }
                    else 
                    {
                        
                        bool updated = false;
                        if (string.IsNullOrWhiteSpace(personel.KadroTuru) && !string.IsNullOrWhiteSpace(valKadroTuru)) {
                            personel.KadroTuru = valKadroTuru;
                            updated = true;
                        }
                        if (string.IsNullOrWhiteSpace(personel.Unvan) && !string.IsNullOrWhiteSpace(valUnvan)) {
                            personel.Unvan = valUnvan;
                            updated = true;
                        }
                        if (personel.BirimId == null && birim != null) {
                            personel.BirimId = birim.Id;
                            updated = true;
                        }
                        if (updated) {
                            _db.Personeller.Update(personel);
                            await _db.SaveChangesAsync();
                        }
                    }

                    
                    var malzeme = products.FirstOrDefault(u => u.Ad.Equals(valMalzemeAdi, StringComparison.OrdinalIgnoreCase));
                    if (malzeme == null)
                    {
                        var sablon = new FormSablon
                        {
                            Baslik = valMalzemeAdi + " Özellikleri",
                            Aciklama = "Excel aktarımı ile otomatik oluşturulmuş şablon",
                            OlusturmaTarihi = DateTime.UtcNow
                        };
                        _db.FormSablonlar.Add(sablon);
                        await _db.SaveChangesAsync();

                        malzeme = new Malzeme
                        {
                            Ad = valMalzemeAdi,
                            Miktar = 1,
                            KategoriId = fallBackCategory.Id,
                            BirimId = birim?.Id,
                            Aciklama = valDurumu,
                            FormSablonId = sablon.Id
                        };
                        _db.Malzemeler.Add(malzeme);
                        await _db.SaveChangesAsync();
                        products.Add(malzeme);
                    }

                    
                    bool hasActiveZimmet = existingZimmets.Any(z => z.MalzemeId == malzeme.Id && z.PersonelId == personel.Id && z.Durum == "Aktif");
                    if (!hasActiveZimmet)
                    {
                        var newZimmet = new Zimmet
                        {
                            MalzemeId = malzeme.Id,
                            PersonelId = personel.Id,
                            Durum = "Aktif",
                            ZimmetTarihi = teslimTarihi,
                            ZimmetMiktari = 1,
                            Notlar = valDurumu
                        };
                        _db.Zimmetler.Add(newZimmet);
                        existingZimmets.Add(newZimmet);
                        eklenenAdet++;
                    }
                    else
                    {
                        atlananAdet++;
                    }
                }

                await _db.SaveChangesAsync();
                TempData["Basarili"] = $"{eklenenAdet} yeni zimmet kaydı başarıyla eklendi." + (atlananAdet > 0 ? $" ({atlananAdet} kayıt zaten mevcut olduğu için atlandı.)" : "");
            }
            catch (Exception ex)
            {
                TempData["Hata"] = "Excel aktarımı sırasında bir hata oluştu: " + ex.Message;
            }

            return RedirectToAction(nameof(Index));
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Iade(int id)
        {
            var zimmet = await _db.Zimmetler.FindAsync(id);
            if (zimmet == null) return NotFound();

            zimmet.Durum = "İade Edildi";
            zimmet.IadeTarihi = DateTime.UtcNow;
            await _db.SaveChangesAsync();

            TempData["Basarili"] = "Zimmet iade edildi.";
            return RedirectToAction(nameof(Index));
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> TumunuSil()
        {
            var zimmets = await _db.Zimmetler.ToListAsync();
            if (zimmets.Any())
            {
                _db.Zimmetler.RemoveRange(zimmets);
                await _db.SaveChangesAsync();
                TempData["Basarili"] = $"{zimmets.Count} adet zimmet kaydı veritabanından tamamen silindi.";
            }
            else
            {
                TempData["Hata"] = "Silinecek zimmet kaydı bulunamadı.";
            }
            return RedirectToAction(nameof(Index));
        }

        private string NormalizeAd(string input)
        {
            if (string.IsNullOrWhiteSpace(input)) return "";
            var n = input.ToLower(new System.Globalization.CultureInfo("tr-TR"));
            n = n.Replace("içme suyu", "içmesuyu");
            n = n.Replace("dairesi", "daire");
            n = n.Replace("müdürlüğü", "müdürlük");
            n = n.Replace("başkanlığı", "başkanlık");
            n = n.Replace(" ", ""); 
            return n;
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Sil(int id)
        {
            var zimmet = await _db.Zimmetler.FindAsync(id);
            if (zimmet == null) return NotFound();

            _db.Zimmetler.Remove(zimmet);
            await _db.SaveChangesAsync();
            TempData["Basarili"] = "Zimmet kaydı silindi.";
            return RedirectToAction(nameof(Index));
        }
    }
}
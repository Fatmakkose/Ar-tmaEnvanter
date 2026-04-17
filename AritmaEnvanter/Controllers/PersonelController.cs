using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AritmaEnvanter.Data;
using AritmaEnvanter.Models.Entities;
using System;

namespace AritmaEnvanter.Controllers
{
    [Authorize]
    public class PersonelController : Controller
    {
        private readonly AppDbContext _db;
        public PersonelController(AppDbContext db) => _db = db;

        public async Task<IActionResult> Index(string? arama, int? birimId)
        {
            ViewBag.Arama = arama;
            ViewBag.BirimId = birimId;
            ViewBag.Birimler = await _db.Birimler.OrderBy(b => b.Ad).ToListAsync();

            var query = _db.Personeller
                
                .AsQueryable();

            if (!string.IsNullOrEmpty(arama))
                query = query.Where(p => p.AdSoyad.Contains(arama) ||
                                         (p.Unvan != null && p.Unvan.Contains(arama)) ||
                                         (p.Email != null && p.Email.Contains(arama)));

            if (birimId.HasValue)
                query = query.Where(p => p.BirimId == birimId);

            return View(await query.OrderBy(p => p.AdSoyad).ToListAsync());
        }
        public async Task<IActionResult> Detay(int id)
        {
            var personel = await _db.Personeller
                
                .Include(p => p.Zimmetler)
                    .ThenInclude(z => z.Malzeme)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (personel == null) return NotFound();
            return View(personel);
        }

        public async Task<IActionResult> Olustur()
        {
            ViewBag.Birimler = await _db.Birimler.OrderBy(b => b.Ad).ToListAsync();
            return View();
        }
        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Olustur(Personel model)
        {
            try
            {
                model.OlusturmaTarihi = DateTime.UtcNow;
                _db.Personeller.Add(model);
                await _db.SaveChangesAsync();
                TempData["Basarili"] = "Personel başarıyla eklendi.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["Hata"] = ex.Message;
                ViewBag.Birimler = await _db.Birimler.OrderBy(b => b.Ad).ToListAsync();
                return View(model);
            }
        }

        public async Task<IActionResult> Duzenle(int id)
        {
            var personel = await _db.Personeller.FindAsync(id);
            if (personel == null) return NotFound();
            ViewBag.Birimler = await _db.Birimler.OrderBy(b => b.Ad).ToListAsync();
            return View(personel);
        }
        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Duzenle(Personel model)
        {
            try
            {
                var mevcut = await _db.Personeller.FindAsync(model.Id);
                if (mevcut == null) return NotFound();
                mevcut.AdSoyad = model.AdSoyad;
                mevcut.Unvan = model.Unvan;
                mevcut.Telefon = model.Telefon;
                mevcut.Email = model.Email;
                mevcut.BirimId = model.BirimId;
                mevcut.KadroTuru = model.KadroTuru;
                await _db.SaveChangesAsync();
                TempData["Basarili"] = "Personel güncellendi.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["Hata"] = ex.Message;
                ViewBag.Birimler = await _db.Birimler.OrderBy(b => b.Ad).ToListAsync();
                return View(model);
            }
        }
        public async Task<IActionResult> ExcelExport()
        {
            var personeller = await _db.Personeller
                
                .OrderBy(p => p.AdSoyad)
                .ToListAsync();

            using var workbook = new ClosedXML.Excel.XLWorkbook();
            var ws = workbook.Worksheets.Add("Personel");

            ws.Cell(1, 1).Value = "Ad Soyad";
            ws.Cell(1, 2).Value = "Ünvan";
            ws.Cell(1, 3).Value = "Birimi"; 
            ws.Cell(1, 4).Value = "Telefon";
            ws.Cell(1, 5).Value = "Email";
            ws.Cell(1, 6).Value = "Eklenme Tarihi";

            var baslik = ws.Range("A1:F1");
            baslik.Style.Font.Bold = true;
            baslik.Style.Fill.BackgroundColor = ClosedXML.Excel.XLColor.FromHtml("#003d7a");
            baslik.Style.Font.FontColor = ClosedXML.Excel.XLColor.White;
            baslik.Style.Alignment.Horizontal = ClosedXML.Excel.XLAlignmentHorizontalValues.Center;

            int satir = 2;
            foreach (var p in personeller)
            {
                ws.Cell(satir, 1).Value = p.AdSoyad;
                ws.Cell(satir, 2).Value = p.Unvan ?? "—";
                ws.Cell(satir, 3).Value = p.Birim?.Ad ?? "—";
                ws.Cell(satir, 4).Value = p.Telefon ?? "—";
                ws.Cell(satir, 5).Value = p.Email ?? "—";
                ws.Cell(satir, 6).Value = p.OlusturmaTarihi.ToLocalTime().ToString("dd.MM.yyyy");
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
                $"MaSKI_Personel_{DateTime.Now:yyyyMMdd}.xlsx");
        }

        [HttpGet]
        public IActionResult ExcelSablon()
        {
            using var workbook = new ClosedXML.Excel.XLWorkbook();
            var ws = workbook.Worksheets.Add("Personel");

            string[] headers = { "Ad Soyad", "Ünvan", "Birimi", "Telefon", "Email" };
            for (int i = 0; i < headers.Length; i++)
            {
                var cell = ws.Cell(1, i + 1);
                cell.Value = headers[i];
                cell.Style.Font.Bold = true;
                cell.Style.Fill.BackgroundColor = ClosedXML.Excel.XLColor.FromHtml("#003d7a");
                cell.Style.Font.FontColor = ClosedXML.Excel.XLColor.White;
            }

            
            ws.Cell(2, 1).Value = "Örnek Personel";
            ws.Cell(2, 2).Value = "Tekniker";
            ws.Cell(2, 3).Value = "Bilgi İşlem";
            ws.Cell(2, 4).Value = "0555...";
            ws.Cell(2, 5).Value = "ornek@AritmaEnvanter.gov.tr";

            ws.Columns().AdjustToContents();
            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            return File(stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Personel_Import_Sablon.xlsx");
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> ExcelImport(IFormFile excelFile)
        {
            if (excelFile == null || excelFile.Length == 0)
            {
                TempData["Hata"] = "Lütfen geçerli bir Excel dosyası seçin.";
                return RedirectToAction(nameof(Index));
            }

            try
            {
                using var stream = new MemoryStream();
                await excelFile.CopyToAsync(stream);
                using var workbook = new ClosedXML.Excel.XLWorkbook(stream);
                var worksheet = workbook.Worksheet(1);
                var rows = worksheet.RangeUsed().RowsUsed().Skip(1); // Başlığı atla

                var birimler = await _db.Birimler.ToListAsync();
                var mevcutPersoneller = await _db.Personeller.ToListAsync();
                int eklenen = 0;
                int hatalar = 0;
                int mukerrer = 0;

                foreach (var row in rows)
                {
                    var adSoyad = row.Cell(1).GetValue<string>();
                    if (string.IsNullOrWhiteSpace(adSoyad)) continue;

                    var unvan = row.Cell(2).GetValue<string>();
                    var birimAdi = row.Cell(3).GetValue<string>();
                    var telefon = row.Cell(4).GetValue<string>();
                    var email = row.Cell(5).GetValue<string>();

                    var birim = birimler.FirstOrDefault(b =>
                    {
                        var dbAd = b.Ad.Trim().ToLower(new System.Globalization.CultureInfo("tr-TR"));
                        var excelAd = birimAdi?.Trim().ToLower(new System.Globalization.CultureInfo("tr-TR"));

                        
                        return dbAd == excelAd ||
                               (excelAd != null && (excelAd.Contains(dbAd) || dbAd.Contains(excelAd)));
                    });

                    if (birim == null)
                    {
                        hatalar++;
                        continue;
                    }

                    
                    bool varMi = mevcutPersoneller.Any(p =>
                        p.AdSoyad.Trim().Equals(adSoyad.Trim(), StringComparison.OrdinalIgnoreCase) &&
                        p.BirimId == birim.Id);

                    if (varMi)
                    {
                        mukerrer++;
                        continue;
                    }

                    var yeniPersonel = new Personel
                    {
                        AdSoyad = adSoyad.Trim(),
                        Unvan = unvan?.Trim(),
                        BirimId = birim.Id,
                        Telefon = telefon?.Trim(),
                        Email = email?.Trim(),
                        OlusturmaTarihi = DateTime.UtcNow
                    };

                    _db.Personeller.Add(yeniPersonel);
                    mevcutPersoneller.Add(yeniPersonel); 
                    eklenen++;
                }

                await _db.SaveChangesAsync();
                TempData["Basarili"] = $"{eklenen} yeni personel eklendi. {mukerrer} mükerrer kayıt atlandı. {hatalar} satırda birim bulunamadı.";
            }
            catch (Exception ex)
            {
                TempData["Hata"] = "Excel işlenirken hata oluştu: " + ex.Message;
            }

            return RedirectToAction(nameof(Index));
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Sil(int id)
        {
            var personel = await _db.Personeller
                .Include(p => p.Zimmetler)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (personel == null) return NotFound();

            if (personel.Zimmetler.Any(z => z.Durum == "Aktif"))
            {
                TempData["Hata"] = "Bu personelde aktif zimmet var. Önce zimmetleri iade edin.";
                return RedirectToAction(nameof(Index));
            }

            _db.Personeller.Remove(personel);
            await _db.SaveChangesAsync();
            TempData["Basarili"] = "Personel silindi.";
            return RedirectToAction(nameof(Index));
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> TopluSil([FromBody] int[] ids)
        {
            if (ids == null || ids.Length == 0)
            {
                return Json(new { success = false, message = "Hiç personel seçilmedi." });
            }

            try
            {
                var silinecekler = await _db.Personeller
                    .Include(p => p.Zimmetler)
                    .Where(p => ids.Contains(p.Id))
                    .ToListAsync();

                int basarili = 0;
                int hatali = 0;

                foreach (var p in silinecekler)
                {
                    if (p.Zimmetler.Any(z => z.Durum == "Aktif"))
                    {
                        hatali++;
                        continue;
                    }

                    _db.Personeller.Remove(p);
                    basarili++;
                }

                await _db.SaveChangesAsync();

                string mesaj = $"{basarili} personel silindi.";
                if (hatali > 0)
                {
                    mesaj += $" {hatali} personel üzerinde aktif zimmet olduğu için silinemedi.";
                }

                return Json(new { success = true, message = mesaj });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Bir hata oluştu: " + ex.Message });
            }
        }
    }
}

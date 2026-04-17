using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AritmaEnvanter.Data;
using AritmaEnvanter.Models.Entities;
using AritmaEnvanter.Models.DTOs;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AritmaEnvanter.Controllers
{
    [Authorize]
    public class FormController : Controller
    {
        private readonly AppDbContext _db;

        public FormController(AppDbContext db)
        {
            _db = db;
        }

        public async Task<IActionResult> Index(string? arama)
        {
            var query = _db.FormSablonlar
                .Include(f => f.Kayitlar)
                .Include(f => f.Alanlar)
                .AsQueryable();

            if (!string.IsNullOrEmpty(arama))
            {
                query = query.Where(f => f.Baslik.Contains(arama));
            }

            var sablonlar = await query
                .OrderByDescending(f => f.OlusturmaTarihi)
                .ToListAsync();

            return View(sablonlar);
        }


        public async Task<IActionResult> TumVarliklar(string? arama)
        {
            ViewBag.Arama = arama;

            var districtClaim = User.Claims.FirstOrDefault(c => c.Type == "DistrictId")?.Value;
            int.TryParse(districtClaim, out int districtId);
            var warehouseClaim = User.Claims.FirstOrDefault(c => c.Type == "DefaultWarehouseId")?.Value;
            int.TryParse(warehouseClaim, out int depoId);
            bool isIlceSorumlusu = User.IsInRole("IlceSorumlusu");

            var query = _db.FormKayitlar
                .Include(k => k.FormSablon)
                    .ThenInclude(s => s.Kategori)


                .Include(k => k.Degerler)
                    .ThenInclude(d => d.FormAlan)
                .Include(k => k.Birim)
                .Include(k => k.Depo)
                .AsQueryable();



            if (!string.IsNullOrEmpty(arama))
            {
                query = query.Where(k =>
                    (k.Barkod != null && k.Barkod.Contains(arama)) ||
                    (k.KayitNo != null && k.KayitNo.Contains(arama)) ||
                    (k.FormSablon.Baslik.Contains(arama)) ||
                    (k.FormSablon.Kategori != null && k.FormSablon.Kategori.Ad.Contains(arama)) ||
                    k.Degerler.Any(d => d.Deger != null && d.Deger.Contains(arama)) ||
                    (k.KaydedenKullanici != null && k.KaydedenKullanici.Contains(arama)));
            }

            var kayitlar = await query.OrderByDescending(k => k.OlusturmaTarihi).ToListAsync();
            return View(kayitlar);
        }


        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Builder(int? id)
        {
            ViewBag.AnaFormlar = await _db.FormSablonlar
    .Where(f => f.UstFormId == null)
    .OrderBy(f => f.Baslik)
    .ToListAsync();

            ViewBag.Kategoriler = await _db.Kategoriler
                .Include(k => k.UstKategori)
                .OrderBy(k => k.UstKategori != null ? k.UstKategori.Ad : k.Ad)
                .ThenBy(k => k.Ad)
                .ToListAsync();

            FormDefinitionDto dto;
            if (id.HasValue)
            {
                var sablon = await _db.FormSablonlar
                    .Include(f => f.Alanlar)
                    .FirstOrDefaultAsync(f => f.Id == id.Value);
                if (sablon == null) return NotFound();

                dto = new FormDefinitionDto
                {
                    Id = sablon.Id,
                    Name = sablon.Baslik,
                    Description = sablon.Aciklama,
                    ParentId = sablon.UstFormId,
                    KategoriId = sablon.KategoriId,
                    Fields = sablon.Alanlar.OrderBy(a => a.Sira).Select(a => new FieldDto
                    {
                        Id = a.Id,
                        Label = a.AlanAdi,
                        FieldType = AlanTipiToInt(a.AlanTipi),
                        IsRequired = a.Gerekli,
                        DisplayOrder = a.Sira,
                        FieldOptions = string.IsNullOrEmpty(a.Secenekler) ? new() :
                            a.Secenekler.Split(',').Select((s, i) => new FieldOptionDto
                            {
                                Id = 0,
                                Value = s.Trim(),
                                DisplayOrder = i
                            }).ToList()
                    }).ToList()
                };
            }
            else
            {
                dto = new FormDefinitionDto();
            }
            return View(dto);
        }
        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> Builder([FromBody] FormDefinitionDto payload)
        {
            try
            {
                FormSablon sablon;

                if (payload.Id > 0)
                {
                    sablon = await _db.FormSablonlar
                        .Include(f => f.Alanlar)
                        .FirstOrDefaultAsync(f => f.Id == payload.Id)
                        ?? new FormSablon();
                    _db.FormAlanlar.RemoveRange(sablon.Alanlar);
                }
                else
                {
                    sablon = new FormSablon { OlusturmaTarihi = DateTime.UtcNow };
                    _db.FormSablonlar.Add(sablon);
                }
                sablon.Baslik = payload.Name;
                sablon.Aciklama = payload.Description;
                sablon.UstFormId = payload.ParentId > 0 ? payload.ParentId : null;
                sablon.KategoriId = payload.KategoriId > 0 ? payload.KategoriId : null;

                if (payload.Id <= 0)
                {
                    if (sablon.UstFormId == null && sablon.KategoriId != null)
                    {
                        Kategori? kat = await _db.Kategoriler.FindAsync(sablon.KategoriId);
                        var count = await _db.FormSablonlar.CountAsync(f => f.KategoriId == sablon.KategoriId && f.UstFormId == null);
                        if (kat != null && kat.HiyerarsikId != null)
                        {
                            sablon.HiyerarsikId = $"{kat.HiyerarsikId}.{count + 1}";
                        }
                    }
                    else if (sablon.UstFormId != null)
                    {
                        var ust = await _db.FormSablonlar.FindAsync(sablon.UstFormId);
                        var count = await _db.FormSablonlar.CountAsync(f => f.UstFormId == sablon.UstFormId);
                        if (ust != null && ust.HiyerarsikId != null)
                        {
                            sablon.HiyerarsikId = $"{ust.HiyerarsikId}.{count + 1}";
                        }
                    }
                }

                if (payload.Fields != null)
                {
                    sablon.Alanlar = payload.Fields.Select(f => new FormAlan
                    {
                        AlanAdi = f.Label,
                        AlanTipi = IntToAlanTipi(f.FieldType),
                        Gerekli = f.IsRequired,
                        Sira = f.DisplayOrder,
                        Secenekler = f.FieldOptions != null && f.FieldOptions.Any()
                            ? string.Join(",", f.FieldOptions.Select(o => o.Value))
                            : null
                    }).ToList();
                }

                await _db.SaveChangesAsync();
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }


        public async Task<IActionResult> Doldur(int id)
        {
            var sablon = await _db.FormSablonlar
                .Include(f => f.Alanlar.OrderBy(a => a.Sira))
                .FirstOrDefaultAsync(f => f.Id == id);
            if (sablon == null) return NotFound();


            ViewBag.AnaKategoriler = await _db.Kategoriler
                .Where(k => k.UstKategoriId == null)
                .OrderBy(k => k.Ad)
                .ToListAsync();


            ViewBag.Personeller = await _db.Personeller

                .OrderBy(p => p.AdSoyad)
                .ToListAsync();


            ViewBag.Birimler = await _db.Birimler
                .Include(b => b.UstBirim)
                .OrderBy(b => b.Ad)
                .ToListAsync();


            ViewBag.Urunler = await _db.Malzemeler


                .OrderBy(u => u.UrunTuru)
                .ToListAsync();


            ViewBag.Lokasyonlar = await _db.Lokasyonlar

                .OrderBy(l => l.Birim!.Ad)
                .ThenBy(l => l.Kat)
                .ToListAsync();

            return View(sablon);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Doldur(int id, IFormCollection form)
        {
            try
            {
                var sablon = await _db.FormSablonlar

                    .Include(f => f.Alanlar)
                    .FirstOrDefaultAsync(f => f.Id == id);
                if (sablon == null) return NotFound();

                string siniflandirma = sablon.Kategori?.HiyerarsikId ?? "00.00.00.00";

                var satirlar = form.Keys
                    .Where(k => k.StartsWith("satir_") && k.Contains("_alan_"))
                    .Select(k => int.Parse(k.Split('_')[1]))
                    .Distinct()
                    .OrderBy(x => x)
                    .ToList();


                var districtClaim = User.Claims.FirstOrDefault(c => c.Type == "DistrictId")?.Value;
                int.TryParse(districtClaim, out int districtId);
                var warehouseClaim = User.Claims.FirstOrDefault(c => c.Type == "DefaultWarehouseId")?.Value;
                int.TryParse(warehouseClaim, out int depoId);


                if (districtId == 0) districtId = 1;

                foreach (var satirNo in satirlar)
                {

                    var mevcutKayitSayisi = await _db.FormKayitlar
                        .CountAsync(k => k.FormSablonId == id);

                    var kayit = new FormKayit
                    {
                        FormSablonId = id,
                        BirimId = districtId,
                        DepoId = depoId > 0 ? depoId : null,
                        KayitNo = $"{id}.{mevcutKayitSayisi + 1}",
                        Barkod = $"255-{siniflandirma}-{mevcutKayitSayisi + 1:D4}-01",
                        OlusturmaTarihi = DateTime.UtcNow,
                        KaydedenKullanici = User.Identity!.Name
                    };

                    foreach (var alan in sablon.Alanlar)
                    {
                        var key = $"satir_{satirNo}_alan_{alan.Id}";
                        string? deger = null;
                        string? gorselUrl = null;


                        if (alan.AlanTipi == "Görsel")
                        {
                            var fileKey = $"{key}_file";
                            var dosya = form.Files[fileKey];
                            if (dosya != null && dosya.Length > 0)
                            {
                                using var ms = new MemoryStream();
                                await dosya.CopyToAsync(ms);
                                var base64 = Convert.ToBase64String(ms.ToArray());
                                gorselUrl = $"data:{dosya.ContentType};base64,{base64}";
                                deger = dosya.FileName;
                            }
                        }
                        else
                        {
                            deger = form[key].ToString();
                        }

                        kayit.Degerler.Add(new FormKayitDeger
                        {
                            FormAlanId = alan.Id,
                            Deger = deger,
                            GorselUrl = gorselUrl
                        });
                    }

                    _db.FormKayitlar.Add(kayit);


                    var personelAlan = sablon.Alanlar.FirstOrDefault(a => a.AlanTipi == "Personel");
                    if (personelAlan != null)
                    {
                        var personelAd = kayit.Degerler.FirstOrDefault(d => d.FormAlanId == personelAlan.Id)?.Deger;
                        if (!string.IsNullOrEmpty(personelAd))
                        {
                            var personel = await _db.Personeller.FirstOrDefaultAsync(p => p.AdSoyad == personelAd);
                            if (personel != null)
                            {

                                var kategoriId = sablon.KategoriId;
                                if (kategoriId == null)
                                {
                                    Kategori? fallbackKategori = null;
                                    if (fallbackKategori == null)
                                    {
                                        fallbackKategori = new Kategori { Ad = "Diğer" };
                                        ;
                                        await _db.SaveChangesAsync();
                                    }
                                    kategoriId = fallbackKategori.Id;
                                }

                                var durumAlan = sablon.Alanlar.FirstOrDefault(a => a.AlanTipi == "Durum");
                                string urunDurum = "Aktif";
                                if (durumAlan != null)
                                {
                                    var secilenDurum = kayit.Degerler.FirstOrDefault(d => d.FormAlanId == durumAlan.Id)?.Deger;
                                    if (!string.IsNullOrEmpty(secilenDurum))
                                    {
                                        urunDurum = secilenDurum;
                                    }
                                }

                                var urun = new Urun
                                {
                                    UrunTuru = $"{sablon.Baslik} ({kayit.KayitNo})",
                                    SeriNo = kayit.Barkod,
                                    Miktar = 1,
                                    KategoriId = kategoriId.Value,
                                    BirimId = personel.BirimId,
                                    DepoId = kayit.DepoId,
                                    Durum = urunDurum,
                                    OlusturmaTarihi = DateTime.UtcNow
                                };
                                _db.Urunler.Add(urun);
                                await _db.SaveChangesAsync();


                                var zimmet = new Zimmet
                                {
                                    UrunId = urun.Id,
                                    PersonelId = personel.Id,
                                    ZimmetTarihi = DateTime.UtcNow,
                                    Durum = "Aktif",
                                    ZimmetMiktari = 1
                                };
                                _db.Zimmetler.Add(zimmet);
                            }
                        }
                    }
                }

                await _db.SaveChangesAsync();
                TempData["Basarili"] = $"{satirlar.Count} ürün kaydı başarıyla eklendi.";
                return RedirectToAction(nameof(Kayitlar), new { id });
            }
            catch (Exception ex)
            {
                TempData["Hata"] = ex.Message;
                return RedirectToAction(nameof(Doldur), new { id });
            }
        }

        public async Task<IActionResult> Kayitlar(int id, string? arama, string? baslangic, string? bitis)
        {
            ViewBag.Arama = arama;
            ViewBag.Baslangic = baslangic;
            ViewBag.Bitis = bitis;

            var districtClaim = User.Claims.FirstOrDefault(c => c.Type == "DistrictId")?.Value;
            int.TryParse(districtClaim, out int districtId);
            var warehouseClaim = User.Claims.FirstOrDefault(c => c.Type == "DefaultWarehouseId")?.Value;
            int.TryParse(warehouseClaim, out int depoId);
            bool isIlceSorumlusu = User.IsInRole("IlceSorumlusu");

            var sablon = await _db.FormSablonlar
                .Include(f => f.Alanlar.OrderBy(a => a.Sira))
                .Include(f => f.Kayitlar)
                    .ThenInclude(k => k.Degerler)
                        .ThenInclude(d => d.FormAlan)
                .Include(f => f.Kayitlar)
                    .ThenInclude(k => k.Birim)
                .Include(f => f.Kayitlar)
                    .ThenInclude(k => k.Depo)
                .FirstOrDefaultAsync(f => f.Id == id);

            if (sablon == null) return NotFound();


            var kayitlarQuery = sablon.Kayitlar.AsQueryable();



            if (!string.IsNullOrEmpty(arama))
                kayitlarQuery = kayitlarQuery.Where(k =>
                    (k.Barkod != null && k.Barkod.Contains(arama)) ||
                    k.Degerler.Any(d => d.Deger != null && d.Deger.Contains(arama)) ||
                    (k.KaydedenKullanici != null && k.KaydedenKullanici.Contains(arama)));

            if (!string.IsNullOrEmpty(baslangic) && DateTime.TryParse(baslangic, out var bas))
                kayitlarQuery = kayitlarQuery.Where(k => k.OlusturmaTarihi >= bas.ToUniversalTime());

            if (!string.IsNullOrEmpty(bitis) && DateTime.TryParse(bitis, out var bit))
                kayitlarQuery = kayitlarQuery.Where(k => k.OlusturmaTarihi <= bit.ToUniversalTime().AddDays(1));

            sablon.Kayitlar = kayitlarQuery.OrderByDescending(k => k.OlusturmaTarihi).ToList();


            var urunQuery = _db.Malzemeler.Where(u => u.KategoriId == sablon.KategoriId);


            if (!string.IsNullOrEmpty(arama))
                urunQuery = urunQuery.Where(u => (u.UrunTuru != null && u.UrunTuru.Contains(arama)) || (u.SeriNo != null && u.SeriNo.Contains(arama)));

            ViewBag.StandartUrunler = await urunQuery.OrderByDescending(u => u.OlusturmaTarihi).ToListAsync();


            var depoStokQuery = _db.DepoStoklar.Include(ds => ds.Malzeme).AsQueryable();


            if (sablon.Kategori != null)
            {
                depoStokQuery = depoStokQuery.Where(ds => ds.Malzeme != null && ds.Malzeme.KategoriId == sablon.KategoriId);
            }
            else
            {

                depoStokQuery = depoStokQuery.Where(ds => ds.UrunAdi != null && ds.UrunAdi.Contains(sablon.Baslik));
            }

            ViewBag.DepoStoklar = await depoStokQuery.ToListAsync();

            return View(sablon);
        }


        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Sil(int id)
        {
            var success = await DeleteSablonInternal(id);
            if (!success) return NotFound();

            TempData["Basarili"] = "Form ve tüm ilişkili kayıtlar silindi.";
            return RedirectToAction(nameof(Index));
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> TopluSil([FromForm] int[] ids)
        {
            if (ids == null || ids.Length == 0)
            {
                TempData["Hata"] = "Silinecek öğe seçilmedi.";
                return RedirectToAction(nameof(Index));
            }

            try
            {
                int silinenAdet = 0;
                foreach (var id in ids)
                {
                    // Şablonun hala var olup olmadığını kontrol et (önceki silmeler tarafından silinmiş olabilir)
                    if (await _db.FormSablonlar.AnyAsync(f => f.Id == id))
                    {
                        if (await DeleteSablonInternal(id))
                        {
                            silinenAdet++;
                        }
                    }
                }

                if (silinenAdet > 0)
                {
                    TempData["Basarili"] = $"{silinenAdet} adet form ve ilişkili kayıtlar başarıyla temizlendi.";
                }
                else
                {
                    TempData["Hata"] = "Seçilen hiçbir kayıt silinemedi.";
                }
            }
            catch (Exception ex)
            {
                TempData["Hata"] = "Toplu silme sırasında bir hata oluştu: " + ex.Message;
            }

            return RedirectToAction(nameof(Index));
        }

        private async Task<bool> DeleteSablonInternal(int id)
        {
            var sablon = await _db.FormSablonlar
                .Include(f => f.AltFormlar)
                .Include(f => f.Alanlar)
                .Include(f => f.Kayitlar)
                    .ThenInclude(k => k.Degerler)
                .FirstOrDefaultAsync(f => f.Id == id);

            if (sablon == null) return false;

            var malzemeler = await _db.Malzemeler.Where(m => m.FormSablonId == id).ToListAsync();
            foreach (var m in malzemeler)
            {
                var hasStock = await _db.DepoStoklar.AnyAsync(s => s.MalzemeId == m.Id && s.Miktar > 0);
                var hasZimmet = await _db.Zimmetler.AnyAsync(z => z.MalzemeId == m.Id);

                if (!hasStock && !hasZimmet)
                {
                    _db.Malzemeler.Remove(m);
                }
                else
                {
                    m.FormSablonId = null;
                }
            }

            foreach (var kayit in sablon.Kayitlar)
            {
                if (kayit.Degerler.Any())
                {
                    _db.FormKayitDegerler.RemoveRange(kayit.Degerler);
                }
            }

            if (sablon.Kayitlar.Any())
            {
                _db.FormKayitlar.RemoveRange(sablon.Kayitlar);
            }

            if (sablon.Alanlar.Any())
            {
                _db.FormAlanlar.RemoveRange(sablon.Alanlar);
            }

            if (sablon.AltFormlar.Any())
            {
                foreach (var alt in sablon.AltFormlar)
                {
                    var altSablon = await _db.FormSablonlar
                        .Include(a => a.Alanlar)
                        .Include(a => a.Kayitlar).ThenInclude(k => k.Degerler)
                        .FirstOrDefaultAsync(a => a.Id == alt.Id);

                    if (altSablon != null)
                    {
                        var altMalzemeler = await _db.Malzemeler.Where(m => m.FormSablonId == altSablon.Id).ToListAsync();
                        foreach (var altM in altMalzemeler)
                        {
                            var hasStock = await _db.DepoStoklar.AnyAsync(s => s.MalzemeId == altM.Id && s.Miktar > 0);
                            var hasZimmet = await _db.Zimmetler.AnyAsync(z => z.MalzemeId == altM.Id);

                            if (!hasStock && !hasZimmet)
                            {
                                _db.Malzemeler.Remove(altM);
                            }
                            else
                            {
                                altM.FormSablonId = null;
                            }
                        }

                        foreach (var altKayit in altSablon.Kayitlar)
                        {
                            if (altKayit.Degerler.Any()) _db.FormKayitDegerler.RemoveRange(altKayit.Degerler);
                        }
                        if (altSablon.Kayitlar.Any()) _db.FormKayitlar.RemoveRange(altSablon.Kayitlar);
                        if (altSablon.Alanlar.Any()) _db.FormAlanlar.RemoveRange(altSablon.Alanlar);

                        _db.FormSablonlar.Remove(altSablon);
                    }
                }
            }

            _db.FormSablonlar.Remove(sablon);
            await _db.SaveChangesAsync();
            return true;
        }


        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SilKayit(int id, int formId)
        {
            var kayit = await _db.FormKayitlar
                .Include(k => k.Degerler)
                .FirstOrDefaultAsync(k => k.Id == id);

            if (kayit == null) return NotFound();

            if (kayit.Degerler.Any())
            {
                _db.FormKayitDegerler.RemoveRange(kayit.Degerler);
            }

            _db.FormKayitlar.Remove(kayit);
            await _db.SaveChangesAsync();
            TempData["Basarili"] = "Kayıt başarıyla silindi.";
            return RedirectToAction(nameof(Kayitlar), new { id = formId });
        }


        [HttpGet]
        public async Task<IActionResult> AltKategorileriGetir(int ustId)
        {
            var altKategoriler = await _db.Kategoriler
                .Where(k => k.UstKategoriId == ustId)
                .Select(k => new { k.Id, k.Ad })
                .OrderBy(k => k.Ad)
                .ToListAsync();

            return Json(altKategoriler);
        }

        [HttpGet]
        public async Task<IActionResult> BarkodSorgula(string barkod)
        {

            var urun = await _db.Malzemeler.FirstOrDefaultAsync(u => u.SeriNo == barkod);
            if (urun != null && urun.Kategori != null)
            {
                return Json(new { success = true, kategoriAd = urun.Kategori.Ad, urunAdi = urun.UrunTuru });
            }


            var kayit = await _db.FormKayitlar
                .Include(f => f.FormSablon)
                .ThenInclude(s => s.Kategori)
                .FirstOrDefaultAsync(f => f.Barkod == barkod);

            if (kayit != null && kayit.FormSablon != null)
            {
                return Json(new
                {
                    success = true,
                    kategoriAd = kayit.FormSablon.Kategori?.Ad ?? "Kategorisiz",
                    urunAdi = kayit.FormSablon.Baslik + " (Kayıt No: " + kayit.KayitNo + ")"
                });
            }

            return Json(new { success = false, message = "Bu barkoda (Demirbaş No) ait herhangi bir ürün veya kayıt bulunamadı." });
        }

        [HttpGet]
        public async Task<IActionResult> ExcelSablon(int id)
        {
            var sablon = await _db.FormSablonlar
                .Include(f => f.Alanlar)
                .FirstOrDefaultAsync(f => f.Id == id);

            if (sablon == null) return NotFound();

            using var workbook = new ClosedXML.Excel.XLWorkbook();
            var ws = workbook.Worksheets.Add(sablon.Baslik.Length > 30 ? sablon.Baslik.Substring(0, 30) : sablon.Baslik);

            var alanlar = sablon.Alanlar.OrderBy(a => a.Sira).ToList();
            for (int i = 0; i < alanlar.Count; i++)
            {
                var cell = ws.Cell(1, i + 1);
                cell.Value = alanlar[i].AlanAdi;
                cell.Style.Font.Bold = true;
                cell.Style.Fill.BackgroundColor = ClosedXML.Excel.XLColor.FromHtml("#003d7a");
                cell.Style.Font.FontColor = ClosedXML.Excel.XLColor.White;
            }



            ws.Columns().AdjustToContents();
            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            return File(stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"{sablon.Baslik}_Aktarim_Sablonu.xlsx");
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> ExcelImport(int id, IFormFile excelFile)
        {
            if (excelFile == null || excelFile.Length == 0)
            {
                TempData["Hata"] = "Lütfen geçerli bir Excel dosyası seçin.";
                return RedirectToAction(nameof(Kayitlar), new { id });
            }

            var sablon = await _db.FormSablonlar

                .Include(f => f.Alanlar)
                .FirstOrDefaultAsync(f => f.Id == id);

            if (sablon == null) return NotFound();

            try
            {
                using var stream = new MemoryStream();
                await excelFile.CopyToAsync(stream);
                using var workbook = new ClosedXML.Excel.XLWorkbook(stream);
                var worksheet = workbook.Worksheet(1);
                var rows = worksheet.RangeUsed().RowsUsed().Skip(1);

                var headers = worksheet.Row(1).Cells().Select(c => c.GetValue<string>().Trim()).ToList();
                int eklenen = 0;


                var districtClaim = User.Claims.FirstOrDefault(c => c.Type == "DistrictId")?.Value;
                int.TryParse(districtClaim, out int districtId);
                var warehouseClaim = User.Claims.FirstOrDefault(c => c.Type == "DefaultWarehouseId")?.Value;
                int.TryParse(warehouseClaim, out int depoId);
                if (districtId == 0) districtId = 1;

                string siniflandirma = sablon.Kategori?.HiyerarsikId ?? "00.00.00.00";

                foreach (var row in rows)
                {

                    var mevcutKayitSayisi = await _db.FormKayitlar.CountAsync(k => k.FormSablonId == id);

                    var kayit = new FormKayit
                    {
                        FormSablonId = id,
                        BirimId = districtId,
                        DepoId = depoId > 0 ? depoId : null,
                        KayitNo = $"{id}.{mevcutKayitSayisi + 1}",
                        Barkod = $"255-{siniflandirma}-{mevcutKayitSayisi + 1:D4}-01",
                        OlusturmaTarihi = DateTime.UtcNow,
                        KaydedenKullanici = User.Identity!.Name
                    };

                    bool rowValid = false;
                    foreach (var alan in sablon.Alanlar)
                    {
                        int colIndex = headers.IndexOf(alan.AlanAdi.Trim());
                        if (colIndex >= 0)
                        {
                            var value = row.Cell(colIndex + 1).GetValue<string>()?.Trim();
                            if (!string.IsNullOrEmpty(value))
                            {
                                rowValid = true;
                                kayit.Degerler.Add(new FormKayitDeger
                                {
                                    FormAlanId = alan.Id,
                                    Deger = value
                                });
                            }
                        }
                    }

                    if (rowValid)
                    {
                        _db.FormKayitlar.Add(kayit);
                        eklenen++;


                        var personelAlan = sablon.Alanlar.FirstOrDefault(a => a.AlanTipi == "Personel");
                        if (personelAlan != null)
                        {
                            var personelAd = kayit.Degerler.FirstOrDefault(d => d.FormAlanId == personelAlan.Id)?.Deger;
                            if (!string.IsNullOrEmpty(personelAd))
                            {
                                var personel = await _db.Personeller.FirstOrDefaultAsync(p => p.AdSoyad == personelAd);
                                if (personel != null)
                                {
                                    var kategoriId = sablon.KategoriId;
                                    if (kategoriId == null)
                                    {
                                        Kategori? fallbackKategori = null;
                                        if (fallbackKategori == null)
                                        {
                                            fallbackKategori = new Kategori { Ad = "Diğer" };
                                            ;
                                        }
                                        else kategoriId = fallbackKategori.Id;
                                    }

                                    var durumAlan = sablon.Alanlar.FirstOrDefault(a => a.AlanTipi == "Durum");
                                    string urunDurum = "Aktif";
                                    if (durumAlan != null)
                                    {
                                        var secilenDurum = kayit.Degerler.FirstOrDefault(d => d.FormAlanId == durumAlan.Id)?.Deger;
                                        if (!string.IsNullOrEmpty(secilenDurum)) urunDurum = secilenDurum;
                                    }

                                    var urun = new Urun
                                    {
                                        UrunTuru = $"{sablon.Baslik} ({kayit.KayitNo})",
                                        SeriNo = kayit.Barkod,
                                        Miktar = 1,
                                        KategoriId = kategoriId ?? 0,
                                        BirimId = personel.BirimId,
                                        DepoId = kayit.DepoId,
                                        Durum = urunDurum,
                                        OlusturmaTarihi = DateTime.UtcNow
                                    };
                                    _db.Urunler.Add(urun);


                                    var zimmet = new Zimmet
                                    {
                                        Urun = urun,
                                        PersonelId = personel.Id,
                                        ZimmetTarihi = DateTime.UtcNow,
                                        Durum = "Aktif",
                                        ZimmetMiktari = 1
                                    };
                                    _db.Zimmetler.Add(zimmet);
                                }
                            }
                        }

                        await _db.SaveChangesAsync();
                    }
                }

                TempData["Basarili"] = $"{eklenen} yeni kayıt başarıyla eklendi.";
            }
            catch (Exception ex)
            {
                TempData["Hata"] = "Excel işlenirken hata oluştu: " + ex.Message;
            }

            return RedirectToAction(nameof(Kayitlar), new { id });
        }


        private int AlanTipiToInt(string tip) => tip switch
        {
            "Metin" => 0,
            "Sayı" => 1,
            "Tarih" => 2,
            "Checkbox" => 3,
            "Seçenek" => 4,
            "Uzun Metin" => 5,
            "Görsel" => 6,
            "Kategori" => 7,
            "Personel" => 8,
            "Durum" => 9,
            "Birim" => 10,
            "Demirbaş" => 11,
            "Lokasyon" => 12,
            _ => 0
        };

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

        [Authorize(Roles = "Admin")]
        [HttpGet]
        public async Task<IActionResult> FixIds()
        {
            var anaKats = new List<dynamic>();
            for (int i = 0; i < anaKats.Count; i++)
            {
                anaKats[i].HiyerarsikId = (i + 1).ToString();
            }
            await _db.SaveChangesAsync();

            var altKats = new List<dynamic>();
            foreach (var alt in altKats)
            {
                Kategori? ust = null;
                if (ust != null)
                {
                    var siblings = 0;
                    alt.HiyerarsikId = $"{ust.HiyerarsikId}.{siblings}";
                }
            }
            await _db.SaveChangesAsync();

            var anaFormlar = await _db.FormSablonlar.Where(f => f.UstFormId == null).OrderBy(f => f.OlusturmaTarihi).ToListAsync();
            foreach (var form in anaFormlar)
            {
                if (form.KategoriId != null)
                {
                    Kategori? k = null;
                    var siblings = await _db.FormSablonlar.Where(f => f.UstFormId == null && f.KategoriId == form.KategoriId && f.OlusturmaTarihi <= form.OlusturmaTarihi).CountAsync();
                    if (k != null) form.HiyerarsikId = $"{k.HiyerarsikId}.{siblings}";
                }
            }
            await _db.SaveChangesAsync();

            var altFormlar = await _db.FormSablonlar.Where(f => f.UstFormId != null).OrderBy(f => f.OlusturmaTarihi).ToListAsync();
            foreach (var alt in altFormlar)
            {
                var ust = await _db.FormSablonlar.FindAsync(alt.UstFormId);
                var siblings = await _db.FormSablonlar.Where(f => f.UstFormId == alt.UstFormId && f.OlusturmaTarihi <= alt.OlusturmaTarihi).CountAsync();
                if (ust != null) alt.HiyerarsikId = $"{ust.HiyerarsikId}.{siblings}";
            }
            await _db.SaveChangesAsync();

            return Ok("Hiyerarşik ID'ler başarıyla güncellendi.");
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Temizle()
        {
           
            var candidatesMalz = await _db.Malzemeler
                .Where(m => string.IsNullOrEmpty(m.UrunTuru) && m.KategoriId == null)
                .ToListAsync();

            int silinenMalzAdet = 0;
            foreach (var m in candidatesMalz)
            {
                var hasStock = await _db.DepoStoklar.AnyAsync(s => s.MalzemeId == m.Id && s.Miktar > 0);
                var hasZimmet = await _db.Zimmetler.AnyAsync(z => z.MalzemeId == m.Id);

                if (!hasStock && !hasZimmet)
                {
                    _db.Malzemeler.Remove(m);
                    silinenMalzAdet++;
                }
            }

           
            await _db.SaveChangesAsync();

            
            var candidatesSablon = await _db.FormSablonlar
                .Include(s => s.Kayitlar)
                .Where(s => s.Baslik.EndsWith(" Özellikleri"))
                .ToListAsync();

            int silinenSablonAdet = 0;
            foreach (var s in candidatesSablon)
            {
                if (!s.Kayitlar.Any())
                {
                    
                    var hasMaterial = await _db.Malzemeler.AnyAsync(m => m.FormSablonId == s.Id);
                    if (!hasMaterial)
                    {
                       
                        var alanlar = await _db.FormAlanlar.Where(a => a.FormSablonId == s.Id).ToListAsync();
                        if (alanlar.Any()) _db.FormAlanlar.RemoveRange(alanlar);

                        _db.FormSablonlar.Remove(s);
                        silinenSablonAdet++;
                    }
                }
            }

            if (silinenMalzAdet > 0 || silinenSablonAdet > 0)
            {
                await _db.SaveChangesAsync();
               
            }

            var ghostMaterials = await _db.Malzemeler
                .Where(m => m.FormSablonId == null && 
                           (m.MalzemeTuru == "Varlık Şablonu" || (m.Ad != null && m.Ad.EndsWith(" Özellikleri"))))
                .ToListAsync();

            int silinenHayaletAdet = 0;
            foreach (var gm in ghostMaterials)
            {
                var hasStock = await _db.DepoStoklar.AnyAsync(s => s.MalzemeId == gm.Id && s.Miktar > 0);
                var hasZimmet = await _db.Zimmetler.AnyAsync(z => z.MalzemeId == gm.Id);

                if (!hasStock && !hasZimmet)
                {
                    _db.Malzemeler.Remove(gm);
                    silinenHayaletAdet++;
                }
            }

            if (silinenHayaletAdet > 0) await _db.SaveChangesAsync();

            if (silinenMalzAdet > 0 || silinenSablonAdet > 0 || silinenHayaletAdet > 0)
            {
                TempData["Basarili"] = $"{silinenMalzAdet} adet gereksiz ürün tanımı, {silinenSablonAdet} adet boş şablon ve {silinenHayaletAdet} adet hayalet kayıt temizlendi.";
            }
            else
            {
                TempData["Hata"] = "Temizlenecek uygun kayıt bulunamadı.";
            }

            return RedirectToAction(nameof(TumVarliklar));
        }
    }
}

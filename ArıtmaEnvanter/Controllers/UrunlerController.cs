using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ArıtmaEnvanter.Data;
using ArıtmaEnvanter.Models.Entities;
using System;

namespace ArıtmaEnvanter.Controllers
{
    [Authorize]
    public class UrunlerController : Controller
    {
        private readonly AppDbContext _db;
        private readonly UserManager<ApplicationUser> _userManager;

        public UrunlerController(AppDbContext db, UserManager<ApplicationUser> userManager)
        {
            _db = db;
            _userManager = userManager;
        }

        private async Task<string> GetKullaniciAdSoyad()
        {
            var user = await _userManager.GetUserAsync(User);
            return user?.AdSoyad ?? User.Identity?.Name ?? "Sistem";
        }

        public async Task<IActionResult> Index(int? depoId, string? arama)
        {
            ViewBag.Depolar = await _db.Depolar.OrderBy(d => d.Ad).ToListAsync();
            ViewBag.SeciliDepo = depoId;
            ViewBag.Arama = arama;

            var stoklarQuery = _db.DepoStoklar
                .Include(s => s.Malzeme)
                .Include(s => s.Depo)
                .Include(s => s.RafTanim)
                .AsQueryable();

            if (depoId.HasValue)
            {
                stoklarQuery = stoklarQuery.Where(s => s.DepoId == depoId.Value);
            }

            if (!string.IsNullOrEmpty(arama))
            {
                stoklarQuery = stoklarQuery.Where(s => s.Malzeme.Ad.Contains(arama) || s.Malzeme.MalzemeTuru.Contains(arama));
            }

            var stoklar = await stoklarQuery.OrderBy(s => s.Malzeme.Ad).ToListAsync();


            var sablonlar = await _db.FormSablonlar.Include(f => f.Kategori).ToListAsync();
            var mevcutMalzemeler = await _db.Malzemeler.ToListAsync();
            bool malzemeSenkOldu = false;

            foreach (var sablon in sablonlar)
            {
                var refMalzeme = mevcutMalzemeler.FirstOrDefault(m => m.FormSablonId == sablon.Id);
                if (refMalzeme == null)
                {
                    _db.Malzemeler.Add(new Malzeme
                    {
                        Ad = sablon.Baslik,
                        Aciklama = sablon.Aciklama,
                        KategoriId = sablon.KategoriId,
                        FormSablonId = sablon.Id,
                        OlusturmaTarihi = DateTime.UtcNow,
                        MalzemeTuru = sablon.Kategori?.Ad ?? "Varlık Şablonu"
                    });
                    malzemeSenkOldu = true;
                }
                else if (refMalzeme.Ad != sablon.Baslik || refMalzeme.KategoriId != sablon.KategoriId)
                {
                    refMalzeme.Ad = sablon.Baslik;
                    refMalzeme.KategoriId = sablon.KategoriId;
                    refMalzeme.MalzemeTuru = sablon.Kategori?.Ad ?? "Varlık Şablonu";
                    _db.Malzemeler.Update(refMalzeme);
                    malzemeSenkOldu = true;
                }
            }

            if (malzemeSenkOldu)
            {
                await _db.SaveChangesAsync();
            }


            ViewBag.Malzemeler = await _db.Malzemeler.OrderBy(m => m.Ad).ToListAsync();
            ViewBag.Raflar = await _db.RafTanimlar.OrderBy(r => r.Ad).ToListAsync();
            ViewBag.Personeller = await _db.Personeller.OrderBy(p => p.AdSoyad).ToListAsync();
            ViewBag.Firmalar = await _db.Firmalar.OrderBy(f => f.FirmaAdi).ToListAsync();
            ViewBag.StoktakiMalzemeler = await _db.DepoStoklar .Include(s => s.Malzeme).Where(s => s.Miktar > 0).Select(s => s.Malzeme).Distinct().OrderBy(m => m.Ad).ToListAsync();
        
            return View(stoklar);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Giris(int malzemeId, int? depoId, int? rafTanimId, string? rafNo, decimal miktar, string? aciklama, IFormCollection form, int? firmaId)
        {
            if (malzemeId <= 0)
            {
                TempData["Hata"] = "Lütfen bir malzeme seçiniz.";
                return RedirectToAction(nameof(Index));
            }

            int gercekDepoId = depoId ?? 0;
            if (gercekDepoId <= 0)
            {
                var ilkDepo = await _db.Depolar.OrderBy(d => d.Id).FirstOrDefaultAsync();
                if (ilkDepo == null)
                {
                    TempData["Hata"] = "Sistemde kayıtlı depo bulunamadı.";
                    return RedirectToAction(nameof(Index));
                }
                gercekDepoId = ilkDepo.Id;
            }

            var malzeme = await _db.Malzemeler.Include(m => m.FormSablon).FirstOrDefaultAsync(m => m.Id == malzemeId);
            if (malzeme == null)
            {
                TempData["Hata"] = "Ürün bulunamadı.";
                return RedirectToAction(nameof(Index));
            }
            if (miktar <= 0)
            {
                TempData["Hata"] = "Giriş miktarı 0'dan büyük olmalıdır.";
                return RedirectToAction(nameof(Index));
            }

            string ekOzellikler = "";
            if (malzeme.FormSablonId.HasValue)
            {
                var alanlar = await _db.FormAlanlar.Where(a => a.FormSablonId == malzeme.FormSablonId.Value).OrderBy(a => a.Sira).ToListAsync();
                var ozellikListesi = new List<string>();
                foreach (var alan in alanlar)
                {
                    if (form.TryGetValue($"DynamicField_{alan.Id}", out var deger) && !string.IsNullOrEmpty(deger.ToString()))
                    {
                        ozellikListesi.Add(deger.ToString());
                    }
                }
                if (ozellikListesi.Any())
                {
                    ekOzellikler = " " + string.Join(" ", ozellikListesi);
                }
            }

            string kayitUrunAdi = malzeme.Ad + ekOzellikler;

            string temizUrunAdi = kayitUrunAdi.Trim();

            var stok = await _db.DepoStoklar
                .FirstOrDefaultAsync(s => s.MalzemeId == malzemeId
                                       && s.DepoId == gercekDepoId
                                       && s.RafTanimId == rafTanimId
                                       && (s.RafNo == null ? rafNo == null : s.RafNo.Trim() == (rafNo ?? "").Trim())
                                       && s.UrunAdi.Trim() == temizUrunAdi);

            if (stok == null)
            {
                stok = new DepoStok
                {
                    MalzemeId = malzemeId,
                    DepoId = gercekDepoId,
                    RafTanimId = rafTanimId,
                    RafNo = (rafNo ?? "").Trim(),
                    Miktar = miktar,
                    UrunAdi = temizUrunAdi,
                    Birim = malzeme.Birim?.Ad ?? "Adet",
                    IslemYapanKisi = await GetKullaniciAdSoyad(),
                    GuncellemeTarihi = DateTime.UtcNow
                };
                _db.DepoStoklar.Add(stok);
            }
            else
            {

                stok.Miktar += miktar;
                stok.IslemYapanKisi = await GetKullaniciAdSoyad();
                stok.GuncellemeTarihi = DateTime.UtcNow;
                _db.DepoStoklar.Update(stok);
            }


            var hareket = new DepoHareket
            {
                MalzemeId = malzemeId,
                HedefDepoId = gercekDepoId,
                KaynakDepoId = gercekDepoId,
                RafTanimId = rafTanimId,
                RafNo = rafNo,
                Miktar = miktar,
                Tarih = DateTime.UtcNow,
                IslemYapanKisi = await GetKullaniciAdSoyad(),
                FirmaId = firmaId
            };

            _db.DepoHareketler.Add(hareket);


            if (malzeme.FormSablonId.HasValue)
            {
                var formSablonId = malzeme.FormSablonId.Value;
                var alanlar = await _db.FormAlanlar.Where(a => a.FormSablonId == formSablonId).ToListAsync();

                var formKayit = new FormKayit
                {
                    FormSablonId = formSablonId,
                    DepoId = gercekDepoId,
                    BirimId = gercekDepoId,
                    OlusturmaTarihi = DateTime.UtcNow,
                    KaydedenKullanici = User.Identity?.Name,
                    Degerler = new List<FormKayitDeger>()
                };

                foreach (var alan in alanlar)
                {
                    if (form.TryGetValue($"DynamicField_{alan.Id}", out var deger))
                    {
                        formKayit.Degerler.Add(new FormKayitDeger
                        {
                            FormAlanId = alan.Id,
                            Deger = deger.ToString()
                        });
                    }
                }

                _db.FormKayitlar.Add(formKayit);
            }

            await _db.SaveChangesAsync();

            TempData["Basarili"] = $"{malzeme.Ad} için {miktar} adet stok girişi yapıldı.";
            return RedirectToAction(nameof(Index), new { depoId });
        }

        [HttpGet]
        public async Task<IActionResult> GetMalzemeFormAlanlari(int malzemeId)
        {
            var malzeme = await _db.Malzemeler
                .Include(m => m.FormSablon)
                    .ThenInclude(fs => fs.Alanlar)
                .Include(m => m.Kategori)
                .Include(m => m.Birim)
                .FirstOrDefaultAsync(m => m.Id == malzemeId);

            if (malzeme == null)
            {
                return Json(new { success = false, alanlar = new string[0] });
            }


            var malzemeBilgi = new
            {
                aciklama = malzeme.Aciklama,
                kategori = malzeme.Kategori?.Ad,
                birim = malzeme.Birim?.Ad,
                malzemeTuru = malzeme.MalzemeTuru,
                alternatifAd = malzeme.AlternatifAd
            };

            if (malzeme.FormSablon == null)
            {
                return Json(new { success = true, formSablonId = 0, alanlar = new string[0], malzeme = malzemeBilgi });
            }

            var alanlar = malzeme.FormSablon.Alanlar.OrderBy(a => a.Sira).Select(a => new
            {
                a.Id,
                a.AlanAdi,
                a.AlanTipi,
                a.Gerekli,
                a.Secenekler
            });

            return Json(new { success = true, formSablonId = malzeme.FormSablonId, alanlar, malzeme = malzemeBilgi });
        }

        [HttpGet]
        public async Task<IActionResult> GetMalzemeMevcutStoklar(int malzemeId)
        {
            var stoklar = await _db.DepoStoklar
                .Include(s => s.RafTanim)
                .Where(s => s.MalzemeId == malzemeId && s.Miktar > 0)
                .OrderBy(s => (s.RafTanim != null ? s.RafTanim.Ad : "Genel Alan"))
                .Select(s => new
                {
                    s.Id,
                    UrunAdi = s.UrunAdi,
                    RafAd = s.RafTanim != null ? s.RafTanim.Ad : "Genel Alan",
                    RafNo = s.RafNo,
                    s.Miktar,
                    s.Birim
                })
                .ToListAsync();

            return Json(new { success = true, stoklar });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Cikis(int stokId, decimal miktar, string cikisTuru, int? personelId, string? aciklama, string? cikisFormNo)
        {
            if (miktar <= 0)
            {
                TempData["Hata"] = "Çıkış miktarı 0'dan büyük olmalıdır.";
                return RedirectToAction(nameof(Index));
            }

            if (cikisTuru == "Demirbaş" && (!personelId.HasValue || personelId.Value <= 0))
            {
                TempData["Hata"] = "Demirbaş çıkışlarında personel seçimi zorunludur.";
                return RedirectToAction(nameof(Index));
            }

            if (string.IsNullOrWhiteSpace(cikisFormNo))
            {
                var rng = new Random();
                cikisFormNo = $"ÇKS-{DateTime.Now:yyMMdd}-{rng.Next(1000, 9999)}";
            }

            var stok = await _db.DepoStoklar
                .Include(s => s.Malzeme)
                .FirstOrDefaultAsync(s => s.Id == stokId);

            if (stok == null)
            {
                TempData["Hata"] = "Stok kaydı bulunamadı.";
                return RedirectToAction(nameof(Index));
            }

            if (stok.Miktar < miktar)
            {
                TempData["Hata"] = $"Çıkış miktarı mevcut stoktan ({stok.Miktar}) fazla olamaz.";
                return RedirectToAction(nameof(Index), new { depoId = stok.DepoId });
            }


            stok.Miktar -= miktar;
            stok.IslemYapanKisi = await GetKullaniciAdSoyad();
            stok.GuncellemeTarihi = DateTime.UtcNow;
            _db.DepoStoklar.Update(stok);


            var hareket = new DepoHareket
            {
                MalzemeId = stok.MalzemeId,
                KaynakDepoId = stok.DepoId,
                HedefDepoId = null,
                RafTanimId = stok.RafTanimId,
                RafNo = stok.RafNo,
                Miktar = miktar,
                Tarih = DateTime.UtcNow,
                CikisFormNo = cikisFormNo,
                IslemYapanKisi = await GetKullaniciAdSoyad(),
                PersonelId = personelId
            };
            _db.DepoHareketler.Add(hareket);


            if (cikisTuru == "Demirbaş" && personelId.HasValue)
            {
                for (int i = 0; i < (int)miktar; i++)
                {
                    var zimmet = new Zimmet
                    {
                        MalzemeId = stok.MalzemeId,
                        PersonelId = personelId.Value,
                        Durum = "Aktif",
                        ZimmetTarihi = DateTime.UtcNow,
                        Notlar = cikisFormNo
                    };
                    _db.Zimmetler.Add(zimmet);
                }
            }

            await _db.SaveChangesAsync();

            TempData["Basarili"] = $"{stok.Malzeme.Ad} ürününden {miktar} adet çıkış yapıldı{(cikisTuru == "Demirbaş" ? " ve personele zimmetlendi" : "")}. (Belge: {cikisFormNo})";
            return RedirectToAction(nameof(CikisRaporu), new { formNo = cikisFormNo });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> TopluCikis(List<TopluCikisItem> items, int? personelId, string? aciklama, string? cikisFormNo)
        {
            if (items == null || !items.Any())
            {
                TempData["Hata"] = "Çıkış yapılacak ürün seçilmedi.";
                return RedirectToAction(nameof(Index));
            }

            if (items.Any(i => i.CikisTuru == "Demirbaş") && (!personelId.HasValue || personelId.Value <= 0))
            {
                TempData["Hata"] = "Demirbaş çıkışlarında personel seçimi zorunludur.";
                return RedirectToAction(nameof(Index));
            }

            if (string.IsNullOrWhiteSpace(cikisFormNo))
            {
                var rng = new Random();
                cikisFormNo = $"TÇKS-{DateTime.Now:yyMMdd}-{rng.Next(1000, 9999)}";
            }

            int basariliCikisSayisi = 0;

            foreach (var item in items)
            {
                if (item.Miktar <= 0) continue;

                var stok = await _db.DepoStoklar
                    .Include(s => s.Malzeme)
                    .FirstOrDefaultAsync(s => s.Id == item.StokId);

                if (stok == null || stok.Miktar < item.Miktar) continue;

                stok.Miktar -= item.Miktar;
                stok.IslemYapanKisi = await GetKullaniciAdSoyad();
                stok.GuncellemeTarihi = DateTime.UtcNow;
                _db.DepoStoklar.Update(stok);

                var hareket = new DepoHareket
                {
                    MalzemeId = stok.MalzemeId,
                    KaynakDepoId = stok.DepoId,
                    HedefDepoId = null,
                    RafTanimId = stok.RafTanimId,
                    RafNo = stok.RafNo,
                    Miktar = item.Miktar,
                    Tarih = DateTime.UtcNow,
                    CikisFormNo = cikisFormNo,
                    IslemYapanKisi = await GetKullaniciAdSoyad(),
                    PersonelId = personelId
                };
                _db.DepoHareketler.Add(hareket);

                if (item.CikisTuru == "Demirbaş" && personelId.HasValue)
                {
                    for (int i = 0; i < (int)item.Miktar; i++)
                    {
                        var zimmet = new Zimmet
                        {
                            MalzemeId = stok.MalzemeId,
                            PersonelId = personelId.Value,
                            Durum = "Aktif",
                            ZimmetTarihi = DateTime.UtcNow,
                            Notlar = cikisFormNo
                        };
                        _db.Zimmetler.Add(zimmet);
                    }
                }

                basariliCikisSayisi++;
            }

            if (basariliCikisSayisi > 0)
            {
                await _db.SaveChangesAsync();
                TempData["Basarili"] = $"Başarıyla {basariliCikisSayisi} kalem ürün çıkışı yapıldı. (Belge: {cikisFormNo})";
                return RedirectToAction(nameof(CikisRaporu), new { formNo = cikisFormNo });
            }
            else
            {
                TempData["Hata"] = "Belirtilen ürünler için stok yetersizdi veya kayıt bulunamadı.";
                return RedirectToAction(nameof(Index));
            }

        }
        [HttpGet]
        public async Task<IActionResult> CikisRaporu(string formNo, int? id)
        {
            List<DepoHareket> hareketler = new List<DepoHareket>();

            if (!string.IsNullOrEmpty(formNo))
            {
                hareketler = await _db.DepoHareketler
                    .Include(h => h.Malzeme)
                        .ThenInclude(m => m.Kategori)
                    .Include(h => h.Malzeme)
                        .ThenInclude(m => m.Birim)
                    .Include(h => h.KaynakDepo)
                    .Include(h => h.RafTanim)
                    .Where(h => h.CikisFormNo == formNo && h.HedefDepoId == null)
                    .ToListAsync();
            }
            else if (id.HasValue)
            {
                var h = await _db.DepoHareketler
                    .Include(h => h.Malzeme)
                        .ThenInclude(m => m.Kategori)
                    .Include(h => h.Malzeme)
                        .ThenInclude(m => m.Birim)
                    .Include(h => h.KaynakDepo)
                    .Include(h => h.RafTanim)
                    .FirstOrDefaultAsync(h => h.Id == id.Value && h.HedefDepoId == null);
                if (h != null)
                {
                    hareketler.Add(h);
                }
            }

            if (!hareketler.Any())
            {
                TempData["Hata"] = "Çıkış raporu bulunamadı.";
                return RedirectToAction(nameof(Index));
            }

            var ilkHareket = hareketler.First();
            if (!string.IsNullOrEmpty(ilkHareket.CikisFormNo))
            {
                var zimmet = await _db.Zimmetler
                    .Include(z => z.Personel)
                    .FirstOrDefaultAsync(z => z.Notlar == ilkHareket.CikisFormNo);

                if (zimmet != null)
                {
                    ViewBag.ZimmetlenenPersonel = zimmet.Personel;
                }
            }

            return View(hareketler);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> StokArtir(int stokId, decimal miktar, string? aciklama)
        {
            if (miktar <= 0)
            {
                TempData["Hata"] = "Giriş miktarı 0'dan büyük olmalıdır.";
                return RedirectToAction(nameof(Index));
            }

            var stok = await _db.DepoStoklar.Include(s => s.Malzeme).FirstOrDefaultAsync(s => s.Id == stokId);
            if (stok == null) return NotFound();

            stok.Miktar += miktar;
            stok.IslemYapanKisi = await GetKullaniciAdSoyad();
            stok.GuncellemeTarihi = DateTime.UtcNow;
            _db.DepoStoklar.Update(stok);

            var hareket = new DepoHareket
            {
                MalzemeId = stok.MalzemeId,
                HedefDepoId = stok.DepoId,
                KaynakDepoId = stok.DepoId,
                RafTanimId = stok.RafTanimId,
                RafNo = stok.RafNo,
                Miktar = miktar,
                Tarih = DateTime.UtcNow,
                IslemYapanKisi = await GetKullaniciAdSoyad()
            };
            _db.DepoHareketler.Add(hareket);

            await _db.SaveChangesAsync();
            TempData["Basarili"] = $"{stok.Malzeme?.Ad} ürününe {miktar} stok eklendi.";
            return RedirectToAction(nameof(Index), new { depoId = stok.DepoId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> IadeAl(int stokId, decimal miktar, string? aciklama, string? cikisFormNo)
        {
            if (miktar <= 0)
            {
                TempData["Hata"] = "İade miktarı 0'dan büyük olmalıdır.";
                return RedirectToAction(nameof(Index));
            }

            if (string.IsNullOrWhiteSpace(cikisFormNo))
            {
                var rng = new Random();
                cikisFormNo = $"IADE-{DateTime.Now:yyMMdd}-{rng.Next(1000, 9999)}";
            }

            var stok = await _db.DepoStoklar.Include(s => s.Malzeme).FirstOrDefaultAsync(s => s.Id == stokId);
            if (stok == null) return NotFound();

            stok.Miktar += miktar;
            stok.IslemYapanKisi = await GetKullaniciAdSoyad();
            stok.GuncellemeTarihi = DateTime.UtcNow;
            _db.DepoStoklar.Update(stok);

            var hareket = new DepoHareket
            {
                MalzemeId = stok.MalzemeId,
                HedefDepoId = stok.DepoId,
                RafTanimId = stok.RafTanimId,
                RafNo = stok.RafNo,
                Miktar = miktar,
                Tarih = DateTime.UtcNow,
                CikisFormNo = cikisFormNo,
                IslemYapanKisi = await GetKullaniciAdSoyad()
            };
            _db.DepoHareketler.Add(hareket);

            await _db.SaveChangesAsync();
            TempData["Basarili"] = $"{stok.Malzeme?.Ad} ürünü için {miktar} adet iade alındı. (Belge: {cikisFormNo})";
            return RedirectToAction(nameof(Index), new { depoId = stok.DepoId });
        }

        [HttpGet]
        public async Task<IActionResult> GetCikisIslemleriByFormNo(string formNo)
        {
            if (string.IsNullOrWhiteSpace(formNo))
            {
                return Json(new { success = false, message = "Form numarası boş olamaz." });
            }

            var cikisHareketleri = await _db.DepoHareketler
                .Include(h => h.Malzeme)
                .Include(h => h.RafTanim)
                .Where(h => h.CikisFormNo == formNo.Trim() && h.HedefDepoId == null) // Çıkış işlemleri
                .Select(h => new
                {
                    h.Id,
                    MalzemeAd = h.Malzeme.Ad,
                    Birim = h.Malzeme.Birim != null ? h.Malzeme.Birim.Ad : "Adet",
                    RafAd = h.RafTanim != null ? h.RafTanim.Ad : "Genel Alan",
                    RafNo = h.RafNo,
                    CikisMiktari = h.Miktar,
                    Tarih = h.Tarih.ToLocalTime().ToString("dd.MM.yyyy HH:mm")
                })
                .ToListAsync();

            if (!cikisHareketleri.Any())
            {
                return Json(new { success = false, message = "Bu form numarasına ait çıkış işlemi bulunamadı." });
            }

            return Json(new { success = true, hareketler = cikisHareketleri });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> TopluIade(string cikisFormNo, List<TopluIadeItem> items, string? aciklama, string? iadeFormNo)
        {
            if (string.IsNullOrWhiteSpace(cikisFormNo) || items == null || !items.Any())
            {
                TempData["Hata"] = "Geçersiz iade talebi.";
                return RedirectToAction(nameof(Index));
            }

            if (string.IsNullOrWhiteSpace(iadeFormNo))
            {
                var rng = new Random();
                iadeFormNo = $"TIADE-{DateTime.Now:yyMMdd}-{rng.Next(1000, 9999)}";
            }

            int basariliIadeSayisi = 0;

            foreach (var item in items)
            {
                if (item.IadeMiktari <= 0) continue;

                var cikisHareket = await _db.DepoHareketler
                    .Include(h => h.Malzeme)
                    .FirstOrDefaultAsync(h => h.Id == item.HareketId && h.CikisFormNo == cikisFormNo);

                if (cikisHareket == null || item.IadeMiktari > cikisHareket.Miktar) continue;


                int? hedefRafId = item.HedefRafId ?? cikisHareket.RafTanimId;
                string? hedefRafNo = !string.IsNullOrWhiteSpace(item.HedefRafNo) ? item.HedefRafNo : cikisHareket.RafNo;


                var stok = await _db.DepoStoklar
                    .FirstOrDefaultAsync(s => s.MalzemeId == cikisHareket.MalzemeId
                                        && s.DepoId == cikisHareket.KaynakDepoId.Value
                                        && s.RafTanimId == hedefRafId
                                        && (s.RafNo == null ? hedefRafNo == null : s.RafNo.Trim() == (hedefRafNo ?? "").Trim()));

                if (stok == null)
                {
                    stok = new DepoStok
                    {
                        MalzemeId = cikisHareket.MalzemeId,
                        DepoId = cikisHareket.KaynakDepoId.Value,
                        RafTanimId = hedefRafId,
                        RafNo = (hedefRafNo ?? "").Trim(),
                        Miktar = item.IadeMiktari,
                        UrunAdi = cikisHareket.Malzeme?.Ad ?? "İade Ürün",
                        Birim = cikisHareket.Malzeme?.Birim?.Ad ?? "Adet",
                        IslemYapanKisi = await GetKullaniciAdSoyad(),
                        GuncellemeTarihi = DateTime.UtcNow
                    };
                    _db.DepoStoklar.Add(stok);
                }
                else
                {
                    stok.Miktar += item.IadeMiktari;
                    stok.IslemYapanKisi = await GetKullaniciAdSoyad();
                    stok.GuncellemeTarihi = DateTime.UtcNow;
                    _db.DepoStoklar.Update(stok);
                }


                var yeniHareket = new DepoHareket
                {
                    MalzemeId = cikisHareket.MalzemeId,
                    HedefDepoId = cikisHareket.KaynakDepoId,
                    KaynakDepoId = null,
                    RafTanimId = hedefRafId,
                    RafNo = hedefRafNo,
                    Miktar = item.IadeMiktari,
                    Tarih = DateTime.UtcNow,
                    CikisFormNo = iadeFormNo,
                    IslemYapanKisi = await GetKullaniciAdSoyad()
                };
                _db.DepoHareketler.Add(yeniHareket);


                var zimmetler = await _db.Zimmetler
                    .Where(z => z.MalzemeId == cikisHareket.MalzemeId && z.Notlar == cikisFormNo && z.Durum != "İade Edildi")
                    .Take((int)item.IadeMiktari)
                    .ToListAsync();

                foreach (var zimmet in zimmetler)
                {
                    zimmet.Durum = "İade Edildi";
                    zimmet.IadeTarihi = DateTime.UtcNow;
                    _db.Zimmetler.Update(zimmet);
                }

                basariliIadeSayisi++;
            }

            if (basariliIadeSayisi > 0)
            {
                await _db.SaveChangesAsync();
                TempData["Basarili"] = $"Başarıyla {basariliIadeSayisi} kalem ürün iadesi alındı. (Belge: {iadeFormNo})";
                return RedirectToAction(nameof(IadeRaporu), new { formNo = iadeFormNo });
            }
            else
            {
                TempData["Hata"] = "Belirtilen ürünler için hatalı miktar girişleri yapıldı veya kayıt bulunamadı.";
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpGet]
        public async Task<IActionResult> IadeRaporu(string formNo)
        {
            if (string.IsNullOrEmpty(formNo))
            {
                TempData["Hata"] = "İade raporu bulunamadı.";
                return RedirectToAction(nameof(Index));
            }

            var hareketler = await _db.DepoHareketler
                .Include(h => h.Malzeme)
                    .ThenInclude(m => m.Kategori)
                .Include(h => h.Malzeme)
                    .ThenInclude(m => m.Birim)
                .Include(h => h.HedefDepo)
                .Include(h => h.RafTanim)
                .Where(h => h.CikisFormNo == formNo && h.HedefDepoId != null)
                .ToListAsync();

            if (!hareketler.Any())
            {
                TempData["Hata"] = "İade raporu bulunamadı.";
                return RedirectToAction(nameof(Index));
            }

            return View(hareketler);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Guncelle(int stokId, int? rafTanimId, string? rafNo, string? urunAdi)
        {
            var stok = await _db.DepoStoklar.FindAsync(stokId);
            if (stok == null) return NotFound();

            stok.RafTanimId = rafTanimId;
            stok.RafNo = rafNo;
            if (!string.IsNullOrWhiteSpace(urunAdi))
            {
                stok.UrunAdi = urunAdi.Trim();
            }
            stok.IslemYapanKisi = await GetKullaniciAdSoyad();
            stok.GuncellemeTarihi = DateTime.UtcNow;

            _db.DepoStoklar.Update(stok);
            await _db.SaveChangesAsync();

            TempData["Basarili"] = "Ürün bilgileri ve kayıt yeri güncellendi.";
            return RedirectToAction(nameof(Index), new { depoId = stok.DepoId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Sil(int id)
        {
            var stok = await _db.DepoStoklar.FindAsync(id);
            if (stok == null) return NotFound();

            int? depoId = stok.DepoId;


            _db.DepoStoklar.Remove(stok);
            await _db.SaveChangesAsync();

            TempData["Basarili"] = "Hatalı girdiğiniz ürün kaydı başarıyla silindi.";
            return RedirectToAction(nameof(Index), new { depoId = depoId });
        }
    }

    public class TopluCikisItem
    {
        public int StokId { get; set; }
        public decimal Miktar { get; set; }
        public string? CikisTuru { get; set; }
    }

    public class TopluIadeItem
    {
        public int HareketId { get; set; }
        public decimal IadeMiktari { get; set; }
        public int? HedefRafId { get; set; }
        public string? HedefRafNo { get; set; }
    }
}

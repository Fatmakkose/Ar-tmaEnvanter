using ArıtmaEnvanter.Data;
using ArıtmaEnvanter.Models.Entities;
using ArıtmaEnvanter.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ArıtmaEnvanter.Controllers
{
    [Authorize]
    public class RafTanimController : Controller
    {
        private readonly AppDbContext _db;

        public RafTanimController(AppDbContext db)
        {
            _db = db;
        }

        public async Task<IActionResult> Index()
        {
            ViewData["Title"] = "Raf Tanımı";
            var raflar = await _db.RafTanimlar
                .OrderBy(r => r.OlusturmaTarihi)
                .ToListAsync();
            return View(raflar);
        }

        public async Task<IActionResult> Ekle()
        {
            ViewData["Title"] = "Yeni Raf Tanımla";
            ViewBag.Personeller = await _db.Personeller.OrderBy(p => p.AdSoyad).ToListAsync();
            return View(new RafTanim());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Ekle(RafTanim model)
        {
            if (!ModelState.IsValid)
            {
                ViewData["Title"] = "Yeni Raf Tanımla";
                return View(model);
            }

            model.OlusturmaTarihi = DateTime.UtcNow;
            _db.RafTanimlar.Add(model);
            await _db.SaveChangesAsync();
            TempData["Basarili"] = "Raf başarıyla tanımlandı.";
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Stoklar(int id)
        {
            var raf = await _db.RafTanimlar.FindAsync(id);
            if (raf == null) return NotFound();

            if (raf.Ad != null && raf.Ad.Trim().ToUpper() == "KİMYASAL")
            {
                int yil = DateTime.Now.Year;
                if (int.TryParse(Request.Query["yil"], out int pYil)) yil = pYil;

                int? ay = null;
                if (int.TryParse(Request.Query["ay"], out int pAy) && pAy >= 1 && pAy <= 12) ay = pAy;

                var girisQuery = _db.KimyasalGirisler.Where(g => g.Tarih.Year == yil);
                if (ay.HasValue) girisQuery = girisQuery.Where(g => g.Tarih.Month == ay.Value);

                var tuketimQuery = _db.KimyasalTuketimler.Where(t => t.Yil == yil);
                if (ay.HasValue) tuketimQuery = tuketimQuery.Where(t => t.Ay == ay.Value);

                var viewModel = new PoliCizelgeViewModel
                {
                    RafId = raf.Id,
                    RafAd = raf.Ad,
                    Yil = yil,
                    Ay = ay,
                    Girisler = await girisQuery.OrderBy(g => g.Tarih).ToListAsync(),
                    Tuketimler = await tuketimQuery.ToListAsync(),
                    Devir = await _db.KimyasalDevirler.FirstOrDefaultAsync(d => d.Yil == yil)
                };
                return View("KimyasalCizelge", viewModel);
            }

            var stoklar = await _db.DepoStoklar
                .Include(s => s.Malzeme)
                .Where(s => s.RafTanimId == id)
                .ToListAsync();

            var hareketler = await _db.DepoHareketler
                .Where(h => h.RafTanimId == id)
                .ToListAsync();



            var rafStokListesi = stoklar.Select(s =>
            {

                decimal cikan = hareketler.Where(h => h.MalzemeId == s.MalzemeId && h.RafNo == s.RafNo && h.HedefDepoId == null).Sum(h => h.Miktar);


                decimal giren = s.Miktar + cikan;

                return new RafStokViewModel
                {
                    StokId = s.Id,
                    RafNo = s.RafNo ?? "Belirtilmemiş",
                    UrunAdi = s.UrunAdi ?? s.Malzeme?.Ad ?? "Bilinmeyen Ürün",
                    Adet = giren,
                    Verilen = cikan,
                    Kalan = s.Miktar
                };
            }).OrderBy(s => s.RafNo).ThenBy(s => s.UrunAdi).ToList();

            ViewBag.RafAd = raf.Ad;
            ViewBag.RafId = raf.Id;
            return View(rafStokListesi);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> StokSil(int stokId, int rafId)
        {
            var stok = await _db.DepoStoklar.FindAsync(stokId);
            if (stok != null)
            {

                _db.DepoStoklar.Remove(stok);
                await _db.SaveChangesAsync();
                TempData["Basarili"] = "Seçilen ürün stoklardan silindi.";
            }
            return RedirectToAction(nameof(Stoklar), new { id = rafId });
        }

        public async Task<IActionResult> Duzenle(int id)
        {
            var raf = await _db.RafTanimlar.FindAsync(id);
            if (raf == null) return NotFound();

            ViewData["Title"] = "Raf Düzenle";
            ViewBag.Personeller = await _db.Personeller.OrderBy(p => p.AdSoyad).ToListAsync();
            return View(raf);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Duzenle(int id, RafTanim model)
        {
            if (id != model.Id) return NotFound();


            if (!ModelState.IsValid)
            {
                ViewBag.Personeller = await _db.Personeller.OrderBy(p => p.AdSoyad).ToListAsync();
                return View(model);
            }

            var raf = await _db.RafTanimlar.FindAsync(id);
            if (raf == null) return NotFound();


            raf.Ad = model.Ad;
            raf.SorumluPersonel = model.SorumluPersonel;
            raf.Iletisim = model.Iletisim;
            raf.Aciklama = model.Aciklama;


            raf.AktifMi = Request.Form["AktifMi"].Contains("true");


            await _db.SaveChangesAsync();
            TempData["Basarili"] = $"{raf.Ad} bilgisi güncellendi.";
            return RedirectToAction(nameof(Index));
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Sil(int id)
        {
            var raf = await _db.RafTanimlar.FindAsync(id);
            if (raf == null) return NotFound();

            _db.RafTanimlar.Remove(raf);
            await _db.SaveChangesAsync();
            TempData["Basarili"] = $"{raf.Ad} silindi.";
            return RedirectToAction(nameof(Index));
        }
        [HttpPost]
        public async Task<IActionResult> PoliTuketimKaydet(int yil, int ay, int gun, decimal adet, decimal kg)
        {

            var kayit = await _db.KimyasalTuketimler.FirstOrDefaultAsync(t => t.Yil == yil && t.Ay == ay && t.Gun == gun);
            if (kayit != null)
            {
                kayit.Adet = adet;
                kayit.Kg = kg;
            }
            else
            {
                _db.KimyasalTuketimler.Add(new KimyasalTuketim { Yil = yil, Ay = ay, Gun = gun, Adet = adet, Kg = kg });
            }
            await _db.SaveChangesAsync();
            return Json(new { success = true });
        }

        [HttpPost]
        public async Task<IActionResult> PoliGirisEkle(DateTime tarih, decimal adet, decimal kg, string aciklama)
        {
            _db.KimyasalGirisler.Add(new KimyasalGiris { Tarih = tarih.ToUniversalTime(), Adet = adet, Kg = kg, Aciklama = aciklama });
            await _db.SaveChangesAsync();
            return Json(new { success = true });
        }

        [HttpPost]
        public async Task<IActionResult> PoliDevirKaydet(int yil, decimal adet, decimal kg)
        {
            var dev = await _db.KimyasalDevirler.FirstOrDefaultAsync(d => d.Yil == yil);
            if (dev != null)
            {
                dev.DevredenAdet = adet;
                dev.DevredenKg = kg;
            }
            else
            {
                _db.KimyasalDevirler.Add(new KimyasalDevir { Yil = yil, DevredenAdet = adet, DevredenKg = kg });
            }
            await _db.SaveChangesAsync();
            return Json(new { success = true });
        }
    }
}

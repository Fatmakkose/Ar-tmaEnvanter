using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AritmaEnvanter.Data;
using System;

namespace AritmaEnvanter.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private readonly AppDbContext _db;
        public HomeController(AppDbContext db) => _db = db;

        public async Task<IActionResult> Index()
        {
            
            if (User.IsInRole("IlceSorumlusu") && _db.CurrentWarehouseId.HasValue)
            {
                return RedirectToAction("Index", "Depo");
            }

            
            ViewBag.BirimSayisi = 0; ;
            ViewBag.KategoriSayisi = 0;
            ViewBag.UrunSayisi = await _db.Malzemeler.CountAsync();
            ViewBag.PersonelSayisi = await _db.Personeller.CountAsync();

            
            ViewBag.SonUrunler = await _db.Malzemeler
                
                
                .OrderByDescending(u => u.OlusturmaTarihi)
                .Take(5).ToListAsync();

            
            ViewBag.SonBirimler = await _db.Birimler
                .Include(b => b.UstBirim)
                .OrderByDescending(b => b.OlusturmaTarihi)
                .Take(5).ToListAsync();

        
            ViewBag.AktifSayisi = await _db.Malzemeler.CountAsync(u => u.Durum == "Aktif");
            ViewBag.BakimdaSayisi = await _db.Malzemeler.CountAsync(u => u.Durum == "Bakýmda");
            ViewBag.HurdaSayisi = await _db.Malzemeler.CountAsync(u => u.Durum == "Hurda");

            
            var birimUrunlerRaw = await _db.Birimler
                .Select(b => new
                {
                    BirimAd = b.Ad,
                    UrunSayisi = b.Urunler.Count
                })
                .OrderByDescending(x => x.UrunSayisi)
                .Take(6).ToListAsync();
            ViewBag.BirimUrunler = birimUrunlerRaw.ToDictionary(x => x.BirimAd, x => x.UrunSayisi);

            
            ViewBag.YaklasanBakimlar = await _db.Bakimlar
                .Include(b => b.Malzeme)
                .Where(b => b.SonrakiBakimTarihi.HasValue &&
                            b.SonrakiBakimTarihi.Value <= DateTime.UtcNow.AddDays(30) &&
                            b.SonrakiBakimTarihi.Value >= DateTime.UtcNow)
                .OrderBy(b => b.SonrakiBakimTarihi)
                .Take(5)
                .ToListAsync();

           
            ViewBag.AktifZimmetSayisi = await _db.Zimmetler.CountAsync(z => z.Durum == "Aktif");

            return View();
        }
    }
}

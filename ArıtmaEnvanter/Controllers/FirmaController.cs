using ArıtmaEnvanter.Data;
using ArıtmaEnvanter.Models.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ArıtmaEnvanter.Controllers
{
    [Authorize]
    public class FirmaController : Controller
    {
        private readonly AppDbContext _db;

        public FirmaController(AppDbContext db)
        {
            _db = db;
        }

        public async Task<IActionResult> Index()
        {
            ViewData["Title"] = "Firma Tanımı";
            var firmalar = await _db.Firmalar.ToListAsync();
            return View(firmalar);
        }

        public IActionResult Ekle()
        {
            ViewData["Title"] = "Yeni Firma Tanımla";
            return View(new Firma());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Ekle(Firma model)
        {
            if (!ModelState.IsValid)
            {
                ViewData["Title"] = "Yeni Firma Tanımla";
                return View(model);
            }

            _db.Firmalar.Add(model);
            await _db.SaveChangesAsync();
            TempData["Basarili"] = "Firma başarıyla tanımlandı.";
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Duzenle(int id)
        {
            var firma = await _db.Firmalar.FindAsync(id);
            if (firma == null) return NotFound();

            ViewData["Title"] = "Firma Düzenle";
            return View(firma);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Duzenle(int id, Firma model)
        {
            if (id != model.Id) return NotFound();

            if (!ModelState.IsValid)
            {
                ViewData["Title"] = "Firma Düzenle";
                return View(model);
            }

            var firma = await _db.Firmalar.FindAsync(id);
            if (firma == null) return NotFound();

            firma.FirmaAdi = model.FirmaAdi;
            firma.IlgiliKisiAdSoyad = model.IlgiliKisiAdSoyad;
            firma.IletisimNumarasi = model.IletisimNumarasi;

            await _db.SaveChangesAsync();
            TempData["Basarili"] = $"{firma.FirmaAdi} bilgisi güncellendi.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Sil(int id)
        {
            var firma = await _db.Firmalar.FindAsync(id);
            if (firma == null) return NotFound();

            _db.Firmalar.Remove(firma);
            await _db.SaveChangesAsync();
            TempData["Basarili"] = $"{firma.FirmaAdi} silindi.";
            return RedirectToAction(nameof(Index));
        }
    }
}

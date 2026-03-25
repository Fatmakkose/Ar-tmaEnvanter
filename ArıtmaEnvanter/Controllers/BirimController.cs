using ArıtmaEnvanter.Data;
using ArıtmaEnvanter.Models.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ArıtmaEnvanter.Controllers
{
    [Authorize]
    public class BirimController : Controller
    {
        private readonly AppDbContext _db;

        public BirimController(AppDbContext db)
        {
            _db = db;
        }

        public async Task<IActionResult> Index(string arama)
        {
            ViewData["Title"] = "Birimler";
            ViewBag.Arama = arama;

            var query = _db.Birimler.AsQueryable();

            if (!string.IsNullOrEmpty(arama))
            {
               
                query = query.Where(b => b.Ad.Contains(arama));
                var arananBirimler = await query.OrderBy(b => b.Ad).ToListAsync();
                return View(arananBirimler);
            }

           
            var kokBirimler = await _db.Birimler
                .Where(b => b.UstBirimId == null)
                .Include(b => b.AltBirimler) 
                .OrderBy(b => b.Ad)
                .ToListAsync();

            var tumBirimler = await _db.Birimler.OrderBy(b => b.Ad).ToListAsync();
            ViewBag.TumBirimler = tumBirimler;

            return View(tumBirimler.Where(b => b.UstBirimId == null).ToList());
        }

        public async Task<IActionResult> Ekle()
        {
            ViewData["Title"] = "Yeni Birim Tanımla";
            ViewBag.UstBirimler = await _db.Birimler.OrderBy(b => b.Ad).ToListAsync();
            return View(new Birim());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Ekle(Birim model)
        {
            if (!ModelState.IsValid)
            {
                ViewData["Title"] = "Yeni Birim Tanımla";
                ViewBag.UstBirimler = await _db.Birimler.OrderBy(b => b.Ad).ToListAsync();
                return View(model);
            }

            model.OlusturmaTarihi = DateTime.UtcNow;
            _db.Birimler.Add(model);
            await _db.SaveChangesAsync();
            TempData["Basarili"] = "Birim başarıyla tanımlandı.";
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Duzenle(int id)
        {
            var birim = await _db.Birimler.FindAsync(id);
            if (birim == null) return NotFound();

            ViewData["Title"] = "Birim Düzenle";
            ViewBag.UstBirimler = await _db.Birimler.Where(b => b.Id != id).OrderBy(b => b.Ad).ToListAsync();
            return View(birim);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Duzenle(int id, Birim model)
        {
            if (id != model.Id) return NotFound();

            if (!ModelState.IsValid)
            {
                ViewData["Title"] = "Birim Düzenle";
                ViewBag.UstBirimler = await _db.Birimler.Where(b => b.Id != id).OrderBy(b => b.Ad).ToListAsync();
                return View(model);
            }

            if (model.UstBirimId == model.Id)
            {
                ModelState.AddModelError("UstBirimId", "Bir birim kendisinin üst birimi olamaz.");
                ViewData["Title"] = "Birim Düzenle";
                ViewBag.UstBirimler = await _db.Birimler.Where(b => b.Id != id).OrderBy(b => b.Ad).ToListAsync();
                return View(model);
            }

            var birim = await _db.Birimler.FindAsync(id);
            if (birim == null) return NotFound();

            birim.Ad = model.Ad;
            birim.UstBirimId = model.UstBirimId;

            await _db.SaveChangesAsync();
            TempData["Basarili"] = $"{birim.Ad} bilgisi güncellendi.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Sil(int id)
        {
            var birim = await _db.Birimler.Include(b => b.AltBirimler).FirstOrDefaultAsync(b => b.Id == id);
            if (birim == null) return NotFound();

            if (birim.AltBirimler.Any())
            {
                TempData["Hata"] = "Bu birime bağlı alt birimler bulunduğu için silinemez. Önce alt birimleri silin veya taşıyın.";
                return RedirectToAction(nameof(Index));
            }

            _db.Birimler.Remove(birim);
            await _db.SaveChangesAsync();
            TempData["Basarili"] = $"{birim.Ad} başarıyla silindi.";
            return RedirectToAction(nameof(Index));
        }
    }
}

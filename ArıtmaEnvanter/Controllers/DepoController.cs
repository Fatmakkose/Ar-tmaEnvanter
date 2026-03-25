using ArıtmaEnvanter.Data;
using ArıtmaEnvanter.Models.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ArıtmaEnvanter.Controllers
{
    [Authorize]
    public class DepoController : Controller
    {
        private readonly AppDbContext _db;

        public DepoController(AppDbContext db)
        {
            _db = db;
        }

        public async Task<IActionResult> Index(string arama)
        {
            ViewData["Title"] = "Depolar";
            ViewBag.Arama = arama;

            var query = _db.Depolar
                .Include(d => d.Birim)
                .AsQueryable();

            if (!string.IsNullOrEmpty(arama))
            {
                query = query.Where(d => d.Ad.Contains(arama));
                var arananDepolar = await query.OrderBy(d => d.Ad).ToListAsync();
                return View(arananDepolar);
            }

            
            var kokDepolar = await _db.Depolar
                .Include(d => d.Birim)
                .Where(d => d.UstDepoId == null)
                .OrderBy(d => d.Ad)
                .ToListAsync();

            var tumDepolar = await _db.Depolar
                .Include(d => d.Birim)
                .OrderBy(d => d.Ad)
                .ToListAsync();
                
            ViewBag.TumDepolar = tumDepolar;

            return View(kokDepolar);
        }

        public async Task<IActionResult> Ekle()
        {
            ViewData["Title"] = "Yeni Depo Tanımla";
            ViewBag.Birimler = await _db.Birimler.OrderBy(b => b.Ad).ToListAsync();
            ViewBag.UstDepolar = await _db.Depolar.OrderBy(d => d.Ad).ToListAsync();
            return View(new Depo());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Ekle(Depo model)
        {
            if (!ModelState.IsValid)
            {
                ViewData["Title"] = "Yeni Depo Tanımla";
                ViewBag.Birimler = await _db.Birimler.OrderBy(b => b.Ad).ToListAsync();
                ViewBag.UstDepolar = await _db.Depolar.OrderBy(d => d.Ad).ToListAsync();
                return View(model);
            }

            _db.Depolar.Add(model);
            await _db.SaveChangesAsync();
            TempData["Basarili"] = "Depo başarıyla tanımlandı.";
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Duzenle(int id)
        {
            var depo = await _db.Depolar.FindAsync(id);
            if (depo == null) return NotFound();

            ViewData["Title"] = "Depo Düzenle";
            ViewBag.Birimler = await _db.Birimler.OrderBy(b => b.Ad).ToListAsync();
            ViewBag.UstDepolar = await _db.Depolar.Where(d => d.Id != id).OrderBy(d => d.Ad).ToListAsync();
            return View(depo);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Duzenle(int id, Depo model)
        {
            if (id != model.Id) return NotFound();

            if (!ModelState.IsValid)
            {
                ViewData["Title"] = "Depo Düzenle";
                ViewBag.Birimler = await _db.Birimler.OrderBy(b => b.Ad).ToListAsync();
                ViewBag.UstDepolar = await _db.Depolar.Where(d => d.Id != id).OrderBy(d => d.Ad).ToListAsync();
                return View(model);
            }

            if (model.UstDepoId == model.Id)
            {
                ModelState.AddModelError("UstDepoId", "Bir depo kendisinin üst deposu olamaz.");
                ViewData["Title"] = "Depo Düzenle";
                ViewBag.Birimler = await _db.Birimler.OrderBy(b => b.Ad).ToListAsync();
                ViewBag.UstDepolar = await _db.Depolar.Where(d => d.Id != id).OrderBy(d => d.Ad).ToListAsync();
                return View(model);
            }

            var depo = await _db.Depolar.FindAsync(id);
            if (depo == null) return NotFound();

            depo.Ad = model.Ad;
            depo.BirimId = model.BirimId;
            depo.UstDepoId = model.UstDepoId;

            await _db.SaveChangesAsync();
            TempData["Basarili"] = $"{depo.Ad} bilgisi güncellendi.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Sil(int id)
        {
            var depo = await _db.Depolar.Include(d => d.AltDepolar).FirstOrDefaultAsync(d => d.Id == id);
            if (depo == null) return NotFound();

            if (depo.AltDepolar.Any())
            {
                TempData["Hata"] = "Bu depoya bağlı alt depolar bulunduğu için silinemez. Önce alt depoları silin veya taşıyın.";
                return RedirectToAction(nameof(Index));
            }

            _db.Depolar.Remove(depo);
            await _db.SaveChangesAsync();
            TempData["Basarili"] = $"{depo.Ad} başarıyla silindi.";
            return RedirectToAction(nameof(Index));
        }
    }
}

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ArıtmaEnvanter.Data;
using ArıtmaEnvanter.Models.Entities;


namespace ArıtmaEnvanter.Controllers
{
    [Authorize]
    public class TaleplerController : Controller
    {
        private readonly AppDbContext _db;
        private readonly UserManager<ApplicationUser> _userManager;

        public TaleplerController(AppDbContext db, UserManager<ApplicationUser> userManager)
        {
            _db = db;
            _userManager = userManager;
        }


        [HttpPost]
        //[IgnoreAntiforgeryToken] 
        public async Task<IActionResult> TopluTalepOlustur([FromBody] TopluTalepRequestModel model)
        {
            if (model?.Items == null || !model.Items.Any())
            {
                return Json(new { success = false, message = "Talep listesi boş olamaz." });
            }

            var user = await _userManager.GetUserAsync(User);

            int sonSiraNo = await _db.MalzemeTalepFormlar.MaxAsync(t => (int?)t.FormSiraNo) ?? 0;

            var yeniForm = new MalzemeTalepForm
            {
                FormSiraNo = sonSiraNo + 1,
                TalepEdenPersonelId = user.Id,
                TalepTarihi = DateTime.UtcNow,
                GenelAciklama = model.Aciklama ?? "",
                Durum = TalepDurumu.Beklemede,
                Satirlar = model.Items.Select(x => new MalzemeTalepSatir
                {
                    StokId = x.StokId,
                    Miktar = x.Miktar
                }).ToList()
            };

            _db.MalzemeTalepFormlar.Add(yeniForm);
            await _db.SaveChangesAsync();

            return Json(new { success = true, message = $"Talebiniz başarıyla oluşturuldu. Form No: MASKİ-{yeniForm.FormSiraNo:D4}" });
        }


        public async Task<IActionResult> GelenTalepler()
        {
            var talepler = await _db.MalzemeTalepFormlar
                .Include(f => f.TalepEdenPersonel)
                .Include(f => f.Satirlar)
                    .ThenInclude(s => s.Stok)
                .OrderByDescending(f => f.TalepTarihi)
                .ToListAsync();

            return View(talepler);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Onayla(int id)
        {
            var form = await _db.MalzemeTalepFormlar
                .Include(f => f.Satirlar).ThenInclude(s => s.Stok)
                .Include(f => f.TalepEdenPersonel)
                .FirstOrDefaultAsync(f => f.Id == id);

            if (form == null || form.Durum != TalepDurumu.Beklemede) return NotFound();

     
            var yetersizStoklar = form.Satirlar.Where(s => s.Stok.Miktar < s.Miktar).ToList();
            if (yetersizStoklar.Any())
            {
                var hataMesaji = "Onaylanamaz! Yetersiz stoklar: " +
                    string.Join(", ", yetersizStoklar.Select(x => $"{x.Stok.UrunAdi} (Talep: {x.Miktar}, Mevcut: {x.Stok.Miktar})"));

                TempData["Hata"] = hataMesaji;
                return RedirectToAction(nameof(GelenTalepler));
            }

            var currentUser = await _userManager.GetUserAsync(User);
            string formNo = $"MASKİ-{form.FormSiraNo:D4}";

            foreach (var satir in form.Satirlar)
            {
                satir.Stok.Miktar -= satir.Miktar;

                _db.DepoHareketler.Add(new DepoHareket
                {
                    MalzemeId = satir.Stok.MalzemeId,
                    KaynakDepoId = satir.Stok.DepoId,
                    Miktar = satir.Miktar,
                    Tarih = DateTime.UtcNow,
                    IslemYapanKisi = currentUser.AdSoyad,
                    FormNo = formNo,
                    RafTanimId = satir.Stok.RafTanimId,
                    RafNo = satir.Stok.RafNo,
                    Aciklama = $"Form No {form.FormSiraNo} ile talep çıkışı.",
                    IslemTuru = "Çıkış"
                });
            }

            form.Durum = TalepDurumu.Onaylandi;
            form.OnayTarihi = DateTime.UtcNow;
            await _db.SaveChangesAsync();

            TempData["Basarili"] = $"MASKİ-{form.FormSiraNo:D4} onaylandı ve stoktan düşüldü.";
            return RedirectToAction(nameof(GelenTalepler));
        }

      
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Reddet(int id)
        {
            var form = await _db.MalzemeTalepFormlar.FindAsync(id);
            if (form == null) return NotFound();

            form.Durum = TalepDurumu.Reddedildi;
            await _db.SaveChangesAsync();

            TempData["Hata"] = $"MASKİ-{form.FormSiraNo:D4} numaralı talep reddedildi.";
            return RedirectToAction(nameof(GelenTalepler));
        }
     

        [HttpGet]
        public async Task<IActionResult> TalepFormu(int id)
        {
            var form = await _db.MalzemeTalepFormlar
                .Include(f => f.TalepEdenPersonel)
                .Include(f => f.Satirlar)
                    .ThenInclude(s => s.Stok)
                        .ThenInclude(st => st.Malzeme) 
                .Include(f => f.Satirlar)
                    .ThenInclude(s => s.Stok)
                        .ThenInclude(st => st.RafTanim)
                     
                .FirstOrDefaultAsync(f => f.Id == id);

            if (form == null) return NotFound();

            return View(form);
        }
    }

    public class TalepSatirViewModel
    {
        public int StokId { get; set; }
        public decimal Miktar { get; set; }
    }
    public class TopluTalepRequestModel
    {
        public List<TalepSatirViewModel> Items { get; set; }
        public string Aciklama { get; set; }
    }
}
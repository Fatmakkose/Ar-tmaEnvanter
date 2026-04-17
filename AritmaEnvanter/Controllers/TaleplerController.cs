using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AritmaEnvanter.Data;
using AritmaEnvanter.Models.Entities;


namespace AritmaEnvanter.Controllers
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
       
        public async Task<IActionResult> TopluTalepOlustur([FromBody] TopluTalepRequestModel model)
        {
            if (model?.Items == null || !model.Items.Any())
            {
                return Json(new { success = false, message = "Talep listesi boþ olamaz." });
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

            return Json(new { success = true, message = $"Talebiniz baþarýyla oluþturuldu. Form No: MASKÝ-{yeniForm.FormSiraNo:D4}" });
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
                .Include(f => f.Satirlar).ThenInclude(s => s.Stok).ThenInclude(st => st.Malzeme)
                .Include(f => f.TalepEdenPersonel)
                .FirstOrDefaultAsync(f => f.Id == id);

            if (form == null || form.Durum != TalepDurumu.Beklemede) return NotFound();

            var currentUser = await _userManager.GetUserAsync(User); 
            string formNoString = $"MASKÝ-{form.FormSiraNo:D4}";

            foreach (var satir in form.Satirlar)
            {
                
                satir.Stok.Miktar -= satir.Miktar;

             
                var yeniHareket = new DepoHareket
                {
                    MalzemeId = satir.Stok.MalzemeId,
                    KaynakDepoId = satir.Stok.DepoId,
                    Miktar = satir.Miktar,
                    Tarih = DateTime.UtcNow,

                    IslemYapanKisi = currentUser.AdSoyad,

                    
                    Aciklama = $"{formNoString} nolu talep onayý. Teslim Alan: {form.TalepEdenPersonel?.AdSoyad}",

                  
                    CikisFormNo = formNoString,
                    FormNo = formNoString,

                 
                    FormKayitId = satir.Stok.FormKayitId,

                   
                    RafTanimId = satir.Stok.RafTanimId,
                    RafNo = satir.Stok.RafNo,

                    IslemTuru = "Çýkýþ"
                };

                _db.DepoHareketler.Add(yeniHareket);
            }

            form.Durum = TalepDurumu.Onaylandi;
            form.OnayTarihi = DateTime.UtcNow;

            await _db.SaveChangesAsync();
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

            TempData["Hata"] = $"MASKÝ-{form.FormSiraNo:D4} numaralý talep reddedildi.";
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

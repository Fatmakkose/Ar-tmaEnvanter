using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AritmaEnvanter.Data;
using System.Linq;
using System.Threading.Tasks;

namespace AritmaEnvanter.Controllers
{
    [Authorize]
    public class DepoHareketController : Controller
    {
        private readonly AppDbContext _db;

        public DepoHareketController(AppDbContext db)
        {
            _db = db;
        }

        public async Task<IActionResult> Index(string arama, string islemTipi, DateTime? baslangicTarihi, DateTime? bitisTarihi)
        {
            ViewBag.Arama = arama;
            ViewBag.IslemTipi = islemTipi;
            ViewBag.BaslangicTarihi = baslangicTarihi?.ToString("yyyy-MM-dd");
            ViewBag.BitisTarihi = bitisTarihi?.ToString("yyyy-MM-dd");

            var query = _db.DepoHareketler
                .Include(h => h.Malzeme)
                .Include(h => h.FormKayit)
                .ThenInclude(f => f.Degerler)
                .Include(h => h.KaynakDepo)
                .Include(h => h.HedefDepo)
                .Include(h => h.RafTanim)
                .Include(h => h.Personel)
                .Include(h => h.Firma)
                .AsQueryable();

            if (!string.IsNullOrEmpty(arama))
            {
                query = query.Where(h => h.Malzeme.Ad.Contains(arama) ||
                                         (h.CikisFormNo != null && h.CikisFormNo.Contains(arama)) ||
                                         (h.IslemYapanKisi != null && h.IslemYapanKisi.Contains(arama)) ||
                                         (h.Personel != null && h.Personel.AdSoyad.Contains(arama)));
            }

            if (!string.IsNullOrEmpty(islemTipi))
            {
                if (islemTipi == "Giris")
                    query = query.Where(h => h.KaynakDepoId == h.HedefDepoId);
                else if (islemTipi == "Cikis")
                    query = query.Where(h => h.HedefDepoId == null || h.HedefDepoId == 0);
                else if (islemTipi == "Iade")
                    query = query.Where(h => h.KaynakDepoId == null && h.HedefDepoId != null);
            }

            if (baslangicTarihi.HasValue)
            {
                var startUtc = baslangicTarihi.Value.Date.ToUniversalTime();
                query = query.Where(h => h.Tarih >= startUtc);
            }

            if (bitisTarihi.HasValue)
            {
                var endUtc = bitisTarihi.Value.Date.AddDays(1).ToUniversalTime();
                query = query.Where(h => h.Tarih < endUtc);
            }

            var hareketler = await query.OrderByDescending(h => h.Tarih).Take(500).ToListAsync();

            return View(hareketler);
        }

        public async Task<IActionResult> Rapor(string arama, string islemTipi, DateTime? baslangicTarihi, DateTime? bitisTarihi)
        {
            ViewBag.Arama = arama;
            ViewBag.IslemTipi = islemTipi;
            ViewBag.BaslangicTarihi = baslangicTarihi?.ToString("yyyy-MM-dd");
            ViewBag.BitisTarihi = bitisTarihi?.ToString("yyyy-MM-dd");

            var query = _db.DepoHareketler
                .Include(h => h.Malzeme)
                .Include(h => h.FormKayit)
            .ThenInclude(f => f.Degerler)
                .Include(h => h.KaynakDepo)
                .Include(h => h.HedefDepo)
                .Include(h => h.RafTanim)
                .Include(h => h.Personel)
                .Include(h => h.Firma)
                .AsQueryable();

            if (!string.IsNullOrEmpty(arama))
            {
                query = query.Where(h => h.Malzeme.Ad.Contains(arama) ||
                                         (h.CikisFormNo != null && h.CikisFormNo.Contains(arama)) ||
                                         (h.FormNo != null && h.FormNo.Contains(arama)) |
                                         (h.IslemYapanKisi != null && h.IslemYapanKisi.Contains(arama)) ||
                                         (h.Personel != null && h.Personel.AdSoyad.Contains(arama)));
            }

            if (!string.IsNullOrEmpty(islemTipi))
            {
                if (islemTipi == "Giris")
                    query = query.Where(h => h.KaynakDepoId == h.HedefDepoId);
                else if (islemTipi == "Cikis")
                    query = query.Where(h => h.HedefDepoId == null || h.HedefDepoId == 0);
                else if (islemTipi == "Iade")
                    query = query.Where(h => h.KaynakDepoId == null && h.HedefDepoId != null);
            }

            if (baslangicTarihi.HasValue)
            {
                var startUtc = baslangicTarihi.Value.Date.ToUniversalTime();
                query = query.Where(h => h.Tarih >= startUtc);
            }

            if (bitisTarihi.HasValue)
            {
                var endUtc = bitisTarihi.Value.Date.AddDays(1).ToUniversalTime();
                query = query.Where(h => h.Tarih < endUtc);
            }

            var hareketler = await query.OrderByDescending(h => h.Tarih).Take(2000).ToListAsync();

            return View(hareketler);
        }
    }
}
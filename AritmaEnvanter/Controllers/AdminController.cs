using AritmaEnvanter.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AritmaEnvanter.Models.Entities;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace AritmaEnvanter.Controllers

{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly Data.AppDbContext _db;

        public AdminController(UserManager<ApplicationUser> userManager,
                               RoleManager<IdentityRole> roleManager,
                               Data.AppDbContext db)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _db = db;
        }

        public async Task<IActionResult> Kullanicilar()
        {
            var kullanicilar = await _userManager.Users.ToListAsync();
            var model = new List<(ApplicationUser, IList<string>, string?, string?)>();
            ViewBag.Birimler = await _db.Birimler.OrderBy(b => b.Ad).ToListAsync();
            
            ViewBag.Depolar = await _db.Depolar.OrderBy(d => d.Ad).ToListAsync();

            foreach (var k in kullanicilar)
            {
                var roller = await _userManager.GetRolesAsync(k);
                var claims = await _userManager.GetClaimsAsync(k);
                var districtClaim = claims.FirstOrDefault(c => c.Type == "DistrictId")?.Value;
                var warehouseClaim = claims.FirstOrDefault(c => c.Type == "DefaultWarehouseId")?.Value;
                model.Add((k, roller, districtClaim, warehouseClaim));
            }

            return View(model);
        }

        
        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DepoAta(string userId, int? depoId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return NotFound();

            var claims = await _userManager.GetClaimsAsync(user);
            var warehouseClaims = claims.Where(c => c.Type == "DefaultWarehouseId").ToList();
            await _userManager.RemoveClaimsAsync(user, warehouseClaims);

            if (depoId.HasValue)
            {
                var depo = await _db.Depolar.FindAsync(depoId);
                if (depo != null)
                {
                    await _userManager.AddClaimAsync(user, new System.Security.Claims.Claim("DefaultWarehouseId", depoId.Value.ToString()));

                    
                    if (depo.BirimId.HasValue)
                    {
                        var currentDistrictClaims = claims.Where(c => c.Type == "DistrictId").ToList();
                        await _userManager.RemoveClaimsAsync(user, currentDistrictClaims);
                        await _userManager.AddClaimAsync(user, new System.Security.Claims.Claim("DistrictId", depo.BirimId.Value.ToString()));
                    }

                    TempData["Basarili"] = $"{user.Email} kullanıcısına {depo.Ad} deposu başarıyla atandı.";
                }
            }
            else
            {
                TempData["Basarili"] = $"{user.Email} kullanıcısının depo ataması kaldırıldı.";
            }

            return RedirectToAction(nameof(Kullanicilar));
        }

        
        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> IlceAta(string userId, int? birimId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return NotFound();

            var claims = await _userManager.GetClaimsAsync(user);
            var districtClaims = claims.Where(c => c.Type == "DistrictId").ToList();
            await _userManager.RemoveClaimsAsync(user, districtClaims);

            if (birimId.HasValue)
            {
                await _userManager.AddClaimAsync(user, new System.Security.Claims.Claim("DistrictId", birimId.Value.ToString()));
                TempData["Basarili"] = $"{user.Email} kullanıcısına ilçe başarıyla atandı.";
            }
            else
            {
                TempData["Basarili"] = $"{user.Email} kullanıcısının ilçe ataması kaldırıldı.";
            }

            return RedirectToAction(nameof(Kullanicilar));
        }

        
        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RolDegistir(string userId, List<string> yeniRoller)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return NotFound();

            var mevcutRoller = await _userManager.GetRolesAsync(user);
            await _userManager.RemoveFromRolesAsync(user, mevcutRoller);

            var gecerliRoller = new[] { "Admin", "IlceSorumlusu", "Kullanici" };
            var atanacakRoller = (yeniRoller ?? new List<string>())
                .Where(r => gecerliRoller.Contains(r))
                .ToList();

            if (atanacakRoller.Any())
                await _userManager.AddToRolesAsync(user, atanacakRoller);

            TempData["Basarili"] = $"{user.Email} kullanıcısının rolleri güncellendi.";
            return RedirectToAction(nameof(Kullanicilar));
        }

        
        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> KullaniciSil(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return NotFound();

            await _userManager.DeleteAsync(user);
            TempData["Basarili"] = "Kullanıcı silindi.";
            return RedirectToAction(nameof(Kullanicilar));
        }
    }
}
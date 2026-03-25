using ArıtmaEnvanter.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using ArıtmaEnvanter.Models.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using ArıtmaEnvanter.Models.ViewModels;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace ArıtmaEnvanter.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly Data.AppDbContext _db;

        public AccountController(UserManager<ApplicationUser> userManager,
                                  SignInManager<ApplicationUser> signInManager,
                                  Data.AppDbContext db)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _db = db;
        }

        
        public IActionResult Login() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(string email, string password, bool beniHatirla = false)
        {
            var result = await _signInManager.PasswordSignInAsync(email, password, beniHatirla, false);
            if (result.Succeeded)
                return RedirectToAction("Index", "Home");

            ModelState.AddModelError("", "Email veya şifre hatalı.");
            return View();
        }
        
        public IActionResult Kayit() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Kayit(string adSoyad, string email, string password)
        {
            var user = new ApplicationUser { AdSoyad = adSoyad, UserName = email, Email = email };
            var result = await _userManager.CreateAsync(user, password);
            if (result.Succeeded)
            {
                var kullaniciSayisi = _userManager.Users.Count();
                if (kullaniciSayisi == 1)
                    await _userManager.AddToRoleAsync(user, "Admin");
                else
                    await _userManager.AddToRoleAsync(user, "Kullanici");

                await _signInManager.SignInAsync(user, false);
                return RedirectToAction("Index", "Home");
            }
            foreach (var error in result.Errors)
                ModelState.AddModelError("", error.Description);
            return View();
        }

        
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Login");
        }

        
        public IActionResult AccessDenied() => View();

        
        [Authorize]
        public async Task<IActionResult> Profil()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login");

            var roles = await _userManager.GetRolesAsync(user);
            var claims = await _userManager.GetClaimsAsync(user);
            var districtId = claims.FirstOrDefault(c => c.Type == "DistrictId")?.Value;
            var warehouseId = claims.FirstOrDefault(c => c.Type == "DefaultWarehouseId")?.Value;

            var model = new ProfileViewModel
            {
                AdSoyad = user.AdSoyad,
                Email = user.Email!,
                Rol = string.Join(", ", roles)
            };

            if (int.TryParse(districtId, out int bId))
                model.BirimAd = (await (null as dynamic))?.Ad;

            if (int.TryParse(warehouseId, out int dId))
                model.DepoAd = (await (null as dynamic))?.Ad;

            
            var personel = await _db.Personeller.FirstOrDefaultAsync(p => p.Email == user.Email);
            if (personel != null)
            {
                model.Zimmetlerim = await _db.Zimmetler
                    .Include(z => z.Malzeme)
                    .ThenInclude(u => u.Kategori)
                    .Where(z => z.PersonelId == personel.Id && z.Durum == "Aktif")
                    .ToListAsync();
            }

            
            model.Taleplerim = await _db.TransferTalepler
                .Include(t => t.KaynakDepo)
                .Include(t => t.HedefDepo)
                .Where(t => t.TalepEdenUserId == user.Id)
                .OrderByDescending(t => t.TalepTarihi)
                .Take(5)
                .ToListAsync();

            return View(model);
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Profil(ProfileViewModel model)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login");

            if (!ModelState.IsValid)
                return View(model);

            
            user.AdSoyad = model.AdSoyad;
            user.Email = model.Email;
            user.UserName = model.Email;

            var updateResult = await _userManager.UpdateAsync(user);
            if (!updateResult.Succeeded)
            {
                foreach (var error in updateResult.Errors)
                    ModelState.AddModelError("", error.Description);
                return View(model);
            }

            
            if (!string.IsNullOrEmpty(model.NewPassword))
            {
                if (string.IsNullOrEmpty(model.CurrentPassword))
                {
                    ModelState.AddModelError("CurrentPassword", "Şifre değiştirmek için mevcut şifrenizi girmelisiniz.");
                    return View(model);
                }

                var passwordResult = await _userManager.ChangePasswordAsync(user, model.CurrentPassword, model.NewPassword);
                if (!passwordResult.Succeeded)
                {
                    foreach (var error in passwordResult.Errors)
                        ModelState.AddModelError("", error.Description);
                    return View(model);
                }
            }

            await _signInManager.RefreshSignInAsync(user);
            TempData["SuccessMessage"] = "Profiliniz başarıyla güncellendi.";
            return RedirectToAction("Profil");
        }
    }
}

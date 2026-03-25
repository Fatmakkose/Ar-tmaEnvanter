using ArıtmaEnvanter.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using ArıtmaEnvanter.Data;
using ArıtmaEnvanter.Models.Entities;
using ArıtmaEnvanter.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    options.Password.RequireDigit = true;
    options.Password.RequiredLength = 6;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = false;
})
.AddEntityFrameworkStores<AppDbContext>()
.AddDefaultTokenProviders()
.AddClaimsPrincipalFactory<CustomUserClaimsPrincipalFactory>();

builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";
    options.AccessDeniedPath = "/Account/AccessDenied";
});

builder.Services.AddControllersWithViews().AddRazorRuntimeCompilation();
builder.Services.AddAntiforgery(options => options.HeaderName = "X-XSRF-TOKEN");
builder.Services.AddHttpContextAccessor();


builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("IlceSorumlusuPolicy", policy =>
        policy.RequireRole("IlceSorumlusu")
              .RequireClaim("DistrictId"));
});

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

using (var scope = app.Services.CreateScope())
{
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
    try
    {
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

        string[] roller = { "Admin", "Kullanici", "IlceSorumlusu" };
        foreach (var rol in roller)
        {
            if (!await roleManager.RoleExistsAsync(rol))
                await roleManager.CreateAsync(new IdentityRole(rol));
        }

        var tumKullanicilar = userManager.Users.ToList();
        foreach (var kullanici in tumKullanicilar)
        {
            var mevcutRoller = await userManager.GetRolesAsync(kullanici);
            if (!mevcutRoller.Any())
            {
                if (tumKullanicilar.IndexOf(kullanici) == 0)
                    await userManager.AddToRoleAsync(kullanici, "Admin");
                else
                    await userManager.AddToRoleAsync(kullanici, "Kullanici");
            }
        }
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Veritabanı başlatılırken bir hata oluştu.");
    }
}

app.Run();

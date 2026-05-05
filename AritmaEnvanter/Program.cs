using AritmaEnvanter.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using AritmaEnvanter.Data;
using AritmaEnvanter.Models.Entities;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Authorization;

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
    options.Cookie.SameSite = SameSiteMode.Lax; 
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
});


builder.Services.AddControllersWithViews(options =>
{
    var policy = new AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .Build();
    options.Filters.Add(new AuthorizeFilter(policy));
})
.AddRazorRuntimeCompilation()
.AddJsonOptions(options =>
{
    options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
    options.JsonSerializerOptions.WriteIndented = true;
});
builder.Services.AddAntiforgery(options => options.HeaderName = "X-XSRF-TOKEN");
builder.Services.AddHttpContextAccessor();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();


builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("IlceSorumlusuPolicy", policy =>
        policy.RequireRole("IlceSorumlusu")
              .RequireClaim("DistrictId"));
});
builder.Services.AddCors(options =>
{
    options.AddPolicy("MobilePolicy", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});
var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseForwardedHeaders(new ForwardedHeadersOptions
{
    ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
});

// app.UseHttpsRedirection();
app.UseCors("MobilePolicy");
app.UseStaticFiles();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Aritma Envanter API V1");
});

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



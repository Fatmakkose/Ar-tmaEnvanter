using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using ArýtmaEnvanter.Models.Entities;
using System.Security.Claims;

namespace ArýtmaEnvanter.Services
{
    public class CustomUserClaimsPrincipalFactory : UserClaimsPrincipalFactory<ApplicationUser, IdentityRole>
    {
        public CustomUserClaimsPrincipalFactory(
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager,
            IOptions<IdentityOptions> optionsAccessor)
            : base(userManager, roleManager, optionsAccessor)
        {
        }

        protected override async Task<ClaimsIdentity> GenerateClaimsAsync(ApplicationUser user)
        {
            var identity = await base.GenerateClaimsAsync(user);
            identity.AddClaim(new Claim("AdSoyad", user.AdSoyad ?? ""));
            return identity;
        }
    }
}

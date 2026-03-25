using Microsoft.AspNetCore.Identity;

namespace ArıtmaEnvanter.Models.Entities
{
    public class ApplicationUser : IdentityUser
    {
        public string AdSoyad { get; set; } = null!;
    }
}
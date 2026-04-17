using Microsoft.AspNetCore.Identity;

namespace AritmaEnvanter.Models.Entities
{
    public class ApplicationUser : IdentityUser
    {
        public string AdSoyad { get; set; } = null!;
    }
}

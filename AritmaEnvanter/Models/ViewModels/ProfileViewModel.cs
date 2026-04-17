using System.ComponentModel.DataAnnotations;

namespace AritmaEnvanter.Models.ViewModels
{
    public class ProfileViewModel
    {
        [Required(ErrorMessage = "Ad Soyad alanı zorunludur.")]
        [Display(Name = "Ad Soyad")]
        public string AdSoyad { get; set; } = null!;

        [Required(ErrorMessage = "Email alanı zorunludur.")]
        [EmailAddress(ErrorMessage = "Geçerli bir email adresi giriniz.")]
        [Display(Name = "Email")]
        public string Email { get; set; } = null!;

        [Display(Name = "Mevcut Şifre")]
        [DataType(DataType.Password)]
        public string? CurrentPassword { get; set; }

        [Display(Name = "Yeni Şifre")]
        [DataType(DataType.Password)]
        [StringLength(100, ErrorMessage = "{0} en az {2} karakter uzunluğunda olmalıdır.", MinimumLength = 6)]
        public string? NewPassword { get; set; }

        [Display(Name = "Yeni Şifre Tekrar")]
        [DataType(DataType.Password)]
        [Compare("NewPassword", ErrorMessage = "Yeni şifreler uyuşmuyor.")]
        public string? ConfirmNewPassword { get; set; }

        [Display(Name = "Rol")]
        public string? Rol { get; set; }

        [Display(Name = "Bağlı Birim/İlçe")]
        public string? BirimAd { get; set; }

        [Display(Name = "Bağlı Depo")]
        public string? DepoAd { get; set; }

        public List<Models.Entities.Zimmet> Zimmetlerim { get; set; } = new();
        public List<Models.Entities.TransferTalep> Taleplerim { get; set; } = new();

    }
}

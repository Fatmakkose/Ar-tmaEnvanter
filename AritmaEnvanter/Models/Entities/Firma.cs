using System.ComponentModel.DataAnnotations;

namespace AritmaEnvanter.Models.Entities
{
    public class Firma
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Firma adı zorunludur.")]
        [Display(Name = "Firma Adı")]
        public string FirmaAdi { get; set; } = string.Empty;

        [Required(ErrorMessage = "İlgili kişi ad soyad zorunludur.")]
        [Display(Name = "İlgili Kişi Ad Soyad")]
        public string IlgiliKisiAdSoyad { get; set; } = string.Empty;

        [Display(Name = "İletişim Numarası")]
        public string? IletisimNumarasi { get; set; }
    }
}

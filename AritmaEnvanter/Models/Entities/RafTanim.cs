using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AritmaEnvanter.Models.Entities
{
    public class RafTanim
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Raf adı zorunludur.")]
        [Display(Name = "Raf Adı")]
        public string Ad { get; set; } = null!;
       
        [Required(ErrorMessage = "Sorumlu personel zorunludur.")]
        [Display(Name = "Sorumlu Personel")]
        public string SorumluPersonel { get; set; } = null!;

        [Display(Name = "İletişim (GSM)")]
        public string? Iletisim { get; set; }

        [Display(Name = "Raf / Fiziksel Konum")]
        public string? FizikselKonum { get; set; }

        [Display(Name = "Açıklama / Notlar")]
        public string? Aciklama { get; set; }

        public DateTime OlusturmaTarihi { get; set; } = DateTime.UtcNow;
        public bool AktifMi { get; set; } = true;
    }
}

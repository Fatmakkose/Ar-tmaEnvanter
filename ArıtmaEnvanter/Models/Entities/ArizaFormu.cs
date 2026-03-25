using System;
using System.ComponentModel.DataAnnotations;

namespace ArıtmaEnvanter.Models.Entities
{
    public class ArizaFormu
    {
        public int Id { get; set; }

        public int UrunId { get; set; }
        public Urun? Urun { get; set; }

        [Required(ErrorMessage = "Arıza tanımı zorunludur.")]
        [Display(Name = "Arıza Tanımı")]
        public string ArizaTanimi { get; set; } = null!;

        [Display(Name = "Durum")]
        public string Durum { get; set; } = "Beklemede"; 

        public DateTime OlusturmaTarihi { get; set; } = DateTime.UtcNow;
    }
}

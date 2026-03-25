using System;
using System.ComponentModel.DataAnnotations;

namespace ArıtmaEnvanter.Models.Entities
{
    public class SatinAlma
    {
        public int Id { get; set; }

        public int UrunId { get; set; }
        public Urun? Urun { get; set; }

        [Display(Name = "Miktar")]
        public decimal Miktar { get; set; }

        [Display(Name = "Birim Fiyat")]
        public decimal BirimFiyat { get; set; }

        [Display(Name = "Toplam Maliyet")]
        public decimal ToplamMaliyet => Miktar * BirimFiyat;

        [Display(Name = "Satın Alma Tarihi")]
        public DateTime Tarih { get; set; } = DateTime.UtcNow;
    }
}

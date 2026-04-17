using System;
using System.ComponentModel.DataAnnotations;

namespace AritmaEnvanter.Models.Entities
{
    public class Bakim
    {
        public int Id { get; set; }

        public int UrunId { get; set; }
        public Urun? Urun { get; set; }

        [Display(Name = "Bakım Tarihi")]
        public DateTime BakimTarihi { get; set; }

        public DateTime? SonrakiBakimTarihi { get; set; } 

        public int? MalzemeId { get; set; } 
        public Malzeme? Malzeme { get; set; }

        [Display(Name = "Açıklama")]
        public string? Aciklama { get; set; }
    }
}

using System;
using System.ComponentModel.DataAnnotations;

namespace AritmaEnvanter.Models.Entities
{
    public class Urun
    {
        public int Id { get; set; }

        [Display(Name = "Dinamik Özellikler (JSON)")]
        public string? DynamicProperties { get; set; } 

        public string? UrunTuru { get; set; }

        public int? KategoriId { get; set; }
        public Kategori? Kategori { get; set; }

        public int? BirimId { get; set; }
        public Birim? Birim { get; set; }

       
        public string? SeriNo { get; set; }
        public string? Durum { get; set; } = "Aktif";
        public decimal? Miktar { get; set; }
        public int? LokasyonId { get; set; }
        public Lokasyon? Lokasyon { get; set; }
        public int? DepoId { get; set; }
        public DateTime OlusturmaTarihi { get; set; } = DateTime.UtcNow;
    }
}

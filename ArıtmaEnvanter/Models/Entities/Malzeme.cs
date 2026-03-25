using System;
using System.ComponentModel.DataAnnotations;

namespace ArıtmaEnvanter.Models.Entities
{
    public class Malzeme
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Malzeme adı zorunludur.")]
        [Display(Name = "Malzeme Adı")]
        public string Ad { get; set; } = null!;

        [Display(Name = "İkinci Ad / Alternatif Ad")]
        public string? AlternatifAd { get; set; }

        [Display(Name = "Kategori")]
        public int? KategoriId { get; set; }
        public Kategori? Kategori { get; set; } 

        [Display(Name = "Birim")]
        public int? BirimId { get; set; }
        public Birim? Birim { get; set; }

        public string? UrunTuru { get; set; } 

        [Display(Name = "Açıklama")]
        public string? Aciklama { get; set; }

        public string? Durum { get; set; } = "Aktif"; 

        public DateTime OlusturmaTarihi { get; set; } = DateTime.UtcNow;

        
        public int? RafId { get; set; }
        public string? Raf { get; set; } 
        public decimal? Miktar { get; set; }
        public string? SeriNo { get; set; }
        public string? MalzemeTuru { get; set; }

        public int? FormSablonId { get; set; }
        public FormSablon? FormSablon { get; set; }

    }
}



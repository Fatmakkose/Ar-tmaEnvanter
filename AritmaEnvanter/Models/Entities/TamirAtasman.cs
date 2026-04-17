using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace AritmaEnvanter.Models.Entities
{
    public class TamirAtasman
    {
        public int Id { get; set; }

        [Display(Name = "Dosya Adı")]
        public string? DosyaAdi { get; set; }

        [Display(Name = "Dosya Yolu")]
        public string? DosyaYolu { get; set; }

        
        public ICollection<DepoHareket> Hareketler { get; set; } = new List<DepoHareket>();
    }
}

using System;
using System.ComponentModel.DataAnnotations;

namespace AritmaEnvanter.Models.Entities
{
    public class KimyasalGiris
    {
        public int Id { get; set; }
        
        [Required]
        public DateTime Tarih { get; set; }
        
        public decimal Adet { get; set; }
        public decimal Kg { get; set; }
        
        public string? Aciklama { get; set; }
    }
}

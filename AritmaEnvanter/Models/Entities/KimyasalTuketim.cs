using System;
using System.ComponentModel.DataAnnotations;

namespace AritmaEnvanter.Models.Entities
{
    public class KimyasalTuketim
    {
        public int Id { get; set; }
        
        [Required]
        public int Yil { get; set; }
        [Required]
        public int Ay { get; set; }
        [Required]
        public int Gun { get; set; }
        
        public decimal Adet { get; set; }
        public decimal Kg { get; set; }
    }
}

using System;
using System.ComponentModel.DataAnnotations;

namespace ArıtmaEnvanter.Models.Entities
{
    public class KimyasalDevir
    {
        public int Id { get; set; }
        
        [Required]
        public int Yil { get; set; }
        
        public decimal DevredenAdet { get; set; }
        public decimal DevredenKg { get; set; }
    }
}

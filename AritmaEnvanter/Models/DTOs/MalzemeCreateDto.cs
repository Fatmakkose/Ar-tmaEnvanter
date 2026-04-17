using AritmaEnvanter.Models.DTOs;
using AritmaEnvanter.Models.Entities;

namespace AritmaEnvanter.Models.DTOs
{
    public class MalzemeCreateDto
    {
        public int? RafId { get; set; }
        public string Ad { get; set; } = null!;
        public string? AlternatifAd { get; set; }
        public string? Birim { get; set; }
        public string? Aciklama { get; set; }
        public string? Kategori { get; set; }
        public string? Raf { get; set; }


        
        public List<FieldDto> Fields { get; set; } = new();
    }
}


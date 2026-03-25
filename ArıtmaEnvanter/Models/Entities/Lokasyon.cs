using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ArıtmaEnvanter.Models.Entities
{
    public class Lokasyon
    {
        public int Id { get; set; }

        public int? BirimId { get; set; }
        public Birim? Birim { get; set; }

        [Display(Name = "Kat")]
        public string? Kat { get; set; } 

        public string? OdaNo { get; set; }

        public int? ParentId { get; set; }
        public Lokasyon? ParentLokasyon { get; set; }

        public ICollection<Lokasyon> AltLokasyonlar { get; set; } = new List<Lokasyon>();
    }
}

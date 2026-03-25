using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ArıtmaEnvanter.Models.Entities
{
    public class Birim
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Birim adı zorunludur.")]
        [Display(Name = "Birim Adı")]
        public string Ad { get; set; } = null!;

        public int? UstBirimId { get; set; }
        public Birim? UstBirim { get; set; }

        public DateTime OlusturmaTarihi { get; set; } = DateTime.UtcNow;
        public ICollection<Urun> Urunler { get; set; } = new List<Urun>();

        public ICollection<Birim> AltBirimler { get; set; } = new List<Birim>();
    }
}

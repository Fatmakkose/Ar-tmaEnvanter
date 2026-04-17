using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace AritmaEnvanter.Models.Entities
{
    public class Kategori
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Kategori adı zorunludur.")]
        [Display(Name = "Kategori Adı")]
        public string Ad { get; set; } = null!;

        public string? HiyerarsikId { get; set; }
        public DateTime OlusturmaTarihi { get; set; } = DateTime.UtcNow;

        public int? UstKategoriId { get; set; }
        public Kategori? UstKategori { get; set; }

        public ICollection<Kategori> AltKategoriler { get; set; } = new List<Kategori>();
    }
}

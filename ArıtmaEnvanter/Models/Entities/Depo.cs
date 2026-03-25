using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ArıtmaEnvanter.Models.Entities
{
    public class Depo
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Depo adı zorunludur.")]
        [Display(Name = "Depo Adı")]
        public string Ad { get; set; } = null!;

        public int? BirimId { get; set; }
        public Birim? Birim { get; set; }

        public int? UstDepoId { get; set; }
        public Depo? UstDepo { get; set; }

        public ICollection<Depo> AltDepolar { get; set; } = new List<Depo>();
    }
}

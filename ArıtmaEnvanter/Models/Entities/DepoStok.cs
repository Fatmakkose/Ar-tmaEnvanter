using System.ComponentModel.DataAnnotations;

namespace ArıtmaEnvanter.Models.Entities
{
    public class DepoStok
    {
        public int Id { get; set; }

        public int MalzemeId { get; set; }
        public Malzeme? Malzeme { get; set; }

        public int DepoId { get; set; }
        public Depo? Depo { get; set; }

        public int? RafTanimId { get; set; }
        public RafTanim? RafTanim { get; set; }
        
        public string? RafNo { get; set; }

        [Display(Name = "Miktar")]
        public decimal Miktar { get; set; }

        public string? UrunAdi { get; set; }
        public string? Birim { get; set; }
        
        [Display(Name = "İşlem Yapan Kişi")]
        public string? IslemYapanKisi { get; set; }

        public DateTime GuncellemeTarihi { get; set; } = DateTime.UtcNow;
    }
}

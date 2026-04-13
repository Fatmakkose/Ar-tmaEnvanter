using ArıtmaEnvanter.Models.Entities;
using System.ComponentModel.DataAnnotations.Schema;

namespace ArıtmaEnvanter.Models.Entities
{
 
    public class MalzemeTalepForm
    {
        public int Id { get; set; }

      
        public int FormSiraNo { get; set; }
       
        public string TalepEdenPersonelId { get; set; }
        [ForeignKey("TalepEdenPersonelId")]
        public virtual ApplicationUser TalepEdenPersonel { get; set; }

        public DateTime TalepTarihi { get; set; } = DateTime.Now;
        public DateTime? OnayTarihi { get; set; }

        public string? GenelAciklama { get; set; }
        public TalepDurumu Durum { get; set; } = TalepDurumu.Beklemede;

        
        public virtual List<MalzemeTalepSatir> Satirlar { get; set; } = new List<MalzemeTalepSatir>();
    }


    public class MalzemeTalepSatir
    {
        public int Id { get; set; }

        public int MalzemeTalepFormId { get; set; }
        [ForeignKey("MalzemeTalepFormId")]
        public virtual MalzemeTalepForm MalzemeTalepForm { get; set; }

        
        public int StokId { get; set; }
        [ForeignKey("StokId")]
        public virtual DepoStok Stok { get; set; }

        public decimal Miktar { get; set; }

    }

    public enum TalepDurumu
    {
        Beklemede,
        Onaylandi,
        Reddedildi,
        StoktaYok
    }
}
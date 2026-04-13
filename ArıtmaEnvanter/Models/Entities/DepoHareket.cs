using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ArıtmaEnvanter.Models.Entities
{
    public class DepoHareket
    {
        public int Id { get; set; }

        public int MalzemeId { get; set; }
        public Malzeme? Malzeme { get; set; }

        public int? KaynakDepoId { get; set; }
        public Depo? KaynakDepo { get; set; }

        public int? HedefDepoId { get; set; }
        public Depo? HedefDepo { get; set; }

        public int? RafTanimId { get; set; }
        public RafTanim? RafTanim { get; set; }

        public string? RafNo { get; set; }

        public decimal Miktar { get; set; }

        public string? IslemYapanKisi { get; set; }

        public DateTime Tarih { get; set; } = DateTime.UtcNow;

        public int? TamirAtasmanId { get; set; }
        public TamirAtasman? TamirAtasman { get; set; }

        public string? CikisFormNo { get; set; }

       
        public string? FormNo { get; set; }
        public string? IslemTuru { get; set; }
       

        public int? FirmaId { get; set; }
        public Firma? Firma { get; set; }

        public int? PersonelId { get; set; }
        [ForeignKey("PersonelId")]
        public virtual Personel? Personel { get; set; }

        public int? FormKayitId { get; set; }
        [ForeignKey("FormKayitId")]
        public virtual FormKayit? FormKayit { get; set; }

        
        public string? Aciklama { get; set; }
    }
}
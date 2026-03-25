using System;

namespace ArıtmaEnvanter.Models.Entities
{
    public class AssetTransaction
    {
        public int Id { get; set; }
        public int UrunId { get; set; }

        public int? EskiLokasyonId { get; set; }
      
        public int? YeniLokasyonId { get; set; }
       

        public DateTime IslemTarihi { get; set; } = DateTime.UtcNow;
        public string? IslemYapanKullaniciId { get; set; }
        public ApplicationUser? IslemYapanKullanici { get; set; }

        public string? Aciklama { get; set; }
        public string? IslemTipi { get; set; } 
    }
}

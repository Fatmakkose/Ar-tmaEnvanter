using System;
using System.ComponentModel.DataAnnotations;

namespace AritmaEnvanter.Models.Entities
{
    public class Transfer
    {
        public int Id { get; set; }

        public int UrunId { get; set; }
        public Urun? Urun { get; set; }

        public int KaynakBirimId { get; set; }
        public Birim? KaynakBirim { get; set; }

        public int HedefBirimId { get; set; }
        public Birim? HedefBirim { get; set; }

        [Display(Name = "Transfer Tarihi")]
        public DateTime TransferTarihi { get; set; } = DateTime.UtcNow;

        [Display(Name = "Açıklama")]
        public string? Aciklama { get; set; }
    }
}

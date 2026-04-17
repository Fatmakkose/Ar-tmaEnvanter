using System;

namespace AritmaEnvanter.Models.Entities
{
    public class Zimmet
    {
        public int Id { get; set; }
        public int PersonelId { get; set; }
        public int MalzemeId { get; set; }
        public Personel Personel { get; set; } = null!;
        public Malzeme Malzeme { get; set; } = null!;
        public int? UrunId { get; set; }
        public Urun? Urun { get; set; }
        public DateTime ZimmetTarihi { get; set; } = DateTime.Now;
        public DateTime? IadeTarihi { get; set; }
        public decimal ZimmetMiktari { get; set; } = 1;
        public string? Notlar { get; set; }
        public string Durum { get; set; } = "Aktif";
    }
}
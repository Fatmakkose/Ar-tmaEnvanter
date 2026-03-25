using System;

namespace ArıtmaEnvanter.Models.Entities
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
        public string? Notlar { get; set; }
        public string Durum { get; set; } = "Aktif";
    }
}
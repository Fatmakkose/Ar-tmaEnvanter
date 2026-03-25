namespace ArıtmaEnvanter.Models.ViewModels
{
    public class RafStokViewModel
    {
        public int StokId { get; set; }
        public string RafNo { get; set; } = null!;
        public string UrunAdi { get; set; } = null!;
        public decimal Adet { get; set; } // Toplam Girdi
        public decimal Verilen { get; set; } // Toplam Çıktı
        public decimal Kalan { get; set; } // Mevcut Stok
    }
}

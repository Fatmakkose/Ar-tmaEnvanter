using System.Collections.Generic;
using ArıtmaEnvanter.Models.Entities;

namespace ArıtmaEnvanter.Models.ViewModels
{
    public class PoliCizelgeViewModel
    {
        public int RafId { get; set; }
        public string RafAd { get; set; }
        public int Yil { get; set; }
        
        // Girdi Listesi (Tarih, Adet, Kg, Aciklama)
        public List<KimyasalGiris> Girisler { get; set; } = new();
        
        // Tüketim Listesi
        public List<KimyasalTuketim> Tuketimler { get; set; } = new();

        // Previous year carry over
        public KimyasalDevir? Devir { get; set; }
    }
}

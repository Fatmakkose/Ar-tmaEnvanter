using System.Collections.Generic;
using AritmaEnvanter.Models.Entities;

namespace AritmaEnvanter.Models.ViewModels
{
    public class PoliCizelgeViewModel
    {
        public int RafId { get; set; }
        public string? RafAd { get; set; }
        public int Yil { get; set; }
        public int? Ay { get; set; }


        public List<KimyasalGiris> Girisler { get; set; } = new();


        public List<KimyasalTuketim> Tuketimler { get; set; } = new();


        public KimyasalDevir? Devir { get; set; }
    }
}

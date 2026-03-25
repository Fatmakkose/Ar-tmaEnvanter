namespace ArıtmaEnvanter.Models.ViewModels
{
    public class EntryItemViewModel
    {
        public string UrunAdi { get; set; } = string.Empty;
        public string? AlternatifAd { get; set; }
        public int Miktar { get; set; }
        public string Birim { get; set; } = "Adet";
        public string? DynamicValuesJson { get; set; }
        public int? FormSablonId { get; set; }
        public int? MalzemeId { get; set; }
    }
}


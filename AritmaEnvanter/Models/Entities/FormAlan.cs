namespace AritmaEnvanter.Models.Entities
{
    public class FormAlan
    {
        public int Id { get; set; }
        public int FormSablonId { get; set; }
        public string AlanAdi { get; set; } = null!;
        public string AlanTipi { get; set; } = "Metin";
        public string? Secenekler { get; set; }
        public bool Gerekli { get; set; } = true;
        public int Sira { get; set; } = 0;

        public FormSablon FormSablon { get; set; } = null!;
    }
}


namespace ArıtmaEnvanter.Models.Entities
{
    public class FormKayit
    {
        public int Id { get; set; }
        public int FormSablonId { get; set; }
        public string? KayitNo { get; set; } 
        public string? Barkod { get; set; } 
        public DateTime OlusturmaTarihi { get; set; } = DateTime.UtcNow;
        public string? KaydedenKullanici { get; set; }

        public int? DepoId { get; set; }


        public int BirimId { get; set; }
        public Birim? Birim { get; set; }
        public Depo? Depo { get; set; }


        public FormSablon FormSablon { get; set; } = null!;
        public ICollection<FormKayitDeger> Degerler { get; set; } = new List<FormKayitDeger>();
    }
}
namespace AritmaEnvanter.Models.Entities
{
    public class FormSablon
    {
        public int Id { get; set; }
        public string Baslik { get; set; }
        public int? KategoriId { get; set; }
        public Kategori? Kategori { get; set; } 
        public string? Aciklama { get; set; }
        public int? UstFormId { get; set; }

        public string? HiyerarsikId { get; set; } 
        public DateTime OlusturmaTarihi { get; set; } = DateTime.UtcNow;

        public FormSablon? UstForm { get; set; }
        public ICollection<FormSablon> AltFormlar { get; set; } = new List<FormSablon>();
        public ICollection<FormAlan> Alanlar { get; set; } = new List<FormAlan>();
        public ICollection<FormKayit> Kayitlar { get; set; } = new List<FormKayit>();
    }
}
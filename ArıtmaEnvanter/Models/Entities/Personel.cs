namespace ArıtmaEnvanter.Models.Entities
{
    public class Personel
    {
        public int Id { get; set; }
        public string AdSoyad { get; set; } = null!;
        public string? Unvan { get; set; }
        public string? Telefon { get; set; }
        public string? Email { get; set; }
        public int? BirimId { get; set; }
        public Birim? Birim { get; set; }

        public string? KadroTuru { get; set; }

        public DateTime OlusturmaTarihi { get; set; } = DateTime.UtcNow;

        public ICollection<Zimmet> Zimmetler { get; set; } = new List<Zimmet>();
    }
}
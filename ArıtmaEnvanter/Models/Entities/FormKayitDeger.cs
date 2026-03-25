namespace ArıtmaEnvanter.Models.Entities
{
    public class FormKayitDeger
    {
        public int Id { get; set; }
        public int FormKayitId { get; set; }
        public int FormAlanId { get; set; }
        public string? Deger { get; set; }

        public FormKayit FormKayit { get; set; } = null!;
        public FormAlan FormAlan { get; set; } = null!;
        public string? GorselUrl { get; set; }
    }
}
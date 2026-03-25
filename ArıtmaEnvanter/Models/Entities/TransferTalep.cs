using System;
using System.ComponentModel.DataAnnotations;

namespace ArıtmaEnvanter.Models.Entities
{
    public class TransferTalep
    {
        public int Id { get; set; }

        public int? KaynakDepoId { get; set; }
        public Depo? KaynakDepo { get; set; }

        public int? HedefDepoId { get; set; }
        public Depo? HedefDepo { get; set; }

        public string? TalepEdenUserId { get; set; }
        public ApplicationUser? TalepEdenUser { get; set; }

        public string? OnaylayanUserId { get; set; }
        public ApplicationUser? OnaylayanUser { get; set; }

        [Display(Name = "Talep Tarihi")]
        public DateTime TalepTarihi { get; set; } = DateTime.UtcNow;

        [Display(Name = "Açıklama")]
        public string? Aciklama { get; set; }

        public string Durum { get; set; } = "Beklemede"; 
    }
}

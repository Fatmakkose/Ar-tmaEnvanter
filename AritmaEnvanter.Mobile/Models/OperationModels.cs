namespace AritmaEnvanter.Mobile.Models
{
    public class MovementItem
    {
        public int Id { get; set; }
        public string MaterialName { get; set; }
        public decimal Amount { get; set; }
        public string Unit { get; set; }
        public string TransactionType { get; set; } // GIR, CIK, TRN
        public string Date { get; set; }
        public string User { get; set; }
        public string WarehouseName { get; set; }
        public string ShelfName { get; set; }
        public string Specification { get; set; } // For details like Size, Color, etc.
        public string FullMaterialName => $"{(MaterialName ?? "").Trim()} {(Specification ?? "").Trim()}".Trim();

        public string DisplayAmount => TransactionType == "GİR" || TransactionType == "IAD" ? $"+{Amount} {Unit}" : $"-{Amount} {Unit}";
        public string StatusColor => TransactionType == "GİR" ? "#34C759" : (TransactionType == "IAD" ? "#0077b6" : "#FF3B30");
        public string TypeIcon => TransactionType == "GİR" ? "arrow_up_circle.png" : (TransactionType == "IAD" ? "arrow_back_circle.png" : "arrow_down_circle.png");
    }

    public class RequestForm
    {
        public int Id { get; set; }
        public int FormNo { get; set; }
        public string RequesterName { get; set; }
        public string Date { get; set; }
        public string Description { get; set; }
        public string Status { get; set; }
        public List<RequestItem> Items { get; set; } = new();

        public string StatusColor => Status switch
        {
            "Beklemede" => "#F39C12",
            "Onaylandi" => "#27AE60",
            "Reddedildi" => "#C0392B",
            _ => "#7F8C8D"
        };

        public bool IsPending => Status == "Beklemede";
    }

    public class RequestItem
    {
        public int Id { get; set; }
        public string MaterialName { get; set; }
        public string Specification { get; set; }
        public decimal Quantity { get; set; }
        public string Unit { get; set; }
    }


    public class FormAlan
    {
        public int Id { get; set; }
        public string AlanAdi { get; set; }
        public string AlanTipi { get; set; }
        public bool Gerekli { get; set; }
        public string Secenekler { get; set; }
        
        // Mobile UI Helpers
        public string Value { get; set; }
        public List<string> Options => !string.IsNullOrEmpty(Secenekler) 
            ? Secenekler.Split(new[] { ',', '\n' }, StringSplitOptions.RemoveEmptyEntries).Select(s => s.Trim()).ToList() 
            : new List<string>();
    }

    public class MetadataItem
    {
        public int Id { get; set; }
        public string? Ad { get; set; } // For Warehouse, Shelf, Material
        public string? AdSoyad { get; set; } // For Personnel
        public string? FirmaAdi { get; set; } // For Company

        public override string ToString() => !string.IsNullOrEmpty(Ad) ? Ad : (!string.IsNullOrEmpty(AdSoyad) ? AdSoyad : FirmaAdi);
    }

    public class ExitCartItem
    {
        public LocalStockItem? Stock { get; set; }
        public decimal Amount { get; set; }
        public string ExitType { get; set; } = "Sarf";
        public MetadataItem? SelectedPersonnel { get; set; }
        public string Note { get; set; }

        public bool IsDemirbas => ExitType == "Demirbaş";
    }
}

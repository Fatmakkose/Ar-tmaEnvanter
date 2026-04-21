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

        public string DisplayAmount => TransactionType == "GİR" ? $"+{Amount} {Unit}" : $"-{Amount} {Unit}";
        public string StatusColor => TransactionType == "GİR" ? "#34C759" : "#FF3B30";
        public string TypeIcon => TransactionType == "GİR" ? "arrow_up_circle.png" : "arrow_down_circle.png";
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
        public string Ad { get; set; } // For Warehouse, Shelf, Material
        public string AdSoyad { get; set; } // For Personnel
        public string FirmaAdi { get; set; } // For Company

        public override string ToString() => !string.IsNullOrEmpty(Ad) ? Ad : (!string.IsNullOrEmpty(AdSoyad) ? AdSoyad : FirmaAdi);
    }

    public class ExitCartItem
    {
        public LocalStockItem Stock { get; set; }
        public decimal Amount { get; set; }
        public string ExitType { get; set; } = "Sarf";
        public MetadataItem SelectedPersonnel { get; set; }
        public string Note { get; set; }

        public bool IsDemirbas => ExitType == "Demirbaş";
    }
}

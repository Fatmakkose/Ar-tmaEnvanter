using SQLite;

namespace AritmaEnvanter.Mobile.Models
{
    public class LocalStockItem
    {
        [PrimaryKey]
        public int Id { get; set; }
        public int MalzemeId { get; set; }
        public string MaterialName { get; set; }
        public string WarehouseName { get; set; }
        public string ShelfName { get; set; }
        public string RafNo { get; set; }
        public decimal Quantity { get; set; }
        public string Unit { get; set; }
        public string LastUpdatedBy { get; set; }
        public string LastUpdatedDate { get; set; }
        public string MaterialType { get; set; } // Liquid, Solid, Barrel, Part
        public string Specification { get; set; } // For details like Size, Color, etc.

        [Ignore]
        public string FormattedDate => string.IsNullOrEmpty(LastUpdatedDate) ? "" : DateTime.Parse(LastUpdatedDate).ToString("dd.MM.yyyy HH:mm");

        [Ignore]
        public string IconSource => MaterialType?.ToLower() switch
        {
            "liquid" or "sıvı" => "icon_liquid.jpg",
            "solid" or "katı" => "icon_solid.jpg",
            "barrel" or "varil" => "icon_barrel.jpg",
            "part" or "parça" => "icon_part.jpg",
            _ => "icon_part.jpg"
        };

        [Ignore]
        public string CriticalStatus => Quantity < 10 ? "DÜŞÜK" : "TAM";

        [Ignore]
        public string StatusColor => Quantity < 10 ? "#FF3B30" : "#34C759";
    }
}

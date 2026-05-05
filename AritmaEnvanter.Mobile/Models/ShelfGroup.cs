using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;
using System.Linq;

namespace AritmaEnvanter.Mobile.Models
{
    public partial class ShelfGroup : ObservableObject
    {
        public string ShelfName { get; set; }
        public string WarehouseName { get; set; }
        public ObservableCollection<RafGroup> RafGroups { get; set; } = new();

        [ObservableProperty]
        private bool isExpanded;

        public int ProductCount => RafGroups.Sum(rg => rg.Products.Count);
        
        // Helper for display
        public string DisplayName => $"{ShelfName} ({WarehouseName})";
    }
}

using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;
using System.Linq;

namespace AritmaEnvanter.Mobile.Models
{
    public partial class RafGroup : ObservableObject
    {
        public string RafNo { get; set; }
        public ObservableCollection<LocalStockItem> Products { get; set; } = new();
        
        [ObservableProperty]
        private bool isExpanded;

        public decimal TotalQuantity => Products.Sum(p => p.Quantity);
    }
}


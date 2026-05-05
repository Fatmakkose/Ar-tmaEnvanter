using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.Input;
using AritmaEnvanter.Mobile.Models;
using AritmaEnvanter.Mobile.Services;
using CommunityToolkit.Mvvm.ComponentModel;

namespace AritmaEnvanter.Mobile.ViewModels
{
    public partial class InventoryViewModel : BaseViewModel
    {
        private readonly ApiService _apiService;
        private readonly DatabaseService _databaseService;

        private List<LocalStockItem> _allStocks = new();
        public ObservableCollection<LocalStockItem> Stocks { get; } = new();
        public ObservableCollection<ShelfGroup> GroupedStocks { get; } = new();
        public ObservableCollection<MetadataItem> Materials { get; } = new();
        public ObservableCollection<MetadataItem> FilteredMaterials { get; } = new();
        public ObservableCollection<MetadataItem> Warehouses { get; } = new();
        public ObservableCollection<MetadataItem> Shelves { get; } = new();
        public ObservableCollection<MetadataItem> Personnel { get; } = new();
        public ObservableCollection<MetadataItem> Companies { get; } = new();
        public ObservableCollection<FormAlan> DynamicFields { get; } = new();
        public ObservableCollection<ExitCartItem> ExitBasket { get; } = new();
        public ObservableCollection<MetadataItem> AllShelves { get; } = new();
        public ObservableCollection<LocalStockItem> ExitAvailableStocks { get; } = new();

        [ObservableProperty] string materialSearchText;
        [ObservableProperty] string exitMaterialSearchText;

        [ObservableProperty] bool isRefreshing;
        [ObservableProperty] string searchText;
        [ObservableProperty] int criticalCount;
        [ObservableProperty] int todayEntryCount;
        [ObservableProperty] int todayExitCount;

        // Overlay Properties
        [ObservableProperty] bool isOperationVisible;
        [ObservableProperty] string operationTitle;
        [ObservableProperty] bool isEntry;
        [ObservableProperty] bool isExit;
        [ObservableProperty] bool isReturn;
        [ObservableProperty] decimal operationAmount;
        [ObservableProperty] string operationUnit = "Adet";
        [ObservableProperty] string operationNote;
        [ObservableProperty] string exitType = "Sarf";

        [ObservableProperty] [NotifyPropertyChangedFor(nameof(IsRafNoVisible))] MetadataItem? selectedMaterial;
        [ObservableProperty] MetadataItem? selectedWarehouse;
        [ObservableProperty] [NotifyPropertyChangedFor(nameof(IsRafNoVisible))] MetadataItem? selectedShelf;
        [ObservableProperty] MetadataItem? selectedPersonnel;
        [ObservableProperty] MetadataItem? selectedCompany;
        [ObservableProperty] string rafNo;
        [ObservableProperty] LocalStockItem? targetStock;
        [ObservableProperty] bool isMaterialListVisible;

        public bool IsRafNoVisible => !IsPolielektrolitKimyasal;

        private bool IsPolielektrolitKimyasal => 
            SelectedMaterial?.Ad?.Contains("POLIELEKTROLIT", StringComparison.OrdinalIgnoreCase) == true && 
            SelectedShelf?.Ad?.Contains("KİMYASAL", StringComparison.OrdinalIgnoreCase) == true;

        public bool IsDemirbas => ExitType == "Demirbaş";

        public InventoryViewModel(ApiService apiService, DatabaseService databaseService)
        {
            _apiService = apiService;
            _databaseService = databaseService;
            Title = "Ürünler";
        }

        [RelayCommand]
        async Task LoadDataAsync()
        {
            if (IsBusy) return;
            try
            {
                IsBusy = true;
                var summary = await _apiService.GetSummaryAsync();
                if (summary != null)
                {
                    CriticalCount = summary.CriticalStockCount;
                    TodayEntryCount = summary.TodayEntryCount;
                    TodayExitCount = summary.TodayExitCount;
                }

                var localStocks = await _databaseService.GetStocksAsync();
                _allStocks = localStocks ?? new();
                ApplyFilter();

                var remoteStocks = await _apiService.GetStocksAsync();
                if (remoteStocks != null)
                {
                    await _databaseService.SaveStocksAsync(remoteStocks);
                    _allStocks = remoteStocks;
                    ApplyFilter();
                }

                // Metadata pre-load
                var meta = await _apiService.GetMetadataAsync();
                if (meta != null)
                {
                    UpdateMetadata(Materials, meta.materials);
                    UpdateMetadata(Warehouses, meta.warehouses);
                    UpdateMetadata(Personnel, meta.personnel);
                    UpdateMetadata(Companies, meta.companies);

                    // Fetch shelves for all warehouses so grouping remains stable across navigation.
                    AllShelves.Clear();
                    if (Warehouses.Any())
                    {
                        var seenShelfIds = new HashSet<int>();
                        foreach (var warehouse in Warehouses.Where(w => w != null))
                        {
                            var shelves = await _apiService.GetShelvesAsync(warehouse.Id);
                            if (shelves == null) continue;

                            foreach (var shelf in shelves)
                            {
                                if (shelf == null || string.IsNullOrWhiteSpace(shelf.Ad)) continue;
                                if (seenShelfIds.Add(shelf.Id))
                                    AllShelves.Add(shelf);
                            }
                        }
                    }

                    // Re-apply grouping with latest shelf definitions.
                    ApplyFilter();
                }
            }
            finally
            {
                IsBusy = false;
                IsRefreshing = false;
            }
        }

        private void UpdateMetadata(ObservableCollection<MetadataItem> target, dynamic source)
        {
            target.Clear();
            if (source == null) return;
            foreach (var item in source)
            {
                target.Add(new MetadataItem 
                { 
                    Id = (int)item.id, 
                    Ad = item.ad?.ToString() ?? "",
                    AdSoyad = item.adSoyad?.ToString() ?? "",
                    FirmaAdi = item.firmaAdi?.ToString() ?? ""
                });
            }
        }

        [RelayCommand]
        void OpenEntry()
        {
            IsEntry = true; IsExit = false; IsReturn = false;
            OperationTitle = "Yeni Stok Girişi";
            ResetForm();
            IsOperationVisible = true;
            ApplyMaterialFilter();
        }

        [RelayCommand]
        void OpenExit()
        {
            IsEntry = false; IsExit = true; IsReturn = false;
            OperationTitle = "Stok Çıkışı (Toplu İşlem)";
            ResetForm();
            ExitBasket.Clear();
            IsOperationVisible = true;
        }

        [RelayCommand]
        void OpenReturn()
        {
            ResetForm();
            IsEntry = false; IsExit = false; IsReturn = true;
            OperationTitle = "Ürün İadesi";
            IsOperationVisible = true;
        }

        [RelayCommand]
        void AddToBasket()
        {
            if (TargetStock == null || OperationAmount <= 0)
            {
                Shell.Current.DisplayAlert("Hata", "Lütfen ürün seçin ve geçerli bir miktar girin.", "Tamam");
                return;
            }

            if (OperationAmount > TargetStock.Quantity)
            {
                Shell.Current.DisplayAlert("Hata", "Stokta yeterli miktar yok.", "Tamam");
                return;
            }

            ExitBasket.Add(new ExitCartItem
            {
                Stock = TargetStock,
                Amount = OperationAmount,
                ExitType = ExitType,
                SelectedPersonnel = SelectedPersonnel,
                Note = OperationNote
            });

            // Reset selection for next item
            TargetStock = null;
            OperationAmount = 0;
            OperationNote = "";
        }

        [RelayCommand]
        void RemoveFromBasket(ExitCartItem item)
        {
            ExitBasket.Remove(item);
        }

        [RelayCommand]
        async Task SelectStock(LocalStockItem stock)
        {
            string action = await Shell.Current.DisplayActionSheet($"{stock.MaterialName}", "İptal", null, "Stok Girişi", "Stok Çıkışı", "İade");
            
            if (action == "İptal" || string.IsNullOrEmpty(action)) return;

            ResetForm();
            TargetStock = stock;
            OperationUnit = stock.Unit;

            switch (action)
            {
                case "Stok Girişi":
                    IsEntry = true; IsExit = false; IsReturn = false;
                    OperationTitle = "Yeni Stok Girişi";
                    IsOperationVisible = true;
                    ApplyMaterialFilter();
                    var mat = Materials.FirstOrDefault(m => m.Ad == stock.MaterialName);
                    if (mat != null) SelectedMaterial = mat;
                    break;
                case "Stok Çıkışı":
                    IsEntry = false; IsExit = true; IsReturn = false;
                    OperationTitle = "Stok Çıkışı (Toplu İşlem)";
                    ExitBasket.Clear();
                    IsOperationVisible = true;
                    break;
                case "İade":
                    IsEntry = false; IsExit = false; IsReturn = true;
                    OperationTitle = "Ürün İadesi";
                    IsOperationVisible = true;
                    break;
            }
        }

        [RelayCommand]
        void ToggleShelf(ShelfGroup group)
        {
            if (group == null) return;
            group.IsExpanded = !group.IsExpanded;
        }

        [RelayCommand]
        void ToggleRaf(RafGroup group)
        {
            if (group == null) return;
            group.IsExpanded = !group.IsExpanded;
        }

        [RelayCommand]
        void SelectMaterial(MetadataItem material)
        {
            SelectedMaterial = material;
        }

        [RelayCommand]
        void CloseOperation()
        {
            IsOperationVisible = false;
        }

        async partial void OnSelectedMaterialChanged(MetadataItem? value)
        {
            if (value == null) return;
            IsMaterialListVisible = false;
            MaterialSearchText = value.ToString();
            
            var details = await _apiService.GetMaterialDetailsAsync(value.Id);
            if (details != null)
            {
                OperationUnit = details.unit.ToString();
                DynamicFields.Clear();
                foreach (var field in details.dynamicFields)
                {
                    DynamicFields.Add(new FormAlan
                    {
                        Id = (int)field.id,
                        AlanAdi = field.alanAdi.ToString(),
                        AlanTipi = field.alanTipi.ToString(),
                        Gerekli = (bool)field.gerekli,
                        Secenekler = field.secenekler.ToString()
                    });
                }
            }
        }

        async partial void OnSelectedWarehouseChanged(MetadataItem? value)
        {
            Shelves.Clear();
            if (value == null) return;
            
            var shelves = await _apiService.GetShelvesAsync(value.Id);
            if (shelves != null)
            {
                foreach (var s in shelves) Shelves.Add(s);
            }
        }

        partial void OnMaterialSearchTextChanged(string value)
        {
            ApplyMaterialFilter();
            IsMaterialListVisible = !string.IsNullOrWhiteSpace(value) && SelectedMaterial?.ToString() != value;
        }

        partial void OnExitMaterialSearchTextChanged(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                ExitAvailableStocks.Clear();
                return;
            }

            var filtered = _allStocks
                .Where(s => s.MaterialName.Contains(value, StringComparison.OrdinalIgnoreCase))
                .ToList();

            ExitAvailableStocks.Clear();
            foreach (var s in filtered) ExitAvailableStocks.Add(s);
        }

        [RelayCommand]
        void SelectExitStock(LocalStockItem stock)
        {
            TargetStock = stock;
            ExitMaterialSearchText = stock.MaterialName;
            OperationUnit = stock.Unit;
            ExitAvailableStocks.Clear();
        }

        private void ApplyMaterialFilter()
        {
            var filtered = string.IsNullOrWhiteSpace(MaterialSearchText)
                ? Materials.ToList()
                : Materials.Where(m => m.ToString().Contains(MaterialSearchText, StringComparison.OrdinalIgnoreCase)).ToList();

            FilteredMaterials.Clear();
            foreach (var m in filtered) FilteredMaterials.Add(m);
        }

        [RelayCommand]
        async Task SaveOperationAsync()
        {
            if (IsEntry)
            {
                if (OperationAmount <= 0)
                {
                    await Shell.Current.DisplayAlert("Hata", "Lütfen geçerli bir miktar girin.", "Tamam");
                    return;
                }
                await PerformSingleSave(new
                {
                    OperationType = "GIRIS",
                    MalzemeId = SelectedMaterial?.Id,
                    Amount = IsPolielektrolitKimyasal ? OperationAmount * 25 : OperationAmount,
                    WarehouseId = SelectedWarehouse?.Id,
                    ShelfId = SelectedShelf?.Id,
                    ShelfNo = IsPolielektrolitKimyasal ? "" : RafNo,
                    CompanyId = SelectedCompany?.Id,
                    Note = OperationNote,
                    DynamicFields = DynamicFields.Select(f => new { FieldId = f.Id, Value = f.Value }).ToList()
                });
            }
            else if (IsReturn)
            {
                if (OperationAmount <= 0)
                {
                    await Shell.Current.DisplayAlert("Hata", "Lütfen geçerli bir miktar girin.", "Tamam");
                    return;
                }

                if (TargetStock == null)
                {
                    await Shell.Current.DisplayAlert("Hata", "Lütfen iade edilecek ürünü seçin.", "Tamam");
                    return;
                }

                await PerformSingleSave(new
                {
                    OperationType = "IADE",
                    StokId = TargetStock.Id,
                    Amount = OperationAmount,
                    Note = OperationNote
                });
            }
            else // Bulk Exit
            {
                if (!ExitBasket.Any())
                {
                    await Shell.Current.DisplayAlert("Hata", "Lütfen çıkış yapılacak ürün ekleyin.", "Tamam");
                    return;
                }

                IsBusy = true;
                int successCount = 0;
                foreach (var item in ExitBasket)
                {
                    var request = new
                    {
                        OperationType = "CIKIS",
                        StokId = item.Stock.Id,
                        Amount = item.Amount,
                        PersonnelId = item.SelectedPersonnel?.Id,
                        ExitType = item.ExitType,
                        Note = item.Note
                    };

                    var (success, _) = await _apiService.PerformOperationAsync(request);
                    if (success) successCount++;
                }
                IsBusy = false;

                if (successCount == ExitBasket.Count)
                {
                    await Shell.Current.DisplayAlert("Başarılı", $"{successCount} adet ürün çıkışı başarıyla yapıldı.", "Tamam");
                    IsOperationVisible = false;
                    await LoadDataAsync();
                }
                else
                {
                    await Shell.Current.DisplayAlert("Kısmi Başarı", $"{successCount}/{ExitBasket.Count} ürün işlendi. Lütfen listeyi kontrol edin.", "Tamam");
                    await LoadDataAsync();
                }
            }
        }

        private async Task PerformSingleSave(object request)
        {
            IsBusy = true;
            var (success, message) = await _apiService.PerformOperationAsync(request);
            IsBusy = false;

            if (success)
            {
                await Shell.Current.DisplayAlert("Başarılı", message, "Tamam");
                IsOperationVisible = false;
                await LoadDataAsync();
            }
            else
            {
                await Shell.Current.DisplayAlert("Hata", message, "Tamam");
            }
        }

        private void ResetForm()
        {
            OperationAmount = 0;
            OperationNote = "";
            RafNo = "";
            DynamicFields.Clear();
            SelectedMaterial = null;
            SelectedWarehouse = null;
            SelectedShelf = null;
            SelectedCompany = null;
            MaterialSearchText = "";
            IsMaterialListVisible = false;
            TargetStock = null;
            ExitMaterialSearchText = "";
        }

        private void ApplyFilter()
        {
            var definedShelfNames = AllShelves
                .Where(s => !string.IsNullOrWhiteSpace(s.Ad))
                .Select(s => s.Ad!.Trim())
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(s => s)
                .ToList();

            var filtered = string.IsNullOrWhiteSpace(SearchText)
                ? _allStocks
                : _allStocks.Where(s =>
                        (!string.IsNullOrWhiteSpace(s.MaterialName) && s.MaterialName.Contains(SearchText, StringComparison.OrdinalIgnoreCase)) ||
                        (!string.IsNullOrWhiteSpace(s.ShelfName) && s.ShelfName.Contains(SearchText, StringComparison.OrdinalIgnoreCase)) ||
                        ResolveShelfDisplayName(s, definedShelfNames).Contains(SearchText, StringComparison.OrdinalIgnoreCase))
                    .ToList();

            Stocks.Clear();
            foreach (var stock in filtered) Stocks.Add(stock);

            var groupedStocks = filtered
                .Where(s => !string.IsNullOrWhiteSpace(s.ShelfName))
                .GroupBy(s => ResolveShelfDisplayName(s, definedShelfNames), StringComparer.OrdinalIgnoreCase)
                .OrderBy(g => g.Key)
                .ToList();

            var groups = new List<ShelfGroup>();
            foreach (var shelfGroup in groupedStocks)
            {
                if (shelfGroup == null || !shelfGroup.Any()) continue;

                var categoryName = shelfGroup.Key;
                var warehouseName = shelfGroup.FirstOrDefault()?.WarehouseName ?? Warehouses.FirstOrDefault()?.Ad ?? "Merkez Depo";

                groups.Add(new ShelfGroup
                {
                    ShelfName = categoryName,
                    WarehouseName = warehouseName,
                    RafGroups = new ObservableCollection<RafGroup>(
                        shelfGroup
                            .GroupBy(s => GetRafGroupName(s, categoryName), StringComparer.OrdinalIgnoreCase)
                            .Select(rg => new RafGroup
                            {
                                RafNo = rg.Key,
                                Products = new ObservableCollection<LocalStockItem>(rg.OrderBy(p => p.MaterialName).ToList()),
                                IsExpanded = !string.IsNullOrWhiteSpace(SearchText)
                            })
                            .OrderBy(rg => rg.RafNo, new NaturalStringComparer())
                            .ToList()
                    ),
                    IsExpanded = !string.IsNullOrWhiteSpace(SearchText)
                });
            }

            var unassignedProducts = filtered.Where(s => string.IsNullOrWhiteSpace(s.ShelfName) || s.ShelfName == "-").ToList();
            if (unassignedProducts.Any() && (string.IsNullOrWhiteSpace(SearchText) || unassignedProducts.Any()))
            {
                groups.Add(new ShelfGroup
                {
                    ShelfName = "Tanımsız Raf",
                    WarehouseName = "-",
                    RafGroups = new ObservableCollection<RafGroup> {
                        new RafGroup { 
                            RafNo = "-", 
                            Products = new ObservableCollection<LocalStockItem>(unassignedProducts),
                            IsExpanded = !string.IsNullOrWhiteSpace(SearchText)
                        }
                    },
                    IsExpanded = !string.IsNullOrWhiteSpace(SearchText)
                });
            }
            
            groups = groups.OrderBy(g => g.ShelfName, new NaturalStringComparer())
                           .ThenBy(g => g.ShelfName == "Tanımsız Raf") // Ensure Tanımsız stays at bottom
                           .ToList();

            // Custom sort to keep "Tanımsız Raf" at the bottom
            var sortedGroups = groups.OrderBy(g => g.ShelfName == "Tanımsız Raf").ThenBy(g => g.ShelfName, new NaturalStringComparer()).ToList();

            GroupedStocks.Clear();
            foreach (var group in sortedGroups) GroupedStocks.Add(group);
        }

        partial void OnSearchTextChanged(string value) => ApplyFilter();
        partial void OnExitTypeChanged(string value) => OnPropertyChanged(nameof(IsDemirbas));

        private static string ResolveShelfDisplayName(LocalStockItem stock, List<string> definedShelfNames)
        {
            if (stock == null || string.IsNullOrWhiteSpace(stock.ShelfName))
                return "Tanımsız Raf";

            var shelfName = stock.ShelfName.Trim();
            
            // Extract root (e.g. "A" from "A-1-1")
            var root = new string(shelfName.TakeWhile(char.IsLetter).ToArray()).ToUpper();
            if (string.IsNullOrEmpty(root))
            {
                var parts = shelfName.Split(new[] { '-', ' ', '.', '_' }, StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length > 0) root = parts[0].ToUpper();
            }

            if (string.IsNullOrEmpty(root)) root = shelfName;

            // Try to find a defined shelf that matches this root
            // e.g. root "A" matches "A Rafları"
            var match = definedShelfNames
                .FirstOrDefault(name => 
                    name.Equals(root, StringComparison.OrdinalIgnoreCase) || 
                    name.StartsWith(root + " ", StringComparison.OrdinalIgnoreCase) ||
                    name.StartsWith(root + "-", StringComparison.OrdinalIgnoreCase) ||
                    (root.Length > 0 && name.StartsWith(root, StringComparison.OrdinalIgnoreCase)));

            if (!string.IsNullOrWhiteSpace(match))
                return match;

            return root.Length <= 3 ? $"{root} Rafları" : root;
        }

        private static string GetRafGroupName(LocalStockItem stock, string shelfDisplayName)
        {
            if (stock == null || string.IsNullOrWhiteSpace(stock.ShelfName))
                return "-";

            // If the shelf name is exactly the same as the group name, and we have a RafNo, use RafNo
            if (stock.ShelfName.Equals(shelfDisplayName, StringComparison.OrdinalIgnoreCase) && !string.IsNullOrWhiteSpace(stock.RafNo))
                return stock.RafNo;

            return stock.ShelfName.Trim();
        }
    }

    public class NaturalStringComparer : IComparer<string>
    {
        public int Compare(string? x, string? y)
        {
            if (x == y) return 0;
            if (x == null) return -1;
            if (y == null) return 1;

            var xParts = System.Text.RegularExpressions.Regex.Split(x.Replace(" ", ""), "([0-9]+)");
            var yParts = System.Text.RegularExpressions.Regex.Split(y.Replace(" ", ""), "([0-9]+)");

            for (int i = 0; i < Math.Min(xParts.Length, yParts.Length); i++)
            {
                if (xParts[i] != yParts[i])
                {
                    if (int.TryParse(xParts[i], out int xInt) && int.TryParse(yParts[i], out int yInt))
                        return xInt.CompareTo(yInt);
                    return string.Compare(xParts[i], yParts[i], StringComparison.OrdinalIgnoreCase);
                }
            }

            return xParts.Length.CompareTo(yParts.Length);
        }
    }
}

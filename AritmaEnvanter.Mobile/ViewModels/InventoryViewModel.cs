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
        public ObservableCollection<MetadataItem> Materials { get; } = new();
        public ObservableCollection<MetadataItem> FilteredMaterials { get; } = new();
        public ObservableCollection<MetadataItem> Warehouses { get; } = new();
        public ObservableCollection<MetadataItem> Shelves { get; } = new();
        public ObservableCollection<MetadataItem> Personnel { get; } = new();
        public ObservableCollection<MetadataItem> Companies { get; } = new();
        public ObservableCollection<FormAlan> DynamicFields { get; } = new();
        public ObservableCollection<ExitCartItem> ExitBasket { get; } = new();
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
        [ObservableProperty] decimal operationAmount;
        [ObservableProperty] string operationUnit = "Adet";
        [ObservableProperty] string operationNote;
        [ObservableProperty] string exitType = "Sarf";

        [ObservableProperty] MetadataItem selectedMaterial;
        [ObservableProperty] MetadataItem selectedWarehouse;
        [ObservableProperty] MetadataItem selectedShelf;
        [ObservableProperty] MetadataItem selectedPersonnel;
        [ObservableProperty] MetadataItem selectedCompany;
        [ObservableProperty] string rafNo;
        [ObservableProperty] LocalStockItem targetStock;
        [ObservableProperty] bool isMaterialListVisible;

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
            IsEntry = true; isExit = false;
            OperationTitle = "Yeni Stok Girişi";
            ResetForm();
            IsOperationVisible = true;
            ApplyMaterialFilter();
        }

        [RelayCommand]
        void OpenExit()
        {
            IsEntry = false; IsExit = true;
            OperationTitle = "Stok Çıkışı (Toplu İşlem)";
            ResetForm();
            ExitBasket.Clear();
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
        void SelectStock(LocalStockItem stock)
        {
            TargetStock = stock;
            OpenExit();
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

        async partial void OnSelectedMaterialChanged(MetadataItem value)
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

        async partial void OnSelectedWarehouseChanged(MetadataItem value)
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
                    Amount = OperationAmount,
                    WarehouseId = SelectedWarehouse?.Id,
                    ShelfId = SelectedShelf?.Id,
                    ShelfNo = RafNo,
                    CompanyId = SelectedCompany?.Id,
                    Note = OperationNote,
                    DynamicFields = DynamicFields.Select(f => new { FieldId = f.Id, Value = f.Value }).ToList()
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
        }

        private void ApplyFilter()
        {
            var filtered = string.IsNullOrWhiteSpace(SearchText)
                ? _allStocks
                : _allStocks.Where(s => s.MaterialName.Contains(SearchText, StringComparison.OrdinalIgnoreCase)).ToList();

            Stocks.Clear();
            foreach (var stock in filtered) Stocks.Add(stock);
        }

        partial void OnSearchTextChanged(string value) => ApplyFilter();
        partial void OnExitTypeChanged(string value) => OnPropertyChanged(nameof(IsDemirbas));
    }
}

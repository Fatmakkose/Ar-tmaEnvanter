using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.Input;
using AritmaEnvanter.Mobile.Models;
using AritmaEnvanter.Mobile.Services;
using CommunityToolkit.Mvvm.ComponentModel;

namespace AritmaEnvanter.Mobile.ViewModels
{
    public partial class MovementsViewModel : BaseViewModel
    {
        private readonly ApiService _apiService;

        public ObservableCollection<MovementItem> Movements { get; } = new();

        [ObservableProperty] bool isRefreshing;
        [ObservableProperty] string selectedType = "Hepsi";
        [ObservableProperty] DateTime startDate = DateTime.Now.AddDays(-30);
        [ObservableProperty] DateTime endDate = DateTime.Now;

        public List<string> FilterTypes { get; } = new() { "Hepsi", "GİR", "ÇIK", "İADE" };

        public MovementsViewModel(ApiService apiService)
        {
            _apiService = apiService;
            Title = "Stok Hareketleri";
        }

        [RelayCommand]
        async Task LoadMovementsAsync()
        {
            if (IsBusy) return;

            try
            {
                IsBusy = true;

                string typeFilter = SelectedType == "Hepsi" ? "" : (SelectedType == "İADE" ? "IAD" : SelectedType);
                
                var results = await _apiService.GetMovementsAsync(
                    typeFilter, 
                    StartDate.ToString("yyyy-MM-dd"), 
                    EndDate.ToString("yyyy-MM-dd"));

                Movements.Clear();
                foreach (var item in results)
                {
                    Movements.Add(item);
                }
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Hata", $"Hareketler yüklenemedi: {ex.Message}", "Tamam");
            }
            finally
            {
                IsBusy = false;
                IsRefreshing = false;
            }
        }

        [RelayCommand]
        void ApplyFilter()
        {
            LoadMovementsCommand.Execute(null);
        }

        // Trigger filter on property changes
        partial void OnSelectedTypeChanged(string value) => ApplyFilter();
        partial void OnStartDateChanged(DateTime value) => ApplyFilter();
        partial void OnEndDateChanged(DateTime value) => ApplyFilter();
    }
}

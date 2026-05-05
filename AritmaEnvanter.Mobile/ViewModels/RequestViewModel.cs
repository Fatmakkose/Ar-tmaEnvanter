using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using AritmaEnvanter.Mobile.Models;
using AritmaEnvanter.Mobile.Services;
using System.Collections.ObjectModel;

namespace AritmaEnvanter.Mobile.ViewModels
{
    public partial class RequestViewModel : BaseViewModel
    {
        private readonly ApiService _apiService;

        public ObservableCollection<RequestForm> Requests { get; } = new();

        [ObservableProperty]
        bool isRefreshing;

        public RequestViewModel()
        {
            _apiService = new ApiService();
            Title = "Talepler";
        }

        [RelayCommand]
        public async Task LoadRequestsAsync()
        {
            if (IsBusy) return;

            try
            {
                IsBusy = true;
                IsRefreshing = true;

                var results = await _apiService.GetTaleplerAsync();
                
                Requests.Clear();
                foreach (var item in results)
                {
                    Requests.Add(item);
                }
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Hata", $"Talepler yüklenirken bir hata oluştu: {ex.Message}", "Tamam");
            }
            finally
            {
                IsBusy = false;
                IsRefreshing = false;
            }
        }

        [RelayCommand]
        public async Task ApproveRequestAsync(RequestForm request)
        {
            if (request == null) return;

            bool confirm = await Shell.Current.DisplayAlert("Onay", $"{request.FormNo} nolu talebi onaylıyor musunuz?", "Evet", "Hayır");
            if (!confirm) return;

            try
            {
                IsBusy = true;
                var (success, message) = await _apiService.ApproveTalepAsync(request.Id);
                
                if (success)
                {
                    await Shell.Current.DisplayAlert("Başarılı", message, "Tamam");
                    await LoadRequestsAsync();
                }
                else
                {
                    await Shell.Current.DisplayAlert("Hata", message, "Tamam");
                }
            }
            finally
            {
                IsBusy = false;
            }
        }

        [RelayCommand]
        public async Task RejectRequestAsync(RequestForm request)
        {
            if (request == null) return;

            bool confirm = await Shell.Current.DisplayAlert("Red", $"{request.FormNo} nolu talebi reddediyor musunuz?", "Evet", "Hayır");
            if (!confirm) return;

            try
            {
                IsBusy = true;
                var (success, message) = await _apiService.RejectTalepAsync(request.Id);
                
                if (success)
                {
                    await Shell.Current.DisplayAlert("Başarılı", message, "Tamam");
                    await LoadRequestsAsync();
                }
                else
                {
                    await Shell.Current.DisplayAlert("Hata", message, "Tamam");
                }
            }
            finally
            {
                IsBusy = false;
            }
        }
    }
}

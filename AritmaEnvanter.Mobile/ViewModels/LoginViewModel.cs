using CommunityToolkit.Mvvm.Input;
using AritmaEnvanter.Mobile.Services;
using CommunityToolkit.Mvvm.ComponentModel;

namespace AritmaEnvanter.Mobile.ViewModels
{
    public partial class LoginViewModel : BaseViewModel
    {
        private readonly ApiService _apiService;

        [ObservableProperty]
        string email;

        [ObservableProperty]
        string password;

        public LoginViewModel(ApiService apiService)
        {
            _apiService = apiService;
            Title = "Giriş Yap";
        }

        [RelayCommand]
        async Task LoginAsync()
        {
            if (IsBusy) return;

            if (string.IsNullOrEmpty(Email) || string.IsNullOrEmpty(Password))
            {
                await Shell.Current.DisplayAlert("Hata", "Lütfen tüm alanları doldurun.", "Tamam");
                return;
            }

            try
            {
                IsBusy = true;
                var (success, message, fullName) = await _apiService.LoginAsync(Email, Password);

                if (success)
                {
                    // Giriş başarılı, ana sayfaya yönlendir
                    await Shell.Current.GoToAsync("//Stoklar"); 
                }
                else
                {
                    await Shell.Current.DisplayAlert("Giriş Başarısız", message, "Tamam");
                }
            }
            finally
            {
                IsBusy = false;
            }
        }
    }
}

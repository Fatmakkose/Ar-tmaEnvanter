using AritmaEnvanter.Mobile.ViewModels;

namespace AritmaEnvanter.Mobile.Views
{
    public partial class LoginPage : ContentPage
    {
        public LoginPage(LoginViewModel viewModel)
        {
            InitializeComponent();
            BindingContext = viewModel;
        }
    }
}

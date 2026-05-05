using AritmaEnvanter.Mobile.ViewModels;

namespace AritmaEnvanter.Mobile.Views
{
    public partial class RequestPage : ContentPage
    {
        private readonly RequestViewModel _viewModel;

        public RequestPage()
        {
            InitializeComponent();
            BindingContext = _viewModel = new RequestViewModel();
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await _viewModel.LoadRequestsAsync();
        }
    }
}

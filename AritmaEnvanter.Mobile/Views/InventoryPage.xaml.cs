using AritmaEnvanter.Mobile.ViewModels;

namespace AritmaEnvanter.Mobile.Views
{
    public partial class InventoryPage : ContentPage
    {
        private readonly InventoryViewModel _viewModel;

        public InventoryPage(InventoryViewModel viewModel)
        {
            InitializeComponent();
            BindingContext = _viewModel = viewModel;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await _viewModel.LoadDataCommand.ExecuteAsync(null);
        }
    }
}

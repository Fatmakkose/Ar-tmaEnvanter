using AritmaEnvanter.Mobile.ViewModels;

namespace AritmaEnvanter.Mobile.Views;

public partial class MovementsPage : ContentPage
{
	public MovementsPage(MovementsViewModel viewModel)
	{
		InitializeComponent();
		BindingContext = viewModel;
	}

    protected override void OnAppearing()
    {
        base.OnAppearing();
        (BindingContext as MovementsViewModel)?.LoadMovementsCommand.Execute(null);
    }
}

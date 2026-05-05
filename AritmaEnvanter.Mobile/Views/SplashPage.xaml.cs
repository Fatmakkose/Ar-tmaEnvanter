using Microsoft.Maui.Controls;

namespace AritmaEnvanter.Mobile.Views;

public partial class SplashPage : ContentPage
{
	public SplashPage()
	{
		InitializeComponent();
	}

	protected override async void OnAppearing()
	{
		base.OnAppearing();

		// Wait for 3 seconds (to show the animation)
		await Task.Delay(3000);

		// Navigate to the Shell (which starts with LoginPage)
		if (Application.Current != null)
		{
			Application.Current.MainPage = new AppShell();
		}
	}
}

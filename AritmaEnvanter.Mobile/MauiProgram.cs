using Microsoft.Extensions.Logging;
using AritmaEnvanter.Mobile.Services;
using AritmaEnvanter.Mobile.ViewModels;
using AritmaEnvanter.Mobile.Views;

namespace AritmaEnvanter.Mobile;

public static class MauiProgram
{
	public static MauiApp CreateMauiApp()
	{
		var builder = MauiApp.CreateBuilder();
		builder
			.UseMauiApp<App>()
			.ConfigureFonts(fonts =>
			{
				fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
				fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
			});

		builder.Services.AddSingleton<DatabaseService>();
		builder.Services.AddSingleton<ApiService>();

		builder.Services.AddTransient<LoginViewModel>();
		builder.Services.AddTransient<LoginPage>();

		builder.Services.AddTransient<InventoryViewModel>();
		builder.Services.AddTransient<InventoryPage>();

		builder.Services.AddTransient<MovementsViewModel>();
		builder.Services.AddTransient<MovementsPage>();

		builder.Services.AddTransient<RequestPage>();
#if DEBUG


		builder.Logging.AddDebug();
#endif

		return builder.Build();
	}
}


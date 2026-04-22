using Microsoft.Extensions.Logging;

namespace MauiDexChallenge.App;

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

		builder.Services.AddSingleton(new Services.AppSettings());
		builder.Services.AddSingleton<HttpClient>();
		builder.Services.AddSingleton<Services.IDexSubmissionService, Services.DexSubmissionService>();
		builder.Services.AddTransient<ViewModels.MainViewModel>();
		builder.Services.AddTransient<MainPage>();

#if DEBUG
		builder.Logging.AddDebug();
#endif

		return builder.Build();
	}
}

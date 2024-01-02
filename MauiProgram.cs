using Microsoft.Extensions.Logging;
using Plugin.LocalNotification;

namespace RemindMe
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
#if ANDROID
            builder.Services.AddTransient<IBackgroundGPSService, BackgroundGPSService>();
#endif

            builder.Services.AddSingleton<MainPage>();
            builder
                .UseMauiApp<App>()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                })
                .UseLocalNotification();
              //  .UseMauiMaps();

#if DEBUG
		builder.Logging.AddDebug();
#endif

            return builder.Build();
        }
    }
}
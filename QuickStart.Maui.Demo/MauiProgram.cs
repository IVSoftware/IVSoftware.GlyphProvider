using Microsoft.Extensions.Logging;

namespace QuickStart.Maui.Demo
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            StartupDiagnostics.RegisterGlobalExceptionHooks();
            StartupDiagnostics.Log("CreateMauiApp: begin");

            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                    fonts.AddFont("icon-basics.ttf", "icon-basics");
                });

#if DEBUG
    		builder.Logging.AddDebug();
#endif

            var app = builder.Build();
            StartupDiagnostics.Log("CreateMauiApp: end");
            return app;
        }
    }
}

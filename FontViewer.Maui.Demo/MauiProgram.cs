using Microsoft.Extensions.Logging;
using Microsoft.Maui.Handlers;
using static System.Net.Mime.MediaTypeNames;

namespace FontViewer.Maui.Demo
{
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
                    fonts.AddFont("icon-basics.ttf", "icon-basics");
                    fonts.AddFont("icon-media-control.ttf", "icon-media-control");
                });

#if DEBUG
    		builder.Logging.AddDebug();
#endif

#if WINDOWS
            // Hover mapper
            ButtonHandler.Mapper.AppendToMapping("GlyphButtonHover", (handler, view) =>
            {
                if (view is GlyphButton button)
                {
                    var native = handler.PlatformView;

                    native.PointerEntered += (_, __) => button.PointerInBounds(true);
                    native.PointerExited += (_, __) => button.PointerInBounds(false);

                    if (button.StdIconName is { } icon)
                    {
                        var tooltip = new Microsoft.UI.Xaml.Controls.ToolTip
                        {
                            Content = icon.ToString(),
                        };
                        Microsoft.UI.Xaml.Controls.ToolTipService.SetToolTip(native, tooltip);
                    }
                }
            });
#endif

            return builder.Build();
        }
    }
}

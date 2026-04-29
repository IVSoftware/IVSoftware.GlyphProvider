using IVSoftware.Portable;
using System.Diagnostics;

namespace QuickStart.Maui.Demo
{
    public partial class MainPage : ContentPage
    {
        public MainPage()
        {
            StartupDiagnostics.Log("MainPage ctor: begin");
            InitializeComponent();
#if WINDOWS
            Loaded += (sender, e) => Window!.Title = "Quick Start";
#endif

            _ = InitAsync();
            StartupDiagnostics.Log("MainPage ctor: end");
        }
        async Task InitAsync()
        {
            try
            {
                StartupDiagnostics.Log("InitAsync: begin");
                await GlyphProvider.WaitAsync();
                StartupDiagnostics.Log("InitAsync: after GlyphProvider.WaitAsync");

                foreach (var icon in Enum.GetValues<GlyphProvider.IconBasics>())
                {
                    var button = new Button
                    {
                        HeightRequest = 50,
                        WidthRequest = 50,
                        BorderColor = Color.FromArgb("#444444"),
                        Margin = new Thickness(1),
                        Padding = 0,
                        FontSize = 18,
                        FontFamily = "icon-basics",
                        Text = icon.ToGlyph(),
                    };
                    flexLayout.Children.Add(button);
                }
                StartupDiagnostics.Log($"InitAsync: populated {flexLayout.Children.Count} buttons");
            }
            catch (Exception ex)
            {
                StartupDiagnostics.LogException("MainPage.InitAsync", ex);
                throw;
            }
        }
    }
}

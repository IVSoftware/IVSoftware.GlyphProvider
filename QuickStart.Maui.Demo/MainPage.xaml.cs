using IVSoftware.Portable;
using System.Diagnostics;

namespace QuickStart.Maui.Demo
{
    public partial class MainPage : ContentPage
    {
        public MainPage()
        {
            InitializeComponent();
#if WINDOWS
            Loaded += (sender, e) => Window!.Title = "Quick Start";
#endif

            _ = InitAsync();
        }
        async Task InitAsync()
        {
            await GlyphProvider.WaitAsync();

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
        }
    }
}

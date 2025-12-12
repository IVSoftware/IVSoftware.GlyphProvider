using IVSoftware.Portable;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace QuickStart.Wpf.Demo
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            _ = InitAsync();
        }
        async Task InitAsync()
        {
            await GlyphProvider.WaitAsync();

            FontFamily fontFamily;

            // "resources/fonts/icon-basics/font/icon-basics.ttf"
            var resource = "icon-basics".GetResourcePath<MainWindow>(".ttf");

            var familyName = resource?.ToWpfFamilyName<MainWindow>();
            // "pack://application:,,,/QuickStart.Wpf.Demo;component/resources/fonts/icon-basics/font/#icon-basics"
            

            if (!string.IsNullOrWhiteSpace(familyName))
            {                               
                fontFamily = new FontFamily(baseUri: GlyphProvider.Wpf.BaseUri, familyName: familyName);

                if (!ValidateFontFamily(fontFamily))
                { 
                    throw new InvalidOperationException($"Validation failed for icon-basics.");
                }

                foreach (var icon in Enum.GetValues<GlyphProvider.IconBasics>())
                {
                    var cMe = icon.ToGlyph(GlyphFormat.UnicodeDisplay);
                    var button = new Button
                    {
                        FontFamily = fontFamily,
                        FontSize = 18,
                        Content = icon.ToGlyph(),
                    };
                    TextOptions.SetTextFormattingMode(button, TextFormattingMode.Display);
                    TextOptions.SetTextRenderingMode(button, TextRenderingMode.ClearType);
                    TextOptions.SetTextHintingMode(button, TextHintingMode.Fixed);
                    WrapPanelIcons.Children.Add(button);
                }
            }
        }
        private bool ValidateFontFamily(FontFamily family)
        {
            foreach (var typeface in family.GetTypefaces())
            {
                if (!typeface.TryGetGlyphTypeface(out GlyphTypeface glyph))
                {
                    return false;
                }
            }
            return true;
        }
    }
}
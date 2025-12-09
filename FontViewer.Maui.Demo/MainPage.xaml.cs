using IVSoftware.Portable;
using IVSoftware.Portable.Collections.Dictionaries;
using Newtonsoft.Json;
using System.Collections;

namespace FontViewer.Maui.Demo
{
    public partial class MainPage : ContentPage
    {
        public MainPage()
        {
            InitializeComponent();
#if WINDOWS
            Loaded += (sender, e) => Window!.Title = "Font Viewer";
#endif

            _ = InitAsync();
        }
        async Task InitAsync()
        {
            await GlyphProvider.WaitAsync();

            foreach (var icon in Enum.GetValues<GlyphProvider.IconBasics>())
            {
                flexLayout.Children.Add(new GlyphButton { StdIconName = icon, });
            }

            // GlyphProvider doesn't enumerate its own provider for GlyphProvider.IconBasics unless
            // requested. This is so that EUD's config space isn't polluted with an unwanted default.
            var iconBasicsProvider = GlyphProvider.Providers[typeof(GlyphProvider.IconBasics)];
            ConfigPicker.ItemsSource = GlyphProvider.Providers.Values.Concat([iconBasicsProvider]).Distinct().ToArray();
            ConfigPicker.SelectedIndex = ConfigPicker.ItemsSource.IndexOf(iconBasicsProvider);
            ConfigPicker.SelectedIndexChanged += OnConfigSelected;
            NavBar.IsVisible = ConfigPicker.ItemsSource.Count > 1;
        }

        private void OnConfigSelected(object? sender, EventArgs e)
        {
            if (ConfigPicker.SelectedItem is GlyphProvider provider)
            {
                flexLayout.Children.Clear();
                if (provider.StdIconEnumType is { } stdIconType)
                {
                    // If an enum type has been defined for provider, use it.
                    foreach (Enum icon in Enum.GetValues(stdIconType))
                    {
                        flexLayout.Children.Add(new GlyphButton { StdIconName = icon, });
                    }
                }
                else
                {
                    // Otherwise, the raw information can still be used to greate iconic buttons.
                    foreach (var info in provider.Glyphs)
                    {
                        flexLayout.Children.Add(
                            new GlyphButton
                            {
                                FontFamily = provider.Name,
                                Text = char.ConvertFromUtf32(info.Code).ToString()
                            });
                    }
                }
            }
        }
    }

    class GlyphButton : Button
    {
        public GlyphButton()
        {
            HeightRequest = 50;
            WidthRequest = 50;
            BorderColor = Color.FromArgb("#444444");
            Margin = new Thickness(1);
            Padding = 0;
            FontSize = 18;

            Clicked += async (sender, e) =>
            {
                if (StdIconName?.ToGlyphInfo() is { } info &&
                    Window?.Page is { } page)
                {
                    var json = JsonConvert.SerializeObject(info, Formatting.Indented);
                    await page.DisplayAlertAsync(null, json, "Close");
                }
            };
        }
        public static readonly BindableProperty GlyphProperty =
            BindableProperty.Create(
                nameof(StdIconName),
                typeof(Enum),
                typeof(GlyphButton),
                default(Enum),
                propertyChanged: OnGlyphChanged);

        public Enum? StdIconName
        {
            get => (Enum?)GetValue(GlyphProperty);
            set => SetValue(GlyphProperty, value);
        }

        static void OnGlyphChanged(BindableObject bindable, object oldValue, object newValue)
        {
            if (newValue is Enum icon)
            {
                var button = (GlyphButton)bindable;
                button.FontFamily = icon.ToCssFontName();
                button.Text = icon?.ToGlyph() ?? string.Empty;
            }
        }

#if WINDOWS
        // Highlight when mouse is hovered.
        public void PointerInBounds(bool isInBounds)
        {
            TextColor = isInBounds ? Colors.Aqua : Colors.Black;
        }
#endif
    }
}

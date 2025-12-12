using IVSoftware.Portable;

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

            ConfigPicker.ItemsSource = 
                GlyphProvider.Providers.Values
                .Concat([iconBasicsProvider])   // Concat the icon-basics provider, which is not automatically included
                .Distinct().ToArray();

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
}

using IVSoftware.Portable;
using IVSoftware.Portable.Common;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using IVSoftware.Portable.Collections.Dictionaries;

namespace FontViewer.Wpf.Demo
{
    public partial class MainWindow : Window
    {
        enum NameType { }
        internal static TolerantDictionary<string, FontFamily> FontFamilyCache = new();
        public MainWindow()
        {
            InitializeComponent();
            _ = InitAsync();

            string localMakeFriend()
            {
                var asm = GetType().Assembly;
                var name = asm.GetName();
                var publicKey = name.GetPublicKey();

                if (publicKey is null || publicKey.Length == 0)
                    throw new InvalidOperationException("Assembly is not strong-named.");

                var hex = BitConverter
                    .ToString(publicKey)
                    .Replace("-", string.Empty)
                    .ToLowerInvariant();

                return $@"[assembly: InternalsVisibleTo(""{name.Name}, PublicKey={hex}"")]";

            }
        }
        async Task InitAsync()
        {
            await GlyphProvider.WaitAsync();

            // GlyphProvider doesn't enumerate its own provider for GlyphProvider.IconBasics unless
            // requested. This is so that EUD's config space isn't polluted with an unwanted default.
            var iconBasicsProvider = GlyphProvider.Providers[typeof(GlyphProvider.IconBasics)];

            var providers = GlyphProvider.Providers.Values.Concat([iconBasicsProvider]).Distinct().ToArray();

            // Import the fonts.
            // We want to make a cache, and use the fully-qualified name of the provider as the key.
            foreach (var provider in providers.OfType<GlyphProvider>())
            {
                var familyName = provider.Name?.GetResourcePath<MainWindow>()?.ToWpfFamilyName<MainWindow>();
                if (!string.IsNullOrWhiteSpace(familyName))
                {
                    var fontFamily = new FontFamily(
                        baseUri: GlyphProvider.Wpf.BaseUri,
                        familyName: familyName);
                    FontFamilyCache[provider.Key] = fontFamily;
                    ComboBoxConfig.Items.Add(provider);
                }
            }

            foreach (var icon in Enum.GetValues<GlyphProvider.IconBasics>())
            {
                WrapPanelIcons.Children.Add(new GlyphButton { StdIconName = icon, });
            }

            ComboBoxConfig.SelectedIndex = ComboBoxConfig.Items.IndexOf(iconBasicsProvider);
            ComboBoxConfig.SelectionChanged += OnConfigSelected;

            NavBar.Visibility = 
                ComboBoxConfig.Items.Count > 1
                ? Visibility.Visible
                : Visibility.Collapsed;
        }

        private void OnConfigSelected(object sender, SelectionChangedEventArgs e)
        {
            if (ComboBoxConfig.SelectedItem is GlyphProvider provider)
            {
                WrapPanelIcons.Children.Clear();
                if (provider.StdIconEnumType is { } stdIconType)
                {
                    // If an enum type has been defined for provider, use it.
                    foreach (Enum icon in Enum.GetValues(stdIconType))
                    {
                        WrapPanelIcons.Children.Add(new GlyphButton { StdIconName = icon, });
                    }
                }
                else
                {
                    // Otherwise, the raw information can still be used to greate iconic buttons.
                    var fontFamily = FontFamilyCache[provider.Key] ?? new();
                    foreach (var info in provider.Glyphs)
                    {
                        WrapPanelIcons.Children.Add(
                            new GlyphButton
                            {
                                FontFamily = fontFamily,
                                Content = char.ConvertFromUtf32(info.Code).ToString()
                            });
                    }
                }
            }
        }
    }
}
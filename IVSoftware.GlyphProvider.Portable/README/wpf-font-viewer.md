# [<](../../README.md)

## WPF Font Viewer

This example uses standard WPF `Button` controls.  
The window defines an empty `WrapPanel`, and the `InitAsync` method fills it with glyph-bearing buttons at runtime.

![WPF Font Viewer](https://raw.githubusercontent.com/IVSoftware/IVSoftware.GlyphProvider/master/IVSoftware.GlyphProvider.Portable/README/img/wpf-font-viewer.png)


The `Font Viewer` version adds the capability of switching between the available font archives using the combo box where providers of type `GlyphProvider` are added to `comboBoxConfig.Items`.

---

### XAML

The page contains a `WrapPanel` that will be populated with glyph buttons.

```xaml
<Window 
    x:Class="FontViewer.Wpf.Demo.MainWindow"
    xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
    xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
    Title="WPF Font Viewer Demo"
    Width="500" Height="250"
    Background="Aqua"
    WindowStartupLocation="CenterScreen">

    <Grid>
        <Grid.RowDefinitions>
            <RowDefinition Height="Auto"/>
            <RowDefinition Height="*"/>
        </Grid.RowDefinitions>

        <!-- NavBar -->
        <Border 
            x:Name="NavBar"
            Grid.Row="0"
            Visibility="Collapsed"
            Height="40"
            Padding="1"
            BorderBrush="Black"
            BorderThickness="1">
            <Grid 
                Background="#EEEEEE">
                <ComboBox
                    x:Name="ComboBoxConfig"
                    FontSize="12.5"
                    Margin="3"/>
            </Grid>
        </Border>

        <ScrollViewer
            Grid.Row="1"
            Background="Azure"
            Margin="2"
            Padding="2">
            <WrapPanel
                x:Name="WrapPanelIcons"
                Orientation="Horizontal"
                ItemHeight="48"
                ItemWidth="48"/>
        </ScrollViewer>
    </Grid>
</Window>
```


### Code-Behind

The providers of type `GlyphProvider` are added to the `ComboBoxConfig.Items`.

```

namespace FontViewer.Wpf.Demo
{
    public partial class MainWindow : Window
    {
        internal static TolerantDictionary<string, FontFamily> FontFamilyCache = new();
        public MainWindow()
        {
            InitializeComponent();
            _ = InitAsync();
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
                WrapPanelIcons.Children.Add(new GlyphButton { OPID = icon, });
            }

            ComboBoxConfig.SelectedIndex = ComboBoxConfig.Items.IndexOf(iconBasicsProvider);
            ComboBoxConfig.SelectionChanged += OnConfigSelected;

            NavBar.Visibility = 
                ComboBoxConfig.Items.Count > 1
                ? Visibility.Visible
                : Visibility.Collapsed;
        }

 ```
 
  Changing the combo box selection repopulates the viewer by iterating the `enum`. If a std enum cannot be found for the provider, the provider's own list is iterated. Having a defined `enum` is (literally) the key to showing tool tips and respoding to clicks.

 ```
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
                        WrapPanelIcons.Children.Add(new GlyphButton { OPID = icon, });
                    }
                }
                else
                {
                    // Otherwise, the raw information can still be used to create iconic buttons. (No ToolTips or Click Action in this case.)
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
```
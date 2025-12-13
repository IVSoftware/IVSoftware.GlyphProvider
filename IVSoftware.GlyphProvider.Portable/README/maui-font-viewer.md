# [<](../../README.md)

## MAUI Font Viewer

This example uses standard MAUI `Button` controls. The XAML sets up an empty `FlexLayout`, and the `InitAsync` method flows iconic buttons into it at runtime with the `icon-basics` as the default.

![MAUI Font Viewer](https://raw.githubusercontent.com/IVSoftware/IVSoftware.GlyphProvider/master/IVSoftware.GlyphProvider.Portable/README/img/maui-font-viewer.png)

### XAML

Picker + FlexLayout

```xaml
<ContentPage 
    xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
    xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
    x:Class="FontViewer.Maui.Demo.MainPage"
    Shell.NavBarIsVisible="False">

    <Grid RowDefinitions="Auto, *">
        <Border
            x:Name="NavBar"
            IsVisible="False"
            HeightRequest="40"
            Stroke="Black"
            StrokeThickness="1"
            Padding="1">
            <Grid
                BackgroundColor="#EEEEEE">
                <Picker
                    x:Name="ConfigPicker"
                    FontSize="12.5"/>
            </Grid>
        </Border>
        <FlexLayout
            x:Name="flexLayout"
            BackgroundColor="Azure"
            Grid.Row="1"
            Direction="Row"
            Wrap="Wrap"
            AlignContent="Start"
            Margin="5"
            Padding="5"/>
    </Grid>
</ContentPage>
```

### Code-Behind

The providers of type `GlyphProvider` are bound as the `ConfigPicker.ItemsSource`.

```
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
 ```

  Changing the picker selection repopulates the viewer by iterating the `enum`. If a std enum cannot be found for the provider, the provider's own list is iterated. Having a defined `enum` is (literally) the key to showing tool tips and respoding to clicks.

 ```
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
                    // Otherwise, the raw information can still be used to create iconic buttons. (No ToolTips or Click Action in this case.)
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
```

### `MauiProgram.cs`

Windows requires **explicit** font registration using `fonts.AddFont`, even if the `MauiFont` .ttf exists in `Resources/Fonts`. Android and iOS pick it up automatically once the build action is `MauiFont`. Here we have added both `icon-basics.ttf` and `icon-media-control.ttf`

For windows builds, a pop-up tool tip is also implemented.

```
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
```
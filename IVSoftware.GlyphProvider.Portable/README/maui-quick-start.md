# [<](../../README.md)


## MAUI Quick Start


This example uses standard MAUI `Button` controls. The XAML sets up an empty `FlexLayout`, and the `InitAsync` method flows iconic buttons into it at runtime. Data-binding scenarios (for example, binding enum values directly in XAML) are covered in the `GlyphButton` section and the platform-specific `Font Viewer` demos linked from the main README.


![MAUI Quick Start](https://raw.githubusercontent.com/IVSoftware/IVSoftware.GlyphProvider/master/IVSoftware.GlyphProvider.Portable/README/img/maui-quick-start.png)

### XAML

The page defines a single `FlexLayout` which will be populated with glyph buttons at startup.

```xml
<ContentPage 
    xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
    xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
    x:Class="QuickStart.Maui.Demo.MainPage"
    Shell.NavBarIsVisible="False">
          
    <ScrollView>
        <FlexLayout
            x:Name="flexLayout"
            BackgroundColor="Azure"
            Direction="Row"
            Wrap="Wrap"
            AlignContent="Start"
            Margin="5"
            Padding="5"/>
    </ScrollView>
</ContentPage>
```

___

### Code Behind


Using the `GlyphProvider.IconBasics` enum (included in this NuGet package) is a convenient way to demonstrate the `ToGlyph()` extension on a standard MAUI `Button`.


```csharp
using IVSoftware.Portable;
// <PackageReference Include="IVSoftware.GlyphProvider.Portable" Version="1.0.0-*" />

namespace QuickStart.Maui.Demo
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
            await GlyphProvider.WaitAsync(); // Optionally: warms the glyph lookup/cache.

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
```
___

### `MauiProgram.cs`

Windows requires **explicit** font registration using `fonts.AddFont`, even if the `MauiFont` .ttf exists in `Resources/Fonts`. Android and iOS pick it up automatically once the build action is `MauiFont`.

```
namespace QuickStart.Maui.Demo
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
                });
            return builder.Build();
        }
    }
}
```

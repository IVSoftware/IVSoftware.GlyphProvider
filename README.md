![sponsoring](https://raw.githubusercontent.com/IVSoftware/IVSoftware.GlyphProvider/master/IVSoftware.GlyphProvider.Portable/README/img/sponsoring.png)

## IVSoftware.GlyphProvider.Portable  [[GitHub](https://github.com/IVSoftware/IVSoftware.GlyphProvider.git)]

This micro  utility (<100 KB) works with custom [Fontello](https://www.fontello.com) webfont packages whether they contain a few glyphs or dozens. Each archive downloaded from Fontello comes with a `config.json` file that catalogs its glyph icons. The `GlyphProvider` class can deserialize this fully, representing each entry as a `Glyph` object. Deserializing the config isn't going to solve any great mystery on its own, but it *does* provide a solid foundation for a handful of small, practical improvements that end up earning their keep.

___

## Quick Start - `icon-basics.ttf`

 ![icon-basics glyphs](https://raw.githubusercontent.com/IVSoftware/IVSoftware.GlyphProvider/master/IVSoftware.GlyphProvider.Portable/README/img/icon-basics.png)

This package includes a small ready-made Fontello subset font. Its job is simply to give the examples something concrete to point at, though you're welcome to use it directly - it carries the same license as any Fontello font.

> _If you're new to Fontello, it helps to know that `config.json` works both ways. You can upload/import it **to** the Fontello site to kick-start a renamed font or to iterate on one you already have._
___

## Features At A Glance

The improvements in this package are made possible by one small gesture: assigning the **Build Action** property of your `config.json` file or files as **Embedded Resource**. It follows that:

1. The font configurations become discoverable and will load on demand at runtime.
2. Deserialization is only the beginning. Fontello represents glyphs as a list, but this utility organizes all such lists into a single dictionary so that the lookups become direct and share the same fast access path.
3. As a one-time setup in temporary code, the `CreateEnumPrototypes()` method produces `enum` definitions you can paste into your project. After recompiling, each font exposes a standard enum that indexes cleanly into the dictionary, usable in code-behind or *directly in XAML with intellisense*.

Once defined, `enum` members are used to call the `ToGlyph()` extension with an optional `GlyphFormat` argument. Before this takes on any hint of magic, let's take a look at a representative member of the generated `enum`. Here, the `[CssName]` attribute maps the canonical keys in the `config.json` itself. This means you can *change* `IconBasics` to some other name, *change* `EllipsisHorizontal` to some other name, and it's still going to work.


```
[CssName("icon-basics")]
public enum IconBasics
{
    [CssName("ellipsis-horizontal")]
    EllipsisHorizontal
}
```

To put a finer point on it, the glyph is indexed by the *enum* member, and the enum *type* carries the font family information as well. Together, they describe the glyph fully: family + name.

Next, a few examples to show the various ways this can be useful.
___

## Using Named Enums

This package includes an enum named GlyphProvider.IconBasics to introduce the idea.

- First, we'll look at how this makes it easy to access the font glyphs.
- Then, in the [section](#generate-named-enums) below, we'll walk through how to generate your own enum definitions using `GlyphProvider.CreateEnumPrototypes()`.

### 1. Maui Button in Code Behind

In C# code, call the extension directly on the enum value.

```
Microsoft.Maui.Controls.Button button = new()
{
    HeightRequest = 50,
    WidthRequest = 50,
    BorderColor = Color.FromArgb("#444444"),
    Margin = new Thickness(1),
    Padding = 0,
    FontSize = 18,
    FontFamily = "icon-basics",
    Text = GlyphProvider.IconBasics.Edit.ToGlyph(),
};
```
___


### 2. List Button Glyphs for use in XAML

This loop shows each enum value rendered with the `GlyphFormat.Xaml` option. 

```
var icons = Enum.GetValues<GlyphProvider.IconBasics>();
foreach (var icon in icons)
{
    Debug.WriteLine($"{icon.ToString().PadRight(20)}{icon.ToGlyph(GlyphFormat.Xaml)}");
}
```

#### Output from Debug
```
Add                 &#xE800;
Delete              &#xE801;
Edit                &#xE802;
EllipsisHorizontal  &#xE803;
EllipsisVertical    &#xE804;
Filter              &#xE805;
Menu                &#xE806;
Search              &#xE807;
Settings            &#xE808;
Checked             &#xE809;
Unchecked           &#xE80A;
Shown               &#xE80B;
Hidden              &#xE80C;
HelpCircled         &#xE80D;
HelpCircledAlt      &#xE80E;
DocEmpty            &#xE80F;
Doc                 &#xE810;
DocNew              &#xE811;
ParentPinCollapsed  &#xE812;
ParentPinExpanded   &#xE813;
ChildPinCollapsed   &#xE814;
ChildPinExpanded    &#xE815;
```

Practically speaking, a standard-issue Maui `Button` can display a glyph if two things are in place:

1. The font family (`FontFamily="icon-basics"`) is specified.
2. The escape sequence (`Text="&#xE80E;"`) is set.

Inspecting the return value in a debugger will get you the right escaped code quickly enough, but it's still a bit clumsy. A cleaner approach is to make a small `Button` subclass (the same idea works in WinForms and WPF) that understands the enum and configures itself automatically.

___

### 3. Button with Bound Glyph Property

Making a lightweight subclass eliminates both of those steps: it takes an enum value, sets the correct font family, and builds the final `Text` for you. The result is a `Button` you can bind directly from XAML without any serious contemplation of escape codes at all.

```
public class GlyphButton : Button
{
    public static readonly BindableProperty StdIconNameProperty =
        BindableProperty.Create(
            nameof(StdIconName),
            typeof(Enum),
            typeof(GlyphButton),
            default(Enum),
            propertyChanged: OnStdIconChanged);

    public Enum? StdIconName
    {
        get => (Enum?)GetValue(StdIconNameProperty);
        set => SetValue(StdIconNameProperty, value);
    }

    static void OnStdIconChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (newValue is Enum icon)
        {
            var button = (GlyphButton)bindable;
            button.FontFamily = icon.ToCssFontName();
            if (button.Text.TryUpdateIconicText(icon, out var textFTR, spacing: 4))
            {
                if (button.BindingContext?.GetType().GetProperty(nameof(Text)) is { } pi)
                {
                    // If a binding exists, don't break it - use it.
                    pi.SetValue(button.BindingContext, textFTR);
                }
                else
                {
                    button.Text = textFTR;
                }
            }
        }
    }
}
```

#### XAML

What started out as a cryptic `Text="&#xE80E"` is now an IntelliSense-backed reference to a static `Enum` value.

```
<local:GlyphButton
    x:Name="CounterBtn"
    Text="Click me" 
    StdIconName="{x:Static gp:IconBasics.HelpCircledAlt}"
    SemanticProperties.Hint="Counts the number of times you click"
    Clicked="OnCounterClicked"
    HorizontalOptions="Fill"
    Padding="0"
    FontSize="18"/>
```

___


## Font (.ttf) Files at the Platform Level


The Fontello archive also contains the `.ttf` font file itself, and platforms have varying requirements for importing it (see table below). Nothing in the preceding sections interacts with the `.ttf` file directly. But of course, none of this is actually going to *work* unless the font family is imported in the first place.

This guide walks through those prerequisites in each platform section. For now, there is one shared setup step: making sure the font files are present in your project in the right location.

To prepare for the next section, either copy the `icon-basics` folder using the instructions below, or download your own `your-icons.zip` from the Fontello website and extract it.


___

### How To Copy

1. In your project, create the `Resources\Fonts` directory if it does not already exist. Locate and open the **content** folder inside the NuGet package, and copy the entire **icon-basics** folder into `Resources\Fonts`.

![copy the icon-basics folder](https://raw.githubusercontent.com/IVSoftware/IVSoftware.GlyphProvider/master/IVSoftware.GlyphProvider.Portable/README/img/copy-icon-basics-from-content.png)

2. Open the properties of `config.json` and set its Build Action to **Embedded Resource**. (Even in WPF this should be Embedded Resource, not Resource.)

3. Open the properties of `icon-basics.ttf` and set its Build Action to:
    - **MauiFont** for .NET MAUI  
    - **Resource** for WPF  
      *(Remember that `config.json` is different. It remains an embedded resource.)*  
    - **EmbeddedResource** for WinForms


 > _WinForms note: Importing a font family requires loading it through `System.Drawing.PrivateFontCollection`. The [repo](https://github.com/IVSoftware/IVSoftware.GlyphProvider.git) shows exactly how to do this. Even better: the WinForms specific `IVSoftware.GlyphProvider.WinForms` NuGet does it *for* you._ 


These requirements are summarized in the table below:

| Platform   | Build Action | Notes |
|------------|--------------------------|-------|
| [**MAUI**](https://github.com/IVSoftware/IVSoftware.GlyphProvider/blob/master/IVSoftware.GlyphProvider.Portable/README/maui-quick-start.md)     | `MauiFont`               | In `MauiProgram.cs`, make an `AddFont` entry in the `ConfigureFonts` block following the pattern shown e.g. for `OpenSans`. |
| [**WinForms**](https://github.com/IVSoftware/IVSoftware.GlyphProvider/blob/master/IVSoftware.GlyphProvider.Portable/README/winforms-quick-start.md)  | `EmbeddedResource`       | Use the platform-specific NuGet `IVSoftware.GlyphProvider.WinForms`, which implements the required `System.Drawing.PrivateFontCollection` for custom fonts. |
| [**WPF**](https://github.com/IVSoftware/IVSoftware.GlyphProvider/blob/master/IVSoftware.GlyphProvider.Portable/README/wpf-quick-start.md)       | `Resource`               | XAML can reference `FontFamily` with `pack://application:,,,/#YourFont` syntax, or do it in code-behind as shown in the full example below. |

___


### Boosting the Cache

This platform-agnostic snippet represents a canonical flow for _any_ client - there are no platform differences as far as this utility is concerned. Left to its own devices, the cache initializes its mapping functions on the first access, but there might be a few ms of latency (just enough to make the all-important first impression a tad sluggish). To avoid this, warm up the cache using the async method shown below (there is no need to await it).


```
public partial class MainUI
{
    public MainUI()
    {
        InitializeComponent();
        _ = InitAsync();
    }

    private async Task InitAsync()
    {
        // Reduce the lazy "first time click" latency.
        await GlyphProvider.BoostCache();
    }
}
```

___

### Platform Quick Starts

These samples show just a few lines of code and employ the built-in `GlyphProvider.IconBasics` enum to address the idiosyncrasies of the main platforms.

[MAUI Quick Start](https://github.com/IVSoftware/IVSoftware.GlyphProvider/blob/master/IVSoftware.GlyphProvider.Portable/README/maui-quick-start.md) - Making sure to import the font in `MauiProgram.cs`

[WinForms Quick Start](https://github.com/IVSoftware/IVSoftware.GlyphProvider/blob/master/IVSoftware.GlyphProvider.Portable/README/winforms-quick-start.md) - Using the platform-specific NuGet to abstract `System.Drawing.PrivateFontCollection`.

[WPF Quick Start](https://github.com/IVSoftware/IVSoftware.GlyphProvider/blob/master/IVSoftware.GlyphProvider.Portable/README/wpf-quick-start.md) - How to succeed using the `pack` syntax in code-behind.

___

## Generating Named Enums

Now let's expand our repertoire by downloading a new `icon-media-control.zip` archive from Fontello. After extracting it, place the resulting folder next to `icon-basics` inside `Resources\Fonts`. Remember to set the **Build Action** of the new `config.json` to **Embedded Resource** - same as before. Importing the `.ttf` file follows the same conventions you used for `icon-basics.ttf`.

As a one-time setup, we need to run `GlyphProvider.CreateEnumPrototypes()`, which discovers all `config.json` files marked as embedded resources scoped to `AppDomain.Current`. It returns a string array where each element is an `enum` definition - one for each config. The snippet below joins them, and when `enumsGen` is shown in a text visualizer the entire block can be copy-pasted as actual code.


```
private async Task InitAsync()
{
    // Reduce the lazy "first time click" latency.
    await GlyphProvider.BoostCache();

#if DEBUG
    // Generate one enum definition per config.json discovered in the assembly.
    // Many apps have more than one font kit, and multiple bundles will produce multiple enums.
    string[] prototypes = await GlyphProvider.CreateEnumPrototypes();

    Debug.Assert(
        prototypes.Any(),
        "You should also see prototypes for any additional config.json files " +
		"that you've marked as Embedded Resource. (Note: in WPF, this must be " +
		"EmbeddedResource - not Resource - for discovery to work.)"
    );

    var enumsGen =
        string.Join(
            $"{Environment.NewLine}{Environment.NewLine}",
            prototypes);

	{ } // < Set a debug break HERE to copy the `enumsGen` from text visualizer to your code.
#endif
}
```

The block below shows a typical result as depicted by the visualizer. There will be one enum for each discovered configuration.

```
[CssName("icon-media-control")]
public enum StdIconMediaControl
{
	[CssName("stop")]
	Stop,

	[CssName("pause")]
	Pause,

	[CssName("to-end")]
	ToEnd,

	[CssName("to-end-alt")]
	ToEndAlt,

	[CssName("to-start")]
	ToStart,

	[CssName("to-start-alt")]
	ToStartAlt,

	[CssName("fast-fw")]
	FastFw,

	[CssName("fast-bw")]
	FastBw,

	[CssName("eject")]
	Eject,
}
```

After rebuild and restart, the new `enum` is fully wired in and ready to use with no additional setup. At this point, feel free to remove the temporary DEBUG section which has fulfilled its purpose.

___


### Font Viewer Demo

This set of demos brings all the pieces together:

1. Employs the same flow/flex layout scheme to display the font glyphs present in each `ttf` file.
2. Features a combo box/picker to choose which discovered font to display.
3. Handles Click/Clicked events for each button, popping up the individual json list item from the config file.
4. On windows builds, displays mouse-hover styling and a tool tip with the friendly glyph name.


[MAUI Font Viewer](https://github.com/IVSoftware/IVSoftware.GlyphProvider/blob/master/IVSoftware.GlyphProvider.Portable/README/maui-font-viewer.md)

[WinForms Font Viewer](https://github.com/IVSoftware/IVSoftware.GlyphProvider/blob/master/IVSoftware.GlyphProvider.Portable/README/winforms-font-viewer.md)

[WPF Font Viewer](https://github.com/IVSoftware/IVSoftware.GlyphProvider/blob/master/IVSoftware.GlyphProvider.Portable/README/wpf-font-viewer.md)

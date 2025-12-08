![sponsoring](https://raw.githubusercontent.com/IVSoftware/IVSoftware.Portable.GlyphProvider/master/IVSoftware.Portable.GlyphProvider/README/img/sponsoring.png)

## IVSoftware.Portable.Glyph Provider

This micro utility works with custom [Fontello](https://www.fontello.com) webfont packages whether they contain a few glyphs or dozens. The `config.json` they include is already a good index. This package builds on it - working with multiple config files, generating name-to-unicode mappings for XAML and C#, and generating `enum` structures ideal for binding glyph properties in XAML that are visible to intellisense.
___

## Quick Start

Platforms have different requirements for `.ttf` files, and these still need to be followed. This utility, however, interacts with the `config.json` not the font itself. 

1. After downloading and extracting the .zipo archive from Fontello, place your webfont folder in the appropriate folder for MAUI, WPF or WinForms, open the properties of `config.json` and set the Build Property to Embedded Resource (that is, even for WPF it should be Embedded Resource and not Resource).

2. Boosting the cache (the dictionary that maps names to glyphs) will often improve latency on the crucial first access. This can be done in an async init method and there is no need to await it.

3. In the same `async` method, you can place temporary code to generate one or more named `enum` structures - this can be pasted to the codebase to define named enums.

Once the `enum` exists in code you can use it to call extensions like `ToGlyph` in C# or XAML formats, and use the enum in XAML for bound glyph properties.

___
## Boosting the Cache

This snippet is shown in MAUI but represents a canonical flow for _any_ client - there are no platform differences as far as this utility is concerned. 

```
public partial class MainPage : ContentPage
{
    public MainPage()
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

## Generate Named Enums Using `GlyphProvider.CreateEnumPrototypes`

To make it easy to generate an `enum` from the font, just make a temporary block when your app is loading using `GlyphProvider.CreateEnumPrototypes()`. This utility will reflect any and all `config.json` files marked as embedded resources in the `AppDomain` and dump the definitions as text - one `enum` for each config - and this text can be manually be copied as actual code.

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

	// Copy the `enumsGen` from text visualizer to your code.
	// The block below gives you an idea of what to expect, but
	// isn't suitable for copying due to the escaped double quotes.

    var expected = @"
[CssName(""icon-basics"")]
public enum StdIconBasics
{
[CssName(""add"")]
Add,

[CssName(""delete"")]
Delete,

[CssName(""edit"")]
Edit,

[CssName(""ellipsis-horizontal"")]
EllipsisHorizontal,

[CssName(""ellipsis-vertical"")]
EllipsisVertical,

[CssName(""filter"")]
Filter,

[CssName(""menu"")]
Menu,

[CssName(""search"")]
Search,

[CssName(""settings"")]
Settings,

[CssName(""checked"")]
Checked,

[CssName(""unchecked"")]
Unchecked,

[CssName(""eye"")]
Eye,

[CssName(""eye-off"")]
EyeOff,

[CssName(""help-circled"")]
HelpCircled,

[CssName(""help-circled-alt"")]
HelpCircledAlt,

[CssName(""doc-empty"")]
DocEmpty,

[CssName(""doc"")]
Doc,

[CssName(""doc-new"")]
DocNew
}".Trim();

    var fontFamily = typeof(IconBasics).ToCssFontFamilyName();
#endif
}
```
___

## Introduction to Enums

Once the named `enum` is defined in C# code, us it to call the `ToGlyph()` extension. There are three return options: the raw unicode which is essentially a string value (not a `char`) like `"\uE802"`for example. The code snippet below assumes that will be an unprintable string out of context, but provides the `GlyphFormat.UnicodeDisplay` in order to get a viewable representation.

```
[TestMethod]
public void Test_IntroductionToEnums()
{
    var enumMember = GlyphProvider.IconBasics.Edit;

    string unicodeGlyph = enumMember.ToGlyph(); // Default GlyphFormat.Unicode

    Assert.AreEqual(
        "U+E802",
        enumMember.ToGlyph(GlyphFormat.UnicodeDisplay),
        "Expecting a viewable representation of the unicode glyph.");

    Assert.AreEqual(
        "&#xE802;",
        enumMember.ToGlyph(GlyphFormat.Xaml),
        "Expecting a value suitable for use in XAML");
}
```

___

## Platform Quick Starts

Although this utility has no direct interactions with the `.ttf` file itself, this section is here to ensure a smooth onboarding experience taking framework differences into account. In particular, setting the Build Action property for the `.ttf` file itself is critical, and varies slightly depending on the framework:

| Platform   | Build Action | Notes |
|------------|--------------------------|-------|
| **MAUI**     | `MauiFont`               | In `MauiProgram.cs` add make an entry in the `ConfigureFonts` block following the existing `OpenSans` pattern. |
| **WPF**      | `Resource`               | XAML can reference it with `pack://application:,,,/YourFont.ttf`. |
| **WinForms** | `EmbeddedResource`       | Requires `PrivateFontCollection` (in the BCL) as shown in the sample code below. |

___
## `Icon-Basics` Font - Works Out Of The Box

Follow the instructions found [here](https://github.com/IVSoftware/IVSoftware.Portable.GlyphProvider/blob/master/IVSoftware.Portable.GlyphProvider/README/icon-basics.md) to copy and provision this font to your Maui, WPF or WinForms project.

___
### MAUI Quick Start

[here](https://github.com/IVSoftware/IVSoftware.Portable.GlyphProvider/blob/master/IVSoftware.Portable.GlyphProvider/README/quick-start-maui.md)

___
### WinForms Quick Start

[here](https://github.com/IVSoftware/IVSoftware.Portable.GlyphProvider/blob/master/IVSoftware.Portable.GlyphProvider/README/quick-start-winforms.md)

___
### WPF Quick Start

[here](https://github.com/IVSoftware/IVSoftware.Portable.GlyphProvider/blob/master/IVSoftware.Portable.GlyphProvider/README/quick-start-wpf.md)



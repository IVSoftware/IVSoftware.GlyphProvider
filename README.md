![sponsoring](https://raw.githubusercontent.com/IVSoftware/IVSoftware.GlyphProvider/master/IVSoftware.GlyphProvider.Portable/README/img/sponsoring.png)

## IVSoftware.GlyphProvider.Portable  [[GitHub](https://github.com/IVSoftware/IVSoftware.GlyphProvider.git)]

This micro utility works with custom [Fontello](https://www.fontello.com) webfont packages whether they contain a few glyphs or dozens. Each package comes with a `config.json` file that catalogs its contents. In Visual Studio, changing the Build Action property of these `config.json` files allows this utility to generate name-to-unicode mappings for XAML and C#, and `enum` structures that can be used directly to access the data. This for example, makes them ideal for binding glyph properties in XAML that are visible to intellisense.

The Fontello archive also contains the `.ttf` font file itself and platforms have varying requirements for importing it (see table below). Generally, this utility doesn't interact with the `.ttf` file directly. The exception is WinForms, which requires the `System.Drawing.PrivateFontCollection` class in order to import the as hoc font files. When developing for WinForms, the recommended NuGet package is `IVSoftware.GlyphProvider.WinForms` which handles this for you.
___

## Quick Start - `icon-basics.ttf`

This package comes with a small custom glyph font or you can design your own on the Fontello site. Instructions for copying this folder can be found in the [IconBasics Readme](https://github.com/IVSoftware/IVSoftware.GlyphProvider/blob/master/IVSoftware.GlyphProvider.Portable/README/icon-basics.md).

1.Place the `icon-basics` webfont folder (or if you prefer, one you have custom provisioned on Fontello) in the appropriate folder for MAUI, WPF or WinForms. 

2. Open the properties of `config.json` and set the Build Action property to Embedded Resource (that is, even for WPF it should be Embedded Resource and not Resource).

3. Open the properties of `icon-basics.ttf` and set its Build Action property to:
    - **MauiFont** for .NET MAUI
    - **Resource** for WPF (remembering that `config.json` is different - it's still an *embedded* resource.)
    - **EmbeddedResource** for WinForms.

4. No initialization is required. The samples below use the `GlyphProvider.IconBasics` enum to demonstrate a simple icon viewer for each platform.

5. To improve latency on the crucial first access, boost the cache (the dictionary that maps names to glyphs) in an async init method called from the main page ctor as shown (there is no need to await this).

6. After launching the demo for your platform, the next step will be automatically generating the corresponding `enum` for new glyph fonts that are created.
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

## Introduction to Enums

When a named `enum` is defined in C# code its members can be used to call the `ToGlyph()` extension. There are three return options: the raw unicode which is essentially a string value (not a `char`) like `"\uE802"`for example. The code snippet below assumes that will be an unprintable string out of context, but provides the `GlyphFormat.UnicodeDisplay` in order to get a viewable representation.

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
| [**MAUI**](#maui-quick-start)     | `MauiFont`               | In `MauiProgram.cs` add make an entry in the `ConfigureFonts` block following the existing `OpenSans` pattern. |
| [**WinForms**](#winforms-quick-start)  | `EmbeddedResource`       | Requires `PrivateFontCollection` (in the BCL) as shown in the sample code below. |
| [**WPF**](#wpf-quick-start)       | `Resource`               | XAML can reference it with `pack://application:,,,/YourFont.ttf`. |



___
### MAUI Quick Start


___
### WinForms Quick Start


___
### WPF Quick Start


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
}
```

The block below shows what to expect in the visualizer. There will be one enum for each `config.json` file.


[CssName("icon-basics")]
public enum StdIconBasics
{
	[CssName("add")]
	Add,

	[CssName("delete")]
	Delete,

	[CssName("edit")]
	Edit,

	[CssName("ellipsis-horizontal")]
	EllipsisHorizontal,

	[CssName("ellipsis-vertical")]
	EllipsisVertical,

	[CssName("filter")]
	Filter,

	[CssName("menu")]
	Menu,

	[CssName("search")]
	Search,

	[CssName("settings")]
	Settings,

	[CssName("checked")]
	Checked,

	[CssName("unchecked")]
	Unchecked,

	[CssName("shown")]
	Shown,

	[CssName("hidden")]
	Hidden,

	[CssName("help-circled")]
	HelpCircled,

	[CssName("help-circled-alt")]
	HelpCircledAlt,

	[CssName("doc-empty")]
	DocEmpty,

	[CssName("doc")]
	Doc,

	[CssName("doc-new")]
	DocNew,

	[CssName("parent-pin-collapsed")]
	ParentPinCollapsed,

	[CssName("parent-pin-expanded")]
	ParentPinExpanded,

	[CssName("child-pin-collapsed")]
	ChildPinCollapsed,

	[CssName("child-pin-expanded")]
	ChildPinExpanded
}
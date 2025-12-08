# Glyph Provider

## Abstract  

**GlyphProvider** eliminates the need to inspect raw `config.json` files by automatically locating and mapping them. The cost of admission? Mark each `config.json` as an Embedded Resource and let discovery take it from there. From that point, CSS-style names are surfaced as strongly typed enum members, keeping raw glyph values and magic codepoints transparent to the developer.  

```csharp
[CssName("icon-basics")]
public enum IconBasics
{
    [CssName("help-circled")]
    Help,

    [CssName("help-circled-alt")]
    HelpReversed,
}
```
Typically, tools like [Fontello](https://www.fontello.com) generate font files in multiple formats plus a config.json rosetta stone that makes mapping names to codepoints straightforward. **GlyphProvider** goes further, capturing these associations as strongly typed enums. In doing so, the mapping gains assembly-specific identity and syntactic sugar, even in sticky cases where font names might otherwise collide.  

This makes for a glyph mapping scheme that is portable, peacefully coexisting with platform-specific font mechanics without ever trying to intervene in them.  

## Benefits  

You get instant gratification — syntax you can use immediately without a lot of fuss or bother.  

```csharp
myButton.Text = IconBasics.Search.ToGlyph(); // Programmatic assignment
```




### Platform Notes

Glyph identity is portable, but font rendering is still governed by each platform's rules. The public repo includes minimal reproductions for **MAUI**, **WinForms**, and **WPF** showing how GlyphProvider integrates without replacing native font mechanics:

| Platform | How fonts are handled | GlyphProvider support |
|----------|-----------------------|-----------------------|
| **WinForms** | Uses `PrivateFontCollection` with `.ttf` embedded as resources | Enums resolve glyphs by name; fonts load via embedded resources |
| **WPF** | Requires fonts marked as `Resource` in the local assembly | Debug helper `CopyEmbeddedFontsFromPackage()` makes embedded fonts usable without manual duplication |
| **MAUI** | Fonts registered via `MauiProgram` `.AddFont` (local) or `.AddEmbeddedResourceFont` (local or external) | Enums and attributes provide declarative glyph identity once the font is registered |

GlyphProvider's role to consistently dispense the "ugly" glyphs in a readable and unambiguous manner!


```
// namespace IVSGlyphProvider.Demo.Maui

// Works independently
// CounterBtn.FontFamily = typeof(IconBasics).ToCssFontFamilyName();

// Also works, but this must be aliased in Maui.AddFont
CounterBtn.FontFamily = nameof(IconBasics);

CounterBtn.Text         = IconBasics.Search.ToGlyph();
CounterBtn.WidthRequest = CounterBtn.Height;

// Alternate formats
var xaml    = IconBasics.Search.ToGlyph(GlyphFormat.Xaml);          // "&#xE807;"
var display = IconBasics.Search.ToGlyph(GlyphFormat.UnicodeDisplay); // "U+E807"

```

---

### Table of Contents

1. [Concept](#concept)  
   - [Discover and Catalog](#discover-and-catalog)  
   - [GlyphProvider with Indexing](#glyphprovider-with-indexing)  
   - [String vs. Enum keys](#string-vs-enum-keys)  
   - [Declarative Bindings using GlyphAttribute](#declarative-bindings-using-glyphattribute)  
2. [Usage](#usage)  
   - [ToGlyph extensions](#toglyph-extensions)  
   - [Return formats](#return-formats)  
   - [XAML Integration](#xaml-integration)  
3. [Why Glyph Provider (Not Font)](#why-glyph-provider-not-font)  

---

## Concept

### Discover and Catalog
Glyph families are discovered once by enumerating embedded resources across the current app domain. Each `config.json` is deserialized into a `GlyphProvider` and cached in a lookup keyed by its declared name.

### GlyphProvider with Indexing
Glyphs can be retrieved by friendly name or enum member:

```csharp
// From string key
string g1 = "basics-icons".ToGlyph("search");

// From enum member
string g2 = "basics-icons".ToGlyph(StdBasicsGlyph.Trash);
```

### String vs. Enum keys
- String keys allow quick fuzzy lookups and are useful during prototyping.  
- But string lookups are fragile in production.  

To promote safety, use `CreateEnumPrototype()` to generate a strongly typed enum scaffold directly from the font's `config.json`. This lets you replace brittle strings with a compile-time contract:

```csharp
string proto = "basics-icons".CreateEnumPrototype();
Console.WriteLine(proto);
```

Output (excerpt):

```csharp
public enum StdBasicsIcons
{
    [Description("search")]
    Search,

    [Description("trash")]
    Trash,
    ...
}
```

By pasting this enum into your project, you get the benefits of both discoverability and compile-time safety.

### Declarative Bindings using GlyphAttribute
Annotate models or properties directly:

```csharp
public class NavBar
{
    [Glyph(FontFamily = "basics-icons", Key = "search")]
    public string? SearchIcon { get; set; }

    [Glyph(FontFamily = "basics-icons", StdEnum = StdBasicsGlyph.Trash)]
    public string? DeleteIcon { get; set; }
}
```

---

## Usage

### ToGlyph extensions
Extension methods resolve glyphs from either string keys or enums.

```csharp
string u = "basics-icons".ToGlyph("filter");                   // "\uE806"
string d = "basics-icons".ToGlyph(StdBasicsGlyph.Filter);      // "\uE806"
```

### Return formats
Glyphs can be retrieved in multiple formats:

```csharp
// Unicode character
"basics-icons".ToGlyph("search", GlyphFormat.Unicode);

// Unicode display form
"basics-icons".ToGlyph("search", GlyphFormat.UnicodeDisplay); // U+E807

// XAML entity
"basics-icons".ToGlyph("search", GlyphFormat.Xaml);           // &#xE807;
```

### XAML Integration
Fonts still must be registered with the platform (MAUI, WPF, WinForms). Once imported, glyph strings returned from `ToGlyph` can be bound directly to text elements or used in control templates.

---

## Why Glyph Provider (Not Font)

Fonts are platform-specific, with their own rules for import and registration. The **Glyph Provider** does not manage those mechanics. Instead, it provides a **portable identity layer**:

- Glyphs are resolved by friendly name, not magic numbers.  
- Enums and attributes canonize those names for maintainability.  
- Applications stay declarative and testable, regardless of platform.  

By separating glyph identity from font rendering, the Glyph Provider keeps your code portable and future-proof.

---

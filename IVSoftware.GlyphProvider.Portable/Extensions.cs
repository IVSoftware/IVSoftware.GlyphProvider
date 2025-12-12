using IVSoftware.Portable.Common.Exceptions;
using System.IO;
using System.Reflection;
using System.Text;
using static System.Net.Mime.MediaTypeNames;

namespace IVSoftware.Portable
{
    public static class GlyphProviderExtensions
    {
        /// <summary>
        /// Return a glyph (or a cosmetic representation of a glyph) in the specified format.
        /// </summary>
        public static string? ToGlyph(this Enum stdGlyph, GlyphFormat format = GlyphFormat.Unicode)
            => GlyphProvider.Providers[stdGlyph.GetType()]?[stdGlyph, format];

        /// <summary>
        /// Reverse lookup the provider key.
        /// </summary>
        public static string? ToProviderKey(this Enum stdGlyph)
            => GlyphProvider.Providers[stdGlyph.GetType()]?.Key;

        /// <summary>
        /// Return the css font family name specified in the [CssNameAttribute] or
        /// fall back to the name of the enum type.
        /// </summary>
        public static string ToCssFontName(this Enum stdGlyph)
            => stdGlyph.GetType().ToCssFontName();

        /// <summary>
        /// Return the css font family name specified in the [CssNameAttribute] or
        /// fall back to the name of the enum type.
        /// </summary>
        public static string ToCssFontName(this Type enumType)
            => enumType.GetCustomAttribute<CssNameAttribute>()?.Name ?? enumType.GetType().Name.ToString();

        /// <summary>
        /// Retrieve the JSON-serialized node for this enum value in config.json.
        /// </summary>
        public static Glyph? ToGlyphInfo(this Enum stdGlyph, GlyphFormat format = GlyphFormat.Unicode)
        {
            if(GlyphProvider.Providers[stdGlyph.GetType()] is { } provider)
            {
                var key = stdGlyph.GetCssNameAttribute()?.Name ?? stdGlyph.ToString();
                var preview = provider.GlyphLookup[key];
                return preview;
            }
            return default;
        }

        /// <summary>
        /// - Retrieves a standard attribute applied to an Enum member, or null if not found.
        /// - Throws if multiple attributes of type TAttr are applied to the same Enum member.
        /// - To retrieve intentional multiple attributes, call GetOnePageAttributes() instead.
        /// </summary>
        internal static TAttr? GetCustomAttribute<TAttr>(this Enum id)
            where TAttr : Attribute
            => id
                .GetType()
                .GetField(id.ToString())
                ?.GetCustomAttributes<TAttr>()
                .SingleOrDefault();

        /// <summary>
        /// - Retrieves a standard attribute applied to an Enum member, or null if not found.
        /// - Throws if multiple attributes of type TAttr are applied to the same Enum member.
        /// - To retrieve intentional multiple attributes, call GetOnePageAttributes() instead.
        /// </summary>
        public static GlyphAttribute? GetGlyphAttribute(this Enum id)
            => id.GetCustomAttribute<GlyphAttribute>();

        /// <summary>
        /// - Retrieves a standard attribute applied to an Enum member, or null if not found.
        /// </summary>
        public static CssNameAttribute? GetCssNameAttribute(this Enum id)
            => id.GetCustomAttribute<CssNameAttribute>();

        /// <summary>
        /// - Retrieves a standard attribute applied to an Enum type, or null if not found.
        /// </summary>
        public static CssNameAttribute? GetCssNameAttribute(this Type enumType)
            => enumType.GetCustomAttribute<CssNameAttribute>();

        /// <summary>
        /// Produces a key in the form "EnumType.Member".
        /// Useful when the member value alone might be insufficiently unique.
        /// </summary>
        internal static string ToFullKey(this Enum member) =>
            $"{member.GetType().Name}.{member}";

        /// <summary>
        /// Produces a key in the form "AssemblyName.EnumType".
        /// This goes beyond using just GetType().Name:
        /// - Within an AppDomain, type names are unique, so that alone is enough
        ///   when you already hold an enum member.
        /// - But when lookup is string-driven (camel, kebab, underscore),
        ///   or the type isn't guaranteed to be an enum, the assembly name
        ///   provides a stronger anchor across packages and contexts.
        /// </summary>
        /// <remarks>
        /// - This key identifies the font family dictionary for a given enum type.
        /// - If called with an enum member, only the declaring type is used at this step.
        /// - If called with a Type directly, the result is the same, without needing a member.
        /// - Once the dictionary is located, the member itself (when present) is used as
        ///   the final key to retrieve the glyph.
        /// </remarks>
        internal static string ToGlyphProviderKey(this Enum member) =>
            member.GetType().ToGlyphProviderKey();
        internal static string ToGlyphProviderKey(this Type type) =>
            $"{type.Assembly.GetName().Name}.{type.ToCssFontName()}";

        public static string ToPascalCase(this string @this)
        {
            if (string.IsNullOrWhiteSpace(@this))
                throw new ArgumentException("Requires non-empty input", nameof(@this));

            // Split on hyphen, underscore, or whitespace
            var parts = @this.Split(new[] { '-', '_', ' ' }, StringSplitOptions.RemoveEmptyEntries);

            var sb = new StringBuilder(@this.Length);
            foreach (var part in parts)
            {
                sb.Append(char.ToUpperInvariant(part[0]));
                if (part.Length > 1)
                    sb.Append(part.Substring(1));
            }

            // Ensure identifier does not start with a digit
            if (char.IsDigit(sb[0]))
                sb.Insert(0, '_');

            return sb.ToString();
        }

        /// <summary>
        /// Return the first Embedded Resource or Resource containing all substrings and ending with ext.
        /// </summary>
        [Canonical]
        public static string[] GetResourcePaths(
            this Assembly asm,
            string[] contains,
            string? ext = null,
            StringComparison? stringComparison = null)
        {
            if (string.IsNullOrWhiteSpace(string.Join(string.Empty, contains)))
            {
                nameof(GetResourcePaths).ThrowHard<ArgumentException>(
                    $"{nameof(GetResourcePaths)} '{nameof(contains)}' provide at least one substring to match.");
            }
            stringComparison ??= StringComparison.OrdinalIgnoreCase;

            var matches = new List<string>();
            foreach (var name in 
                     asm.GetManifestResourceNames()
                     .Where(_=> 
                        contains.All(substr=>_.Contains(substr, (StringComparison)stringComparison)) ||
                        _.EndsWith(".g.resources", (StringComparison)stringComparison)))
            {
                if(name.EndsWith(".g.resources", (StringComparison)stringComparison))
                {
                    using (var stream = asm.GetManifestResourceStream(name))
                    using (var reader = stream is null ? null : new System.Resources.ResourceReader(stream))
                    {
                        if (reader is not null)
                        {
                            foreach (System.Collections.DictionaryEntry entry in reader)
                            {
                                if( entry.Key is string sub &&
                                    !string.IsNullOrWhiteSpace(sub) &&
                                    contains.All(_=>sub.Contains(_, (StringComparison)stringComparison)))
                                {
                                    matches.Add(sub);
                                }
                            }
                        }
                    }
                }
                else
                {
                    matches.Add(name);
                }
            }
            if (ext is null)
            {
                return matches.Distinct().ToArray();
            }
            else
            {
                if (!ext.StartsWith("."))
                {
                    ext = "." + ext;
                }
                return matches.Where(_=>_.EndsWith(ext, StringComparison.OrdinalIgnoreCase)).Distinct().ToArray();
            }
        }

        public static string? GetResourcePath(
            this Assembly asm,
            string resource,
            string? ext = null,
            StringComparison? stringComparison = null)
            => asm.GetResourcePaths([resource], ext, stringComparison).FirstOrDefault();

        public static string? GetResourcePath<T>(
            this string resource,
            string? ext = null,
            StringComparison? stringComparison = null)
            => typeof(T).Assembly.GetResourcePath(resource, ext);

        /// <summary>
        /// Converts a discovered WPF resource path (e.g. a TTF inside .g.resources)
        /// into a WPF-friendly FontFamily source string. This string can be used as the
        /// <paramref name="familyName"/> argument when constructing a new
        /// <see cref="System.Windows.Media.FontFamily"/>.
        ///
        /// WPF requires that embedded font families be referenced using a *folder-based*
        /// pack URI, ending with "#<family>" where <family> is the font's internal family
        /// name. The URI can be absolute (canonical form) or relative (XAML-style).
        ///
        /// Examples:
        ///    resource path:
        ///        "resources/fonts/icon-basics/font/icon-basics.ttf"
        ///
        ///    relative family name:
        ///        "./resources/fonts/icon-basics/font/#icon-basics"
        ///
        ///    absolute family name:
        ///        "pack://application:,,,/AssemblyName;component/resources/fonts/icon-basics/font/#icon-basics"
        ///
        /// <para>
        /// By default, this method derives the family name from the TTF filename
        /// (e.g. "icon-basics"). If the font's true internal family name differs,
        /// supply it explicitly using the <paramref name="internalName"/> parameter.
        /// </para>
        /// </summary>
        public static string ToWpfFamilyName<T>(
            this string resource, 
            string? internalName = null,    // If different e.g. "icon-basics" holds "icon_basics-Regular"
            bool relative = false)          // Default to abs path
        {
            var dir = Path.GetDirectoryName(resource)
                ?.Replace('\\', '/')
                ?? throw new InvalidOperationException("Invalid resource path.");

            internalName ??= Path.GetFileNameWithoutExtension(resource);

            var assemblyName = typeof(T).Assembly.GetName().Name;

            string preview;
            if (relative)
            {
                preview = 
                    $"./{dir}/#{internalName}";
            }
            else
            {
                // Build the canonical WPF pack URI
                preview = 
                    $"pack://application:,,,/{assemblyName};component/{dir}/#{internalName}";
            }
            return preview;
        }

        static readonly char MinGlyph = '\uE000'; // typical PUA start
        static readonly char MaxGlyph = '\uF8FF'; // typical PUA end
        public static bool IsGlyphChar(this char c) => 
            c >= MinGlyph && c <= MaxGlyph;
        public static bool IsGlyphChar(this char? c) =>
            c is not null && 
            ((char)c).IsGlyphChar();

        public static bool TryUpdateIconicText(this string textB4, Enum? stdIconName, out string textFTR, int spacing = 1)
        {
            var spaces = string.Join(string.Empty, Enumerable.Repeat(" ", spacing));
            char? c = textB4?.FirstOrDefault();
            if (c.IsGlyphChar())
            {
                textB4 = textB4!.Substring(1).TrimStart();
            }
            textFTR = (
                stdIconName?.ToGlyph() is { } glyph
                ? $"{glyph}{spaces}{textB4}"
                : textB4
            )!;
            return textFTR != textB4;
        }
    }
}

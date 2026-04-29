using IVSoftware.Portable.Common.Attributes;
using IVSoftware.Portable.Common.Exceptions;
using IVSoftware.Portable.Internal;
using Newtonsoft.Json;
using System.Collections;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;

namespace IVSoftware.Portable
{
    public enum GlyphFormat
    {
        Unicode,
        UnicodeDisplay,
        Xaml,
    }
    /// <summary>
    /// Extracts the friendly glyph names by discovering the 'config.json' 
    /// embedded resource corresponding to the name of glyph font ttf file.
    /// </summary>
    public partial class GlyphProvider
    {
        public GlyphProvider(Assembly asm)
        {
            _keyPrefix = asm.GetName().Name ?? $"KeyError-{Guid.NewGuid()}";
        }

        [JsonIgnore]
        public string Key => $"{_keyPrefix}.{Name}";
        readonly string _keyPrefix;


        /// <summary>
        /// Returns the glyph string in the requested GlyphFormat or
        /// the first character of the key (uppercased) if not found.
        /// </summary>
        /// <remarks>
        /// - Basically: 
        ///   We have no control over the glyph names found in the config.json file, and 
        ///   these are often in kebab format. Likewise, while we offer a utility to make
        ///   a linted enum from the config.json there is no requirement for the end user 
        ///   dev to take this approach.
        /// - So, for this to work:
        ///   Preferred: 
        ///     Make a [Description] attribute with the exact case-sensitive
        ///     css name from the config.json file. This way, the enum member
        ///     gets to be something like "Help" instead of "help-circled-alt"
        ///   Otherwise:
        ///     The StdEnum member must be a case-sensitive match, bearing
        ///     in mind that that spaces and hyphens are not allowed. Which
        ///     translates to "this almost never works".
        /// </remarks>
        [Indexer]
        public string this[Enum stdEnum, GlyphFormat format = GlyphFormat.Unicode]
        {
            get
            {
                var css = stdEnum.GetCustomAttribute<CssNameAttribute>()?.Name;
                if (css is null)
                {
                    stdEnum.ThrowSoft<CustomAttributeFormatException>($"Expecting {nameof(CssNameAttribute)} on member. Using ToString instead.");
                    css = stdEnum.ToString();
                }
                string? cMe = null;
                if(GlyphLookup.TryGetValue(css, out var glyph) && glyph is not null)
                {
                    cMe = FormatCode(glyph.Code, format);
                }
                if(cMe is null)
                {
                    stdEnum.ThrowSoft<KeyNotFoundException>();
                    cMe = stdEnum.ToString().First().ToString().ToUpper();
                }
                return cMe;
            }
        }

        /// <summary>
        /// Retrieves a glyph string in the requested GlyphFormat.
        /// </summary>
        /// <remarks>
        /// - This overload is heuristic: 
        ///   The key is normalized (camelCase, kebab, underscore) 
        ///   and matched against known glyphs and returns the first. 
        /// - If a unique match is found, returns the glyph (e.g., "\uE801" or "&#xE801;").  
        /// - If ambiguous or not found, returns the first character of the key, uppercased.
        /// </remarks>
        [Indexer]
        public string this[Assembly asm, string fuzzyKey, GlyphFormat format = GlyphFormat.Unicode]
        {
            get
            {
                if (string.IsNullOrWhiteSpace(fuzzyKey))
                    return string.Empty;

                string fallback = fuzzyKey.First().ToString().ToUpper();
                int? code = null;

                var keyParts = localNormalizeKey(fuzzyKey);

                var exactMatches = GlyphLookup.Keys
                    .Where(k => localNormalizeKey(k).SequenceEqual(keyParts, StringComparer.OrdinalIgnoreCase))
                    .ToList();

                if (exactMatches.Count == 1)
                {
                    if (GlyphLookup[exactMatches[0]] is { } glyph)
                    {
                        code = glyph.Code;
                    }
                }
                else if (exactMatches.Count > 1)
                {
                    this.ThrowSoft<InvalidOperationException>(
                        $"Ambiguous match for key '{fuzzyKey}': {string.Join(", ", exactMatches)}");
                    return fallback;
                }

                if (code == null)
                {
                    var partialMatches = GlyphLookup.Keys
                        .Where(k => localNormalizeKey(k).Intersect(keyParts, StringComparer.OrdinalIgnoreCase).Any())
                        .ToList();

                    if (partialMatches.Count == 1)
                    {
                        if (GlyphLookup[partialMatches[0]] is { } glyph)
                        {
                            code = glyph.Code;
                        }
                    }
                    else
                    {
                        if (partialMatches.Count > 1)
                            Debug.Fail($"Ambiguous partial match for key '{fuzzyKey}': {string.Join(", ", partialMatches)}");
                        return fallback;
                    }
                }
                string? cMe = null;
                cMe = FormatCode(code, format);
                cMe ??= fuzzyKey.First().ToString().ToUpper();
                return cMe;

                #region L o c a l   M e t h o d
                string[] localNormalizeKey(string input)
                {
                    if (string.IsNullOrWhiteSpace(input))
                        return Array.Empty<string>();

                    // Replace delimiters with space, then split camelCase and spaces
                    string sanitized = Regex.Replace(input, @"[-_]", " ");
                    return Regex
                        .Split(sanitized, @"(?<=[a-z0-9])(?=[A-Z])|\s+")
                        .Where(s => !string.IsNullOrWhiteSpace(s))
                        .ToArray();
                }
                #endregion
            }
        }
        private static string? FormatCode(int? code, GlyphFormat format)
        {
            if (code.HasValue)
            {
                return format switch
                {
                    GlyphFormat.Unicode => char.ConvertFromUtf32(code.Value),
                    GlyphFormat.UnicodeDisplay => $"U+{code.Value:X4}",
                    GlyphFormat.Xaml => $"&#x{code.Value:X};",
                    _ => throw new NotImplementedException($"Bad case: {format}"),
                };
            }
            else return null;
        }

        private static readonly object _lock = new object();
        public static TimeSpan BoostTimeOut { get; set; } = TimeSpan.FromSeconds(1);

        [JsonProperty]
        public List<Glyph> Glyphs { get; set; } = [];
        /// <summary>
        /// The JSON populated the Glyphs and this dict provides the indexer.
        /// </summary>


        [JsonIgnore]
        internal TolerantDictionary<string, Glyph> GlyphLookup
        {
            get
            {
                if (_glyphLookup is null)
                {
                    _glyphLookup = new();
                    foreach (var glyph in Glyphs)
                    {
#if DEBUG
                        Debug.WriteLine($"260119 {glyph.Css}");
#endif
                        _glyphLookup[glyph.Css] = glyph; 
                    }
                }
                return _glyphLookup;
            }
        }
        TolerantDictionary<string, Glyph>? _glyphLookup = null;

        #region J S O N    P R O P E R T I E S

        enum DefaultId {  Create };
        static uint _autoIdCount = 0;
        static DefaultId getAutoId() => (DefaultId)(++_autoIdCount);
        public string? Name { get; set; }
        public string? CssPrefixText { get; set; }
        public bool CssUseSuffix { get; set; }
        public bool Hinting { get; set; }
        public int UnitsPerEm { get; set; }
        public int Ascent { get; set; }

        #endregion  J S O N    P R O P E R T I E S

        internal string CreateEnumPrototype()
        {
            List<string>
                members = new(),
                builder = new();
            foreach (var name in GlyphLookup.Keys)
            {
                // Only allow glyph names that can be linted into valid C# identifiers.
                // Regex: letters, digits, underscore, and hyphen (lint strips hyphen/underscore).
                if (!Regex.IsMatch(name, @"^[A-Za-z0-9\-_]+$"))
                {
                    continue;
                }
                members.Add($"\t[CssName(\"{name}\")]\n\t{localLintTerm(name)}");
            }
            builder.Clear();

            builder.Add($"[CssName(\"{Name}\")]");
            var enumType = localLintTerm(string.IsNullOrWhiteSpace(Name) ? "Prototype" : Name);

            if (enumType.Contains("glyph", StringComparison.InvariantCultureIgnoreCase) ||
               enumType.Contains("icon", StringComparison.InvariantCultureIgnoreCase))
            {
                builder.Add($"public enum Std{enumType}");
            }
            else
            {
                builder.Add($"public enum Std{enumType}Glyph");
            }
            builder.Add($"{{");
            builder.Add(string.Join($",\n\n", members));
            builder.Add($"}}");
            var joined = string.Join("\n", builder);
            return joined;

            string localLintTerm(string expr)
            {
                if (string.IsNullOrWhiteSpace(expr)) throw new InvalidOperationException("Requires non-empty term");

                var parts = expr.Split(new[] { '-', '_' }, StringSplitOptions.RemoveEmptyEntries);
                var aspirant = string.Concat(parts.Select(p => char.ToUpperInvariant(p[0]) + p.Substring(1)));

                // Ensure identifier does not start with a digit
                if (char.IsDigit(aspirant[0]))
                {
                    aspirant = "_" + aspirant;
                }
                return aspirant;
            }
        }

        /// <summary>
        /// Kick off a background task that preloads all GlyphProvider caches.
        /// Safe to call multiple times; subsequent calls are ignored.
        /// </summary>
        public static async Task BoostCache()
        {
            await Task.Run(() => _ = AppDomainAssemblyCache);
            await _ready.Task.WaitAsync(TimeSpan.FromSeconds(1));
        }

        public static async Task WaitAsync()
        {
            var readyB4 = _ready;
            restarted:
            _ = Task.Run(() => _ = AppDomainAssemblyCache);
            await _ready.Task.WaitAsync(TimeSpan.FromSeconds(1000));

            if(!ReferenceEquals(readyB4, _ready))
            {
                goto restarted;
            }
        }
        public static ReadOnlyCollection<Assembly> AppDomainAssemblyCache
        {
            get
            {
                if (_appDomainAssemblyCache is null)
                {
                    _appDomainAssemblyCache = new();
                    foreach(var asm in AppDomain.CurrentDomain
                        .GetAssemblies()
                        .Where(_ => AllowASM(_)))
                    {
                        if(_appDomainAssemblyCache.Add(asm))
                        {
                            EnumerateASM(asm);
                        }
                    }
                    AppDomain.CurrentDomain.AssemblyLoad += (sender, e) =>
                    {
                        if (AllowASM(e.LoadedAssembly) && _appDomainAssemblyCache.Add(e.LoadedAssembly))
                        {
                            EnumerateASM(e.LoadedAssembly);
                        }
                    };
                    _ready.SetResult();
                }
                return new(_appDomainAssemblyCache.ToList());
            }
        }
        static HashSet<Assembly> _appDomainAssemblyCache = null!;
        static TaskCompletionSource _ready = new TaskCompletionSource();

        internal static bool AllowASM(Assembly asm)
        {
            if (string.IsNullOrWhiteSpace(asm.Location))
            {
                return false;
            }
            else
            {
                if (ExcludedPrefixes.Any(_ => asm.GetName().Name?.StartsWith(_) == true))
                {
                    ExcludedLocations.Add(asm.Location);
                    return false;
                }
                else
                {
                    AllowedLocations.Add(asm.Location);
                    return true;
                }
            }
        }
        public static string[] ExcludedPrefixes
        {
            get => _excludedPrefixes;
            set
            {
                if (!Equals(_excludedPrefixes, value))
                {
                    _excludedPrefixes = value;
                }
            }
        }
        static string[] _excludedPrefixes =
        {
            "Accessibility.",
            "Microsoft.",
            "Newtonsoft.",
            "SQLite.",
            "System.",
        };

        internal static List<string> AllowedLocations { get; } = new List<string>();
        internal static List<string> ExcludedLocations { get; } = new List<string>();
        internal static void EnumerateASM(Assembly asm)
        {
            var cMe1 = asm.GetName().Name;
            var cMe2 = asm.GetManifestResourceNames();
            bool foundConfig = false;
            foreach (var resourcePath in cMe2)
            {
                if (resourcePath.EndsWith("config.json", StringComparison.InvariantCultureIgnoreCase))
                {
                    using var stream = asm.GetManifestResourceStream(resourcePath) ?? throw new Exception();
                    using var reader = new StreamReader(stream);
                    var json = reader.ReadToEnd();
                    var glyphProvider = new GlyphProvider(asm);
                    JsonConvert.PopulateObject(json, glyphProvider);
                    if (string.IsNullOrWhiteSpace(glyphProvider.Name))
                    {
                        json.ThrowSoft<JsonException>("Defective config file.");
                    }
                    Providers[glyphProvider.Key] = glyphProvider;
                    foundConfig = true;
                }
            }
            // Only if config is found do we enumerate the Enums.
            if (foundConfig)
            {
                foreach (var type in asm.ExportedTypes.Where(_ => _.IsEnum)) 
                {
                    if (type.GetCustomAttribute<CssNameAttribute>() is { } attr)
                    {
                        var key = $"{asm.GetName().Name}.{attr.Name}";
                        if(Providers[key] is { } provider)
                        {
                            provider.StdIconEnumType = type;
                        }
                        else
                        {   /* G T K */
                            // This just means that the config doesn't have a corresponding enum.
                        }
                    }
                }
            }
        }
#if false        
        public static ReadOnlyDictionary<string, GlyphProvider> FontFamilies
            => new ReadOnlyDictionary<string, GlyphProvider>(FontFamilyLookupProvider.GetFontFamilies());
#endif
        [JsonDictionary]
        [DebuggerDisplay("Count={Count}")]
        public class GlyphProviderDictionary : IReadOnlyDictionary<string, GlyphProvider>
        {
            readonly TolerantDictionary<string, GlyphProvider> _base = new();

            public int Count => _base.AsReadOnly.Count;
            public IEnumerable<string> Keys => _base.AsReadOnly.Keys;
            public IEnumerable<GlyphProvider> Values => _base.AsReadOnly.Values;

            [Indexer]
            public GlyphProvider? this[string key]
            {
                get => _base[key];
                set => _base[key] = value;
            }

            public bool ContainsKey(string key) => _base.AsReadOnly.ContainsKey(key);

            public bool TryGetValue(string key, out GlyphProvider value)
            {
                if (_base.TryGetValue(key, out var preview) && preview is not null)
                {
                    value = preview;
                    return true;
                }
                value = null!;
                return false;
            }

            [Indexer]
            public GlyphProvider? this[Type stdEnumType] => this[stdEnumType, @throw: false];

            [Indexer]
            public GlyphProvider? this[Type stdEnumType, bool? @throw]
            {
                get
                {
                    // Best case is that the cache has been boosted
                    // beforehand, but if we lose the race then
                    // perform the enumeration synchronously now.
                    _ = AppDomainAssemblyCache;

                    GlyphProvider? preview = null;
                    if (stdEnumType.IsEnum)
                    {
                        var key = stdEnumType.ToGlyphProviderKey();

                        // if(key == "IVSoftware.GlyphProvider.Portable.IconBasics")
                        // {...} Depending on how the font is imported.

                        if (key == "IVSoftware.GlyphProvider.Portable.icon-basics")
                        {
                            preview = IconBasicsProvider;
                        }
                        else
                        {
                            preview = _base[key];
                            if (preview is null)
                            {
                                // This is a helpful reminder for both IFDs and EUDs!
                                // The font, and the indexing of the font's glyphs, are
                                // separate concerns. But as far as indexing is concerned:
                                // - If the enum is in one assy and the config.json is in
                                //   another, then indexing won't work. 
                                // - This is the intended design and avoids naming collisions
                                //   that might otherwise occur.
                                // - This isn't to be confused with the font itself, which
                                //   follows normal platform rules for importing.
                                var msg = "ADVISORY: The enum 'key' and the config.json file MUST reside in the same assembly.";
                                switch (@throw)
                                {
                                    case null:
                                        // Explicitly disable the System.Diagnostics.Debug break
                                        break;
                                    // [Default]
                                    case false:
                                        // System.Diagnostics.Debug break + advisory client throw + option to Escalate.
                                        Debug.Fail(msg);
                                        this.ThrowSoft<KeyNotFoundException>(msg, @throw: @throw);
                                        break;
                                    case true:
                                        // System.Diagnostics.Debug break + hard client throw + option to Handle.
                                        Debug.Fail(msg);
                                        this.ThrowHard<KeyNotFoundException>(msg, @throw: @throw);
                                        break;
                                }
                            }
                            if (preview is null)
                            {
                            }
                        }
                        return preview;
                    }
                    return preview;
                }
            }

            internal event EventHandler<CollectionChangingEventArgs>? CollectionChanging
            {
                add => _base.CollectionChanging += value;
                remove => _base.CollectionChanging -= value;
            }

            public IEnumerator<KeyValuePair<string, GlyphProvider>> GetEnumerator()
                => _base.AsReadOnly.GetEnumerator();

            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        }
        public static GlyphProviderDictionary Providers
        {
            get
            {
                if (_providers is null)
                {
                    _providers = new GlyphProviderDictionary();
                    _providers.CollectionChanging += (sender, e) =>
                    {
#if DEBUG && false
                        var msg = $"260122.A: {e.Action} {JsonConvert.SerializeObject(e.NewItems, Formatting.Indented)}";
                        Debug.WriteLine(msg);
                        _providers.Advisory(msg);
#endif
                    };
                }
                return _providers;
            }
        }
        static GlyphProviderDictionary? _providers = null;

        private static GlyphProvider IconBasicsProvider
        {
            get
            {
                if (_iconBasicsProvider is null)
                {
                    var asm = typeof(GlyphProvider).Assembly;

                    var names = asm.GetManifestResourceNames();
                    { }
                    _iconBasicsProvider = new GlyphProvider(asm);

                    using var stream =
                        asm
                        .GetManifestResourceStream("IVSoftware.Portable.config.icon-basics.json");
                    if (stream is null)
                    {
                        typeof(GlyphProvider)
                            .ThrowFramework<NullReferenceException>(
                            $"Expecting resource `config.icon-basics.json` is found in {asm.GetName().Name}");
                    }
                    else
                    {
                        using var reader = new StreamReader(stream);
                        var json = reader.ReadToEnd();
                        var glyphProvider = new GlyphProvider(asm);
                        JsonConvert.PopulateObject(json, glyphProvider);
                        if (string.IsNullOrWhiteSpace(glyphProvider.Name))
                        {
                            json.ThrowSoft<JsonException>("Defective config file.");
                        }
                        _iconBasicsProvider = glyphProvider;
                        _iconBasicsProvider.StdIconEnumType = typeof(IconBasics);
                    }
                }
                return _iconBasicsProvider;
            }
        }
        static GlyphProvider? _iconBasicsProvider = null;

        [JsonIgnore]
        public Type? StdIconEnumType { get; private set; }
        public static class Wpf
        {
            public static Uri BaseUri { get; } = new("pack://application:,,,/");
        }


        /// <summary>
        /// Enumerate att config.json embedded resources in assembly.
        /// </summary>
        public static async Task<string[]> CreateEnumPrototypes()
        {
            var builder = new List<string>();
            await BoostCache();
            foreach (var glyphProvider in Providers.Values.OfType<GlyphProvider>())
            {
                builder.Add(glyphProvider.CreateEnumPrototype());
            }
            return builder.ToArray();
        }
        public override string ToString() => Key;
    }
}

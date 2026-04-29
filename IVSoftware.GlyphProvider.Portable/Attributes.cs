using System.ComponentModel;
using System.Reflection;

namespace IVSoftware.Portable
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public class GlyphAttribute : Attribute
    {
        public GlyphAttribute() { }
        public GlyphAttribute(string key) => Key = key;
        
        /// <summary>
        /// Use {Type}{Member} to access.
        /// </summary>
        /// <remarks>
        /// An attribute constructor can have a *specific* enum type,
        /// but it's not legal to have an arg of class Enum.So, to 
        /// generalize it, this might be a tad clumsy but it works.
        /// </remarks>
        public GlyphAttribute(Type stdEnumType, string key)
        {
            if (!stdEnumType.IsEnum)
                throw new ArgumentException("Type must be an enum.", nameof(stdEnumType));

            var value = Enum.Parse(stdEnumType, key);
            StdEnum = (Enum)value;
        }
        public string FontFamily
        {
            get =>
                string.IsNullOrEmpty(_fontFamily)
                ? "basics-icons"
                : _fontFamily;
            set
            {
                if (!Equals(_fontFamily, value))
                {
                    _fontFamily = value;
                }
            }
        }
        string _fontFamily = string.Empty;

        public string Key
        {
            get =>
                string.IsNullOrEmpty(_key)
                ? "help-circled-alt"
                : _key;
            set
            {
                if (!Equals(_key, value))
                {
                    _key = value;
                    _stdEnum = null;
                }
            }
        }
        string _key = string.Empty;

        public Enum? StdEnum
        {
            get => _stdEnum;
            private set
            {
                if (!Equals(_stdEnum, value))
                {
                    if (value is not null)
                    {
                        _key =
                            value.GetCustomAttribute<CssNameAttribute>()
                            ?.Name
                            ?? value.ToString();
                        if( value.GetType().GetCustomAttribute<CssNameAttribute>()?.Name is { } cssName &&
                            !string.IsNullOrWhiteSpace(cssName))
                        {
                            _fontFamily = cssName;
                        }
                    }
                    _stdEnum = value;
                }
            }
        }
        Enum? _stdEnum = default;
    }

    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Enum)]
    public class CssNameAttribute : Attribute
    {
        public CssNameAttribute(string name)
        {
            Name = name;
        }
        public string Name { get; }
    }


    [AttributeUsage(AttributeTargets.Property)]
    public class AliasAttribute : Attribute
    {
        public AliasAttribute(string alias, params string[] more)
        {
            var all = new List<string>();

            foreach (var term in new[] { alias }.Concat(more))
            {
                if (string.IsNullOrWhiteSpace(term)) continue;

                all.AddRange(
                    term.Split(',')
                        .Select(_ => _.Trim())
                        .Where(_ => _.Length > 0));
            }

            Aliases = all.Distinct().ToArray();
        }

        public string[] Aliases { get; }
    }

    /// <summary>
    /// Options for LayoutOptionAttribute
    /// </summary>
    [Flags]
    public enum LayoutOptionFlag
    {
        /// <summary>
        /// Typical for command bars consisting of iconized buttons.
        /// </summary>
        Horizontal = 0x0,

        /// <summary>
        /// Typical for modal command bar stacks with vertical GlyphButtons showing Text.
        /// </summary>
        Vertical = 0x1,

        /// <summary>
        /// Default button style showing glyph only.
        /// </summary>
        /// <remarks>
        /// This style implies a button where WidthTracksHeight.
        /// - Example
        /// [🔍]
        /// </remarks>
        Glyph = 0x2,

        /// <summary>
        /// Modal stack button style showing text only.
        /// </summary>
        /// <remarks>
        /// This style implies a button with a letterbox aspect.
        /// - Example
        /// [Search]
        /// </remarks>
        Text = 0x4,

        /// <summary>
        /// This style implies a combined view.
        /// - Example
        /// [🔍 Search]
        /// </summary>
        GlyphAndText = Glyph | Text,
    }

    /// <summary>
    /// Intended for enum types that define configuration groups.
    /// </summary>
    /// <remarks>
    /// - Typically, each *member* will have a [Glyph] attribute in this case.
    /// - However. Vertical stacks often present GlyphButtons with Text instead of Icons.
    /// - Decorating the enum group can avoid font family searches on non-existent types,
    ///   while still guarding non-explicit types with warnings for assembly consistency.
    /// </remarks>
    [AttributeUsage(AttributeTargets.Enum, AllowMultiple = false)]
    public class LayoutOptionsAttribute : Attribute
    {
        public LayoutOptionsAttribute(LayoutOptionFlag options)
        {
            Options = options;
        }
        public LayoutOptionFlag Options { get; } = LayoutOptionFlag.Horizontal | LayoutOptionFlag.Glyph;
    }
}

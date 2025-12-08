using System.ComponentModel;
using System.Reflection;

namespace IVSoftware.Portable
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public class GlyphAttribute : Attribute
    {
        public GlyphAttribute() { }
        public GlyphAttribute(string key) => Key = key;
        
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
}

using IVSoftware.WinOS.MSTest.Extensions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using System.Diagnostics;

namespace IVSoftware.Portable.MSTest
{
    [TestClass]
    public sealed class TestClass_GlyphProvider
    {
        [TestMethod]
        public async Task Test_EnumGen()
        {
            string actual, expected;

            await GlyphProvider.BoostCache();

            actual = JsonConvert.SerializeObject(GlyphProvider.Providers, Formatting.Indented);
            actual.ToClipboardExpected();
            { }
            expected = @" 
{
  ""IVSoftware.Portable.GlyphProvider.MSTest.IconBasics"": {
    ""Glyphs"": [
      {
        ""Uid"": ""0677f879e75956571d8cbbb478487c47"",
        ""Css"": ""add"",
        ""Code"": 59392,
        ""Src"": ""typicons"",
        ""Selected"": null,
        ""Svg"": {
          ""Path"": """",
          ""Width"": 0
        },
        ""Search"": []
      },
      {
        ""Uid"": ""f48ae54adfb27d8ada53d0fd9e34ee10"",
        ""Css"": ""delete"",
        ""Code"": 59393,
        ""Src"": ""fontawesome"",
        ""Selected"": null,
        ""Svg"": {
          ""Path"": """",
          ""Width"": 0
        },
        ""Search"": []
      },
      {
        ""Uid"": ""62b0580ee8edc3a3edfbf68a47c852d5"",
        ""Css"": ""edit"",
        ""Code"": 59394,
        ""Src"": ""elusive"",
        ""Selected"": null,
        ""Svg"": {
          ""Path"": """",
          ""Width"": 0
        },
        ""Search"": []
      },
      {
        ""Uid"": ""107ce08c7231097c7447d8f4d059b55f"",
        ""Css"": ""ellipsis-horizontal"",
        ""Code"": 59395,
        ""Src"": ""fontawesome"",
        ""Selected"": null,
        ""Svg"": {
          ""Path"": """",
          ""Width"": 0
        },
        ""Search"": []
      },
      {
        ""Uid"": ""750058837a91edae64b03d60fc7e81a7"",
        ""Css"": ""ellipsis-vertical"",
        ""Code"": 59396,
        ""Src"": ""fontawesome"",
        ""Selected"": null,
        ""Svg"": {
          ""Path"": """",
          ""Width"": 0
        },
        ""Search"": []
      },
      {
        ""Uid"": ""4109c474ff99cad28fd5a2c38af2ec6f"",
        ""Css"": ""filter"",
        ""Code"": 59397,
        ""Src"": ""fontawesome"",
        ""Selected"": null,
        ""Svg"": {
          ""Path"": """",
          ""Width"": 0
        },
        ""Search"": []
      },
      {
        ""Uid"": ""559647a6f430b3aeadbecd67194451dd"",
        ""Css"": ""menu"",
        ""Code"": 59398,
        ""Src"": ""fontawesome"",
        ""Selected"": null,
        ""Svg"": {
          ""Path"": """",
          ""Width"": 0
        },
        ""Search"": []
      },
      {
        ""Uid"": ""9dd9e835aebe1060ba7190ad2b2ed951"",
        ""Css"": ""search"",
        ""Code"": 59399,
        ""Src"": ""fontawesome"",
        ""Selected"": null,
        ""Svg"": {
          ""Path"": """",
          ""Width"": 0
        },
        ""Search"": []
      },
      {
        ""Uid"": ""e99461abfef3923546da8d745372c995"",
        ""Css"": ""settings"",
        ""Code"": 59400,
        ""Src"": ""fontawesome"",
        ""Selected"": null,
        ""Svg"": {
          ""Path"": """",
          ""Width"": 0
        },
        ""Search"": []
      },
      {
        ""Uid"": ""dd6c6b221a1088ff8a9b9cd32d0b3dd5"",
        ""Css"": ""checked"",
        ""Code"": 59401,
        ""Src"": ""fontawesome"",
        ""Selected"": null,
        ""Svg"": {
          ""Path"": """",
          ""Width"": 0
        },
        ""Search"": []
      },
      {
        ""Uid"": ""4b900d04e8ab8c82f080c1cfbac5772c"",
        ""Css"": ""unchecked"",
        ""Code"": 59402,
        ""Src"": ""fontawesome"",
        ""Selected"": null,
        ""Svg"": {
          ""Path"": """",
          ""Width"": 0
        },
        ""Search"": []
      },
      {
        ""Uid"": ""e45a3da2ebde8bc8e30a873f3bd51f30"",
        ""Css"": ""eye"",
        ""Code"": 59403,
        ""Src"": ""elusive"",
        ""Selected"": null,
        ""Svg"": {
          ""Path"": """",
          ""Width"": 0
        },
        ""Search"": []
      },
      {
        ""Uid"": ""d218294e6f9f7191f6b0b3d1ff6239ff"",
        ""Css"": ""eye-off"",
        ""Code"": 59404,
        ""Src"": ""elusive"",
        ""Selected"": null,
        ""Svg"": {
          ""Path"": """",
          ""Width"": 0
        },
        ""Search"": []
      },
      {
        ""Uid"": ""a3d734a5b4bec33fc3aa459d82092b23"",
        ""Css"": ""help-circled"",
        ""Code"": 59405,
        ""Src"": ""mfglabs"",
        ""Selected"": null,
        ""Svg"": {
          ""Path"": """",
          ""Width"": 0
        },
        ""Search"": []
      },
      {
        ""Uid"": ""3e02a8849305ac80a0e36302f461f265"",
        ""Css"": ""help-circled-alt"",
        ""Code"": 59406,
        ""Src"": ""mfglabs"",
        ""Selected"": null,
        ""Svg"": {
          ""Path"": """",
          ""Width"": 0
        },
        ""Search"": []
      },
      {
        ""Uid"": ""7141927f949e757c7e218cf70d9dceb4"",
        ""Css"": ""doc-empty"",
        ""Code"": 59407,
        ""Src"": ""mfglabs"",
        ""Selected"": null,
        ""Svg"": {
          ""Path"": """",
          ""Width"": 0
        },
        ""Search"": []
      },
      {
        ""Uid"": ""f978da58836f23373882916f05fb70b4"",
        ""Css"": ""doc"",
        ""Code"": 59408,
        ""Src"": ""linecons"",
        ""Selected"": null,
        ""Svg"": {
          ""Path"": """",
          ""Width"": 0
        },
        ""Search"": []
      },
      {
        ""Uid"": ""9e0404ba55575a540164db9a5ad511df"",
        ""Css"": ""doc-new"",
        ""Code"": 59409,
        ""Src"": ""elusive"",
        ""Selected"": null,
        ""Svg"": {
          ""Path"": """",
          ""Width"": 0
        },
        ""Search"": []
      },
      {
        ""Uid"": ""735cea31a94ec284285c15ebc45ecfc8"",
        ""Css"": ""https://fontello.com/"",
        ""Code"": 59393,
        ""Src"": ""custom_icons"",
        ""Selected"": false,
        ""Svg"": {
          ""Path"": ""M375.2 250H375V253.9 291.7 333.3 666.7 708.3 746.1 750H375.2C377.3 819.4 434 875 503.9 875H625V802.1 541.7 458.3 218.8 125H503.9C434 125 377.3 180.6 375.2 250ZM0 367.4V632.6C0 697.4 52.6 750 117.4 750H312.5V666.7 333.3 250H117.4C52.6 250 0 302.6 0 367.4ZM958.3 458.3H687.5V541.7H958.3C981.3 541.7 1000 523 1000 500 1000 477 981.3 458.3 958.3 458.3Z"",
          ""Width"": 1000
        },
        ""Search"": [
          ""parent_pin_collapsed""
        ]
      }
    ],
    ""Name"": ""icon-basics"",
    ""CssPrefixText"": null,
    ""CssUseSuffix"": false,
    ""Hinting"": true,
    ""UnitsPerEm"": 0,
    ""Ascent"": 850
  }
}"
            ;

            Assert.AreEqual(
                expected.NormalizeResult(),
                actual.NormalizeResult(),
                "Expecting initialized GlyphProvider."
            );

            // Generate one enum definition per config.json discovered in the assembly.
            // Many apps have more than one font kit, and multiple bundles will produce multiple enums.
            string[] prototypes = await GlyphProvider.CreateEnumPrototypes();

            Debug.Assert(
                prototypes.Any(),
                "You should also see prototypes for any additional config.json files " +
                "that you've marked as Embedded Resource. (Note: in WPF, this must be " +
                "EmbeddedResource — not Resource — for discovery to work.)"
           );

            var enumsGen =
                string.Join(
                    $"{Environment.NewLine}{Environment.NewLine}",
                    prototypes);

            actual = enumsGen;
            actual.ToClipboardExpected();
            { }
            expected = @" 
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
}";

            Assert.AreEqual(
                expected.NormalizeResult(),
                actual.NormalizeResult(),
                "Expecting result to match."
            );
        }

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
    }
}

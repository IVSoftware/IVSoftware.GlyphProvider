using IVSoftware.Portable;
using IVSoftware.WinForms;

namespace QuickStart.WinForms.Demo
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();
            _ = InitAsync();
        }
        async Task InitAsync()
        {
            await GlyphProvider.WaitAsync();

            // Retrieve FontFamily from System.Drawing.PrivateFontCollection (in WinForms NuGet)
            if (GlyphProvider.Providers[typeof(GlyphProvider.IconBasics)] is GlyphProvider provider &&
                provider.GetFontFamily() is FontFamily fontFamily)
            {
                var font = new Font(fontFamily, 12.5F);

                foreach (var icon in Enum.GetValues<GlyphProvider.IconBasics>())
                {
                    var button = new Button
                    {
                        Height = 50,
                        Width = 50,
                        Margin = new Padding(1),
                        Padding = new(),

                        Font = font,
                        Text = icon.ToGlyph(),
                        // ------------------------------------
                        // Required for any label or button
                        // that renders a glyph in WinForms.
                        UseCompatibleTextRendering = true,
                        // ------------------------------------
                    };
                    flowLayoutPanel.Controls.Add(button);
                }
            }
        }
    }
}

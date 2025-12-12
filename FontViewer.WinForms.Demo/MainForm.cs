using IVSoftware.Portable;
using IVSoftware.WinForms;

namespace FontViewer.WinForms.Demo
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

            if (GlyphProvider.Providers[typeof(GlyphProvider.IconBasics)] is GlyphProvider provider &&
                provider.GetFontFamily() is FontFamily fontFamily)
            {
                var font = new Font(fontFamily, 12.5F);
                foreach (var icon in Enum.GetValues<GlyphProvider.IconBasics>())
                {
                    flowLayoutPanel.Controls.Add(new GlyphButton { Font = font, StdIconName = icon});
                }
            }

            string[] prototypes = await GlyphProvider.CreateEnumPrototypes();
            var enumGen = string.Join(Environment.NewLine, prototypes);
            { }

            // GlyphProvider doesn't enumerate its own provider for GlyphProvider.IconBasics unless
            // requested. This is so that EUD's config space isn't polluted with an unwanted default.
            var iconBasicsProvider = GlyphProvider.Providers[typeof(GlyphProvider.IconBasics)];
            foreach(
                var item in 
                GlyphProvider.Providers.Values
                .Concat([iconBasicsProvider]).OfType<GlyphProvider>().Distinct())
            {
                comboBoxConfig.Items.Add(item);
            }
            comboBoxConfig.SelectedIndex = comboBoxConfig.Items.IndexOf(iconBasicsProvider);
            comboBoxConfig.SelectedIndexChanged += OnConfigSelected;
            comboBoxConfig.Visible = comboBoxConfig.Items.Count > 1;
        }

        private void OnConfigSelected(object? sender, EventArgs e)
        {
            if (comboBoxConfig.SelectedItem is GlyphProvider provider && provider.GetFontFamily() is { } fontFamily)
            {
                var font = new Font(fontFamily, 12.5F);
                flowLayoutPanel.Controls.Clear();
                if (provider.StdIconEnumType is { } stdIconType)
                {
                    // If an enum type has been defined for provider, use it.
                    foreach (Enum icon in Enum.GetValues(stdIconType))
                    {
                        flowLayoutPanel.Controls.Add(new GlyphButton { Font = font, StdIconName = icon });
                    }
                }
                else
                {
                    // Otherwise, the raw information can still be used to greate iconic buttons.
                    foreach (var info in provider.Glyphs)
                    {
                        flowLayoutPanel.Controls.Add(
                            new GlyphButton
                            {
                                Font = font,
                                Text = char.ConvertFromUtf32(info.Code).ToString()
                            });
                    }
                }
            }
        }
    }
}

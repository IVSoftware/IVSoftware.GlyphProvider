using IVSoftware.Portable;
using IVSoftware.WinOS;
using Newtonsoft.Json;
using System.ComponentModel;

namespace WinformsFontViewerDemo
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

    /// <summary>
    /// The easy way is to subclass Button, giving it
    /// a first-class Enum property for str icon values.
    /// </summary>
    class GlyphButton : Button
    {
        public GlyphButton()
        {
            Height = 50;
            Width = 50;
            Margin = new Padding(1);
            Padding = new();
            // ------------------------------------
            // Required for any label or button
            // that renders a glyph in WinForms.
            UseCompatibleTextRendering = true;
            // ---------------------------------===
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Enum? StdIconName
        {
            get => _stdIconName;
            set
            {
                if (!Equals(_stdIconName, value))
                {
                    _stdIconName = value;
                    if (_stdIconName is Enum icon)
                    {
                        Text = _stdIconName?.ToGlyph() ?? string.Empty;
                    }
                }
            }
        }
        Enum? _stdIconName = default;
        protected override void OnMouseEnter(EventArgs e)
        {
            base.OnMouseEnter(e);
            ForeColor = Color.Aqua;
        }
        protected override void OnMouseHover(EventArgs e)
        {
            base.OnMouseHover(e);
            if (StdIconName is Enum icon)
            {
                Point offset = new Point(25, -25);
                _tooltip.Show(icon.ToString(), this, offset, 4000);
            }
        }
        protected override void OnMouseLeave(EventArgs e)
        {
            base.OnMouseLeave(e);
            _tooltip.Hide(this);
            ForeColor = SystemColors.ControlText;
        }
        protected override void OnMouseClick(MouseEventArgs e)
        {
            base.OnMouseClick(e);
            if (StdIconName?.ToGlyphInfo() is { } info)
            {
                var json = JsonConvert.SerializeObject(info, Formatting.Indented);
                MessageBox.Show(json);
            }
        }
        static ToolTip _tooltip = new ToolTip
        {
            AutoPopDelay = 5000,
            InitialDelay = 500,
            ReshowDelay = 100,
            ShowAlways = true,
            OwnerDraw = false // set true if you want custom drawing later
        };
    }
}

using IVSoftware.Portable;
using IVSoftware.WinOS;
using Newtonsoft.Json;
using System.ComponentModel;
using System.Xml.Linq;

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
                provider.LoadEmbeddedFont() is FontFamily fontFamily)
            {
                IconBasics = new Font(fontFamily, 12.5F);
                var padding = new Padding(1, 1, 1, 1);
                foreach (var icon in Enum.GetValues<GlyphProvider.IconBasics>())
                {
                    var button = new Button
                    {
                        TextAlign = ContentAlignment.MiddleCenter,
                        Size = new(50, 50),
                        Margin = padding,
                        Font = IconBasics,
                        Tag = icon,
                    };

                    // ------------------------------------
                    // Required for any label or button
                    // that renders a glyph in WinForms.
                    button.UseCompatibleTextRendering = true;
                    // ---------------------------------===

                    button.Text = icon.ToGlyph();
                    button.MouseEnter += Any_MouseEnter;
                    button.MouseHover += Any_MouseHover;
                    button.MouseLeave += Any_MouseLeave;
                    button.Click += Any_Click;
                    flowLayoutPanel.Controls.Add(button);
                }
            }

            string[] prototypes = await GlyphProvider.CreateEnumPrototypes();
            { }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Font? IconBasics { get; private set; }

        private void Any_MouseEnter(object? sender, EventArgs e) 
            => ((Control)sender!).ForeColor = Color.Aqua;
        private void Any_MouseHover(object? sender, EventArgs e)
        {
            if (sender is Control control && control.Tag is Enum icon)
            {
                string tooltipText = icon.ToString();
                Point offset = new Point(25, -25);
                _tooltip.Show(tooltipText, control, offset, 4000);
            }
        }

        private void Any_MouseLeave(object? sender, EventArgs e)
        {
            if (sender is Control control)
            {
                _tooltip.Hide(control);
                control.ForeColor = SystemColors.ControlText;
            }
        }

        private void Any_Click(object? sender, EventArgs e)
        {
            if(((Control)sender!)!.Tag is Enum icon && icon.ToGlyphInfo() is { } info)
            {
                MessageBox.Show(JsonConvert.SerializeObject(info, Formatting.Indented));
            }
        }

        ToolTip _tooltip = new ToolTip
        {
            AutoPopDelay = 5000,
            InitialDelay = 500,
            ReshowDelay = 100,
            ShowAlways = true,
            OwnerDraw = false // set true if you want custom drawing later
        };
    }
}

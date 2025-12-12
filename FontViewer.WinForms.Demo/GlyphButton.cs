using Newtonsoft.Json;
using System.ComponentModel;
using IVSoftware.Portable;

namespace FontViewer.WinForms.Demo
{
    /// <summary>
    /// The easy way is to subclass Button, giving it
    /// a first-class Enum property for str icon values.
    /// </summary>
    public class GlyphButton : Button
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
            // ------------------------------------
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

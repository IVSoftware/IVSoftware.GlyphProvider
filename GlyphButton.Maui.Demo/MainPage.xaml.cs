
using IVSoftware.Portable;

namespace GlyphButton.Maui.Demo
{
    public partial class MainPage : ContentPage
    {
        GlyphProvider.IconBasics[] _icons = Enum.GetValues<GlyphProvider.IconBasics>();

        int count = 0, mod;

        public MainPage()
        {
            InitializeComponent();
            _ = GlyphProvider.BoostCache();
#if WINDOWS
            Loaded += (sender, e) => Window!.Title = "Glyph Button";
#endif
        }

        private void OnCounterClicked(object? sender, EventArgs e)
        {
            mod = count % _icons.Length;
            var icon = _icons[mod];
            string
                prefix = icon.ToGlyph()!,
                suffix = count == 0 ? " " : "s";

            count++;
            CounterBtn.Text = $"{prefix}    Clicked {count} time{suffix}";
        }
    }

    public class GlyphButton : Button
    {
        public static readonly BindableProperty StdIconNameProperty =
            BindableProperty.Create(
                nameof(StdIconName),
                typeof(Enum),
                typeof(GlyphButton),
                default(Enum),
                propertyChanged: OnStdIconChanged);

        public Enum? StdIconName
        {
            get => (Enum?)GetValue(StdIconNameProperty);
            set => SetValue(StdIconNameProperty, value);
        }

        static void OnStdIconChanged(BindableObject bindable, object oldValue, object newValue)
        {
            if (newValue is Enum icon)
            {
                var button = (GlyphButton)bindable;
                button.FontFamily = icon.ToCssFontName();
                if (button.Text.TryUpdateIconicText(icon, out var textFTR, spacing: 4))
                {
                    if (button.BindingContext?.GetType().GetProperty(nameof(Text)) is { } pi)
                    {
                        // If a binding exists, don't break it - use it.
                        pi.SetValue(button.BindingContext, textFTR);
                    }
                    else
                    {
                        button.Text = textFTR;
                    }
                }
            }
        }
    }
}

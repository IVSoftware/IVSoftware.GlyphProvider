using Newtonsoft.Json;
using IVSoftware.Portable;

namespace FontViewer.Maui.Demo
{
    public class GlyphButton : Button
    {
        public GlyphButton()
        {
            HeightRequest = 50;
            WidthRequest = 50;
            BorderColor = Color.FromArgb("#444444");
            Margin = new Thickness(1);
            Padding = 0;
            FontSize = 18;

            Clicked += async (sender, e) =>
            {
                if (StdIconName?.ToGlyphInfo() is { } info &&
                    Window?.Page is { } page)
                {
                    var json = JsonConvert.SerializeObject(info, Formatting.Indented);
                    await page.DisplayAlertAsync(null, json, "Close");
                }
            };
        }
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

                var text = icon.ToGlyph();
                if (button.BindingContext?.GetType().GetProperty(nameof(Text)) is { } pi)
                {
                    // If a binding exists, don't break it - use it.
                    pi.SetValue(button.BindingContext, text);
                }
                else
                {
                    button.Text = text;
                }
            }
        }

#if WINDOWS
        // Highlight when mouse is hovered.
        public void PointerInBounds(bool isInBounds)
        {
            TextColor = isInBounds ? Colors.Aqua : Colors.Black;
        }
#endif
    }
}

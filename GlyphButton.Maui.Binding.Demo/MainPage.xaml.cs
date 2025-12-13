using IVSoftware.Portable;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace GlyphButton.Maui.Binding.Demo
{
    public partial class MainPage : ContentPage
    {
        public MainPage()
        {
            InitializeComponent();
            _ = GlyphProvider.BoostCache();
#if WINDOWS
            Loaded += (sender, e) => Window!.Title = "Glyph Button";
#endif
        }
    }

    class MainPageBindingContext : INotifyPropertyChanged
    {
        int count = 0, mod;
        public MainPageBindingContext()
        {
            ClickedCommand = new Command(OnClicked);
        }
        public ICommand ClickedCommand { get; }
        private void OnClicked(object o)
        {
            mod = count % _icons.Length;
            count++;
            Text = $"Clicked {count} time{(count == 0 ? string.Empty : "s")}";

            StdIconName = _icons[mod];
        }

        GlyphProvider.IconBasics[] _icons = Enum.GetValues<GlyphProvider.IconBasics>();
        public Enum? StdIconName
        {
            get => _stdIconName;
            set
            {
                if (!Equals(_stdIconName, value))
                {
                    _stdIconName = value;
                    if(_stdIconName?.GetType()?.GetCssNameAttribute()?.Name is { } fontFamily && fontFamily is not null)
                    {
                        FontFamily = fontFamily;
                    }
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(FontFamily));
                }
            }
        }
        Enum? _stdIconName = GlyphProvider.IconBasics.HelpCircledAlt;

        public string FontFamily { get; set; } = "icon-basics";

        public string Text
        {
            get => _text;
            set
            {
                if (!Equals(_text, value))
                {
                    _text = value;
                    OnPropertyChanged();
                }
            }
        }
        string _text = "Click Me";


        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        public event PropertyChangedEventHandler? PropertyChanged;
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

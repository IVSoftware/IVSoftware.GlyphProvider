using IVSoftware.Portable;
using IVSoftware.Portable.Collections.Dictionaries;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;

namespace FontViewer.WPF.Demo
{
    public partial class MainWindow : Window
    {
        internal static TolerantDictionary<string, FontFamily> FontFamilyCache = new();
        public MainWindow()
        {
            InitializeComponent();

            _ = InitAsync();
        }
        async Task InitAsync()
        {
            await GlyphProvider.WaitAsync();

            // GlyphProvider doesn't enumerate its own provider for GlyphProvider.IconBasics unless
            // requested. This is so that EUD's config space isn't polluted with an unwanted default.
            var iconBasicsProvider = GlyphProvider.Providers[typeof(GlyphProvider.IconBasics)];

            var providers = GlyphProvider.Providers.Values.Concat([iconBasicsProvider]).Distinct().ToArray();
            var asmName = typeof(MainWindow).Assembly.GetName().Name;

            // Import the fonts.
            // We want to make a cache, and use the fully-qualified name of the provider as the key.
            foreach (var provider in providers.OfType<GlyphProvider>())
            {
                var fontFamily = new FontFamily(
                    baseUri   :  new Uri("pack://application:,,,/"),
                    familyName: $"./Resources/Fonts/{provider.Name}/font/#{provider.Name}");
                FontFamilyCache[provider.Key] = fontFamily;
                ComboBoxConfig.Items.Add(provider);
            }

            foreach (var icon in Enum.GetValues<GlyphProvider.IconBasics>())
            {
                WrapPanelIcons.Children.Add(new GlyphButton { StdIconName = icon, });
            }

            ComboBoxConfig.SelectedIndex = ComboBoxConfig.Items.IndexOf(iconBasicsProvider);
            ComboBoxConfig.SelectionChanged += OnConfigSelected;

            NavBar.Visibility = 
                ComboBoxConfig.Items.Count > 1
                ? Visibility.Visible
                : Visibility.Collapsed;
        }

        private void OnConfigSelected(object sender, SelectionChangedEventArgs e)
        {
            if (ComboBoxConfig.SelectedItem is GlyphProvider provider)
            {
                WrapPanelIcons.Children.Clear();
                if (provider.StdIconEnumType is { } stdIconType)
                {
                    // If an enum type has been defined for provider, use it.
                    foreach (Enum icon in Enum.GetValues(stdIconType))
                    {
                        WrapPanelIcons.Children.Add(new GlyphButton { StdIconName = icon, });
                    }
                }
                else
                {
                    // Otherwise, the raw information can still be used to greate iconic buttons.
                    var fontFamily = FontFamilyCache[provider.Key] ?? new();
                    foreach (var info in provider.Glyphs)
                    {
                        WrapPanelIcons.Children.Add(
                            new GlyphButton
                            {
                                FontFamily = fontFamily,
                                Content = char.ConvertFromUtf32(info.Code).ToString()
                            });
                    }
                }
            }
        }
    }
    public class GlyphButton : Button
    {
        private static readonly ToolTip _tooltip = new ToolTip
        {
            Placement = PlacementMode.Relative,
            StaysOpen = true
        };

        public GlyphButton()
        {
            Width = 50;
            Height = 50;
            Margin = new Thickness(1);
            Padding = new Thickness(0);
            FontSize = 18;
            BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#444444"));
            BorderThickness = new Thickness(1);

            SnapsToDevicePixels = true;
            UseLayoutRounding = true;
            TextOptions.SetTextFormattingMode(this, TextFormattingMode.Display);
            TextOptions.SetTextRenderingMode(this, TextRenderingMode.ClearType);
            TextOptions.SetTextHintingMode(this, TextHintingMode.Fixed);

            MouseEnter += (_, __) => SetHover(true);
            MouseLeave += OnMouseLeave;
            MouseMove += OnMouseHover;   // WPF's closest analogue to WinForms Hover

            Click += OnClick;
        }

        public static readonly DependencyProperty StdIconNameProperty =
            DependencyProperty.Register(
                nameof(StdIconName),
                typeof(Enum),
                typeof(GlyphButton),
                new PropertyMetadata(null, OnStdIconNameChanged));

        public Enum StdIconName
        {
            get => (Enum)GetValue(StdIconNameProperty);
            set => SetValue(StdIconNameProperty, value);
        }

        private static void OnStdIconNameChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            if (sender is GlyphButton btn &&
                e.NewValue is Enum icon &&
                icon.ToProviderKey() is string key &&
                MainWindow.FontFamilyCache[key] is FontFamily fontFamily)
            {
                btn.FontFamily = fontFamily;
                btn.Content = icon.ToGlyph();
            }
        }

        private void OnMouseHover(object? sender, MouseEventArgs e)
        {
            if (StdIconName is Enum icon)
            {
                var offset = new Point(25, -25);
                _tooltip.Content = icon.ToString();

                // The WinForms Show() analogue
                _tooltip.PlacementTarget = this;
                _tooltip.HorizontalOffset = offset.X;
                _tooltip.VerticalOffset = offset.Y;

                if (!_tooltip.IsOpen)
                {
                    ShowTooltipWithDelay(250);
                }
            }
        }

        private async void ShowTooltipWithDelay(int delayMs)
        {
            await Task.Delay(delayMs);

            // Still hovering?
            if (IsMouseOver)
            {
                _tooltip.IsOpen = true;
            }
        }

        private void OnMouseLeave(object? sender, MouseEventArgs e)
        {
            _tooltip.IsOpen = false;
            SetHover(false);
        }

        private void SetHover(bool isHovering)
            => Foreground = new SolidColorBrush(isHovering ? Colors.Aqua : Colors.Black);

        private void OnClick(object sender, RoutedEventArgs e)
        {
            if (StdIconName?.ToGlyphInfo() is { } info)
            {
                var json = Newtonsoft.Json.JsonConvert.SerializeObject(info, Newtonsoft.Json.Formatting.Indented);
                MessageBox.Show(json, "", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
    }
}
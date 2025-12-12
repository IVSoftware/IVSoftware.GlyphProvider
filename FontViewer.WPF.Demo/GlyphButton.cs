using IVSoftware.Portable;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;

namespace FontViewer.Wpf.Demo
{
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

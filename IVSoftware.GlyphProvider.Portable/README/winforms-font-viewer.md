# [<](../../README.md)

## WinForms Font Viewer

The designer sets up an empty `FlowLayoutPanel`. The `InitAsync` method populates it with iconic buttons at runtime. WinForms loads custom fonts through `System.Drawing.PrivateFontCollection` (handled automatically by the platform-specific NuGet package). Every glyph-bearing control must set `UseCompatibleTextRendering = true` - without it, WinForms will not render Unicode PUA glyphs at all.


![WinForms Font Viewer](https://raw.githubusercontent.com/IVSoftware/IVSoftware.GlyphProvider/master/IVSoftware.GlyphProvider.Portable/README/img/winforms-font-viewer.png)

The `Font Viewer` version adds the capability of switching between the available font archives using the combo box where providers of type `GlyphProvider` are added to `comboBoxConfig.Items`.

```
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
 ```
 
  Changing the combo box selection repopulates the viewer by iterating the `enum`. If a std enum cannot be found for the provider, the provider's own list is iterated. Having a defined `enum` is (literally) the key to showing tool tips and respoding to clicks.

 ```
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
                // Otherwise, the raw information can still be used to create iconic buttons. (No ToolTips or Click Action in this case.)
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
```
using Chnuschti.Controls;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chnuschti.Themes.Default;

public class DefaultTheme : Theme
{
    public SKColor BorderColor;
    public SKColor BackgroundColor;
    public SKColor AccentColor;
    public SKColor AccentBright;
    public SKColor AccentDark;
    public SKColor DisabledColor;
    public SKColor HoverColor;
    public SKColor OffColor;
    public SKColor TextColor;
    public float Thickness { get; set; } = 1.5f;

    public DefaultTheme() : base("Default")
    {
        BorderColor = new SKColor(30, 30, 30, 255);
        BackgroundColor = new SKColor(40, 40, 40, 255);
        AccentColor = new SKColor(255, 113, 43, 255);
        AccentBright = AccentColor.AdjBrightness(60f);
        AccentDark = AccentColor.AdjBrightness(-60f);
        DisabledColor = new SKColor(128, 128, 128, 255);
        HoverColor = SKColors.Gray.WithAlpha(30);
        OffColor = SKColors.Gray;
        TextColor = SKColors.White;

        Resources
                .Add<Screen, Style>(new ScreenStyle(this))
                .Add<Label, Style>(new LabelLargeStyle(this), "LabelLarge")
                .Add<Label, Style>(new LabelMediumStyle(this), string.Empty) // default label style
                .Add<Label, Style>(new LabelSmallStyle(this), "LabelSmall")
                .Add<StackPanel, Style>(new StackPanelStyle(this))
                .Add<ContentControl, Style>(new ContentControlStyle(this))
                .Add<ItemsControl, Style>(new ItemsControlStyle(this))
                .Add<Button, Style>(new ButtonStyle(this))
                .Add<CheckBox, Style>(new CheckboxStyle(this))
                .Add<Icon, Style>(new IconStyle(this))
                ;
    }
}

using Chnuschti.Controls;
using SkiaSharp;

namespace Chnuschti.Themes.Default;

public class DefaultTheme : Theme
{
    public DefaultTheme() : base("Default")
    {
        BorderColor = new SKColor(30, 30, 30, 255);
        BackgroundColor = new SKColor(40, 40, 40, 255);
        AccentColor = new SKColor(255, 113, 43, 255);
        AccentBright = AccentColor.AdjBrightness(30f);
        AccentDark = AccentColor.AdjBrightness(-30f);
        DisabledColor = new SKColor(128, 128, 128, 255);
        HoverColor = SKColors.Gray.WithAlpha(30);
        OffColor = SKColors.Gray;
        TextColor = SKColors.White;
        ShadowColor = new SKColor(0, 0, 0, 100);

        BorderThickness = 1.5f;
        Width = 52;
        Height = 24;
        HandleOff = Height / 2.0f;
        HandleOn = Height - 6.0f;
        HandlePressed = Height - 4.0f;
        HandleHover = Height + 8.0f;
        Radius = Height / 2.0f;
        PressedRadius = 5.0f;

        Resources
            .Add<Screen, Style>(new ScreenStyle())
            .Add<Label, Style>(new LabelExtraLargeStyle(), "ExtraLarge")
            .Add<Label, Style>(new LabelLargeStyle(), "Large")
            .Add<Label, Style>(new LabelMediumStyle(), "Medium")
            .Add<Label, Style>(new LabelMediumStyle(), string.Empty)
            .Add<Label, Style>(new LabelSmallStyle(), "Small")
            .Add<StackPanel, Style>(new StackPanelStyle())
            .Add<DockPanel, Style>(new DockPanelStyle())
            .Add<ContentControl, Style>(new ContentControlStyle())
            .Add<ItemsControl, Style>(new ItemsControlStyle())
            .Add<Button, Style>(new ButtonStyle())
            .Add<Button, Style>(new ButtonFlatStyle(), "Flat")
            .Add<Button, Style>(new ButtonOutlinedStyle(), "Outlined")
            .Add<CheckBox, Style>(new CheckboxStyle())
            .Add<Icon, Style>(new IconStyle())
            .Add<TabControl, Style>(new TabControlStyle())
            .Add<ScrollBar, Style>(new ScrollBarStyle())
            ;
    }
}

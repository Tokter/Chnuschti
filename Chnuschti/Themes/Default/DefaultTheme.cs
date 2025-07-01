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
    public SKColor BorderColor = new SKColor(30, 30, 30, 255);
    public SKColor BackgroundColor = new SKColor(40, 40, 40, 255);

    public DefaultTheme() : base("Default")
    {
        Resources
            .Add<Screen, Style>(ScreenStyle.CreateStyle(this))
            .Add<Label, Style>(LabelStyle.CreateStyle())
            .Add<StackPanel, Style>(StackPanelStyle.CreateStyle())
            .Add<ContentControl, Style>(ContentControlStyle.CreateStyle())
            .Add<Button, Style>(ButtonStyle.CreateStyle(this))
            .Add<CheckBox, Style>(CheckboxStyle.CreateStyle(this));
    }
}

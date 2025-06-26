using Chnuschti.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chnuschti.Themes.Default;

public class DefaultTheme : Theme
{
    public DefaultTheme() : base("Default")
    {
        Resources
            .Add<Label, Style>(LabelStyle.CreateStyle());
    }
}

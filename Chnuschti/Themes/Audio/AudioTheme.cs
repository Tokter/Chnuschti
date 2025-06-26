using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chnuschti.Themes.Audio;

public class AudioTheme : Theme
{
    public AudioTheme() : base("Audio")
    {
        Resources
            .Add<Chnuschti.Button>(ButtonStyle.CreateButtonStyle()); //Default button style
    }
}

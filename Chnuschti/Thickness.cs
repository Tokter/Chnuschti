using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chnuschti;

public readonly struct Thickness
{
    public readonly float Left, Top, Right, Bottom;
    public Thickness(float uniform) : this(uniform, uniform, uniform, uniform) { }
    public Thickness(float horizontal, float vertical) : this(horizontal, vertical, horizontal, vertical) { }
    public Thickness(float l, float t, float r, float b)
        => (Left, Top, Right, Bottom) = (l, t, r, b);
    public float Horizontal => Left + Right;
    public float Vertical => Top + Bottom;

    //Conversion to SKSize
    public static implicit operator SKSize(Thickness t) => new SKSize(t.Horizontal, t.Vertical);
}

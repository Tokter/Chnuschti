using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chnuschti;

public class Theme
{
    public string Name { get; }
    public ResourceDictionary Resources { get; } = new();
    public Theme(string name) => Name = name;

    //Colors
    public SKColor BorderColor;
    public SKColor BackgroundColor;
    public SKColor AccentColor;
    public SKColor AccentBright;
    public SKColor AccentDark;
    public SKColor DisabledColor;
    public SKColor HoverColor;
    public SKColor OffColor;
    public SKColor TextColor;
    public SKColor ShadowColor;

    //Control Sizes
    public float BorderThickness = 1.5f;
    public float Width = 52;
    public float Height = 24;
    public float HandleOff = 16f;
    public float HandleOn = 24f;
    public float HandlePressed = 28f;
    public float HandleHover = 40f;
    public float Radius = 12.0f;
    public float PressedRadius = 5f;
    public float MinScrollBarSize = 10.0f;
}

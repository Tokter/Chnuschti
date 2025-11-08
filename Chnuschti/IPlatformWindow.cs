using Chnuschti.Controls;
using SkiaSharp;

namespace Chnuschti;

public interface IPlatformWindow
{
    public string Title { get; set; }
    public WindowState WindowState { get; set; }
    public SKPoint Location { get; set; }
    public SKPoint Size { get; set; }
}

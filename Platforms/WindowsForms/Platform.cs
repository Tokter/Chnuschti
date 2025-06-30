using SkiaSharp;
using SkiaSharp.Views.Desktop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Intrinsics.X86;
using System.Text;
using System.Threading.Tasks;

namespace Chnuschti.WindowsForms;

public class Platform : IPlatform
{
    private SKGLControl _control;
    public ChnuschtiApp Application { get; }

    public Platform(SKGLControl control, ChnuschtiApp application)
    {
        _control = control;
        _control.Resize += Control_Resize;
        _control.PaintSurface += Control_PaintSurface;
        _control.KeyDown += Control_KeyDown;
        _control.KeyUp += Control_KeyUp;
        _control.MouseMove += Control_MouseMove;
        _control.MouseDown += Control_MouseDown;
        _control.MouseUp += Control_MouseUp;
        _control.MouseWheel += Control_MouseWheel;
        Application = application;
    }

    public void Initialize()
    {
    }

    private void Control_PaintSurface(object? sender, SKPaintGLSurfaceEventArgs e)
    {
        if (Application.Screen == null) return;
        Application.Screen.Render(e.Surface.Canvas);

        //Draw mouse position for debugging at the bottom left corner
        using var paint = new SKPaint
        {
            Color = SKColors.Red,
        };
        using var font = new SKFont
        {
            Size = 12 * Application.Screen.ScaleY,
        };
        e.Surface.Canvas.DrawText($"Mouse: {MousePosX:F2}, {MousePosY:F2}", 10 * Application.Screen.ScaleX, e.Info.Height - 10 * Application.Screen.ScaleY, font, paint);

        _control.Invalidate();
    }

    private void Control_Resize(object? sender, EventArgs e)
    { 
        float zoomFactor = 1.0f;

        if (Application.Screen == null) return;
        Application.Screen.SetSize(_control.Width / zoomFactor, _control.Height / zoomFactor);
        Application.Screen.ScaleX = zoomFactor;
        Application.Screen.ScaleY = zoomFactor;
    }

    #region Input handling
    bool _lastShift = false;
    bool _lastControl = false;
    bool _lastAlt = false;
    float MousePosX = 0.0f;
    float MousePosY = 0.0f;

    private void Control_KeyDown(object? sender, KeyEventArgs e)
    {
        _lastShift = e.Shift;
        _lastControl = e.Control;
        _lastAlt = e.Alt;
        Application.ProcessInputEvent(InputEvent.KeyDown((Key)e.KeyValue, _lastShift, _lastControl, _lastAlt));
    }

    private void Control_KeyUp(object? sender, KeyEventArgs e)
    {
        _lastShift = e.Shift;
        _lastControl = e.Control;
        _lastAlt = e.Alt;
        Application.ProcessInputEvent(InputEvent.KeyUp((Key)e.KeyValue, _lastShift, _lastControl, _lastAlt));
    }

    public void Control_MouseMove(object? sender, MouseEventArgs e)
    {
        MousePosX = e.X / Application.Screen.ScaleX;
        MousePosY = e.Y / Application.Screen.ScaleY;
        Application.ProcessInputEvent(InputEvent.MouseMove(e.X, e.Y, (MouseButtons)((int)e.Button >> 20), _lastShift, _lastControl, _lastAlt));
    }

    private void Control_MouseDown(object? sender, MouseEventArgs e)
    {
        Application.ProcessInputEvent(InputEvent.MouseDown(e.X, e.Y, (MouseButtons)((int)e.Button >> 20), _lastShift, _lastControl, _lastAlt));
    }

    private void Control_MouseUp(object? sender, MouseEventArgs e)
    {
        Application.ProcessInputEvent(InputEvent.MouseUp(e.X, e.Y, (MouseButtons)((int)e.Button >> 20), _lastShift, _lastControl, _lastAlt));
    }

    private void Control_MouseWheel(object? sender, MouseEventArgs e)
    {
        Application.ProcessInputEvent(InputEvent.MouseWheel(e.Delta, _lastShift, _lastControl, _lastAlt));
    }

    #endregion
}
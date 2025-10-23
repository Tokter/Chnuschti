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
    private Form _form;
    private SKGLControl _control;
    public ChnuschtiApp Application { get; }

    public Platform(Form form, SKGLControl control, ChnuschtiApp application)
    {
        _form = form;
        _control = control;

        _control.Resize += Control_Resize;
        _control.PaintSurface += Control_PaintSurface;
        _control.KeyDown += Control_KeyDown;
        _control.KeyUp += Control_KeyUp;
        _control.KeyPress += Control_KeyPress;
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
        Application.Render(e.Surface.Canvas);

        _control.Invalidate();
    }

    private void Control_Resize(object? sender, EventArgs e)
    { 
        if (Application.Screen == null) return;
        Application.SetSize(_control.Width, _control.Height);
    }

    #region Input handling
    bool _lastShift = false;
    bool _lastControl = false;
    bool _lastAlt = false;

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

    private void Control_KeyPress(object? sender, KeyPressEventArgs e)
    {
        Application.ProcessInputEvent(InputEvent.KeyPress(e.KeyChar.ToString()));
    }

    public void Control_MouseMove(object? sender, MouseEventArgs e)
    {
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
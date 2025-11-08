using SkiaSharp.Views.Desktop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using WindowsForms;

namespace Chnuschti.WindowsForms;

public class FocusableSKGLControl : SKGLControl
{
    private PlatformWindow _platformWindow { get; set; }

    public FocusableSKGLControl(PlatformWindow platformWindow) : base()
    {
        _platformWindow = platformWindow;
    }

    protected override bool IsInputKey(Keys keyData)
    {
        // Treat all keys as input keys so they are sent to OnKeyDown
        return true;
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);
        Console.WriteLine($"Key pressed: {e.KeyCode}");
    }

    protected override void OnCreateControl()
    {
        base.OnCreateControl();
        this.TabStop = true; // Make sure it can be focused
    }

    protected override void OnMouseDown(MouseEventArgs e)
    {
        base.OnMouseDown(e);
        this.Focus(); // Allow mouse click to focus control
    }

    protected override void OnMouseMove(MouseEventArgs e)
    {
        base.OnMouseMove(e);
        var ht = _platformWindow.GetHitZone(e.Location, ClientSize.Width, ClientSize.Height);

        Cursor desired = Cursors.Default;

        // Map HT codes to cursors
        if (_platformWindow.WindowState != Chnuschti.Controls.WindowState.Maximized)
            desired =
            (ht == Win32.HitTestValues.Left || ht == Win32.HitTestValues.Right) ? Cursors.SizeWE :
            (ht == Win32.HitTestValues.Top || ht == Win32.HitTestValues.Bottom) ? Cursors.SizeNS :
            (ht == Win32.HitTestValues.TopLeft || ht == Win32.HitTestValues.BottomRight) ? Cursors.SizeNWSE :
            (ht == Win32.HitTestValues.TopRight || ht == Win32.HitTestValues.BottomLeft) ? Cursors.SizeNESW :
            Cursors.Default; // caption/client

        if (this.Cursor != desired) this.Cursor = desired;
    }

    protected override void WndProc(ref Message m)
    {
        switch (m.Msg)
        {         
            case Win32.WM_NCHITTEST:
                Point screen = new Point(m.LParam.ToInt32());
                Point client = PointToClient(screen);
                var ht = _platformWindow.GetHitZone(client, ClientSize.Width, ClientSize.Height);
                if (ht != Win32.HitTestValues.Nowhere)
                {
                    if (_platformWindow.WindowState == Chnuschti.Controls.WindowState.Maximized &&
                        (ht == Win32.HitTestValues.Left || ht == Win32.HitTestValues.Right ||
                         ht == Win32.HitTestValues.Top || ht == Win32.HitTestValues.Bottom ||
                         ht == Win32.HitTestValues.TopLeft || ht == Win32.HitTestValues.TopRight ||
                         ht == Win32.HitTestValues.BottomLeft || ht == Win32.HitTestValues.BottomRight))
                    {
                        // When maximized, disable resizing
                        m.Result = (IntPtr)Win32.HitTestValues.Client;
                        return;
                    }

                    m.Result = (IntPtr)Win32.HitTestValues.Transparent;
                    return;
                }
                break;
        }
        base.WndProc(ref m);
    }
}

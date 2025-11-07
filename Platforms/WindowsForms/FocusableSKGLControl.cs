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
        var ht = GetHitZone(e.Location);

        // Map HT codes to cursors
        Cursor desired =
            (ht == Win32.HitTestValues.Left || ht == Win32.HitTestValues.Right) ? Cursors.SizeWE :
            (ht == Win32.HitTestValues.Top || ht == Win32.HitTestValues.Bottom) ? Cursors.SizeNS :
            (ht == Win32.HitTestValues.TopLeft || ht == Win32.HitTestValues.BottomRight) ? Cursors.SizeNWSE :
            (ht == Win32.HitTestValues.TopRight || ht == Win32.HitTestValues.BottomLeft) ? Cursors.SizeNESW :
            Cursors.Default; // caption/client

        if (this.Cursor != desired) this.Cursor = desired;
    }

    private const int cGrip = 4;      // Grip size
    private const int cCaption = 8;   // Caption bar height;
    protected override void WndProc(ref Message m)
    {
        switch (m.Msg)
        {
            case Win32.WM_NCHITTEST:
                Point screen = new Point(m.LParam.ToInt32());
                Point client = PointToClient(screen);
                var ht = GetHitZone(client);
                if (ht != Win32.HitTestValues.Nowhere)
                {
                    m.Result = (IntPtr)Win32.HitTestValues.Transparent;
                    return;
                }
                break;
        }

        base.WndProc(ref m);
    }

    private Win32.HitTestValues GetHitZone(Point pos)
    {
        int x = pos.X;
        int y = pos.Y;
        int w = ClientSize.Width;
        int h = ClientSize.Height;

        bool caption = y <= cCaption;
        bool left = x <= cGrip;
        bool right = x >= w - cGrip;
        bool top = y <= cGrip;
        bool bottom = y >= h - cGrip;

        if (left && top) return Win32.HitTestValues.TopLeft;
        if (right && top) return Win32.HitTestValues.TopRight;
        if (left && bottom) return Win32.HitTestValues.BottomLeft;
        if (right && bottom) return Win32.HitTestValues.BottomRight;
        if (left) return Win32.HitTestValues.Left;
        if (right) return Win32.HitTestValues.Right;
        //if (top) return Win32.HitTestValues.Top;
        if (bottom) return Win32.HitTestValues.Bottom;
        if (caption) return Win32.HitTestValues.Caption;

        return Win32.HitTestValues.Nowhere;
    }
}

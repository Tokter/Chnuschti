using SkiaSharp.Views.Desktop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
}

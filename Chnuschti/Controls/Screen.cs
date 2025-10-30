using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;

namespace Chnuschti.Controls;

public class Screen : ContentControl
{
    private readonly Random rand = new();

    public Screen()
    {
        InitializeComponent();
        HotReloadManager.RegisterScreen(this);
        IsRoot = true;
    }

    protected virtual void InitializeComponent()
    {
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            HotReloadManager.UnregisterScreen(this);
        }

        base.Dispose(disposing);
    }
}

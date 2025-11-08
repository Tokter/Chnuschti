using Chnuschti.Controls;
using SkiaSharp;
using SkiaSharp.Views.Desktop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Intrinsics.X86;
using System.Text;
using System.Threading.Tasks;
using WindowsForms;

namespace Chnuschti.WindowsForms;

public class Platform : IPlatform
{
    private Dictionary<Window,PlatformWindow> _windows = new();

    public ChnuschtiApp Application { get; }

    public Platform(ChnuschtiApp application)
    {
        Application = application;
    }

    public void Initialize()
    {
    }

    public IEnumerable<Window> Windows => _windows.Keys;

    public void CreateWindow(Window window)
    {
        var pf = new PlatformWindow(window);
        _windows[window] = pf;
    }

    public void CloseWindow(Window window)
    {
        if (_windows.TryGetValue(window, out var pf))
        {
            pf.Dispose();
            _windows.Remove(window);
        }
    }

    public void ReplaceWindow(Window oldWindow, Window newWindow)
    {
        if (_windows.TryGetValue(oldWindow, out var pf))
        {
            _windows.Remove(oldWindow);
            _windows.Add(newWindow, pf);
        }
    }

    public IPlatformWindow? GetPlatformWindow(Window window)
    {
        if (_windows.TryGetValue(window, out var pf))
        {
            return pf;
        }
        return null;
    }

    public void Run()
    {
        _windows.First().Value.Run();
    }
}
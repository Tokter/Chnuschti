using Chnuschti.Controls;
using Chnuschti.Themes.Default;
using Microsoft.VisualBasic;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Chnuschti.InputEvent;

namespace Chnuschti;

public class ChnuschtiApp
{
    public static ChnuschtiApp Current { get; private set; } = null!;
    public IPlatform? Platform { get; private set; }
    public IViewLocator? ViewLocator { get; set; }

    public void Configure(IPlatform platform)
    {
        Platform = platform;
        HotReloadManager.RegisterApp(this);
        if (Current != null)
        {
            throw new InvalidOperationException("An instance of ChnuschtiApp is already configured as the current application.");
        }
        Current = this;
    }

    /// <summary>
    /// Creates and returns the main application window.
    /// </summary>
    /// <remarks>This method should be overridden in a derived class to configure and return the main window
    /// of the application. The default implementation throws a <see cref="NotImplementedException"/>.</remarks>
    /// <returns>A <see cref="Window"/> object representing the main window of the application.</returns>
    /// <exception cref="NotImplementedException">Thrown if the method is not overridden in a derived class.</exception>
    public virtual Window CreateMainWindow()
    {
        throw new NotImplementedException("CreateMainWindow must be overridden in a derived class to configure the main window.");
    }
}

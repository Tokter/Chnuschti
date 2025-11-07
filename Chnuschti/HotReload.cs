using Chnuschti.Controls;
using System;
using System.Reflection;
using System.Reflection.Metadata;
using System.Runtime.CompilerServices;

namespace Chnuschti;
public static class ViewHotReloadHandler
{
    /// <summary>
    /// Called automatically by the runtime when hot reload updates types.
    /// </summary>
    public static void UpdateApplication(Type[]? updatedTypes)
    {
        if (updatedTypes == null) return;

        bool stylesUpdated = false;

        foreach (var type in updatedTypes)
        {
            // Check for control updates
            if (typeof(Control).IsAssignableFrom(type))
            {
                HotReloadManager.ReloadViewsOfType(type);
            }
            
            // Check for style-related updates
            if (IsStyleRelatedType(type))
            {
                stylesUpdated = true;
            }
        }

        // If any style-related types were updated, refresh all styles and update live controls
        if (stylesUpdated)
        {
            RefreshStyles(updatedTypes);
        }
    }

    private static bool IsStyleRelatedType(Type type)
    {
        // Check if the type is related to styles based on naming convention or inheritance
        return type.Name.EndsWith("Style") || 
               type.Name.EndsWith("Theme") || 
               type.Name.EndsWith("Renderer") ||
               type.Name.EndsWith("Resource") ||
               typeof(Style).IsAssignableFrom(type) ||
               typeof(Theme).IsAssignableFrom(type) ||
               typeof(RenderState).IsAssignableFrom(type);
    }

    private static void RefreshStyles(Type[] updatedTypes)
    {        
        // Now reload affected themes
        if (ThemeManager.Current != null)
        {
            // Get the type of the current theme
            var themeType = ThemeManager.Current.GetType();
            
            // Check if the theme type itself needs reloading
            bool reloadTheme = updatedTypes.Contains(themeType);
            
            if (reloadTheme)
            {
                // Create a new instance of the theme and apply it
                try
                {
                    var newTheme = Activator.CreateInstance(themeType) as Theme;
                    if (newTheme != null)
                    {
                        ThemeManager.RegisterAndApply(newTheme);
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Failed to hot reload theme: {ex}");
                }
            }
            else
            {
                ThemeManager.Current.Resources.RecreateStyles(ThemeManager.Current, updatedTypes);

                // Just refresh the existing theme's styles for affected controls
                ApplyStylesToLiveControls();
            }
        }
    }

    private static void ApplyStylesToLiveControls()
    {
        // Apply updated styles to all active windows and their visual trees
        foreach (var window in HotReloadManager.LiveWindows)
        {
            RefreshStyles(window);
        }
    }

    private static void RefreshStyles(VisualElement element)
    {
        // Invalidate the element's style
        element.Style = null;
        element.Style = ThemeManager.Current.Resources.Get<Style>(element.GetType(), element.StyleKey);
        
        // Reset rendering resources
        element.InvalidateDrawResources();
        
        // Cascade to children
        foreach (var child in element.Children)
        {
            RefreshStyles(child);
        }
    }

    /// <summary>
    /// Optional: used to invalidate static caches (e.g. compiled templates or styles).
    /// </summary>
    public static void ClearCache(Type[]? updatedTypes)
    {
        if (updatedTypes == null) return;
    }
}

public static class HotReloadManager
{
    // Reference to the application instance
    private static ChnuschtiApp? _app;

    public static void RegisterApp(ChnuschtiApp app)
    {
        _app = app;
    }

    public static IEnumerable<Window> LiveWindows => _app?.Platform?.Windows ?? Array.Empty<Window>();

    public static void ReloadViewsOfType(Type viewType)
    {
        // Case 1: The updated type is a Window itself (like MainWindow)
        if (typeof(Window).IsAssignableFrom(viewType))
        {
            foreach(var window in _app!.Platform!.Windows)
            {
                if (window.GetType() == viewType)
                {
                    ReloadWindow(window, viewType);
                }
            }
        }
        // Case 2: The updated type is content inside a Window
        else
        {
            foreach (var window in _app!.Platform!.Windows)
            {
                if (window.Content?.GetType() == viewType)
                {
                    var oldView = window.Content;
                    var vm = oldView.DataContext;

                    oldView.Dispose();

                    if (Activator.CreateInstance(viewType) is Control newView)
                    {
                        newView.DataContext = vm;
                        window.Content = newView;
                    }
                }
            }
        }
    }

    private static void ReloadWindow(Window window, Type viewType)
    {
        var oldWindow = window;
        var oldDataContext = oldWindow.DataContext;
        var oldContent = oldWindow.Content;

        if (Activator.CreateInstance(viewType) is Window newWindow)
        {
            newWindow.Width = oldWindow.Width;
            newWindow.Height = oldWindow.Height;
            newWindow.Scale = oldWindow.Scale;
            newWindow.DataContext = oldDataContext;

            // Update app reference to the new Window
            _app?.Platform?.ReplaceWindow(oldWindow, newWindow);

            oldWindow.Dispose();
        }
    }
}
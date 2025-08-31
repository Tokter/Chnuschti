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
        // Apply updated styles to all active screens and their visual trees
        foreach (var screen in HotReloadManager._liveScreens)
        {
            RefreshStyles(screen);
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
    // Track all active screens or top-level controls
    internal static readonly List<Screen> _liveScreens = new();
    // Reference to the application instance
    private static ChnuschtiApp? _app;

    public static void RegisterScreen(Screen screen)
    {
        if (!_liveScreens.Contains(screen))
            _liveScreens.Add(screen);
    }

    public static void UnregisterScreen(Screen screen)
    {
        _liveScreens.Remove(screen);
    }

    public static void RegisterApp(ChnuschtiApp app)
    {
        _app = app;
    }

    public static void ReloadViewsOfType(Type viewType)
    {
        // Case 1: The updated type is a Screen itself (like MainWindow)
        if (typeof(Screen).IsAssignableFrom(viewType) && _app?.Screen?.GetType() == viewType)
        {
            var oldScreen = _app.Screen;
            var oldDataContext = oldScreen.DataContext;
            var oldContent = oldScreen.Content;

            if (Activator.CreateInstance(viewType) is Screen newScreen)
            {
                newScreen.DataContext = oldDataContext;
                               
                // Update app reference to the new screen
                _app.Screen = newScreen;
                
                // Set the size to match the current app dimensions
                newScreen.ScaleX = _app.Scale;
                newScreen.ScaleY = _app.Scale;
                
                oldScreen.Dispose();
            }
        }
        // Case 2: The updated type is content inside a Screen
        else
        {
            foreach (var screen in _liveScreens)
            {
                if (screen.Content?.GetType() == viewType)
                {
                    var oldView = screen.Content;
                    var vm = oldView.DataContext;

                    oldView.Dispose();

                    if (Activator.CreateInstance(viewType) is Control newView)
                    {
                        newView.DataContext = vm;
                        screen.Content = newView;
                    }
                }
            }
        }
    }
}
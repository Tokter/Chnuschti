using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chnuschti;

// A very small theme manager (singleton for convenience)
public static class ThemeManager
{
    private static readonly Dictionary<string, Theme> _themes = new();
    public static Theme Current { get; private set; } = new("Default");

    public static void Register(Theme t) => _themes[t.Name] = t;
    public static void Apply(string themeName)
    {
        if (_themes.TryGetValue(themeName, out var t)) Current = t;
        else throw new ArgumentException("Theme not found");
        ThemeChanged?.Invoke(null, EventArgs.Empty);
    }

    public static void RegisterAndApply(Theme t)
    {
        Register(t);
        Apply(t.Name);
    }

    public static event EventHandler? ThemeChanged;
}

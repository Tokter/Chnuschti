using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chnuschti.Controls;

public class Icon : Control
{
    public static readonly DependencyProperty IconKindProperty = DependencyProperty.Register(nameof(IconKind), typeof(IconKind), typeof(Icon), new PropertyMetadata(IconKind.AccountAlertOutline, OnInvalidateDrawResources));

    public IconKind IconKind { get => (IconKind)GetValue(IconKindProperty)!; set => SetValue(IconKindProperty, value);
    }

    public Icon()
    {
        //Get the default style from the current theme
        Style = ThemeManager.Current.Resources.Get<Icon, Style>();
        IsHitTestVisible = false; // Labels are not interactive by default
    }

    // --------------------------------------------------------------------
    //  Property-change helpers
    // --------------------------------------------------------------------
    private static void OnInvalidateDrawResources(DependencyObject d, DependencyProperty p, object? oldV, object? newV)
    {
        if (d is Icon i)
        {
            i.InvalidateDrawResources(); // will also invalidate measure, arrange & matrices
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chnuschti;

public static class UI
{
    public static T With<T>(this T element, Action<T> cfg) where T : VisualElement
    { cfg(element); return element; }

    public static T AddTo<T>(this T child, VisualElement parent) where T : VisualElement
    { parent.Add(child); return child; }
}

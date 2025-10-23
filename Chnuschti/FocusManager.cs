using Chnuschti.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Chnuschti.InputEvent;

namespace Chnuschti;

/*
Optional niceties (easy to add later)

- Focus scopes: containers (dialogs) that trap Tab inside. Add IsFocusScope on panels; FocusManager can limit CollectFocusable to the current scope.
- Default/Cancel buttons: on KeyDown Enter/Escape, walk up from Focused to find a Button with IsDefault/IsCancel, click it.
- Access keys: parse &File → Alt+F activation.
- Visual focus cues: in Control.OnGotFocus/OnLostFocus, invalidate visuals; render a focus rectangle (dotted/halo) in the control’s renderer.
 */

public sealed class FocusManager
{
    public static FocusManager Instance { get; } = new();

    private Control? _focused;
    private ulong _indexedVersion;

    public Control? Focused => _focused;

    public bool RequestFocus(Control c)
    {
        if (!c.IsEnabled || !c.IsVisible || !c.Focusable) return false;
        if (ReferenceEquals(_focused, c)) return true;

        var old = _focused;
        _focused = c;

        if (old != null) old.IsFocused = false;
        c.IsFocused = true;
        return true;
    }

    public void ClearFocus()
    {
        if (_focused != null)
        {
            _focused.IsFocused = false;
            _focused = null;
        }
    }

    // Tab navigation: find next/prev focusable in visual order + TabIndex
    public bool MoveFocusNext(VisualElement root)
    {
        if (root.TreeVersion != _indexedVersion)
        {
            VisualTreeHelper.AssignVisualIndices(root);
            _indexedVersion = root.TreeVersion;
        }
        return MoveFocus(root, forward: true);
    }

    public bool MoveFocusPrev(VisualElement root)
    {
        if (root.TreeVersion != _indexedVersion)
        {
            VisualTreeHelper.AssignVisualIndices(root);
            _indexedVersion = root.TreeVersion;
        }
        return MoveFocus(root, forward: false);
    }

    private bool MoveFocus(VisualElement root, bool forward)
    {
        var focusables = new List<Control>(128);
        CollectFocusable(root, focusables);

        if (focusables.Count == 0) return false;

        // Order by (TabIndex, visual order) like WPF
        focusables.Sort((a, b) =>
        {
            int ti = a.TabIndex.CompareTo(b.TabIndex);
            if (ti != 0) return ti;
            return a.VisualIndex.CompareTo(b.VisualIndex); // you can compute this during tree walk
        });

        int start = _focused != null ? focusables.IndexOf(_focused) : -1;
        int idx = start;

        for (int step = 0; step < focusables.Count; step++)
        {
            idx = forward
                ? (idx + 1 + focusables.Count) % focusables.Count
                : (idx - 1 + focusables.Count) % focusables.Count;

            var c = focusables[idx];
            if (c.IsEnabled && c.IsVisible && c.Focusable && c.IsTabStop)
                return RequestFocus(c);
        }
        return false;
    }

    private void CollectFocusable(VisualElement e, List<Control> dest)
    {
        if (e is Control c) dest.Add(c);
        foreach (var child in e.Children) CollectFocusable(child, dest);
    }

    // Keyboard dispatch entry points
    public void DispatchKeyDown(InputEvent e) { _focused?.OnKeyDown(e); }
    public void DispatchKeyUp(InputEvent e) { _focused?.OnKeyUp(e); }
    public void DispatchTextInput(TextInputEvent e) { _focused?.OnTextInput(e); }
}

using SkiaSharp;
using System.Collections.Generic;
using System.Linq;

namespace Chnuschti.Controls;

public static class PopupManager
{
    private static readonly List<Popup> _popups = new();

    public static IReadOnlyList<Popup> ActivePopups => _popups;

    public static void Register(Popup p)
    {
        if (!_popups.Contains(p))
        {
            // If dialog modal, optionally clear non-modal popups
            if (p.IsModal)
            {
                for (int i = _popups.Count - 1; i >= 0; i--)
                {
                    if (!_popups[i].IsModal) _popups[i].Close();
                }
            }
            _popups.Add(p);
        }
    }

    public static void Unregister(Popup p)
    {
        _popups.Remove(p);
    }

    // Called by root after it has rendered its normal content.
    public static void RenderPopups(VisualElement root, SKCanvas canvas, double dt)
    {
        if (_popups.Count == 0) return;

        // Layout each popup (they are not in the normal tree).
        foreach (var p in _popups)
        {
            p.PerformLayout(root);
        }

        // Draw optional modal overlay (single combined pass) if any modal is active.
        bool hasModal = _popups.Any(p => p.IsModal && p.ShowModalOverlay);
        if (hasModal)
        {
            canvas.Save();
            // Full-screen dim (assuming root covers window client area).
            using var overlay = new SKPaint { Color = new SKColor(0, 0, 0, 90), Style = SKPaintStyle.Fill };
            var w = root.ContentBounds.Width;
            var h = root.ContentBounds.Height;
            canvas.DrawRect(new SKRect(0, 0, w, h), overlay);
            canvas.Restore();
        }

        // Render popups in order; later ones appear above earlier ones.
        foreach (var p in _popups)
        {
            canvas.Save();
            //canvas.SetMatrix(p.WorldMatrix); // We'll adjust – see helper below if needed
            canvas.SetMatrix(p.Owner.WorldMatrix);

            p.Render(canvas, dt);

            canvas.Restore();
        }
    }

    // Hit test – popups first. Returns popup or child if hit; null otherwise.
    public static VisualElement? HitTest(SKPoint screenPt)
    {
        for (int i = _popups.Count - 1; i >= 0; i--)
        {
            var hit = _popups[i].HitTest(screenPt);
            if (hit != null) return hit;
        }
        return null;
    }

    public static void CloseAllNonModal() =>
        _popups.Where(p => !p.IsModal).ToList().ForEach(p => p.Close());

    public static void CloseAll() =>
        _popups.ToList().ForEach(p => p.Close());

    // Called on pointer up to dismiss if outside and CloseOnOutsideClick.
    public static bool TryHandleOutsideClick(SKPoint screenPt)
    {
        // If any modal present, ignore outside click dismissal unless desired.
        bool hasModal = _popups.Any(p => p.IsModal);
        if (hasModal) return false;

        foreach (var p in _popups)
        {
            if (!p.CloseOnOutsideClick) continue;
            var hit = p.HitTest(screenPt);
            if (hit != null) return false; // inside a popup
        }

        if (_popups.Any(p => !p.IsModal))
        {
            CloseAllNonModal();
            return true;
        }
        return false;
    }
}
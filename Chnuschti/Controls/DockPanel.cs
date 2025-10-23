using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chnuschti.Controls
{
    public enum Dock { Left, Top, Right, Bottom }

    public sealed class DockPanel : Control
    {
        public DockPanel()
        {
            // Panels are layout containers and typically don't handle input.
            IsHitTestVisible = false;
        }

        public static readonly DependencyProperty DockProperty = DependencyProperty.RegisterAttached(name: nameof(Dock), propertyType: typeof(Dock), ownerType: typeof(DockPanel), metadata: new PropertyMetadata(Dock.Left));
        public static readonly DependencyProperty LastChildFillProperty = DependencyProperty.Register(nameof(LastChildFill), typeof(bool), typeof(DockPanel), new PropertyMetadata(true, OnLayoutAffecting));

        public static Dock GetDock(DependencyObject el) => (Dock)el.GetValue(DockProperty)!;

        public static void SetDock(DependencyObject el, Dock value) => el.SetValue(DockProperty, value);

        public bool LastChildFill
        {
            get => (bool)GetValue(LastChildFillProperty)!;
            set => SetValue(LastChildFillProperty, value);
        }

        private static void OnLayoutAffecting(DependencyObject d, DependencyProperty p, object? o, object? n)
        {
            if (d is DockPanel dp) dp.InvalidateMeasure();
        }

        protected override void ArrangeContent(SKRect content)
        {
            // content is already local: (0,0) .. (W,H)
            var remaining = content;

            int lastVisibleIdx = -1;
            for (int i = 0; i < Children.Count; i++)
                if (Children[i].IsVisible) lastVisibleIdx = i;

            for (int i = 0; i < Children.Count; i++)
            {
                var child = Children[i];
                if (!child.IsVisible) continue;

                var m = child.Margin;
                var desired = child.DesiredSize; // includes margins
                bool isLastFill = LastChildFill && (i == lastVisibleIdx);

                if (isLastFill)
                {
                    // Fill everything that’s left (minus margins).
                    var fillRect = VisualElement.ShrinkBy(remaining, m);

                    // Fill: ignore desired size; use the full remaining space.
                    // We keep ApplyAlignment ONLY to honor non-stretch cross-axis alignment.
                    var aligned = ApplyAlignment(fillRect, fillRect.Width, fillRect.Height);
                    child.Arrange(aligned);
                    remaining = SKRect.Empty;
                    break;
                }

                // Size the band on the dock axis using desired - margins, span the orthogonal axis.
                float bandW = Math.Max(0, desired.Width - m.Horizontal);
                float bandH = Math.Max(0, desired.Height - m.Vertical);

                switch (GetDock(child))
                {
                    case Dock.Left:
                        {
                            var band = new SKRect(
                                remaining.Left + m.Left,
                                remaining.Top + m.Top,
                                remaining.Left + m.Left + bandW,
                                remaining.Bottom - m.Bottom);

                            // NO alignment for non-fill: place directly in band.
                            child.Arrange(band);

                            // Consume desired size including margins on the dock axis
                            remaining.Left += desired.Width;
                            break;
                        }

                    case Dock.Right:
                        {
                            var band = new SKRect(
                                remaining.Right - m.Right - bandW,
                                remaining.Top + m.Top,
                                remaining.Right - m.Right,
                                remaining.Bottom - m.Bottom);

                            child.Arrange(band);

                            remaining.Right -= desired.Width;
                            break;
                        }

                    case Dock.Top:
                        {
                            var band = new SKRect(
                                remaining.Left + m.Left,
                                remaining.Top + m.Top,
                                remaining.Right - m.Right,
                                remaining.Top + m.Top + bandH);

                            child.Arrange(band);

                            remaining.Top += desired.Height;
                            break;
                        }

                    default: // Dock.Bottom
                        {
                            var band = new SKRect(
                                remaining.Left + m.Left,
                                remaining.Bottom - m.Bottom - bandH,
                                remaining.Right - m.Right,
                                remaining.Bottom - m.Bottom);

                            child.Arrange(band);

                            remaining.Bottom -= desired.Height;
                            break;
                        }
                }
            }
        }

    }
}
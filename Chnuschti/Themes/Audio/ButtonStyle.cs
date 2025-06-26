using Chnuschti.Controls;
using SkiaSharp;

namespace Chnuschti.Themes.Audio
{
    public static class ButtonStyle
    {
        //public static object CreateButtonTemplate(
        //    SKColor normalColor,
        //    SKColor hoverColor,
        //    SKColor pressedColor, 
        //    float cornerRadius = 6f)
        //{
        //    //return owner =>
        //    //{
        //    //    var btn = (Chnuschti.Button)owner;

        //    //    var bord = new Border
        //    //    {
        //    //        CornerRadius = cornerRadius,
        //    //        Padding = btn.Padding            // pick up existing padding
        //    //    };

        //    //    void ApplyState()
        //    //    {
        //    //        var col = btn.IsEnabled ? normalColor : normalColor.WithAlpha(100);
        //    //        if (btn.IsMouseOver) col = hoverColor;
        //    //        if (btn.IsMouseOver && btn.IsPressed) // extension below
        //    //            col = pressedColor;
        //    //        bord.Background = col;
        //    //    }

        //    //    // one-shot + live updates
        //    //    ApplyState();
        //    //    btn.PropertyChanged += (_, e) =>
        //    //    {
        //    //        if (e.PropertyName is nameof(Chnuschti.Button.IsMouseOver) or
        //    //                              nameof(Chnuschti.Button.IsEnabled))
        //    //            ApplyState();
        //    //    };

        //    //    // forward Button.Content into the Border
        //    //    bord.Content = btn.Content;

        //    //    return bord;        // VisualElement that Render() can visit
        //    //};
        //}

        public static Style CreateButtonStyle()
        {
            var s = new Style();
            s.Add(Chnuschti.Button.BackgroundProperty, SKColors.DodgerBlue);
            s.Add(Chnuschti.Button.PaddingProperty, new Thickness(12, 6, 12, 6));
            s.Add(Chnuschti.Button.ForegroundProperty, SKColors.White);

            s.Renderer = (Canvas) =>
            {

            };

            return s;
        }
    }
}

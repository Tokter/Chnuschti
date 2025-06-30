// -------------------------------------------------------------------------
//  Button – derives from ContentControl, fires Click when hit-tested
// -------------------------------------------------------------------------
using System;
using Chnuschti.Controls;
using SkiaSharp;

namespace Chnuschti;

public class Button : ContentControl
{
    public Button()
    {
        //Get the default style from the current theme
        Style = ThemeManager.Current.Resources.Get<Button,Style>();
    }

    // ---- dependency-properties -----------------------------------------
    public static readonly DependencyProperty CommandProperty = DependencyProperty.Register(nameof(Command), typeof(ICommand), typeof(Button), new PropertyMetadata(null, OnCommandChanged));
    public static readonly DependencyProperty CommandParameterProperty = DependencyProperty.Register(nameof(CommandParameter), typeof(object), typeof(Button), new PropertyMetadata(null));

    public ICommand? Command
    {
        get => (ICommand?)GetValue(CommandProperty);
        set => SetValue(CommandProperty, value);
    }

    public object? CommandParameter
    {
        get => GetValue(CommandParameterProperty);
        set => SetValue(CommandParameterProperty, value);
    }

    // ---- click event ----------------------------------------------------
    public event EventHandler? Click;

    protected override void OnClick(object? sender, EventArgs e)
    {
        base.OnClick(sender, e);

        Click?.Invoke(this, EventArgs.Empty);

        if (Command?.CanExecute(CommandParameter) ?? false) Command.Execute(CommandParameter);
    }


    // ---- layout: use ContentControl defaults ---------------------------

    private static void OnCommandChanged(DependencyObject d, DependencyProperty p, object? oldC, object? newC)
    {
        var btn = (Button)d;

        if (oldC is ICommand oldCmd) oldCmd.CanExecuteChanged -= btn.OnCanExecuteChanged;
        if (newC is ICommand newCmd) newCmd.CanExecuteChanged += btn.OnCanExecuteChanged;

        // First query immediately
        btn.UpdateIsEnabled();
    }

    private void OnCanExecuteChanged(object? sender, EventArgs e) => UpdateIsEnabled();

    private void UpdateIsEnabled()
    {
        IsEnabled = Command?.CanExecute(CommandParameter) ?? true;
    }
}

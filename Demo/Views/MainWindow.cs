using Chnuschti;
using Chnuschti.Controls;
using Chnuschti.Themes.Default;
using Demo.ViewModels;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Demo.Views;

public class MainWindow : Screen
{
    private Button? _button;

    public MainWindow()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        VisualElement.ShowLayoutDebug = false;

        Content = new StackPanel()
            .With(p => p.Padding = new Thickness(5));

        new Label { Text = "Alice", Margin = new Thickness(4), Bold = true }
        .AddTo(Content)
        .SetBinding(Label.TextProperty, this.OneWayToDC<MainViewModel, string>(mvm => mvm.Title));


        new Label { Text = "Bob", Margin = new Thickness(4) }.AddTo(Content);
        new Label { Text = "Carol", Margin = new Thickness(4) }.AddTo(Content);
        _button = new Button { Content = "Click me", Margin = new Thickness(4), Padding = new Thickness(3) }
        .AddTo(Content);
        _button.SetBinding(Button.CommandProperty, this.OneWayToDC((MainViewModel mvm) => mvm.ChangeTitle));
        _button.SetBinding(Button.IsEnabledProperty, this.OneWayToDC((MainViewModel mvm) => mvm.IsButtonEnabled));

        var cb = new CheckBox
        {
            Content = "Button Enabled",
            Margin = new Thickness(4)
        };
        cb.SetBinding(CheckBox.IsCheckedProperty, this.TwoWayToDC((MainViewModel mvm) => mvm.IsButtonEnabled));
        cb.SetBinding(Button.IsEnabledProperty, this.OneWayToDC((MainViewModel mvm) => mvm.IsCheckboxEnabled));
        cb.AddTo(Content);

        var cb2 = new CheckBox
        {
            Content = new Icon { IconKind = IconKind.AccountAlertOutline },
            Margin = new Thickness(4),
            Foreground = SKColors.Green.AdjBrightness(61),
            Background = SKColors.Green,
        };
        cb2.SetBinding(CheckBox.IsCheckedProperty, this.TwoWayToDC((MainViewModel mvm) => mvm.IsCheckboxEnabled));
        cb2.AddTo(Content);

        new Icon { }.AddTo(Content);
    }
}

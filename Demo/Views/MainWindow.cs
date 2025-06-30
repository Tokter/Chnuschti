using Chnuschti;
using Chnuschti.Controls;
using Chnuschti.Themes.Default;
using Demo.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Demo.Views;

public class MainWindow : Screen
{
    private Chnuschti.Button? _button;

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
        _button = new Chnuschti.Button { Content = "Click me", Margin = new Thickness(4), Padding = new Thickness(3) }
        .AddTo(Content);
        _button.SetBinding(Chnuschti.Button.CommandProperty, this.OneWayToDC((MainViewModel mvm) => mvm.ChangeTitle));
    }
}

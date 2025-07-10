using Chnuschti;
using Chnuschti.Controls;
using Chnuschti.Themes.Default;
using Demo.ViewModels;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Demo.Views;

public class MainWindow : Screen
{
    private Button? _button;

    protected override void InitializeComponent()
    {
        VisualElement.ShowLayoutDebug = false;

        Content = new ItemsControl
        {
            ItemsPanel = new StackPanel
            {
                Orientation = Orientation.Vertical,
                HorizontalContentAlignment = HorizontalAlignment.Stretch,
                VerticalContentAlignment = VerticalAlignment.Stretch,
                Padding = new Thickness(5),
            }, 
            ItemTemplate = new DataTemplate<Person>(p =>
            {
                var sp = new StackPanel
                {
                    Orientation = Orientation.Horizontal,
                    HorizontalContentAlignment = HorizontalAlignment.Stretch,
                    VerticalContentAlignment = VerticalAlignment.Center,
                    Margin = new Thickness(4),
                };
                sp.Add(
                    new Icon { IconKind = IconKind.Account, Margin = new Thickness(4) },
                    new Label
                    {
                        Text = p.Name,
                        Margin = new Thickness(4),
                        HorizontalContentAlignment = HorizontalAlignment.Stretch,
                        VerticalContentAlignment = VerticalAlignment.Center,
                        StyleKey = "LabelLarge",
                        MinWidth = 120,
                    },
                    new Label
                    {
                        Text = p.Age.ToString(),
                        Margin = new Thickness(4),
                        HorizontalContentAlignment = HorizontalAlignment.Stretch,
                        VerticalContentAlignment = VerticalAlignment.Center,
                    });
                return sp;
            })
        };
        Content.SetBinding(ItemsControl.ItemsSourceProperty, this.OneWayToDC<MainViewModel, ObservableCollection<Person>>(mvm => mvm.People));
        

        /*
        Content = new StackPanel()
        {
            Padding = new Thickness(5),
            HorizontalContentAlignment = HorizontalAlignment.Stretch,
        };

        new Label
        {
            Text = "Alice", 
            Margin = new Thickness(4), 
            //Bold = true,
            StyleKey = "LabelLarge",
        }
        .AddTo(Content)
        .SetBinding(Label.TextProperty, this.OneWayToDC<MainViewModel, string>(mvm => mvm.Title));


        new Label
        {
            Text = "Bob", 
            Margin = new Thickness(4),
            HorizontalContentAlignment = HorizontalAlignment.Center,
            VerticalContentAlignment = VerticalAlignment.Center,
        }.AddTo(Content);

        new Label 
        {
            Text = "Carol", 
            Margin = new Thickness(4),
            StyleKey = "LabelSmall",
        }.AddTo(Content);
        _button = new Button
        {
            Content = "Click me",
            Margin = new Thickness(4), 
            Padding = new Thickness(3),
            HorizontalContentAlignment = HorizontalAlignment.Center,
            VerticalContentAlignment = VerticalAlignment.Center,
        }
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
            Content = new Icon { IconKind = IconKind.Account, Margin = new Thickness(4) },
            Margin = new Thickness(4),
            Foreground = SKColors.Green.AdjBrightness(61),
            Background = SKColors.Green,
        };
        cb2.SetBinding(CheckBox.IsCheckedProperty, this.TwoWayToDC((MainViewModel mvm) => mvm.IsCheckboxEnabled));
        cb2.AddTo(Content);

        new Icon
        {
            IconKind = IconKind.DanceBallroom, 
            Width = 200, 
            Height = 200,
            HorizontalContentAlignment = HorizontalAlignment.Stretch,
            VerticalContentAlignment = VerticalAlignment.Stretch,
        }.AddTo(Content); 
        */
    }
}

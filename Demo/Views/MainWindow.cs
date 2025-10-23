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

        TabItem? stats = null;

        Content = new TabControl()
            .With(tc =>
            {
                tc.Margin = new Thickness(5);
                tc.StripPlacement = TabStripPlacement.Top;
                //tc.HeaderTemplate = new DataTemplate<TabItem>(tab =>
                //{
                //    // Example: header with title + ✖ close-button
                //    var row = new StackPanel { Orientation = Orientation.Horizontal };
                //    row.Children(
                //        new Label
                //        {
                //            Text = tab.Header?.ToString() ?? "Untitled",
                //            Margin = new Thickness(4),
                //        },

                //        new Button
                //        {
                //            Content = "X",
                //            Padding = new Thickness(0, 0, 2, 0),
                //            StyleKey = "Flat",
                //            Command = new DelegateCommand(_ => tab.ParentTabControl?.RemoveTab(tab))
                //        });
                //    return row;
                //});
            })
            .Add(tc =>
                new TabItem()
                .With(ti => ti.Header = "Home")
                .Content(new StackPanel()
                    .With(sp => sp.Orientation = Orientation.Vertical)
                    .Children(
                        new Label { Text = "Small", Margin = new Thickness(18), StyleKey = "Small" },
                        new Label { Text = "Medium", Margin = new Thickness(18), StyleKey = "Medium" },
                        new Label { Text = "Large", Margin = new Thickness(18), StyleKey = "Large" },
                        new Label { Text = "Extra Large", Margin = new Thickness(18), StyleKey = "ExtraLarge" }
                    )
                )
            )
            .Add(tc =>
                new TabItem()
                .With(ti => ti.Header = "Scroll Bars")
                .Content(
                    new DockPanel()
                    .Children(
                        new ScrollBar
                        {
                            Orientation = Orientation.Vertical,
                            Margin = new Thickness(10),
                            Minimum = 0,
                            Maximum = 100,
                            Viewport = 20,
                        }.With(sb => DockPanel.SetDock(sb, Dock.Left))
                        //.SetBinding(ScrollBar.ValueProperty, this.OneWayToDC<MainViewModel, float>(mvm => mvm.ScrollBar1Value))
                        ,
                        new ScrollBar
                        {
                            Orientation = Orientation.Horizontal,
                            Margin = new Thickness(10),
                            Minimum = 0,
                            Maximum = 100,
                            Viewport = 20,
                        }.With(sb => DockPanel.SetDock(sb, Dock.Top)),
                        new StackPanel()
                            .Children(
                                //Create a label and bind Text to scroll position for testing
                                new Label { Text = "Scroll Position: 0" }
                            )
                    ))
                .Out(out stats)
            )
            .Add(tc =>
                new TabItem()
                .With(ti => ti.Header = "Buttons")
                .Content(
                    new StackPanel()
                        .With(sp =>
                        {
                            sp.Orientation = Orientation.Vertical;
                        })
                        .Children(
                            new CheckBox
                            {
                                Content = "Button Enabled",
                                Margin = new Thickness(18)
                            },
                            new CheckBox
                            {
                                Content = IconKind.Account,
                                Margin = new Thickness(18),
                                Foreground = SKColors.Green.AdjBrightness(61),
                                Background = SKColors.Green,
                            },
                            new Button()
                            {
                                Content = "Click me",
                                Margin = new Thickness(18),
                            },
                            new Button()
                            {
                                Content = "Click me",
                                Margin = new Thickness(18),
                                Background = SKColors.Purple,
                            },
                            new Button()
                            {
                                Content = "Click me",
                                Margin = new Thickness(18),
                                Background = SKColors.Blue,
                            },
                            new Button()
                            {
                                Content = "Outlined",
                                StyleKey = "Outlined",
                                Margin = new Thickness(18),
                                Background = SKColors.Blue,
                            },
                            new Button()
                            {
                                Content = "Flat",
                                StyleKey = "Flat",
                                Margin = new Thickness(18),
                                Background = SKColors.Blue,
                            }
                        )
                    )
            )
            .Add(tc =>
                new TabItem()
                .With(ti => ti.Header = "DockPanel")
                .Content(
                    new DockPanel()
                        .Children(
                            new Label 
                            {
                                Text = "Left",
                                Margin = new Thickness(10), 
                                VerticalContentAlignment=VerticalAlignment.Center 
                            }.With(l => DockPanel.SetDock(l, Dock.Left)),
                            new Label 
                            { 
                                Text = "Right", 
                                Margin = new Thickness(10), 
                                VerticalContentAlignment = VerticalAlignment.Center 
                            }.With(l => DockPanel.SetDock(l, Dock.Right)),
                            new Label 
                            { 
                                Text = "Top", 
                                Margin = new Thickness(10),
                                HorizontalContentAlignment = HorizontalAlignment.Center
                            }.With(l => DockPanel.SetDock(l, Dock.Top)),
                            new Label 
                            { 
                                Text = "Bottom", 
                                Margin = new Thickness(10),
                                HorizontalContentAlignment = HorizontalAlignment.Center
                            }.With(l => DockPanel.SetDock(l, Dock.Bottom)),
                            new Label 
                            { 
                                Text = "Center (fills remaining space)", 
                                Margin = new Thickness(10),
                                HorizontalContentAlignment = HorizontalAlignment.Center,
                                VerticalContentAlignment = VerticalAlignment.Center
                            }
                        )
                )
            );
    }
}

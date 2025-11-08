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

public class MainWindow : Window
{
    private Button? _button;
    private Label _menuLabel;

    protected override void InitializeComponent()
    {
        VisualElement.ShowLayoutDebug = false;

        Content = new DockPanel()
            .Children(
                CreateTitleBar().Dock(Dock.Top),
                CreateMainContent()
            );
    }

    private Control CreateTitleBar()
    {
        return new DockPanel()
            .Children(
                new Button
                {
                    Content = new Icon { IconKind = IconKind.Close, Margin = new Thickness(4, 8) },
                    StyleKey = "Flat",
                    Command = new DelegateCommand(_ => this.Close())
                }
                .Dock(Dock.Right),

                new Button
                {
                    Content = new Icon { IconKind = IconKind.WindowMaximize, Margin = new Thickness(4,8) },
                    StyleKey = "Flat",
                    Command = new DelegateCommand(_ =>
                    {
                        if (this.WindowState == WindowState.Maximized)
                        {
                            this.WindowState = WindowState.Normal;
                        }
                        else
                        {
                            this.WindowState = WindowState.Maximized;
                        }
                    })
                }
                .Dock(Dock.Right),

                new Button
                {
                    Content = new Icon { IconKind = IconKind.WindowMinimize, Margin = new Thickness(4, 8) },
                    StyleKey = "Flat",
                    Command = new DelegateCommand(_ =>
                    {
                        if (this.WindowState == WindowState.Minimized)
                        {
                            this.WindowState = WindowState.Normal;
                        }
                        else
                        {
                            this.WindowState = WindowState.Minimized;
                        }
                    })
                }
                .Dock(Dock.Right),

                new Label
                {
                    StyleKey = "Large",
                    Margin = new Thickness(10,0),
                    VerticalContentAlignment = VerticalAlignment.Center,
                    IsWindowDragArea = true,
                }
                .Bind(Label.TextProperty, this.OneWayToDC<MainViewModel, string>(mvm => mvm.Title))
            );
    }

    private Control CreateMainContent()
    {
        TabItem? stats = null;

        return new TabControl()
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
                            Width = 12,
                        }
                        .Dock(Dock.Left)
                        .Bind(ScrollBar.ValueProperty, this.TwoWayToDC<MainViewModel, float>(mvm => mvm.ScrollBar1Value)),
                        new ScrollBar
                        {
                            Orientation = Orientation.Horizontal,
                            Margin = new Thickness(10),
                            Minimum = 0,
                            Maximum = 100,
                            Viewport = 20,
                            Height = 12
                        }
                        .Dock(Dock.Top)
                        .Bind(ScrollBar.ValueProperty, this.TwoWayToDC<MainViewModel, float>(mvm => mvm.ScrollBar2Value)),
                        new StackPanel()
                            .Children(
                                //Create a label and bind Text to scroll position for testing
                                new Label().Bind(Label.TextProperty, this.OneWayToDC<MainViewModel, string>(mvm => mvm.ScrollBar1ValueText)),
                                new Label().Bind(Label.TextProperty, this.OneWayToDC<MainViewModel, string>(mvm => mvm.ScrollBar2ValueText))
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
                                VerticalContentAlignment = VerticalAlignment.Center
                            }.Dock(Dock.Left),
                            new Label
                            {
                                Text = "Right",
                                Margin = new Thickness(10),
                                VerticalContentAlignment = VerticalAlignment.Center
                            }.Dock(Dock.Right),
                            new Label
                            {
                                Text = "Top",
                                Margin = new Thickness(10),
                                HorizontalContentAlignment = HorizontalAlignment.Center
                            }.Dock(Dock.Top),
                            new Label
                            {
                                Text = "Bottom",
                                Margin = new Thickness(10),
                                HorizontalContentAlignment = HorizontalAlignment.Center
                            }.Dock(Dock.Bottom),
                            new Label
                            {
                                Text = "Center (fills remaining space)",
                                Margin = new Thickness(10),
                                HorizontalContentAlignment = HorizontalAlignment.Center,
                                VerticalContentAlignment = VerticalAlignment.Center
                            }
                        )
                )
            )
            .Add(tc =>
                new TabItem()
                .With(ti => ti.Header = "Menu")
                .Content(
                    new DockPanel()
                    .Children(
                        new Menu().Dock(Dock.Top)
                        .Add(
                            new MenuItem { Icon = new Icon { IconKind = IconKind.File }, Text = "File" }
                                .Add(
                                    new MenuItem
                                    {
                                        Icon = new Icon { IconKind = IconKind.ContentSave },
                                        Text = "Save",
                                        Shortcut = "Ctrl+S",
                                        Command = new DelegateCommand(_ =>
                                        {
                                            if (_menuLabel != null)
                                                _menuLabel.Text = "Menu Click";
                                        })
                                    },
                                    new MenuItem { Icon = new Icon { IconKind = IconKind.FileDocument }, Text = "Open", Shortcut = "Ctrl+O" }
                                ),
                            new MenuItem { Icon = new Icon { IconKind = IconKind.AccountEdit }, Text = "Edit" }
                                .Add(
                                    new MenuItem { Icon = new Icon { IconKind = IconKind.SelectAll }, Text = "Select All" },
                                    new MenuItem { Icon = new Icon { IconKind = IconKind.SelectRemove }, Text = "Select None" }
                                )
                        ),
                        new Label
                        {
                            Text = "Menu demo (hover over 'File' or 'Edit' menu items)",
                            Margin = new Thickness(10),
                            HorizontalContentAlignment = HorizontalAlignment.Center,
                            VerticalContentAlignment = VerticalAlignment.Center,
                        }.With(l => _menuLabel = l)
                    )
                )
            )
            ;
    }
}
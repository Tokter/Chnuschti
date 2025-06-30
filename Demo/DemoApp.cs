﻿using Chnuschti;
using Chnuschti.Controls;
using Chnuschti.Themes.Default;
using CommunityToolkit.Mvvm.DependencyInjection;
using Demo.ViewModels;
using Demo.Views;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace Demo;

public class DemoApp : ChnuschtiApp
{
    protected override void OnStartup()
    {
        SetupDependencyInjection();

        ThemeManager.RegisterAndApply(new DefaultTheme()); // Register the default theme

        ViewLocator = new ViewLocator();

        Screen = new MainWindow
        {
            DataContext = Ioc.Default.GetService<MainViewModel>()
        };

    }

    private void SetupDependencyInjection()
    {
        var services = new ServiceCollection();
        ConfigureViewModels(services);
        ConfigureViews(services);
        var provider = services.BuildServiceProvider();
        Ioc.Default.ConfigureServices(provider);
    }

    private void ConfigureViewModels(IServiceCollection services)
    {
        foreach (var vm in Assembly.GetExecutingAssembly().GetTypes().Where(t => typeof(ViewModelBase).IsAssignableFrom(t)))
        {
            services.Add(new ServiceDescriptor(vm, vm, ServiceLifetime.Transient));
        }
    }

    private void ConfigureViews(IServiceCollection services)
    {
        foreach (var vm in Assembly.GetExecutingAssembly().GetTypes().Where(t => typeof(UserControl).IsAssignableFrom(t)))
        {
            services.Add(new ServiceDescriptor(vm, vm.Name, ServiceLifetime.Transient));
        }
    }

}

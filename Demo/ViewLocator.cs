using Chnuschti;
using Chnuschti.Controls;
using Demo.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Demo;

public class ViewLocator : IViewLocator
{
    public bool Match(object? data)
    {
        return data is ViewModelBase;
    }

    public Control? Build(object? data)
    {
        if (data is ViewModelBase vm)
        {
            if (vm.View == null)
            {
                var name = data.GetType().FullName!.Replace("ViewModel", "View");
                var type = Type.GetType(name);

                if (type != null)
                {
                    vm.View = (Control)Activator.CreateInstance(type)!;
                }
                else
                {
                    vm.View = new TextBlock { Text = name };
                }
            }
            return vm.View;
        }
        return null;
    }
}

using Chnuschti.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Demo.ViewModels;

public class ViewModelBase : ObservableObject
{
    public Control? View { get; set; }
}
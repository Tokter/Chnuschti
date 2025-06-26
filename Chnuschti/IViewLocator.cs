using Chnuschti.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chnuschti;

public interface IViewLocator
{
    bool Match(object? data);
    Control? Build(object? data);
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chnuschti;

public interface ICommand
{
    void Execute(Object? obj);
    bool CanExecute(Object? obj);
    event EventHandler? CanExecuteChanged;
}

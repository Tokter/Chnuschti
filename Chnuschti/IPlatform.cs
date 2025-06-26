using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chnuschti;

public interface IPlatform
{
    ChnuschtiApp Application { get; }
    void Initialize();
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chnuschti;

public class Theme
{
    public string Name { get; }
    public ResourceDictionary Resources { get; } = new();
    public Theme(string name) => Name = name;
}

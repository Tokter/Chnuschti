using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chnuschti.Controls;

public class TextBlock : Control
{
    public string Text { get; set; } = string.Empty;

    public override string ToString()
    {
        return $"TextBlock: {Text}";
    }
}

using Chnuschti.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chnuschti;

public interface IPlatform
{
    ChnuschtiApp Application { get; }
    IEnumerable<Window> Windows { get; }
    void Initialize();
    void CreateWindow(Window window);
    void CloseWindow(Window window);
    void ReplaceWindow(Window oldWindow, Window newWindow);
    IPlatformWindow? GetPlatformWindow(Window window);
    void Run();
}

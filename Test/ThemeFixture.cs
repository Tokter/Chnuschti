using Chnuschti;
using Chnuschti.Themes.Default;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Test.TestControls;

namespace Test;

public class ThemeFixture
{
    public ThemeFixture()
    {
        ThemeManager.RegisterAndApply(new TestTheme());
    }
}

public class TestTheme : DefaultTheme
{
    public TestTheme() : base()
    {
        Resources.Add<TestBlock, Style>(new TestBlockStyle());
    }
}
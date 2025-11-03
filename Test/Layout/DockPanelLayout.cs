using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Test.Layout
{
    public class DockPanelLayout : IClassFixture<ThemeFixture>
    {
        private ThemeFixture _themeFixture;
        private const float ScreenWidth = 800;
        private const float ScreenHeight = 600;

        public DockPanelLayout(ThemeFixture themeFixture)
        {
            _themeFixture = themeFixture;
        }
    }
}

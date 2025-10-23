using Chnuschti;
using Chnuschti.WindowsForms;
using Demo;
using System.Windows.Forms;

namespace WindowsFormsDemo
{
    public partial class MainWindow : Form
    {
        public MainWindow()
        {
            InitializeComponent();
            this.Shown += (s, e) =>
            {
                skglControl.Focus();
            };

            //Make sure the client size is set to the size of the form
            this.ClientSize = new Size(640, 480);

            AppBuilder
                .Configure<DemoApp>()
                .UsePlatform((app) => new Platform(this, skglControl, app))
                .Run(Environment.GetCommandLineArgs());
        }
    }
}

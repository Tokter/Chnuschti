using Chnuschti;
using Chnuschti.WindowsForms;
using Demo;

namespace WindowsFormsDemo
{
    public partial class MainWindow : Form
    {
        public MainWindow()
        {
            InitializeComponent();

            //Make sure the client size is set to the size of the form
            this.ClientSize = new Size(640, 480);

            AppBuilder
                .Configure<DemoApp>()
                .UsePlatform((app) => new Platform(skglControl, app))
                .Run(Environment.GetCommandLineArgs());
        }
    }
}

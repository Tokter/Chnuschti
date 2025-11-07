using Chnuschti;
using Chnuschti.WindowsForms;
using Demo;

namespace WindowsFormsDemo
{
    internal static class Program
    {
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            // To customize application configuration such as set high DPI settings or default font,
            // see https://aka.ms/applicationconfiguration.
            ApplicationConfiguration.Initialize();

            AppBuilder
                .Configure<DemoApp>()
                .UsePlatform((app) => new Platform(app))
                .Run(Environment.GetCommandLineArgs());
        }
    }
}
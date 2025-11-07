namespace Chnuschti;

public class AppBuilder
{
    private IPlatform? _platform;
    private string[]? _args;
    private ChnuschtiApp? _app;

    /// <summary>
    /// Configures the application with a specific implementation of ChnuschtiApp.
    /// </summary>
    /// <typeparam name="T">The type of the application implementation.</typeparam>
    /// <returns>The configured AppBuilder instance.</returns>
    public static AppBuilder Configure<T>() where T : ChnuschtiApp, new()
    {
        var builder = new AppBuilder();
        builder.UseApplication(new T());
        return builder;
    }

    /// <summary>
    /// Configures the application to use a specific platform.
    /// </summary>
    /// <param name="setupPlatform">A function that takes an instance of <see cref="ChnuschtiApp"/> and returns an implementation of <see
    /// cref="IPlatform"/>. This function is used to set up the platform for the application.</param>
    /// <returns>The current instance of <see cref="AppBuilder"/>, allowing for method chaining.</returns>
    /// <exception cref="InvalidOperationException">Thrown if the application has not been configured. Ensure that <see cref="Configure"/> is called before invoking
    /// this method.</exception>
    public AppBuilder UsePlatform(Func<ChnuschtiApp, IPlatform> setupPlatform)
    {
        if (_app == null)
        {
            throw new InvalidOperationException("Application not configured. Call Configure() first.");
        }
        _platform = setupPlatform(_app);
        return this;
    }

    /// <summary>
    /// Executes the application using the specified command-line arguments.
    /// </summary>
    /// <remarks>This method initializes the configured platform and application before execution. Ensure that
    /// both the platform and application are properly configured prior to calling this method.</remarks>
    /// <param name="args">An array of command-line arguments to pass to the application.</param>
    /// <exception cref="InvalidOperationException">Thrown if the platform is not configured or the application is not configured. Ensure <see cref="UsePlatform"/>
    /// is called to configure the platform and <see cref="Configure"/> is called to configure the application before
    /// invoking this method.</exception>
    public void Run(string[] args)
    {
        if (_platform == null)
        {
            throw new InvalidOperationException("Platform not configured. Call UsePlatform() first.");
        }
        _platform.Initialize();

        if (_app == null)
        {
            throw new InvalidOperationException("Application not configured. Call Configure() first.");
        }
        _args = args;
        _app.Configure(_platform);
        var mainWindow = _app.CreateMainWindow();
        _platform.CreateWindow(mainWindow);
        _platform.Run();
    }

    private void UseApplication(ChnuschtiApp app)
    {
        _app = app;
    }
}

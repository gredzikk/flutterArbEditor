using System.IO;
using System.Windows;

using flutterArbEditor.Models;

using Microsoft.Extensions.DependencyInjection;

namespace flutterArbEditor
{
    public partial class App : Application
    {
        public static IServiceProvider ServiceProvider { get; private set; } = null!;

        public App()
        {
            var services = new ServiceCollection();
            ConfigureServices(services);
            ServiceProvider = services.BuildServiceProvider();
        }

        private void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton<ILogger, FileLogger>();
            services.AddSingleton<MainWindow>();
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            var logger = ServiceProvider.GetRequiredService<ILogger>();
            logger.Initialize();
            logger.LogInfo("Flutter ARB Editor starting.");

            var mainWindow = ServiceProvider.GetRequiredService<MainWindow>();

            // Check if opened with a project file
            if (e.Args.Length > 0 && File.Exists(e.Args[0]))
            {
                var filePath = e.Args[0];
                if (Path.GetExtension(filePath).Equals(".aep", StringComparison.OrdinalIgnoreCase))
                {
                    try
                    {
                        var projectFile = ProjectFile.LoadFromFile(filePath);
                        // You'll need to add a method to MainViewModel to load project
                        // mainWindow.LoadProject(projectFile);
                        logger.LogInfo($"Loaded project from command line: {filePath}");
                    }
                    catch (Exception ex)
                    {
                        logger.LogError($"Failed to load project from command line: {filePath}", ex);
                    }
                }
            }

            mainWindow.Show();
        }

        protected override void OnExit(ExitEventArgs e)
        {
            var logger = ServiceProvider.GetRequiredService<ILogger>();
            logger.LogInfo("Flutter ARB Editor exiting.");
            base.OnExit(e);
        }
    }
}
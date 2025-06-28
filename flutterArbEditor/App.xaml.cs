using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using flutterArbEditor.ViewModels;

namespace flutterArbEditor
{
    public partial class App : Application
    {
        public static IServiceProvider ServiceProvider { get; private set; }

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
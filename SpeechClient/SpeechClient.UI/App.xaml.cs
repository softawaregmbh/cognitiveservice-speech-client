using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using softaware.ViewPort.Core;
using softaware.ViewPort.Wpf;
using SpeechClient.Audio;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace SpeechClient.UI
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        /// <summary>
        /// Gets the service provider.
        /// </summary>
        public IServiceProvider ServiceProvider { get; private set; }

        /// <summary>
        /// Gets the configuration.
        /// </summary>
        public IConfiguration Configuration { get; private set; }

        /// <summary>
        /// Raises the <see cref="E:System.Windows.Application.Startup" /> event.
        /// </summary>
        /// <param name="e">A <see cref="T:System.Windows.StartupEventArgs" /> that contains the event data.</param>
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            var builder = new ConfigurationBuilder()
                 .SetBasePath(Directory.GetCurrentDirectory())
                 .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                 .AddJsonFile("appsettings.dev.json", optional: true, reloadOnChange: true);

            this.Configuration = builder.Build();

            var serviceCollection = new ServiceCollection();
            this.ConfigureServices(serviceCollection);

            this.ServiceProvider = serviceCollection.BuildServiceProvider();

            var mainWindow = this.ServiceProvider.GetRequiredService<MainWindow>();
            mainWindow.Show();
        }

        private void ConfigureServices(IServiceCollection services)
        {
            services.Configure<SpeechRecognizerSettings>(this.Configuration.GetSection(nameof(SpeechRecognizerSettings)));
            services.AddSingleton<SpeechRecognizer>();

            services.AddSingleton<IUIThread, WpfUIThread>();

            services.AddScoped<MainViewModel>();
            services.AddTransient(typeof(MainWindow));
        }
    }
}

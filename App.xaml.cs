using CefSharp;
using CefSharp.Wpf;
using System.Windows;

namespace Naviguard
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            var settings = new CefSettings()
            {
                // Guarda la caché en disco para cargas más rápidas en visitas repetidas
                CachePath = System.IO.Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, "cache"),
                // Reduce la cantidad de logs para mejorar el rendimiento
                LogSeverity = LogSeverity.Disable
            };

            // Inicializa CefSharp con las configuraciones
            Cef.Initialize(settings, performDependencyCheck: true, browserProcessHandler: null);

            base.OnStartup(e);
        }

        protected override void OnExit(ExitEventArgs e)
        {
            // Apaga CefSharp correctamente al cerrar la aplicación
            Cef.Shutdown();
            base.OnExit(e);
        }
    }
}

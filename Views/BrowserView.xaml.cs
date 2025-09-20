using System.Windows.Controls;
using CefSharp.Core;
using Naviguard.Handlers;
using Naviguard.Proxy;
using Naviguard.ViewModels;

namespace Naviguard.Views
{
    public partial class BrowserView : UserControl
    {
        public BrowserView()
        {
            InitializeComponent();
           /* this.Loaded += BrowserView_Loaded;*/
        }

       /*private void BrowserView_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            if (DataContext is BrowserViewModel vm)
            {
                if (vm.RequiresProxy)
                {
                    ApplyProxyIfNeeded();
                }

                // Asigna la URL al navegador
                Browser.Address = vm.Url;
            }
        }

        private void ApplyProxyIfNeeded()
        {
            var proxyManager = new ProxyManager();
            var proxyInfo = proxyManager.GetProxy();

            if (proxyInfo != null)
            {
                var rcSettings = new RequestContextSettings();
                rcSettings.PersistSessionCookies = true;
                rcSettings.PersistUserPreferences = true;

                // ✅ Aquí se pasa el proxy como argumento de Chromium
                rcSettings.SetPreference("proxy", new
                {
                    mode = "fixed_servers",
                    server = proxyInfo.GetProxyString()
                });

                var rc = new RequestContext(rcSettings);

                Browser.RequestHandler = new RequestHandler(proxyInfo);
                Browser.RequestContext = rc;
            }
        }*/

    }
}
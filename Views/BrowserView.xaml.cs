using CefSharp;
using Naviguard.Handlers;
using Naviguard.Models;
using System.Windows.Controls;
using Cef = CefSharp.Cef;
using System.Windows;

namespace Naviguard.Views
{
    public partial class BrowserView : UserControl
    {

        public BrowserView()
        {
            InitializeComponent();
            Browser.LoadingStateChanged += Browser_LoadingStateChanged;

        }
        public void LoadPage(Pagina pagina, ProxyInfo? proxyInfo = null)
        {
            if (pagina.requires_proxy && proxyInfo != null)
            {
                var proxySettings = new Dictionary<string, object>
                {
                    ["mode"] = "fixed_servers",
                    ["server"] = proxyInfo.GetProxyString()
                };

                var requestContext = new CefSharp.RequestContext(new CefSharp.RequestContextSettings());

                // 🔥 Ejecutar en el hilo de CEF
                Cef.UIThreadTaskFactory.StartNew(() =>
                {
                    bool success = requestContext.SetPreference("proxy", proxySettings, out string error);
                });

                Browser.RequestContext = requestContext;
                Browser.RequestHandler = new RequestHandler(proxyInfo);
            }
            else
            {
                Browser.RequestHandler = null;
            }
            Browser.Load(pagina.url);
        }

        private void Browser_LoadingStateChanged(object sender, LoadingStateChangedEventArgs e)
        {
            Dispatcher.Invoke(() =>
            {
                if (e.IsLoading)
                {
                    LoadingOverlay.Visibility = Visibility.Visible;
                }
                else
                {
                    LoadingOverlay.Visibility = Visibility.Collapsed;
                }
            });
        }

    }

}
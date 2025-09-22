using CefSharp;
using Naviguard.Handlers;
using Naviguard.Models;
using Naviguard.Proxy;
using Naviguard.Repositories;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using Cef = CefSharp.Cef;

namespace Naviguard.Views
{
    public partial class BrowserView : UserControl
    {
        private PageCredential? _credencialActual;
        private Pagina _paginaActual;
        private bool _isAutoLoginRunning = false;
        public BrowserView()
        {
            InitializeComponent();
            Browser.LoadingStateChanged += Browser_LoadingStateChanged;
        }
        public void LoadPage(Pagina pagina, ProxyInfo? proxyInfo = null)
        {
            _paginaActual = pagina;

            // 🚀 Buscar credenciales en la BD
            var credRepo = new PageCredentialRepository();
            _credencialActual = credRepo.ObtenerCredencialPorPagina(pagina.page_id);

            if (pagina.requires_proxy && proxyInfo != null)
            {
                var proxySettings = new Dictionary<string, object>
                {
                    ["mode"] = "fixed_servers",
                    ["server"] = proxyInfo.GetProxyString()
                };

                var requestContext = new CefSharp.RequestContext(new CefSharp.RequestContextSettings());

                Cef.UIThreadTaskFactory.StartNew(() =>
                {
                    requestContext.SetPreference("proxy", proxySettings, out string error);
                });

                Browser.RequestContext = requestContext;
                Browser.RequestHandler = new RequestHandler(proxyInfo);
            }
            else
            {
                Browser.RequestHandler = null;
            }

            Browser.JsDialogHandler = new JsDialogHandler();
            Browser.MenuHandler = new NoContextMenuHandler();
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
                    if (_credencialActual != null)
                    {
                        _isAutoLoginRunning = true;
                        EjecutarAutoLogin();
                    }
                    else
                    {
                        LoadingOverlay.Visibility = Visibility.Collapsed;
                    }
                }
            });
        }
        private async void EjecutarAutoLogin()
        {
            try
            {
                // Esperar a que DOM esté listo
                await Browser.GetMainFrame().EvaluateScriptAsync($@"
                    document.getElementById('txtemail').value = '{_credencialActual.Username}';
                    document.getElementById('txtpas').value = '{_credencialActual.Password}';
                    document.getElementById('txtcarac').value = document.getElementById('txtcodcarac').value;
                    document.querySelector('.btn_access').click();
                ");
                Dispatcher.Invoke(() =>
                {
                    LoadingOverlay.Visibility = Visibility.Collapsed;
                    _isAutoLoginRunning = false;
                });
            }
            catch (Exception ex)
            {
                Dispatcher.Invoke(() =>
                {
                    LoadingOverlay.Visibility = Visibility.Collapsed;
                    _isAutoLoginRunning = false;
                });
            }
        }

    }

}
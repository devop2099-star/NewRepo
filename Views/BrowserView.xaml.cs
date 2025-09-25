using CefSharp;
using Naviguard.Handlers;
using Naviguard.Models;
using Naviguard.Repositories;
using System.Windows.Controls;

namespace Naviguard.Views
{
    public partial class BrowserView : UserControl
    {
        private Pagina _paginaActual;
        private PageCredential? _credencialActual;
        private bool _isAutoLoginRunning = false;

        public BrowserView()
        {
            InitializeComponent();
            Browser.FrameLoadEnd += Browser_FrameLoadEnd;
            Browser.JsDialogHandler = new JsDialogHandler();
            Browser.MenuHandler = new NoContextMenuHandler();
        }

        public async Task LoadPage(Pagina pagina, ProxyInfo? proxyInfo = null)
        {
            _paginaActual = pagina;
            var credRepo = new PageCredentialRepository();

            // CORRECCIÓN 2: El 'await' ahora funciona correctamente
            _credencialActual = await credRepo.ObtenerCredencialPorPaginaAsync(pagina.page_id);
            _isAutoLoginRunning = false;

            // Tu lógica de proxy aquí...
            if (pagina.requires_proxy && proxyInfo != null)
            {
                var requestContext = Browser.RequestContext ?? new RequestContext();
                await Cef.UIThreadTaskFactory.StartNew(async () =>
                {
                    await requestContext.SetProxyAsync(proxyInfo.Host, proxyInfo.Port);
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


        private void Browser_FrameLoadEnd(object sender, FrameLoadEndEventArgs e)
        {
            if (e.Frame.IsMain)
            {
                if (_credencialActual != null && !_isAutoLoginRunning)
                {
                    _isAutoLoginRunning = true;
                    EjecutarAutoLogin();
                }
            }
        }

        private async void EjecutarAutoLogin()
        {
            try
            {
                await Browser.GetMainFrame().EvaluateScriptAsync($@"
                    document.getElementById('txtemail').value = '{_credencialActual.Username}';
                    document.getElementById('txtpas').value = '{_credencialActual.Password}';
                    document.getElementById('txtcarac').value = document.getElementById('txtcodcarac').value;
                    document.querySelector('.btn_access').click();
                ");
            }
            catch
            {
            }
            finally
            {
                _isAutoLoginRunning = false;
            }
        }
    }
}
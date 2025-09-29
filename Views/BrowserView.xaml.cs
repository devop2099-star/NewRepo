using CefSharp;
using Naviguard.Handlers;
using Naviguard.Models;
using Naviguard.Repositories;
using System.Diagnostics;
using System.Windows.Controls;

namespace Naviguard.Views
{
    public partial class BrowserView : UserControl
    {
        private Pagina _paginaActual;
        private (string Username, string Password)? _loginCredentials;
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
            Debug.WriteLine($"[BrowserView] Iniciando LoadPage para: '{pagina.page_name}' (ID: {pagina.page_id})");

            _paginaActual = pagina;
            _loginCredentials = null;
            _isAutoLoginRunning = false;

            try
            {
                if (UserSession.IsLoggedIn)
                {
                    if (pagina.requires_custom_login)
                    {
                        Debug.WriteLine($"[BrowserView] 🔎 Buscando credencial PERSONALIZADA para User: {UserSession.ApiUserId}, Page: {pagina.page_id}");
                        var customCredRepo = new CredentialRepository();
                        var customCredential = await customCredRepo.GetCredentialAsync(UserSession.ApiUserId, pagina.page_id);
                        if (customCredential != null)
                        {
                            _loginCredentials = (customCredential.username, customCredential.password);
                            Debug.WriteLine($"[BrowserView] ✅ Credencial PERSONALIZADA encontrada. Usuario: '{_loginCredentials.Value.Username}'");
                        }
                        else
                        {
                            Debug.WriteLine($"[BrowserView] ❌ NO se encontró credencial PERSONALIZADA.");
                        }
                    }
                    else if (pagina.requires_login)
                    {
                        Debug.WriteLine($"[BrowserView] 🔎 Buscando credencial GENERAL para Page: {pagina.page_id}");
                        var generalCredRepo = new PageCredentialRepository();
                        var generalCredential = await generalCredRepo.ObtenerCredencialPorPaginaAsync(pagina.page_id);
                        if (generalCredential != null)
                        {
                            _loginCredentials = (generalCredential.Username, generalCredential.Password);
                            Debug.WriteLine($"[BrowserView] ✅ Credencial GENERAL encontrada. Usuario: '{_loginCredentials.Value.Username}'");
                        }
                        else
                        {
                            Debug.WriteLine($"[BrowserView] ❌ NO se encontró credencial GENERAL.");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[BrowserView] 💥 ERROR AL BUSCAR CREDENCIALES: {ex.Message}");
            }

            Debug.WriteLine($"[BrowserView] Resumen: ¿Credenciales listas para usar? -> {(_loginCredentials.HasValue ? "Sí" : "No")}");

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
            Browser.LifeSpanHandler = new BlockPopupsHandler();
            Browser.Load(pagina.url);
        }

        private void Browser_FrameLoadEnd(object sender, FrameLoadEndEventArgs e)
        {
            if (e.Frame.IsMain)
            {
                Debug.WriteLine($"[BrowserView] FrameLoadEnd para: {e.Url}. ¿Hay credenciales?: {(_loginCredentials.HasValue ? "Sí" : "No")}");
                if (_loginCredentials != null && !_isAutoLoginRunning)
                {
                    _isAutoLoginRunning = true;
                    Debug.WriteLine("[BrowserView] Ejecutando AutoLogin...");
                    EjecutarAutoLogin();
                }
            }
        }

       private async void EjecutarAutoLogin()
        {
            if (!_loginCredentials.HasValue) return; 

            try
            {
                Debug.WriteLine($"[BrowserView] 💉 Inyectando login con Usuario: '{_loginCredentials.Value.Username}'");

                string script = $@"
                    document.getElementById('txtemail').value = '{_loginCredentials.Value.Username}';
                    document.getElementById('txtpas').value = '{_loginCredentials.Value.Password}';
                    document.getElementById('txtcarac').value = document.getElementById('txtcodcarac').value;
                    document.querySelector('.btn_access').click();
                ";

                Debug.WriteLine($"[BrowserView] 📋 Ejecutando script JS:\n{script}");

                await Browser.GetMainFrame().EvaluateScriptAsync(script);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[BrowserView] 💥 Error en EjecutarAutoLogin: {ex.Message}");
            }
            finally
            {
                _isAutoLoginRunning = false;
            }
        }
    }
}
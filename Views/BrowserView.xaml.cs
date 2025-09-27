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

            // ===== INICIO DE CAMBIO IMPORTANTE: TRY-CATCH =====
            try
            {
                // Lógica de decisión de credenciales
                if (UserSession.IsLoggedIn)
                {
                    if (pagina.requires_custom_login)
                    {
                        var customCredRepo = new CredentialRepository();
                        var customCredential = await customCredRepo.GetCredentialAsync(UserSession.ApiUserId, pagina.page_id);
                        if (customCredential != null)
                        {
                            _loginCredentials = (customCredential.username, customCredential.password);
                        }
                    }
                    else if (pagina.requires_login)
                    {
                        var generalCredRepo = new PageCredentialRepository();
                        var generalCredential = await generalCredRepo.ObtenerCredencialPorPaginaAsync(pagina.page_id);
                        if (generalCredential != null)
                        {
                            _loginCredentials = (generalCredential.Username, generalCredential.Password);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // Si algo falla al buscar credenciales, lo veremos aquí
                Debug.WriteLine($"[BrowserView] ERROR AL BUSCAR CREDENCIALES: {ex.Message}");
            }
            // ===== FIN DE CAMBIO IMPORTANTE: TRY-CATCH =====

            Debug.WriteLine($"[BrowserView] Credenciales encontradas: {(_loginCredentials.HasValue ? "Sí" : "No")}");

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
            Debug.WriteLine($"[BrowserView] Cargando URL: {pagina.url}");
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
            try
            {
                await Browser.GetMainFrame().EvaluateScriptAsync($@"
                    document.getElementById('txtemail').value = '{_loginCredentials.Value.Username}';
                    document.getElementById('txtpas').value = '{_loginCredentials.Value.Password}';
                    document.getElementById('txtcarac').value = document.getElementById('txtcodcarac').value;
                    document.querySelector('.btn_access').click();
                ");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[BrowserView] Error en EjecutarAutoLogin: {ex.Message}");
            }
            finally
            {
                _isAutoLoginRunning = false;
            }
        }
    }
}
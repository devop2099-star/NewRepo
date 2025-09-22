using Cef = CefSharp.Cef;
using Naviguard.Handlers;
using Naviguard.Models.Naviguard.Models;
using Naviguard.Proxy;
using System.Diagnostics;
using System.Windows.Controls;

namespace Naviguard.Views
{
    public partial class BrowserView : UserControl
    {

        public BrowserView()
        {
            InitializeComponent();

        }
        public void LoadPage(Pagina pagina, ProxyManager.ProxyInfo? proxyInfo = null)
        {
            if (pagina.RequiresProxy && proxyInfo != null)
            {
                Debug.WriteLine($"🌐 Aplicando proxy: {proxyInfo.GetProxyString()} para la página {pagina.NombrePagina}");

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
                    Debug.WriteLine(success
                        ? $"✅ Proxy aplicado correctamente ({proxyInfo.GetProxyString()})"
                        : $"⚠️ Error al aplicar proxy: {error}");
                });

                Browser.RequestContext = requestContext;
                Browser.RequestHandler = new RequestHandler(proxyInfo);
            }
            else
            {
                Debug.WriteLine($"⚡ Cargando sin proxy la página: {pagina.NombrePagina}");
                Browser.RequestHandler = null;
            }

            Debug.WriteLine($"➡️ Navegando a: {pagina.Url}");
            Browser.Load(pagina.Url);
        }



    }

}
using CefSharp;
using CefSharp.Core;
using Naviguard.Handlers;
using Naviguard.Models.Naviguard.Models;
using Naviguard.Proxy;
using Naviguard.ViewModels;
using System.Security.Policy;
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
                Browser.RequestHandler = new RequestHandler(proxyInfo);
            }
            else
            {
                Browser.RequestHandler = null; // sin proxy
            }

            Browser.Load(pagina.Url);
        }


    }

}
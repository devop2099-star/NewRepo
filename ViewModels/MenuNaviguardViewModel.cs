using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Naviguard.Models;
using Naviguard.Models.Naviguard.Models;
using Naviguard.Proxy;
using Naviguard.Repositories;
using Naviguard.Views;
using System.Collections.ObjectModel;
using static Naviguard.Proxy.ProxyManager;

namespace Naviguard.ViewModels
{
    public partial class MenuNaviguardViewModel : ObservableObject
    {
        [ObservableProperty]
        private object _currentContentViewModel;

        public ObservableCollection<Pagina> Paginas { get; set; }
        private readonly PaginaRepository _paginaRepository;

        public MenuNaviguardViewModel()
        {
            _paginaRepository = new PaginaRepository();
            Paginas = new ObservableCollection<Pagina>();
            CargarPaginas();

            if (Paginas.Count > 0)
            {
                Navigate(Paginas[0]);
            }
        }

        private void CargarPaginas()
        {
            try
            {
                var paginasDesdeDb = _paginaRepository.ObtenerPaginas();
                foreach (var pagina in paginasDesdeDb)
                {
                    Paginas.Add(pagina);
                }
            }
            catch (System.Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error al cargar páginas desde la BD: {ex.Message}");
            }
        }

        [RelayCommand]
        private void Navigate(Pagina pagina)
        {
            if (pagina == null || string.IsNullOrEmpty(pagina.Url)) return;

            var browserView = new BrowserView();

            ProxyManager.ProxyInfo? proxyInfo = null;
            if (pagina.RequiresProxy)
            {
                var proxyManager = new ProxyManager();
                proxyInfo = proxyManager.GetProxy();
            }

            browserView.LoadPage(pagina, proxyInfo);

            CurrentContentViewModel = browserView;
        }
    }
}
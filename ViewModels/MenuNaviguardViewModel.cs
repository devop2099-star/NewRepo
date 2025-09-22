using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Naviguard.Models;
using Naviguard.Proxy;
using Naviguard.Repositories;
using Naviguard.Views;
using System.Collections.ObjectModel;
using System.Text.RegularExpressions;

namespace Naviguard.ViewModels
{
    public partial class MenuNaviguardViewModel : ObservableObject
    {
        [ObservableProperty]
        private object _currentContentViewModel;
        public ObservableCollection<Group> Grupos { get; set; }
        private readonly GrupoRepository _grupoRepository;

        public MenuNaviguardViewModel()
        {
            _grupoRepository = new GrupoRepository();
            Grupos = new ObservableCollection<Group>();
            CargarGrupos();
        }

        private void CargarGrupos()
        {
            try
            {
                var gruposDesdeDb = _grupoRepository.ObtenerGruposConPaginas();
                foreach (var grupo in gruposDesdeDb)
                {
                    Grupos.Add(grupo);
                }
            }
            catch (System.Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error al cargar grupos desde la BD: {ex.Message}");
            }
        }
            
        [RelayCommand]
        private void Navigate(Pagina pagina)
        {
            if (pagina == null || string.IsNullOrEmpty(pagina.url)) return;

            var browserView = new BrowserView();

            ProxyInfo? proxyInfo = null;
            if (pagina.requires_proxy)
            {
                var proxyManager = new ProxyManager();
                proxyInfo = proxyManager.GetProxy();
            }

            PageCredential? credencial = null;
            var credRepo = new PageCredentialRepository();
            credencial = credRepo.ObtenerCredencialPorPagina(pagina.page_id);

            browserView.LoadPage(pagina, proxyInfo);

            CurrentContentViewModel = browserView;
        }
    }
}
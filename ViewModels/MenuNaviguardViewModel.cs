using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Naviguard.Models;
using Naviguard.Proxy;
using Naviguard.Repositories;
using Naviguard.Views;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows;

namespace Naviguard.ViewModels
{
    public partial class MenuNaviguardViewModel : ObservableObject
    {
        [ObservableProperty]
        private object _currentBrowserView;

        public ObservableCollection<Pagina> PaginasDelGrupo { get; set; }
        public ObservableCollection<Pagina> PestañasAbiertas { get; set; }

        [ObservableProperty]
        private Pagina _pestañaSeleccionada;

        private readonly GrupoRepository _grupoRepository;
        private readonly long _groupId;

        public MenuNaviguardViewModel(long groupId)
        {
            _grupoRepository = new GrupoRepository();
            _groupId = groupId;
            Debug.WriteLine($"[MenuNaviguardViewModel] ViewModel creado para el Group ID: {_groupId}");
            PaginasDelGrupo = new ObservableCollection<Pagina>();
            PestañasAbiertas = new ObservableCollection<Pagina>();
            CargarPaginasDelGrupo();
        }

        private void CargarPaginasDelGrupo()
        {
            var paginas = _grupoRepository.ObtenerPaginasPorGrupo(_groupId);
            Debug.WriteLine($"[MenuNaviguardViewModel] Se encontraron {paginas.Count} páginas para el Group ID: {_groupId}");

            PaginasDelGrupo.Clear();
            foreach (var p in paginas)
            {
                PaginasDelGrupo.Add(p);
            }

            Debug.WriteLine("[MenuNaviguardViewModel] Buscando páginas fijadas para abrir automáticamente...");
            foreach (var paginaFijada in paginas.Where(p => p.pin_in_group == 1))
            {
                Debug.WriteLine($" -> Abriendo página fijada: '{paginaFijada.page_name}'");
                AbrirPagina(paginaFijada);
            }
        }

        [RelayCommand]
        private void AbrirPagina(Pagina pagina)
        {
            if (pagina == null) return;

            if (!PestañasAbiertas.Any(p => p.page_id == pagina.page_id))
            {
                if (PestañasAbiertas.Count >= 5)
                {
                   Debug.WriteLine("No puedes abrir más de 5 pestañas.", "Límite de Pestañas Alcanzado", MessageBoxButton.OK, MessageBoxImage.Information);
                    return; 
                }
                PestañasAbiertas.Add(pagina);
            }
            PestañaSeleccionada = pagina;
        }

        [RelayCommand]
        private void CerrarPestaña(Pagina pagina)
        {
            if (pagina == null) return;
            PestañasAbiertas.Remove(pagina);
        }

        partial void OnPestañaSeleccionadaChanged(Pagina value)
        {
            if (value != null)
            {
                var browserView = new BrowserView();
                ProxyInfo? proxyInfo = null;
                if (value.requires_proxy)
                {
                    var proxyManager = new ProxyManager();
                    proxyInfo = proxyManager.GetProxy();
                }

                browserView.LoadPage(value, proxyInfo);

                CurrentBrowserView = browserView;
            }
            else
            {
                CurrentBrowserView = null;
            }
        }

    }
}
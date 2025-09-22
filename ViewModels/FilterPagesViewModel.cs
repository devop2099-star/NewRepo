// ViewModels/FilterPagesViewModel.cs

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Naviguard.Models;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

public class FilterPagesViewModel : ObservableObject
{
    // --- Propiedades (sin cambios) ---
    private string _pageName;
    public string PageName { get => _pageName; set => SetProperty(ref _pageName, value); }

    private string _pageDescription;
    public string PageDescription { get => _pageDescription; set => SetProperty(ref _pageDescription, value); }

    private string _pageUrl;
    public string PageUrl { get => _pageUrl; set => SetProperty(ref _pageUrl, value); }

    private bool _requiresProxy;
    public bool RequiresProxy { get => _requiresProxy; set => SetProperty(ref _requiresProxy, value); }

    private bool _requiresLogin;
    public bool RequiresLogin { get => _requiresLogin; set => SetProperty(ref _requiresLogin, value); }

    private string _groupName;
    public string GroupName { get => _groupName; set => SetProperty(ref _groupName, value); }

    private string _groupDescription;
    public string GroupDescription { get => _groupDescription; set => SetProperty(ref _groupDescription, value); }

    public ObservableCollection<PageListItemViewModel> AvailablePages { get; }

    public IAsyncRelayCommand SavePageCommand { get; }
    public IAsyncRelayCommand CreateGroupCommand { get; }

    public FilterPagesViewModel()
    {
        AvailablePages = new ObservableCollection<PageListItemViewModel>();
        SavePageCommand = new AsyncRelayCommand(SavePageAsync);
        CreateGroupCommand = new AsyncRelayCommand(CreateGroupAsync);

        LoadAvailablePages();
    }

    // ===== MÉTODO COMPLETADO =====
    private void LoadAvailablePages()
    {
        // Simula la carga de datos desde la base de datos
        var pagesFromDb = new[]
        {
            new Page { page_id = 1, page_name = "Google" },
            new Page { page_id = 2, page_name = "Facebook" },
            new Page { page_id = 3, page_name = "GitHub" }
        };

        AvailablePages.Clear();
        foreach (var page in pagesFromDb)
        {
            AvailablePages.Add(new PageListItemViewModel(page));
        }
    }

    // ===== MÉTODO COMPLETADO =====
    private async Task SavePageAsync()
    {
        if (string.IsNullOrWhiteSpace(PageName) || string.IsNullOrWhiteSpace(PageUrl))
        {
            MessageBox.Show("El nombre y la URL de la página son obligatorios.", "Error");
            return;
        }

        var newPage = new Page
        {
            page_name = this.PageName,
            description = this.PageDescription,
            url = this.PageUrl,
            requires_proxy = this.RequiresProxy,
            requires_login = this.RequiresLogin,
            state = 1,
            created_at = DateTime.Now
        };

        // Aquí iría tu lógica para guardar 'newPage' en la base de datos
        MessageBox.Show($"Página '{newPage.page_name}' guardada con éxito.", "Éxito");

        PageName = string.Empty;
        PageDescription = string.Empty;
        PageUrl = string.Empty;
        RequiresProxy = false;
        RequiresLogin = false;
        LoadAvailablePages(); 
    }

    private async Task CreateGroupAsync()
    {
        if (string.IsNullOrWhiteSpace(GroupName))
        {
            MessageBox.Show("El nombre del grupo es obligatorio.", "Error de Validación");
            return;
        }

        var newGroup = new PageGroup
        {
            group_name = this.GroupName,
            description = this.GroupDescription
        };

        long newGroupId = new Random().Next(100, 1000);

        var selectedPages = AvailablePages.Where(p => p.IsSelected).ToList();

        foreach (var pageVM in selectedPages)
        {
            Console.WriteLine($"Asignando Página ID {pageVM.PageData.page_id} a Grupo ID {newGroupId}");
        }

        MessageBox.Show($"Grupo '{newGroup.group_name}' creado con {selectedPages.Count} página(s) asignada(s).", "Éxito");

        GroupName = string.Empty;
        GroupDescription = string.Empty;
        foreach (var pageVM in AvailablePages)
        {
            pageVM.IsSelected = false;
        }



    }
}
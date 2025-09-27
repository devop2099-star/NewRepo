using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Naviguard.Models;
using Naviguard.Repositories;
using System.Collections.ObjectModel;
using System.Windows;

public class FilterPagesViewModel : ObservableObject
{
    private readonly PaginaRepository _paginaRepository;

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

    private bool _requiresCustomLogin;
    public bool RequiresCustomLogin { get => _requiresCustomLogin; set => SetProperty(ref _requiresCustomLogin, value); }

    private string _groupName;
    public string GroupName { get => _groupName; set => SetProperty(ref _groupName, value); }

    private string _groupDescription;
    public string GroupDescription { get => _groupDescription; set => SetProperty(ref _groupDescription, value); }

    private string _credentialUsername;
    public string CredentialUsername { get => _credentialUsername; set => SetProperty(ref _credentialUsername, value); }
    private string _credentialPassword;
    public string CredentialPassword { get => _credentialPassword; set => SetProperty(ref _credentialPassword, value); }

    public ObservableCollection<PageListItemViewModel> AvailablePages { get; }

    public IAsyncRelayCommand SavePageCommand { get; }
    public IAsyncRelayCommand CreateGroupCommand { get; }

    public FilterPagesViewModel()
    {
        _paginaRepository = new PaginaRepository();
        AvailablePages = new ObservableCollection<PageListItemViewModel>();
        SavePageCommand = new AsyncRelayCommand(SavePageAsync);
        CreateGroupCommand = new AsyncRelayCommand(CreateGroupAsync);
        LoadAvailablePages();
    }

    private void LoadAvailablePages()
    {
        try
        {
            var paginasFromDb = _paginaRepository.ObtenerPaginas();

            Application.Current.Dispatcher.Invoke(() =>
            {
                AvailablePages.Clear();
                foreach (var pagina in paginasFromDb) 
                {
                    AvailablePages.Add(new PageListItemViewModel(pagina));
                }
            });
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error al cargar las páginas desde la base de datos: {ex.Message}", "Error de Conexión");
        }
    }

    private async Task SavePageAsync()
    {
        if (string.IsNullOrWhiteSpace(PageName) || string.IsNullOrWhiteSpace(PageDescription) || string.IsNullOrWhiteSpace(PageUrl))
        {
            MessageBox.Show("El Nombre, la Descripción y la URL de la página son obligatorios.", "Error de Validación");
            return;
        }

        var newPage = new Pagina
        {
            page_name = this.PageName,
            description = this.PageDescription,
            url = this.PageUrl,
            requires_proxy = this.RequiresProxy,
            requires_login = this.RequiresLogin,
            requires_custom_login = this.RequiresCustomLogin, 
            state = 1,
            created_at = DateTime.UtcNow
        };
        try
        {
            long newPageId = await _paginaRepository.AddPageAsync(newPage);
            if (newPage.requires_login && !string.IsNullOrWhiteSpace(this.CredentialUsername))
            {
                await _paginaRepository.AddCredentialAsync(newPageId, this.CredentialUsername, this.CredentialPassword);
            }

            MessageBox.Show($"Página '{newPage.page_name}' guardada con éxito.", "Éxito");

            PageName = string.Empty;
            PageDescription = string.Empty;
            PageUrl = string.Empty;
            RequiresProxy = false;
            RequiresLogin = false;
            RequiresCustomLogin = false; 

            LoadAvailablePages();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error al guardar la página: {ex.Message}", "Error de Base de Datos");
        }
    }

    private async Task CreateGroupAsync()
    {
        if (string.IsNullOrWhiteSpace(GroupName) || string.IsNullOrWhiteSpace(GroupDescription))
        {
            MessageBox.Show("El Nombre y la Descripción del grupo son obligatorios.", "Error de Validación");
            return;
        }

        var selectedPageVMs = AvailablePages.Where(p => p.IsSelected).ToList();
        if (selectedPageVMs.Count == 0)
        {
            MessageBox.Show("Debes seleccionar al menos una página para el grupo.", "Error de Validación");
            return;
        }

        try
        {
            var selectedPageIds = selectedPageVMs.Select(p => p.PageData.page_id).ToList();
            long newGroupId = await _paginaRepository.AddGroupAsync(this.GroupName, this.GroupDescription);
            await _paginaRepository.AddPagesToGroupAsync(newGroupId, selectedPageIds);
            MessageBox.Show($"Grupo '{GroupName}' creado con {selectedPageVMs.Count} página(s) asignada(s).", "Éxito");

            GroupName = string.Empty;
            GroupDescription = string.Empty;
            foreach (var pageVM in AvailablePages)
            {
                pageVM.IsSelected = false;
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error al crear el grupo: {ex.Message}", "Error de Base de Datos");
        }
    }
}

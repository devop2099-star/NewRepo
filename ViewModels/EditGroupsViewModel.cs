using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Naviguard.Models;
using Naviguard.Repositories;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows;

namespace Naviguard.ViewModels
{
    public partial class EditGroupsViewModel : ObservableObject
    {
        private readonly GrupoRepository _grupoRepository;
        private readonly PaginaRepository _paginaRepository;

        // --- Propiedad para controlar el modo de edición ---
        [ObservableProperty]
        private bool _isEditingPages;

        // --- Propiedades para Búsqueda y Selección ---
        [ObservableProperty]
        private string _searchText;
        private Group _selectedGroup;
        private Pagina _selectedPage;

        // --- Colecciones para las listas ---
        [ObservableProperty] private ObservableCollection<Group> _allGroups;
        [ObservableProperty] private ObservableCollection<Group> _filteredGroups;
        [ObservableProperty] private ObservableCollection<Pagina> _allPages;
        [ObservableProperty] private ObservableCollection<Pagina> _filteredPages;

        // --- Propiedades para el formulario de EDICIÓN DE GRUPOS ---
        [ObservableProperty] private string _editGroupName;
        [ObservableProperty] private string _editGroupDescription;
        [ObservableProperty] private bool _editGroupIsPinned;

        // --- Propiedades para el formulario de EDICIÓN DE PÁGINAS ---
        [ObservableProperty] private string _editPageName;
        [ObservableProperty] private string _editPageDescription;
        [ObservableProperty] private string _editPageUrl;
        [ObservableProperty] private bool _editRequiresProxy;
        [ObservableProperty] private bool _editRequiresLogin;
        [ObservableProperty] private bool _editRequiresCustomLogin;
        [ObservableProperty] private string _editCredentialUsername;
        [ObservableProperty] private string _editCredentialPassword;

        // --- Visibilidad de los paneles ---
        public bool IsGroupSelected => SelectedGroup != null;
        public bool IsPageSelected => SelectedPage != null;

        // --- Comandos ---
        public IAsyncRelayCommand UpdateGroupCommand { get; }
        public IAsyncRelayCommand UpdatePageCommand { get; }

        [ObservableProperty] private ObservableCollection<SelectablePageViewModel> _allPagesChecklist;
        public IRelayCommand<SelectablePageViewModel> TogglePinPageInGroupCommand { get; }
       
        public IAsyncRelayCommand<Group> DeleteGroupCommand { get; }
        public IAsyncRelayCommand<Pagina> DeletePageCommand { get; }

        public EditGroupsViewModel()
        {
            _grupoRepository = new GrupoRepository();
            _paginaRepository = new PaginaRepository();

            AllGroups = new();
            FilteredGroups = new();
            AllPages = new();
            FilteredPages = new();
            AllPagesChecklist = new();
            TogglePinPageInGroupCommand = new RelayCommand<SelectablePageViewModel>(TogglePinPageInGroup);
            UpdateGroupCommand = new AsyncRelayCommand(UpdateGroupAsync, () => IsGroupSelected);
            UpdatePageCommand = new AsyncRelayCommand(UpdatePageAsync, () => IsPageSelected);

            DeleteGroupCommand = new AsyncRelayCommand<Group>(DeleteGroup);
            DeletePageCommand = new AsyncRelayCommand<Pagina>(DeletePage);

            LoadGroupData();
        }

        private async Task DeleteGroup(Group groupToDelete)
        {
            if (groupToDelete == null) return;

            var result = MessageBox.Show($"¿Estás seguro de que quieres eliminar el grupo '{groupToDelete.group_name}'?",
                                         "Confirmar Eliminación",
                                         MessageBoxButton.YesNo,
                                         MessageBoxImage.Warning);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    await _grupoRepository.SoftDeleteGroupAsync(groupToDelete.group_id);

                    // Eliminar de las listas en la UI para una actualización instantánea
                    AllGroups.Remove(groupToDelete);
                    FilteredGroups.Remove(groupToDelete);

                    if (SelectedGroup == groupToDelete)
                    {
                        SelectedGroup = null; // Limpiar el panel de edición si el grupo eliminado estaba seleccionado
                    }

                    MessageBox.Show("Grupo eliminado correctamente.", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error al eliminar el grupo: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private async Task DeletePage(Pagina pageToDelete)
        {
            if (pageToDelete == null) return;

            var result = MessageBox.Show($"¿Estás seguro de que quieres eliminar la página '{pageToDelete.page_name}'?",
                                         "Confirmar Eliminación",
                                         MessageBoxButton.YesNo,
                                         MessageBoxImage.Warning);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    await _paginaRepository.SoftDeletePageAsync(pageToDelete.page_id);

                    AllPages.Remove(pageToDelete);
                    FilteredPages.Remove(pageToDelete);

                    if (SelectedPage == pageToDelete)
                    {
                        SelectedPage = null; // Limpiar el panel de edición
                    }

                    MessageBox.Show("Página eliminada correctamente.", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error al eliminar la página: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        partial void OnIsEditingPagesChanged(bool value)
        {
            SearchText = string.Empty;
            SelectedGroup = null;
            SelectedPage = null;

            if (value) 
            {
                LoadPageData();
            }
            else 
            {
                LoadGroupData();
            }
        }

        private void LoadGroupData()
        {
            AllGroups.Clear();
            var groups = _grupoRepository.ObtenerGruposConPaginas();
            foreach (var g in groups) AllGroups.Add(g);
            FilterGroups();
        }

        private void LoadPageData()
        {
            AllPages.Clear();
            var pages = _paginaRepository.ObtenerPaginas();
            foreach (var p in pages) AllPages.Add(p);
            FilterPages();
        }

        partial void OnSearchTextChanged(string value)
        {
            if (IsEditingPages) FilterPages();
            else FilterGroups();
        }

        private void FilterGroups()
        {
            FilteredGroups.Clear();
            var filtered = string.IsNullOrWhiteSpace(SearchText)
                ? AllGroups
                : AllGroups.Where(g => g.group_name.Contains(SearchText, StringComparison.OrdinalIgnoreCase));
            foreach (var g in filtered) FilteredGroups.Add(g);
        }

        private void FilterPages()
        {
            FilteredPages.Clear();
            var filtered = string.IsNullOrWhiteSpace(SearchText)
                ? AllPages
                : AllPages.Where(p => p.page_name.Contains(SearchText, StringComparison.OrdinalIgnoreCase));
            foreach (var p in filtered) FilteredPages.Add(p);
        }

        public Group SelectedGroup { get => _selectedGroup; set { if (SetProperty(ref _selectedGroup, value)) OnGroupSelected(); } }
        public Pagina SelectedPage { get => _selectedPage; set { if (SetProperty(ref _selectedPage, value)) OnPageSelected(); } }

        private void OnGroupSelected()
        {
            OnPropertyChanged(nameof(IsGroupSelected));
            UpdateGroupCommand.NotifyCanExecuteChanged();
            if (SelectedGroup == null)
            {
                AllPagesChecklist.Clear(); 
                return;
            }

            EditGroupName = SelectedGroup.group_name;
            EditGroupDescription = SelectedGroup.description;
            EditGroupIsPinned = SelectedGroup.pin == 1;

            var allPagesInSystem = _paginaRepository.ObtenerPaginas();
            var assignedPageIds = new HashSet<long>(SelectedGroup.Paginas.Select(p => p.page_id));
            var pinnedPageIdsInGroup = SelectedGroup.PinnedPageIds; 

            AllPagesChecklist.Clear();
            foreach (var page in allPagesInSystem)
            {
                AllPagesChecklist.Add(new SelectablePageViewModel
                {
                    Page = page,
                    IsSelected = assignedPageIds.Contains(page.page_id),
                    IsPinnedInGroup = pinnedPageIdsInGroup.Contains(page.page_id)
                });
            }
        }

        private void TogglePinPageInGroup(SelectablePageViewModel pageVM)
        {
            if (pageVM == null || SelectedGroup == null) return;

            if (!pageVM.IsSelected)
            {
                MessageBox.Show("Primero debe asignar la página al grupo para poder fijarla.", "Aviso", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            pageVM.IsPinnedInGroup = !pageVM.IsPinnedInGroup;
            Debug.WriteLine($"[UI-ONLY] Estado del pin para '{pageVM.Page.page_name}' cambiado a: {pageVM.IsPinnedInGroup}");
        }

        private async void OnPageSelected()
        {
            OnPropertyChanged(nameof(IsPageSelected));
            UpdatePageCommand.NotifyCanExecuteChanged();
            if (SelectedPage == null) return;

            EditPageName = SelectedPage.page_name;
            EditPageDescription = SelectedPage.description;
            EditPageUrl = SelectedPage.url;
            EditRequiresProxy = SelectedPage.requires_proxy;
            EditRequiresLogin = SelectedPage.requires_login;
            EditRequiresCustomLogin = SelectedPage.requires_custom_login;

            EditCredentialUsername = string.Empty;
            EditCredentialPassword = string.Empty;

            if (SelectedPage.requires_login)
            {
                var credential = await _paginaRepository.GetCredentialForPageAsync(SelectedPage.page_id);

                if (credential != null)
                {
                    EditCredentialUsername = credential.Username;
                    EditCredentialPassword = credential.Password;
                }
                else
                {
                    Debug.WriteLine("[VIEWMODEL] El repositorio devolvió NULL. Los campos de credenciales quedarán vacíos.");
                }
            }
            else
            {
                Debug.WriteLine("[VIEWMODEL] La página NO requiere login. No se buscarán credenciales.");
            }
        }

        private async Task UpdateGroupAsync()
        {
            Debug.WriteLine("\n--- COMANDO UpdateGroupAsync EJECUTADO ---");

            if (string.IsNullOrWhiteSpace(EditGroupName) || string.IsNullOrWhiteSpace(EditGroupDescription))
            {
                MessageBox.Show("El nombre y la descripción del grupo no pueden estar vacíos.", "Validación fallida", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // 1. Preparamos los datos del grupo a actualizar
            SelectedGroup.group_name = EditGroupName;
            SelectedGroup.description = EditGroupDescription;
            SelectedGroup.pin = EditGroupIsPinned ? (short)1 : (short)0;
            Debug.WriteLine($"[INFO] Datos del GRUPO a actualizar: Nombre='{SelectedGroup.group_name}', Pin={SelectedGroup.pin}");

            // 2. Creamos una lista detallada de las páginas asignadas y su estado de pin
            var pagesToAssign = AllPagesChecklist
                .Where(p => p.IsSelected)
                .Select(p => new PageAssignmentInfo
                {
                    PageId = p.Page.page_id,
                    IsPinned = p.IsPinnedInGroup
                })
                .ToList();

            Debug.WriteLine($"[INFO] Se van a asignar {pagesToAssign.Count} páginas:");
            foreach (var pageInfo in pagesToAssign)
            {
                Debug.WriteLine($" -> ID: {pageInfo.PageId}, Está Fijada: {pageInfo.IsPinned}");
            }

            try
            {
                Debug.WriteLine("[INFO] Llamando al repositorio para la actualización completa...");
                await _grupoRepository.UpdateGroupAsync(SelectedGroup, pagesToAssign);
                MessageBox.Show("Grupo actualizado correctamente.", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
                Debug.WriteLine("[SUCCESS] Repositorio finalizado. Recargando datos...");

                LoadGroupData(); // Se cambia LoadInitialData por LoadGroupData para claridad
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[FATAL] Error en UpdateGroupAsync: {ex.Message}");
                MessageBox.Show($"Error al actualizar el grupo: {ex.Message}", "Error de Base de Datos", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task UpdatePageAsync()
        {
            if (string.IsNullOrWhiteSpace(EditPageName) || string.IsNullOrWhiteSpace(EditPageUrl) || string.IsNullOrWhiteSpace(EditPageDescription))
            {
                MessageBox.Show("Nombre, Descripción y URL son obligatorios.", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            SelectedPage.page_name = EditPageName;
            SelectedPage.description = EditPageDescription;
            SelectedPage.url = EditPageUrl;
            SelectedPage.requires_proxy = EditRequiresProxy;
            SelectedPage.requires_login = EditRequiresLogin;
            SelectedPage.requires_custom_login = EditRequiresCustomLogin;

            try
            {
                await _paginaRepository.UpdatePageAsync(SelectedPage);
                if (SelectedPage.requires_login)
                {
                    await _paginaRepository.UpdateOrInsertCredentialAsync(SelectedPage.page_id, EditCredentialUsername, EditCredentialPassword);
                }
                MessageBox.Show("Página actualizada correctamente.", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
                LoadPageData();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al actualizar la página: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LoadInitialData()
        {
            var groups = _grupoRepository.ObtenerGruposConPaginas();
            AllGroups.Clear();
            foreach (var group in groups)
            {
                AllGroups.Add(group);
            }
            FilterGroups();
        }
    }

    public partial class SelectablePageViewModel : ObservableObject
    {
        [ObservableProperty]
        private Pagina _page;

        [ObservableProperty]
        private bool _isSelected;

        [ObservableProperty]
        private bool _isPinnedInGroup;
    }
}
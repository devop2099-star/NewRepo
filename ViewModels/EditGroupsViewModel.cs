using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Naviguard.Models;
using Naviguard.Repositories;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace Naviguard.ViewModels
{
    public partial class EditGroupsViewModel : ObservableObject
    {
        private readonly GrupoRepository _grupoRepository;
        private readonly PaginaRepository _paginaRepository;

        [ObservableProperty]
        private ObservableCollection<Group> _allGroups;

        [ObservableProperty]
        private ObservableCollection<Group> _filteredGroups;

        [ObservableProperty]
        private ObservableCollection<SelectablePageViewModel> _allPagesChecklist;

        private Group _selectedGroup;
        public Group SelectedGroup
        {
            get => _selectedGroup;
            set
            {
                if (SetProperty(ref _selectedGroup, value))
                {
                    OnGroupSelected();
                    OnPropertyChanged(nameof(IsGroupSelected));
                }
            }
        }

        [ObservableProperty]
        private string _searchText;

        [ObservableProperty]
        private string _editGroupName;
        [ObservableProperty]
        private string _editGroupDescription;
        [ObservableProperty]
        private bool _editGroupIsPinned;

        public bool IsGroupSelected => SelectedGroup != null;

        public IAsyncRelayCommand UpdateGroupCommand { get; }

        public EditGroupsViewModel()
        {
            _grupoRepository = new GrupoRepository();
            _paginaRepository = new PaginaRepository();

            AllGroups = new ObservableCollection<Group>();
            FilteredGroups = new ObservableCollection<Group>();
            AllPagesChecklist = new ObservableCollection<SelectablePageViewModel>();

            UpdateGroupCommand = new AsyncRelayCommand(UpdateGroupAsync, () => IsGroupSelected);

            LoadInitialData();
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

        partial void OnSearchTextChanged(string value)
        {
            FilterGroups();
        }

        private void FilterGroups()
        {
            FilteredGroups.Clear();
            var filtered = string.IsNullOrWhiteSpace(SearchText)
                ? AllGroups
                : AllGroups.Where(g => g.group_name.Contains(SearchText, StringComparison.OrdinalIgnoreCase));

            foreach (var group in filtered)
            {
                FilteredGroups.Add(group);
            }
        }

        private void OnGroupSelected()
        {
            if (SelectedGroup == null) return;

            EditGroupName = SelectedGroup.group_name;
            EditGroupDescription = SelectedGroup.description;
            EditGroupIsPinned = SelectedGroup.pin == 1;

            var allPages = _paginaRepository.ObtenerPaginas();
            var assignedPageIds = new HashSet<long>(SelectedGroup.Paginas.Select(p => p.page_id));

            AllPagesChecklist.Clear();
            foreach (var page in allPages)
            {
                AllPagesChecklist.Add(new SelectablePageViewModel
                {
                    Page = page,
                    IsSelected = assignedPageIds.Contains(page.page_id)
                });
            }

            UpdateGroupCommand.NotifyCanExecuteChanged();
        }

        private async Task UpdateGroupAsync()
        {
            if (string.IsNullOrWhiteSpace(EditGroupName) || string.IsNullOrWhiteSpace(EditGroupDescription))
            {
                MessageBox.Show("El nombre y la descripción del grupo no pueden estar vacíos.", "Validación fallida", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            SelectedGroup.group_name = EditGroupName;
            SelectedGroup.description = EditGroupDescription;
            SelectedGroup.pin = EditGroupIsPinned ? (short)1 : (short)0;

            var selectedPageIds = AllPagesChecklist
            .Where(p => p.IsSelected)
            .Select(p => p.Page.page_id)
            .ToList();

            try
            {
                await _grupoRepository.UpdateGroupAsync(SelectedGroup, selectedPageIds);
                MessageBox.Show("Grupo actualizado correctamente.", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);

                LoadInitialData();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al actualizar el grupo: {ex.Message}", "Error de Base de Datos", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }

    // Clase auxiliar para manejar el estado de selección de las páginas
    public partial class SelectablePageViewModel : ObservableObject
    {
        [ObservableProperty]
        private Pagina _page;

        [ObservableProperty]
        private bool _isSelected;
    }
}
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Naviguard.BusinessInfo.Models;
using Naviguard.Models;
using Naviguard.Repositories;
using System.Collections.ObjectModel;
using System.Windows.Input; 

namespace Naviguard.BusinessInfo.ViewModels
{
    public partial class AssignUserToGroupsViewModel : ObservableObject
    {
        private readonly BusinessStructureRepository _repository;
        private readonly GrupoRepository _groupRepository;
        private readonly UserAssignmentRepository _assignmentRepository;
        private List<SelectableGroupWrapper> _allSelectableGroups;
        private List<Group> _allGroups;
        [ObservableProperty] private ObservableCollection<BusinessDepartment> departments;
        [ObservableProperty] private ObservableCollection<BusinessArea> areas;
        [ObservableProperty] private ObservableCollection<BusinessSubarea> subareas;
        [ObservableProperty] private BusinessDepartment selectedDepartment;
        [ObservableProperty] private BusinessArea selectedArea;
        [ObservableProperty] private BusinessSubarea selectedSubarea;
        [ObservableProperty] private string filterName;
        [ObservableProperty] private ObservableCollection<FilteredUser> filteredUsers;
        [ObservableProperty] private FilteredUser selectedUser;

        [ObservableProperty] private string groupSearchText;
        [ObservableProperty] private ObservableCollection<SelectableGroupWrapper> availableGroups;
        [ObservableProperty] private ObservableCollection<Group> assignedGroups;
        [ObservableProperty] private ObservableCollection<Group> filteredGroups; 
        [ObservableProperty] private Group selectedAvailableGroup;
        public bool IsUserSelected => SelectedUser != null;

        public IAsyncRelayCommand FilterCommand { get; }
        public ICommand AddSelectedGroupsCommand { get; }
        public ICommand ClearFilterCommand { get; }
        public IAsyncRelayCommand<Group> RemoveAssignedGroupCommand { get; } 

        public AssignUserToGroupsViewModel()
        {
            _repository = new BusinessStructureRepository();
            _groupRepository = new GrupoRepository();
            _assignmentRepository = new UserAssignmentRepository(); 

            _allSelectableGroups = new List<SelectableGroupWrapper>();
            Departments = new ObservableCollection<BusinessDepartment>();
            Areas = new ObservableCollection<BusinessArea>();
            Subareas = new ObservableCollection<BusinessSubarea>();
            FilteredUsers = new ObservableCollection<FilteredUser>();
            AvailableGroups = new ObservableCollection<SelectableGroupWrapper>();
            AssignedGroups = new ObservableCollection<Group>();

            FilterCommand = new AsyncRelayCommand(FilterUsersAsync);
            ClearFilterCommand = new RelayCommand(ClearFilters);
            AddSelectedGroupsCommand = new AsyncRelayCommand(AddSelectedGroupsAsync);
            RemoveAssignedGroupCommand = new AsyncRelayCommand<Group>(RemoveAssignedGroupAsync); 

            LoadDepartmentsAsync();
        }

        private async Task RemoveAssignedGroupAsync(Group groupToRemove)
        {
            if (groupToRemove == null || SelectedUser == null) return;

            try
            {
                await _assignmentRepository.RemoveGroupFromUserAsync(SelectedUser.id_user, groupToRemove.group_id);

                AssignedGroups.Remove(groupToRemove);

                RefreshAvailableGroupsList();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error al quitar grupo: {ex.Message}");
            }
        }

        async partial void OnSelectedUserChanged(FilteredUser value)
        {
            OnPropertyChanged(nameof(IsUserSelected));
            AssignedGroups.Clear();
            AvailableGroups.Clear();

            if (value != null)
            {
                if (!_allSelectableGroups.Any())
                {
                    var allGroupsList = _groupRepository.ObtenerGrupos();
                    _allSelectableGroups = allGroupsList.Select(g => new SelectableGroupWrapper(g)).ToList();
                }

                var userGroups = await _assignmentRepository.GetGroupsByUserIdAsync(value.id_user);
                foreach (var group in userGroups) { AssignedGroups.Add(group); }

                RefreshAvailableGroupsList();
            }
        }

        private async Task AddSelectedGroupsAsync()
        {
            if (SelectedUser == null) return;

            var selectedWrappers = _allSelectableGroups.Where(w => w.IsSelected).ToList();
            if (!selectedWrappers.Any()) return;

            var groupIdsToAdd = selectedWrappers.Select(w => w.Group.group_id).ToList();

            try
            {
                await _assignmentRepository.AssignGroupsToUserAsync(SelectedUser.id_user, groupIdsToAdd);

                foreach (var wrapper in selectedWrappers)
                {
                    if (!AssignedGroups.Any(g => g.group_id == wrapper.Group.group_id))
                    {
                        AssignedGroups.Add(wrapper.Group);
                    }
                }
                RefreshAvailableGroupsList();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error al asignar grupos: {ex.Message}");
            }
        }

        partial void OnGroupSearchTextChanged(string value)
        {
            RefreshAvailableGroupsList();
        }

        private void RefreshAvailableGroupsList()
        {
            var assignedGroupIds = new HashSet<long>(AssignedGroups.Select(g => g.group_id));

            var available = _allSelectableGroups
                .Where(w => !assignedGroupIds.Contains(w.Group.group_id) &&
                            (string.IsNullOrWhiteSpace(GroupSearchText) ||
                             w.Group.group_name.Contains(GroupSearchText, StringComparison.OrdinalIgnoreCase)));

            AvailableGroups.Clear();
            foreach (var wrapper in available)
            {
                wrapper.IsSelected = false; 
                AvailableGroups.Add(wrapper);
            }
        }

        private void ClearFilters()
        {
            FilterName = string.Empty;
            SelectedDepartment = null;

            FilteredUsers.Clear();
        }

        private async Task FilterUsersAsync()
        {
            int? deptId = SelectedDepartment?.id_bnsdpt;
            int? areaId = SelectedArea?.id_bnsarea;
            int? subareaId = SelectedSubarea?.id_bnsbar;

            var results = await _repository.FilterUsersAsync(FilterName, deptId, areaId, subareaId);

            FilteredUsers.Clear();
            foreach (var user in results)
            {
                FilteredUsers.Add(user);
            }
        }

        private async void LoadDepartmentsAsync()
        {
            var deptList = await _repository.GetDepartmentsAsync();
            Departments.Clear();
            foreach (var dept in deptList)
            {
                Departments.Add(dept);
            }
        }

        async partial void OnSelectedDepartmentChanged(BusinessDepartment value)
        {
            Areas.Clear();
            Subareas.Clear();
            if (value != null)
            {
                var areaList = await _repository.GetAreasByDepartmentAsync(value.id_bnsdpt);
                foreach (var area in areaList)
                {
                    Areas.Add(area);
                }
            }
        }

        async partial void OnSelectedAreaChanged(BusinessArea value)
        {
            Subareas.Clear();
            if (value != null)
            {
                var subareaList = await _repository.GetSubareasByAreaAsync(value.id_bnsarea);
                foreach (var sub in subareaList)
                {
                    Subareas.Add(sub);
                }
            }
        }
    }
}
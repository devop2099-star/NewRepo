using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Naviguard.Repositories;
using System.Collections.ObjectModel;
using System.Diagnostics;

namespace Naviguard.ViewModels
{
    public partial class GroupsPagesViewModel : ObservableObject
    {
        public ObservableCollection<Group> Grupos { get; set; }

        private readonly UserAssignmentRepository _assignmentRepository;

        public Action<Group> NavigateToGroupAction { get; set; }

        public GroupsPagesViewModel()
        {
            _assignmentRepository = new UserAssignmentRepository();
            Grupos = new ObservableCollection<Group>();
            CargarGruposDelUsuario(); 
        }

        private async void CargarGruposDelUsuario()
        {
            if (!UserSession.IsLoggedIn)
            {
                Debug.WriteLine("No hay una sesión de usuario activa para cargar los grupos.", "Sesión no encontrada");
                return;
            }

            try
            {
                long userId = UserSession.ApiUserId;

                var gruposDesdeDb = await _assignmentRepository.GetGroupsByUserIdAsync((int)userId);

                Grupos.Clear(); 
                foreach (var grupo in gruposDesdeDb)
                {
                    Grupos.Add(grupo);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error al cargar los grupos del usuario: {ex.Message}", "Error");
            }
        }

        [RelayCommand]
        private void OpenGroup(Group group)
        {
            if (group == null) return;
            Debug.WriteLine($"[GroupsPagesViewModel] Panel presionado. Grupo: '{group.group_name}', ID: {group.group_id}");
            NavigateToGroupAction?.Invoke(group);
        }
    }
}
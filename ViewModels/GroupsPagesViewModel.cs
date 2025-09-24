using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Naviguard.Repositories;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows;

namespace Naviguard.ViewModels
{
    public partial class GroupsPagesViewModel : ObservableObject
    {
        public ObservableCollection<Group> Grupos { get; set; }
        private readonly GrupoRepository _grupoRepository;

        public Action<Group> NavigateToGroupAction { get; set; }

        public GroupsPagesViewModel()
        {
            _grupoRepository = new GrupoRepository();
            Grupos = new ObservableCollection<Group>();
            CargarGrupos();
        }

        private void CargarGrupos()
        {
            try
            {
                var gruposDesdeDb = _grupoRepository.ObtenerGrupos();
                foreach (var grupo in gruposDesdeDb)
                {
                    Grupos.Add(grupo);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cargar los grupos: {ex.Message}", "Error");
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
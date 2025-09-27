using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Naviguard.Models;
using Naviguard.Repositories;
using System.Collections.ObjectModel;
using System.Windows;

namespace Naviguard.ViewModels
{
    public partial class CredentialsUserPageViewModel : ObservableObject
    {
        private readonly PaginaRepository _paginaRepository;
        private readonly CredentialRepository _credentialRepository;

        public FilteredUser CurrentUser { get; }
        public string AssignedUserFullName => CurrentUser?.full_name;

        [ObservableProperty]
        private Pagina _selectedPage;

        [ObservableProperty]
        private string _credentialUsername;
        [ObservableProperty]
        private string _credentialPassword;

        [ObservableProperty]
        private ObservableCollection<Pagina> _customLoginPages;

        public bool IsPageSelected => SelectedPage != null;

        public IAsyncRelayCommand SaveCredentialsCommand { get; }

        public CredentialsUserPageViewModel(FilteredUser user)
        {
            CurrentUser = user;
            _paginaRepository = new PaginaRepository();
            _credentialRepository = new CredentialRepository();

            CustomLoginPages = new ObservableCollection<Pagina>();
            SaveCredentialsCommand = new AsyncRelayCommand(SaveCredentialsAsync, () => IsPageSelected);

            LoadPagesAsync();
        }

        async partial void OnSelectedPageChanged(Pagina value)
        {
            OnPropertyChanged(nameof(IsPageSelected));
            SaveCredentialsCommand.NotifyCanExecuteChanged();

            if (value != null)
            {
                var credential = await _credentialRepository.GetCredentialAsync(CurrentUser.id_user, value.page_id);

                CredentialUsername = credential?.username ?? string.Empty;
                CredentialPassword = credential?.password ?? string.Empty;
            }
            else
            {
                CredentialUsername = string.Empty;
                CredentialPassword = string.Empty;
            }
        }

        private async void LoadPagesAsync()
        {
            var pages = await _paginaRepository.GetPagesRequiringCustomLoginAsync();
            CustomLoginPages.Clear();
            foreach (var page in pages)
            {
                CustomLoginPages.Add(page);
            }
        }

        private async Task SaveCredentialsAsync()
        {
            if (SelectedPage == null) return;

            if (string.IsNullOrWhiteSpace(CredentialUsername))
            {
                MessageBox.Show("El campo Usuario no puede estar vacío.", "Error de Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            try
            {
                await _credentialRepository.UpdateOrInsertCredentialAsync(CurrentUser.id_user, SelectedPage.page_id, CredentialUsername, CredentialPassword);
                MessageBox.Show($"Credenciales para la página '{SelectedPage.page_name}' guardadas correctamente.", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);

                CredentialUsername = string.Empty;
                CredentialPassword = string.Empty;
                SelectedPage = null;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al guardar credenciales: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
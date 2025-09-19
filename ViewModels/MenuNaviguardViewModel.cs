using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Naviguard.ViewModels
{
    public partial class MenuNaviguardViewModel : ObservableObject
    {
        [ObservableProperty]
        private object _currentContentViewModel;

        public MenuNaviguardViewModel()
        {
        }

        [RelayCommand]
        private void Navigate(string url)
        {
            if (!string.IsNullOrEmpty(url))
            {
                CurrentContentViewModel = new BrowserViewModel { Url = url };
            }
        }
    }
}
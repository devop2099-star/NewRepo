using CommunityToolkit.Mvvm.ComponentModel;

namespace Naviguard.ViewModels
{
    public partial class BrowserViewModel : ObservableObject
    {
        [ObservableProperty]
        private string _url;

        [ObservableProperty]
        private bool _requiresProxy;
    }
}
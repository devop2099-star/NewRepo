using CommunityToolkit.Mvvm.ComponentModel;

namespace Naviguard.BusinessInfo.ViewModels
{
    public partial class SelectableGroupWrapper : ObservableObject
    {
        [ObservableProperty]
        private bool isSelected;

        public Group Group { get; }

        public SelectableGroupWrapper(Group group)
        {
            Group = group;
        }
    }
}
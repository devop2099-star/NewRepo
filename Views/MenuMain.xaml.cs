using Naviguard.ViewModels;
using System.Windows;
using System.Windows.Input;

namespace Naviguard.Views
{
    public partial class MenuMain : Window
    {
        private FilterPagesNav _filterPagesNav;
        private EditGroups _editGroups;

        public MenuMain()
        {
            InitializeComponent();
            btnNav_Click(null, null);
        }

        private void MainBorder_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed) this.DragMove();
        }
        private void Border_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left) DragMove();
        }

        private void btnNav_Click(object sender, RoutedEventArgs e)
        {
            var groupsViewModel = new GroupsPagesViewModel();

            groupsViewModel.NavigateToGroupAction = NavigateToGroupView;

            var groupsView = new GroupsPages();
            groupsView.DataContext = groupsViewModel;

            ContentPresenter.Content = groupsView;
        }

        public void NavigateToGroupView(Group group)
        {
            var menuViewModel = new MenuNaviguardViewModel(group.group_id);

            var menuView = new menuNaviguardPages();

            menuView.DataContext = menuViewModel;

            ContentPresenter.Content = menuView;
        }

        private void btnFilterPages_Click(object sender, RoutedEventArgs e)
        {
            if (_filterPagesNav == null)
            {
                _filterPagesNav = new FilterPagesNav();
            }
            ContentPresenter.Content = _filterPagesNav;
        }

        private void btnEditGroups_Click(object sender, RoutedEventArgs e)
        {
            if (_editGroups == null)
            {
                _editGroups = new EditGroups();
            }
            ContentPresenter.Content = _editGroups;
        }
    }
}
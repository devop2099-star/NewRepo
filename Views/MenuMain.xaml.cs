using System.Windows;
using System.Windows.Input;

namespace Naviguard.Views
{
    public partial class MenuMain : Window
    {
        private FilterPagesNav _filterPagesNav;

        public MenuMain()
        {
            InitializeComponent();
        }

        private void MainBorder_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
                this.DragMove();
        }

        private void Border_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
                DragMove();
        }

        private void btnNav_Click(object sender, RoutedEventArgs e)
        {
            ContentPresenter.Content = new menuNaviguardPages();
        }

        private void btnFilterPages_Click(object sender, RoutedEventArgs e)
        {
            if (_filterPagesNav == null)
            {
                _filterPagesNav = new FilterPagesNav();
            }
            ContentPresenter.Content = _filterPagesNav;
        }
    }
}
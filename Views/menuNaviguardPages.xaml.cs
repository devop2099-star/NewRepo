using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Animation;

namespace Naviguard.Views
{
    public partial class menuNaviguardPages : UserControl
    {
        private bool isMenuHidden = false;

        public menuNaviguardPages()
        {
            InitializeComponent();
        }
        private void GroupButton_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            if (button != null && button.ContextMenu != null)
            {
                button.ContextMenu.PlacementTarget = button;
                button.ContextMenu.IsOpen = true;
            }
        }

        private void OpenPagesMenu_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            if (button != null && button.ContextMenu != null)
            {
                button.ContextMenu.PlacementTarget = button;
                button.ContextMenu.IsOpen = true;
            }
        }

        private void TopMenu_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (!isMenuHidden)
            {
                if (FindResource("HideTopMenuStoryboard") is Storyboard storyboard)
                {
                    storyboard.Begin();
                    isMenuHidden = true;
                }
            }
        }

        private void MenuHandle_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (isMenuHidden)
            {
                if (FindResource("ShowTopMenuStoryboard") is Storyboard storyboard)
                {
                    storyboard.Begin();
                    isMenuHidden = false;
                }
            }
        }
    }
}
using Naviguard.ViewModels;
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
            DataContext = new MenuNaviguardViewModel();
        }
        private void GroupButton_Click(object sender, RoutedEventArgs e)
        {
            // Obtiene el botón que fue presionado
            var button = sender as Button;
            if (button != null && button.ContextMenu != null)
            {
                // Le dice al ContextMenu que se posicione relativo al botón
                button.ContextMenu.PlacementTarget = button;
                // Abre el ContextMenu
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
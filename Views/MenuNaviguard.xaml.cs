using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Animation;
using Naviguard.ViewModels; 

namespace Naviguard.Views
{
    public partial class MenuNaviguard : Window
    {
        private bool isMenuHidden = false;

        public MenuNaviguard()
        {
            InitializeComponent();
            DataContext = new MenuNaviguardViewModel();
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
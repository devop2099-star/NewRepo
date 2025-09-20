using Naviguard.ViewModels;
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
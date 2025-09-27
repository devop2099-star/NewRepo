using Naviguard.Models;
using Naviguard.ViewModels;
using System.Windows;

namespace Naviguard.Views
{
    public partial class CredentialsUserPage : Window
    {
        public CredentialsUserPage(FilteredUser user)
        {
            InitializeComponent();
            this.DataContext = new CredentialsUserPageViewModel(user);
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}

using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Naviguard.Connections;
using Naviguard.Views; 

namespace Naviguard.Login
{
    public partial class Login : Window
    {
        private readonly ApiClient _apiClient = new ApiClient();

        public Login()
        {
            InitializeComponent();
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                Login_Click(btnLogin, new RoutedEventArgs());
            }
        }

        private void txtUsername_TextChanged(object sender, TextChangedEventArgs e)
        {
            placeholderUsername.Visibility = string.IsNullOrEmpty(txtUsername.Text) ? Visibility.Visible : Visibility.Collapsed;
        }

        private void txtPassword_PasswordChanged(object sender, RoutedEventArgs e)
        {
            placeholderPassword.Visibility = string.IsNullOrEmpty(txtPassword.Password) ? Visibility.Visible : Visibility.Collapsed;
        }

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
                this.DragMove();
        }

        private void txtUsername_GotFocus(object sender, RoutedEventArgs e)
        {
            borderUsername.BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#0078D7"));
        }

        private void txtUsername_LostFocus(object sender, RoutedEventArgs e)
        {
            borderUsername.BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#CCCCCC")); 
        }

        private void txtPassword_GotFocus(object sender, RoutedEventArgs e)
        {
            borderPassword.BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#0078D7")); 
        }

        private void txtPassword_LostFocus(object sender, RoutedEventArgs e)
        {
            borderPassword.BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#CCCCCC")); 
        }

        private async void Login_Click(object sender, RoutedEventArgs e)
        {
            string username = txtUsername.Text;
            string password = txtPassword.Password;

            btnLogin.IsEnabled = false;

            bool loginExitoso = await _apiClient.LoginAsync(username, password);

            if (loginExitoso)
            {
                var menu = new MenuMain();
                menu.Show();
                this.Close();
            }
            else
            {
                btnLogin.IsEnabled = true;
            }
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }
    }
}

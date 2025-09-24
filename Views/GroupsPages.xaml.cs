using Naviguard.Models; // <-- Añadir este using para acceder al modelo 'Group'
using System.Diagnostics; // <-- Añadir este using para Debug.WriteLine
using System.Windows;
using System.Windows.Controls;

namespace Naviguard.Views
{
    public partial class GroupsPages : UserControl
    {
        public GroupsPages()
        {
            InitializeComponent();
        }

        private void InfoButton_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            if (button == null) return;

            var group = button.DataContext as Group;
            if (group != null)
            {
                Debug.WriteLine($"InfoButton Clicked! Group: '{group.group_name}', Description: '{group.description}'");
            }

            if (button.ContextMenu != null)
            {
                button.ContextMenu.PlacementTarget = button;
                button.ContextMenu.IsOpen = true;
                e.Handled = true; 
            }
        }
    }
}
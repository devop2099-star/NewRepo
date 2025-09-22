using System.Windows.Controls;


namespace Naviguard.Views
{
    public partial class FilterPagesNav : UserControl
    {
        public FilterPagesNav()
        {
            InitializeComponent();
            this.DataContext = new FilterPagesViewModel();
        }
    }
}

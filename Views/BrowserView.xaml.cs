using System.Windows.Controls;
using Naviguard.Handlers; 

namespace Naviguard.Views
{
    public partial class BrowserView : UserControl
    {
        public BrowserView()
        {
            InitializeComponent();
            Browser.RequestHandler = new RequestHandler();
        }
    }
}
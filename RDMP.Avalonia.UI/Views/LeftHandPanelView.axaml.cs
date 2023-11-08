using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using NPOI.OpenXmlFormats.Dml;
using RDMP.Avalonia.UI.ViewModels;

namespace RDMP.Avalonia.UI.Views
{
    public partial class LeftHandPanelView : UserControl
    {

        public LeftHandPanelView()
        {
            InitializeComponent();
            DataContext = new LeftHandPanelViewModel();
        }


      
    }
}

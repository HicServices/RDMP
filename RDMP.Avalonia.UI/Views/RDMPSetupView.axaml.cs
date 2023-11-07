using Avalonia.Controls;
using RDMP.Avalonia.UI.ViewModels;

namespace RDMP.Avalonia.UI.Views;

public partial class RDMPSetupView : UserControl
{
    public RDMPSetupView()
    {
        this.DataContext = new RDMPSetupViewModel();
        InitializeComponent();
    }
}

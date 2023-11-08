using Avalonia.Controls;
using RDMP.Avalonia.UI.Services.BootstrapService;
using RDMP.Avalonia.UI.Services.RepositoryLocatorService;
using RDMP.Avalonia.UI.ViewModels;
using System;

namespace RDMP.Avalonia.UI.Views;

public partial class MainView : UserControl
{
    public MainView()
    {
        InitializeComponent();
        DataContext = new MainViewModel();
    }
}

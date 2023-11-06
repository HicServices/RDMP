using Avalonia.Controls;
using RDMP.Avalonia.UI.Services.BootstrapService;
using RDMP.Avalonia.UI.Services.RepositoryLocatorService;
using System;

namespace RDMP.Avalonia.UI.Views;

public partial class MainView : UserControl
{
    public MainView()
    {
        InitializeComponent();
        if(BootstrapService._startup.RepositoryLocator is null)
        {
            Console.WriteLine("We've not got any info!");
        }
        else
        {
            Console.WriteLine("We have info!");
        }
    }
}

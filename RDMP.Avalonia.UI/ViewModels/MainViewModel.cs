using RDMP.Avalonia.UI.Services.BootstrapService;
using System;

namespace RDMP.Avalonia.UI.ViewModels;

public class MainViewModel : ViewModelBase
{
    public string Greeting => "Welcome to Avalonia!";


    public MainViewModel()
    {
        //ShowStartup = BootstrapService._startup.RepositoryLocator is null;
    }

    private bool ShouldShowStartup() { return BootstrapService._startup.RepositoryLocator is null || BootstrapService._startup.RepositoryLocator.CatalogueRepository is null || BootstrapService._startup.RepositoryLocator.DataExportRepository is null; }

    public bool ShowSetup => ShouldShowStartup();
}

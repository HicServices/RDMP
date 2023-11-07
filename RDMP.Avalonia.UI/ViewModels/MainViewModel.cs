using Avalonia.Data;
using Rdmp.Core.Startup;
using Rdmp.Core.Startup.Events;
using RDMP.Avalonia.UI.Services.BootstrapService;
using ReactiveUI;
using System;

namespace RDMP.Avalonia.UI.ViewModels;

public class MainViewModel : ReactiveObject
{
    public string Greeting => "Welcome to RDMP";

    private void StartupDatabaseFound(object sender, PlatformDatabaseFoundEventArgs eventArgs) {
        Console.WriteLine('a');
        ShowSetup = false;
    }
    private void StartupPluginPatcherFound(object sender, PluginPatcherFoundEventArgs eventArgs) {
        Console.WriteLine('b');
    }



    public MainViewModel()
    {
        ShowSetup = ShouldShowSetup();
        BootstrapService._startup.DatabaseFound += StartupDatabaseFound;
        BootstrapService._startup.PluginPatcherFound += StartupPluginPatcherFound;
    }

    private bool _showSetup = false;

    private bool ShouldShowSetup() { return BootstrapService._startup.RepositoryLocator is null || BootstrapService._startup.RepositoryLocator.CatalogueRepository is null || BootstrapService._startup.RepositoryLocator.DataExportRepository is null; }

    public bool ShowSetup
    {
        get => _showSetup;
        set => this.RaiseAndSetIfChanged(ref _showSetup, value);
    }
}

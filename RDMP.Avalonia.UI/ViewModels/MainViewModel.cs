using Avalonia.Data;
using Avalonia.Markup.Xaml.Templates;
using Rdmp.Core.Startup;
using Rdmp.Core.Startup.Events;
using RDMP.Avalonia.UI.Services.BootstrapService;
using ReactiveUI;
using System;

namespace RDMP.Avalonia.UI.ViewModels;

public class MainViewModel : ReactiveObject
{

    private ReactiveObject _currentViewModel;

    private void StartupDatabaseFound(object sender, PlatformDatabaseFoundEventArgs eventArgs) {
        CurrentViewModel = new MainAppViewModel();
    }
    private void StartupPluginPatcherFound(object sender, PluginPatcherFoundEventArgs eventArgs) {
    }



    public MainViewModel()
    {
        _currentViewModel = ShouldShowSetup() ? new RDMPSetupViewModel(): new MainAppViewModel();
        BootstrapService._startup.DatabaseFound += StartupDatabaseFound;
        BootstrapService._startup.PluginPatcherFound += StartupPluginPatcherFound;
    }

    public ReactiveObject CurrentViewModel
    {
        get => _currentViewModel;
        private set => this.RaiseAndSetIfChanged(ref _currentViewModel, value);
    }


    private bool ShouldShowSetup() { return BootstrapService._startup.RepositoryLocator is null || BootstrapService._startup.RepositoryLocator.CatalogueRepository is null || BootstrapService._startup.RepositoryLocator.DataExportRepository is null; }
}

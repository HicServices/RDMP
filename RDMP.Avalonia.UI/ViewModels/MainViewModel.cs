using Avalonia.Data;
using RDMP.Avalonia.UI.Services.BootstrapService;
using ReactiveUI;
using System;

namespace RDMP.Avalonia.UI.ViewModels;

public class MainViewModel : ReactiveObject
{
    public string Greeting => "Welcome to Avalonia123!";


    private void DoSomething()
    {
        Console.WriteLine("yay");
    }

    public MainViewModel()
    {
        ShowSetup = ShouldShowSetup();
        //if (BootstrapService._startup is not null)
        //{
        //    BootstrapService._startup += DoSomething;
        //}
    }

    private bool _showSetup = false;

    private bool ShouldShowSetup() { return BootstrapService._startup.RepositoryLocator is null || BootstrapService._startup.RepositoryLocator.CatalogueRepository is null || BootstrapService._startup.RepositoryLocator.DataExportRepository is null; }

    public bool ShowSetup
    {
        get => _showSetup;
        set => this.RaiseAndSetIfChanged(ref _showSetup, value);
    }
}

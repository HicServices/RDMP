using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;
using DynamicData.Binding;
using Rdmp.Core.Curation.Data;
using RDMP.Avalonia.UI.Services.BootstrapService;
using RDMP.Avalonia.UI.Services.RepositoryLocatorService;
using RDMP.Avalonia.UI.ViewModels;
using ReactiveUI;
using System;
using System.Reactive;

namespace RDMP.Avalonia.UI.Views;

public partial class SimpleDataTestView : UserControl
{
    public SimpleDataTestView()
    {
        InitializeComponent();
        //    Catalogue[] catalogues = BootstrapService._startup.RepositoryLocator.CatalogueRepository.GetAllObjects<Catalogue>();
        //    Console.WriteLine(catalogues.Length);

    }
}

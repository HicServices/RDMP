using Rdmp.Core.Curation.Data;
using RDMP.Avalonia.UI.Services.BootstrapService;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Disposables;
using System.Text;
using System.Threading.Tasks;

namespace RDMP.Avalonia.UI.ViewModels;

public class SimpleDataTestViewModel: ReactiveObject, IActivatableViewModel
{
    public SimpleDataTestViewModel() {
        //this.WhenActivated((CompositeDisposable disposables) =>
        //{
        //    /* handle activation */
        //    Disposable
        //        .Create(() =>
        //        { /* handle deactivation */
        Catalogue[] catalogues = BootstrapService._startup.RepositoryLocator.CatalogueRepository.GetAllObjects<Catalogue>();
        Console.WriteLine(catalogues.Length);
        //        })
        //        .DisposeWith(disposables);
        //});
    }

    public ViewModelActivator Activator =>new ViewModelActivator();
}



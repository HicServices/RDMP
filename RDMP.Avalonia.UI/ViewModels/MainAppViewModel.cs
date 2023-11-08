using RDMP.Avalonia.UI.Services.BootstrapService;
using Rdmp.Core.Curation.Data;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RDMP.Avalonia.UI.ViewModels
{
    public class MainAppViewModel : ReactiveObject, IActivatableViewModel
    {
        //public string test = "test";
        public MainAppViewModel() {

            Catalogue[] catalogues = BootstrapService._startup.RepositoryLocator.CatalogueRepository.GetAllObjects<Catalogue>();
            Console.WriteLine(catalogues.Length);
        }

        public ViewModelActivator Activator => new ViewModelActivator();
    }


}

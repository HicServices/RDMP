using RDMP.Avalonia.UI.Services.BootstrapService;
using Rdmp.Core.Curation.Data;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using DynamicData.Binding;

namespace RDMP.Avalonia.UI.ViewModels
{

    public class CatalogueTreeViewModel : ReactiveObject
    {

        private ObservableCollection<Catalogue> _catalogues = new();

        public ObservableCollection<Catalogue> Catalogues { get => _catalogues; set => this.RaiseAndSetIfChanged(ref _catalogues, value); }

        public CatalogueTreeViewModel()
        {
            var catalogues = BootstrapService._startup.RepositoryLocator.CatalogueRepository.GetAllObjects<Catalogue>();
            foreach (Catalogue catalogue in catalogues)
            {
                _catalogues.Add(catalogue);
            }

        }
    }
}

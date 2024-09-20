using Rdmp.Core.CommandExecution;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rdmp.Core.Datasets;

public class InternalDatasetProvider : IDatasetProvider
{
    private readonly IBasicActivateItems _activator;
    public InternalDatasetProvider(IBasicActivateItems activator)
    {
        _activator = activator;
    }

    public Curation.Data.Datasets.Dataset FetchDatasetByID(int id)
    {
        return _activator.RepositoryLocator.CatalogueRepository.GetAllObjectsWhere<Curation.Data.Datasets.Dataset>("ID", id).FirstOrDefault();
    }

    public List<Curation.Data.Datasets.Dataset> FetchDatasets()
    {
        return _activator.RepositoryLocator.CatalogueRepository.GetAllObjects<Curation.Data.Datasets.Dataset>().ToList();
    }
}

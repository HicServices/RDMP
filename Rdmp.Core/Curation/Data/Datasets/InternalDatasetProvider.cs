using Rdmp.Core.CommandExecution;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;

namespace Rdmp.Core.Curation.Data.Datasets;

/// <summary>
/// Provider for internal datasets
/// </summary>
public class InternalDatasetProvider : IDatasetProvider
{
    private readonly IBasicActivateItems _activator;
    public InternalDatasetProvider(IBasicActivateItems activator, DatasetProviderConfiguration configuration = null, HttpClient client = null)
    {
        _activator = activator;
    }

    public Dataset AddExistingDatasetWithReturn(string name, string url)
    {
        throw new System.NotImplementedException();
    }

    public Dataset Create(Catalogue catalogue)
    {
        throw new System.NotImplementedException();
    }

    /// <inheritdoc/>
    public Curation.Data.Datasets.Dataset FetchDatasetByID(int id)
    {
        return _activator.RepositoryLocator.CatalogueRepository.GetAllObjectsWhere<Curation.Data.Datasets.Dataset>("ID", id).FirstOrDefault();
    }

    /// <inheritdoc/>
    public List<Curation.Data.Datasets.Dataset> FetchDatasets()
    {
        return _activator.RepositoryLocator.CatalogueRepository.GetAllObjects<Curation.Data.Datasets.Dataset>().ToList();
    }

    public void UpdateUsingCatalogue(Dataset dataset, Catalogue catalogue)
    {
        throw new System.NotImplementedException();
    }
}
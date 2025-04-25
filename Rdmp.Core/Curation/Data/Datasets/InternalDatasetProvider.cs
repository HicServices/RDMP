using Rdmp.Core.CommandExecution;
using System.Collections.Generic;
using System.Linq;

namespace Rdmp.Core.Datasets;

/// <summary>
/// Provider for internal datasets
/// </summary>
public class InternalDatasetProvider : IDatasetProvider
{
    private readonly IBasicActivateItems _activator;
    public InternalDatasetProvider(IBasicActivateItems activator)
    {
        _activator = activator;
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
}
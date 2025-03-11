using Rdmp.Core.CommandExecution;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Curation.Data.Datasets;
using Rdmp.Core.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rdmp.Core.Datasets;

public abstract class PluginDatasetProvider : IDatasetProvider
{
    protected DatasetProviderConfiguration Configuration { get; }
    protected ICatalogueRepository Repository { get; }
    protected IBasicActivateItems Activator { get; }

    protected PluginDatasetProvider(IBasicActivateItems activator, DatasetProviderConfiguration configuration)
    {
        Configuration = configuration;
        Activator = activator;
        Repository = activator.RepositoryLocator.CatalogueRepository;
    }

    public abstract Curation.Data.Datasets.Dataset FetchDatasetByID(int id);

    public abstract List<Curation.Data.Datasets.Dataset> FetchDatasets();

    public abstract void AddExistingDataset(string name, string url);

    public abstract Dataset AddExistingDatasetWithReturn(string name, string url);

    public abstract Curation.Data.Datasets.Dataset Create(Catalogue catalogue);

    public abstract void Update(string uuid, PluginDataset datasetUpdates);
    public abstract void UpdateUsingCatalogue(PluginDataset dataset, Catalogue catalogue);

}

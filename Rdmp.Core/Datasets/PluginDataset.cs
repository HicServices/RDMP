using Rdmp.Core.Curation.Data.Datasets;
using Rdmp.Core.Repositories;
using System.Data.Common;


namespace Rdmp.Core.Datasets
{
    /// <summary>
    /// Base class to allow all plugin dataset types to be based off
    /// </summary>
    public class PluginDataset : Dataset
    {
        public PluginDataset(ICatalogueRepository catalogueRepository, string name) : base(catalogueRepository, name) { }
        public PluginDataset() { }

        public PluginDataset(ICatalogueRepository repository, DbDataReader r) : base(repository, r) { }
    }
}

using NPOI.OpenXmlFormats.Dml;
using Rdmp.Core.Repositories;
using System.Data.Common;


namespace Rdmp.Core.Curation.Data.Datasets
{
    /// <summary>
    /// Base class to allow all plugin dataset types to be based off
    /// </summary>
    public class PluginDataset : Dataset
    {
        public PluginDataset(ICatalogueRepository catalogueRepository, string name) : base(catalogueRepository, name) { }
        public PluginDataset() { }

        public PluginDataset(ICatalogueRepository repository, DbDataReader r) : base(repository, r) { }

        public override string GetID() {
            return ID.ToString();
        }
        public override string GetRemoteID()
        {
            return ID.ToString();
        }
    }
}
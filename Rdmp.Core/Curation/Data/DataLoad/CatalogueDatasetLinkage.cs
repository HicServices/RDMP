using Rdmp.Core.Curation.Data.Datasets;
using Rdmp.Core.Repositories;
using System.Collections.Generic;
using System.Data.Common;

namespace Rdmp.Core.Curation.Data.DataLoad
{
    class CatalogueDatasetLinkage : DatabaseEntity
    {

        public Catalogue Catalogue {get;set;}
        public Dataset Dataset { get; set; }

        public CatalogueDatasetLinkage() { }

        public CatalogueDatasetLinkage(ICatalogueRepository repository, Catalogue catalogue,Dataset dataset)
        {
            Catalogue = catalogue;
            Dataset = dataset;
            repository.InsertAndHydrate(this, new Dictionary<string, object>
            {
                {"Catalogue_ID", catalogue.ID },
                { "Dataset_ID", dataset.ID}
            });
        }

        public CatalogueDatasetLinkage(ICatalogueRepository repository, DbDataReader r): base(repository, r)
        {
            Catalogue = repository.GetObjectByID<Catalogue>(int.Parse(r["Catalogue_ID"].ToString()));
            Dataset = repository.GetObjectByID<Dataset>(int.Parse(r["Dataset_ID"].ToString()));
        }
    }
}

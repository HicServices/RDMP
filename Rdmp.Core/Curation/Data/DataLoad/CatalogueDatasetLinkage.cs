using Rdmp.Core.Curation.Data.Datasets;
using Rdmp.Core.MapsDirectlyToDatabaseTable;
using Rdmp.Core.Repositories;
using System.Collections.Generic;
using System.Data.Common;

namespace Rdmp.Core.Curation.Data.DataLoad
{
    public class CatalogueDatasetLinkage : DatabaseEntity
    {

        private int _catalogueID;
        private int _datasetID;
        private ICatalogueRepository _repository;

        [NoMappingToDatabase]
        public Catalogue Catalogue => _repository.GetObjectByID<Catalogue>(_catalogueID);

        public int Catalogue_ID { get => _catalogueID; set => SetField(ref _catalogueID, value); }

        [NoMappingToDatabase]
        public Dataset Dataset => _repository.GetObjectByID<Dataset>(_datasetID);

        public int Dataset_ID { get => _datasetID; set => SetField(ref _datasetID, value); }

        public bool Autoupdate { get; set; }

        public CatalogueDatasetLinkage() { }

        public CatalogueDatasetLinkage(ICatalogueRepository repository, Catalogue catalogue, Dataset dataset, bool autoupdate = false)
        {
            _repository = repository;
            _catalogueID = catalogue.ID;
            _datasetID = dataset.ID;
            repository.InsertAndHydrate(this, new Dictionary<string, object>
            {
                {"Catalogue_ID", catalogue.ID },
                { "Dataset_ID", dataset.ID},
                {"Autoupdate", autoupdate==true?1:0 }
            });
        }

        public CatalogueDatasetLinkage(ICatalogueRepository repository, DbDataReader r) : base(repository, r)
        {
            _repository = repository;
            _catalogueID = int.Parse(r["Catalogue_ID"].ToString());
            _datasetID = int.Parse(r["Dataset_ID"].ToString());
            Autoupdate = r["Autoupdate"].ToString() != "0";
        }
    }
}

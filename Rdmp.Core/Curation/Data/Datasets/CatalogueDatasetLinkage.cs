// Copyright (c) The University of Dundee 2025-2025
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using Rdmp.Core.MapsDirectlyToDatabaseTable;
using Rdmp.Core.Repositories;
using System.Collections.Generic;
using System.Data.Common;

namespace Rdmp.Core.Curation.Data.Datasets
{
    public class CatalogueDatasetLinkage: DatabaseEntity
    {
        private int _catalogueID;
        private int _datasetID;
        private ICatalogueRepository _repository;

        #region Relationships
        [NoMappingToDatabase]
        public Catalogue Catalogue => _repository.GetObjectByID<Catalogue>(_catalogueID);


        [NoMappingToDatabase]
        public Dataset Dataset => _repository.GetObjectByID<Dataset>(_datasetID);

        #endregion

        public int Catalogue_ID { get => _catalogueID; set => SetField(ref _catalogueID, value); }

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

using System.Collections.Generic;
using CatalogueLibrary.Nodes;
using CatalogueLibrary.Repositories;
using DataExportLibrary.Data.DataTables;
using DataExportLibrary.Data.DataTables.DataSetPackages;
using DataExportLibrary.Repositories;
using MapsDirectlyToDatabaseTable;

namespace DataExportLibrary.Nodes
{
    public class ExtractableDataSetsNode : SingletonNode
    {
        public ExtractableDataSet[] ExtractableDataSets { get; set; }
        public ExtractableDataSetPackage[] Packages { get; set; }
        public IDataExportRepository DataExportRepository { get; set; }

        public ExtractableDataSetsNode(IDataExportRepository repository, ExtractableDataSet[] extractableDataSets, ExtractableDataSetPackage[] packages)
            : base("Extractable Catalogues")
        {
            DataExportRepository = repository;
            ExtractableDataSets = extractableDataSets;
            Packages = packages;
        }


    }
}

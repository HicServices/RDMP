using System.Collections.Generic;
using System.Linq;
using CatalogueLibrary.Data;
using DataExportLibrary.Data.DataTables;
using DataExportLibrary.Interfaces.Data.DataTables;

namespace CatalogueManager.ExtractionUIs.FilterUIs.Options
{
    public class DeployedExtractionFilterUIOptions : FilterUIOptions
    {
        private ISqlParameter[] _globals;
        private ITableInfo[] _tables;
        private IColumn[] _columns;

        public DeployedExtractionFilterUIOptions(DeployedExtractionFilter deployedExtractionFilter) : base(deployedExtractionFilter)
        {
            var selectedDataSet = deployedExtractionFilter.GetDataset();

            
            var ds = selectedDataSet.ExtractableDataSet;
            var c = selectedDataSet.ExtractionConfiguration;
            
            _tables = ds.Catalogue.GetTableInfoList(false);
            _globals = c.GlobalExtractionFilterParameters;

            List<IColumn> columns = new List<IColumn>();
            
            columns.AddRange(c.GetAllExtractableColumnsFor(ds));
            columns.AddRange(c.Project.GetAllProjectCatalogueColumns(ExtractionCategory.ProjectSpecific));

            _columns = columns.ToArray();
        }

        public override ITableInfo[] GetTableInfos()
        {
            return _tables;
        }

        public override ISqlParameter[] GetGlobalParametersInFilterScope()
        {
            return _globals;
        }

        public override IColumn[] GetIColumnsInFilterScope()
        {
            return _columns;
        }
    }
}
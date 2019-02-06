// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

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
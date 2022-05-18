// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Data;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.QueryBuilding;
using ReusableLibraryCode.DataAccess;

namespace Rdmp.Core.DataExport.DataExtraction.UserPicks
{
    /// <summary>
    /// Identifies a TableInfo that acts as a Lookup for a given dataset which is being extracted.  Lookup tables can be extracted along with the extracted data
    /// set (See ExtractableDatasetBundle).
    /// </summary>
    public class BundledLookupTable : IBundledLookupTable
    {
        public ITableInfo TableInfo { get; set; }

        public BundledLookupTable(ITableInfo tableInfo)
        {
            if(!tableInfo.IsLookupTable())
                throw new Exception("TableInfo " + tableInfo + " is not a lookup table");

            TableInfo = tableInfo;
        }

        public override string ToString()
        {
            return TableInfo.ToString();
        }

        /// <summary>
        /// Reads lookup data from the <see cref="TableInfo"/> using <see cref="DataAccessContext.DataExport"/>
        /// </summary>
        /// <returns></returns>
        public DataTable GetDataTable()
        {
            var tbl = TableInfo.Discover(DataAccessContext.DataExport);
            var server = tbl.Database.Server;

            var dt = new DataTable();

            using (var con = server.GetConnection())
            {
                con.Open();
                using (var da = server.GetDataAdapter(
                    server.GetCommand(GetDataTableFetchSql(),con)))
                {
                    da.Fill(dt);
                }
            }
            return dt;
        }

        public string GetDataTableFetchSql()
        {
            var catas = TableInfo.GetAllRelatedCatalogues();
            QueryBuilder qb;

            if (catas.Length == 1)
            {
                // if there is a Catalogue associated with this TableInfo use its extraction instead
                var cata = catas[0];
                var eis = cata.GetAllExtractionInformation(ExtractionCategory.Core);
                
                if(eis.Length > 0)
                {
                    qb = new QueryBuilder(null, null, new[] { TableInfo });
                    qb.AddColumnRange(eis);
                    return qb.SQL;
                }
            }

            return $"select * from {TableInfo.GetFullyQualifiedName()}";
        }
    }
}
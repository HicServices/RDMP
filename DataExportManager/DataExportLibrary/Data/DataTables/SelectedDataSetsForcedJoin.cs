// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CatalogueLibrary.Data;
using CatalogueLibrary.Repositories;
using DataExportLibrary.Data.LinkCreators;
using DataExportLibrary.Interfaces.Data.DataTables;
using MapsDirectlyToDatabaseTable;
using MapsDirectlyToDatabaseTable.Injection;

namespace DataExportLibrary.Data.DataTables
{
    /// <summary>
    /// Specifies that the given data export dataset in configuration x (SelectedDataSets) should include a mandatory join on the table TableInfo regardless of 
    /// what columns are selected in the extraction query.  The most common use case for this is to extract a dataset with WhereSQL that references a custom data
    /// table e.g. 'Questionnaire answer x > 5'.  In that scenario the <see cref="TableInfo"/> would be the 'Project Specific Catalogue' dataset 'Questionnaire'
    /// and the <see cref="SelectedDataSets"/> would be the dataset you were extracting in your study e.g. 'biochemistry'.
    /// 
    /// <para>A <see cref="JoinInfo"/> must still exist to tell <see cref="CatalogueLibrary.QueryBuilding.QueryBuilder"/> how to write the Join section of the query.</para>
    /// </summary>
    public class SelectedDataSetsForcedJoin : DatabaseEntity, ISelectedDataSetsForcedJoin, IInjectKnown<TableInfo>
    {
        #region Database Properties

        private int _selectedDataSets_ID;
        private int _tableInfo_ID;
        private Lazy<TableInfo> _knownTableInfo;

        #endregion

        public int SelectedDataSets_ID
        {
            get { return _selectedDataSets_ID; }
            set { SetField(ref _selectedDataSets_ID, value); }
        }
        public int TableInfo_ID
        {
            get { return _tableInfo_ID; }
            set { SetField(ref _tableInfo_ID, value); }
        }

        #region Relationships
        /// <inheritdoc cref="TableInfo_ID"/>
        [NoMappingToDatabase]
        public TableInfo TableInfo
        {
            get { return _knownTableInfo.Value; }
        }
        #endregion

        public SelectedDataSetsForcedJoin(IDataExportRepository repository,SelectedDataSets sds, TableInfo tableInfo)
        {
            repository.InsertAndHydrate(this, new Dictionary<string, object>()
            {
                {"SelectedDataSets_ID",sds.ID},
                {"TableInfo_ID",tableInfo.ID},
            });

            if (ID == 0 || Repository != repository)
                throw new ArgumentException("Repository failed to properly hydrate this class");

            ClearAllInjections();
        }
        internal SelectedDataSetsForcedJoin(IRepository repository, DbDataReader r): base(repository, r)
        {
            SelectedDataSets_ID = Convert.ToInt32(r["SelectedDataSets_ID"]);
            TableInfo_ID = Convert.ToInt32(r["TableInfo_ID"]);

            ClearAllInjections();
        }

        public void InjectKnown(TableInfo instance)
        {
            _knownTableInfo = new Lazy<TableInfo>(()=>instance);
        }

        public void ClearAllInjections()
        {
            _knownTableInfo = new Lazy<TableInfo>(FetchTableInfo);
        }

        private TableInfo FetchTableInfo()
        {
            return ((IDataExportRepository) Repository).CatalogueRepository.GetObjectByID<TableInfo>(TableInfo_ID);
        }
    }
}

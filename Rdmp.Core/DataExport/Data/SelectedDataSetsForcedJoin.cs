// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.Data.Common;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.MapsDirectlyToDatabaseTable;
using Rdmp.Core.MapsDirectlyToDatabaseTable.Injection;
using Rdmp.Core.Repositories;

namespace Rdmp.Core.DataExport.Data;

/// <inheritdoc cref="ISelectedDataSetsForcedJoin" />
public class SelectedDataSetsForcedJoin : DatabaseEntity, ISelectedDataSetsForcedJoin, IInjectKnown<TableInfo>
{
    #region Database Properties

    private int _selectedDataSets_ID;
    private int _tableInfo_ID;
    private Lazy<TableInfo> _knownTableInfo;

    #endregion

    /// <inheritdoc />
    public int SelectedDataSets_ID
    {
        get => _selectedDataSets_ID;
        set => SetField(ref _selectedDataSets_ID, value);
    }

    /// <inheritdoc />
    public int TableInfo_ID
    {
        get => _tableInfo_ID;
        set => SetField(ref _tableInfo_ID, value);
    }

    #region Relationships

    /// <inheritdoc cref="TableInfo_ID" />
    [NoMappingToDatabase]
    public TableInfo TableInfo => _knownTableInfo.Value;

    #endregion


    public SelectedDataSetsForcedJoin()
    {
        ClearAllInjections();
    }

    /// <summary>
    ///     Creates a new declaration in the <paramref name="repository" /> database that the given
    ///     <paramref name="tableInfo" /> should
    ///     always be joined against when extract the <paramref name="sds" />.
    /// </summary>
    /// <param name="repository"></param>
    /// <param name="sds"></param>
    /// <param name="tableInfo"></param>
    public SelectedDataSetsForcedJoin(IDataExportRepository repository, SelectedDataSets sds, ITableInfo tableInfo)
    {
        repository.InsertAndHydrate(this, new Dictionary<string, object>
        {
            { "SelectedDataSets_ID", sds.ID },
            { "TableInfo_ID", tableInfo.ID }
        });

        if (ID == 0 || Repository != repository)
            throw new ArgumentException("Repository failed to properly hydrate this class");

        ClearAllInjections();
    }

    internal SelectedDataSetsForcedJoin(IRepository repository, DbDataReader r) : base(repository, r)
    {
        SelectedDataSets_ID = Convert.ToInt32(r["SelectedDataSets_ID"]);
        TableInfo_ID = Convert.ToInt32(r["TableInfo_ID"]);

        ClearAllInjections();
    }

    /// <inheritdoc />
    public void InjectKnown(TableInfo instance)
    {
        _knownTableInfo = new Lazy<TableInfo>(instance);
    }

    /// <inheritdoc />
    public void ClearAllInjections()
    {
        _knownTableInfo = new Lazy<TableInfo>(FetchTableInfo);
    }

    private TableInfo FetchTableInfo()
    {
        return ((IDataExportRepository)Repository).CatalogueRepository.GetObjectByID<TableInfo>(TableInfo_ID);
    }
}
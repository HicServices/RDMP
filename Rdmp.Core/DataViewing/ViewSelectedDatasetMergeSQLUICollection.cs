// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using FAnsi.Discovery.QuerySyntax;
using NPOI.OpenXmlFormats.Spreadsheet;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Curation.Data.Dashboarding;
using Rdmp.Core.DataExport.Data;
using Rdmp.Core.DataExport.DataExtraction.Commands;
using Rdmp.Core.DataExport.DataExtraction.Pipeline.Destinations;
using Rdmp.Core.DataExport.DataExtraction.UserPicks;
using Rdmp.Core.DataLoad.Triggers;
using Rdmp.Core.DataLoad.Triggers.Implementations;
using Rdmp.Core.ReusableLibraryCode.DataAccess;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace Rdmp.Core.DataViewing;

internal class ViewSelectedDatasetMergeSQLUICollection : PersistableObjectCollection, IViewSQLAndResultsCollection
{
    private ExtractDatasetCommand _request;

    private ISelectedDataSets SelectedDataset => DatabaseObjects.OfType<ISelectedDataSets>().FirstOrDefault();

    public bool DisableRun => true;

    public ViewSelectedDatasetMergeSQLUICollection()
    {
    }

    public ViewSelectedDatasetMergeSQLUICollection(ISelectedDataSets dataset) : this()
    {
        DatabaseObjects.Add(dataset);
    }

    public string GetSql()
    {
        BuildRequest();
        var columns = _request.ColumnsToExtract;
        var table = _request.QueryBuilder.TablesUsedInQuery.FirstOrDefault().Discover(DataAccessContext.InternalDataProcessing);

       

        DataColumn[] pkColumns= columns.Where(c => c.ColumnInfo.ExtractionInformations.First().IsPrimaryKey).Select(c => new DataColumn() { 
            ColumnName = c.GetRuntimeName()
        }).ToArray();
        DataColumn[] nonPkColumns = columns.Where(c => !c.ColumnInfo.ExtractionInformations.First().IsPrimaryKey).Select(c => new DataColumn()
        {
            ColumnName = c.GetRuntimeName()
        }).ToArray();
        var mergeSql = MSSqlMergeDestination.GetMergeSQL(table.GetFullyQualifiedName(), "someTempTable",pkColumns,nonPkColumns,true,GetQuerySyntaxHelper());
        return mergeSql;

    }

    private void BuildRequest()
    {
        if (_request != null)
            return;

        var ec = SelectedDataset.ExtractionConfiguration;

        if (ec.Cohort_ID == null)
            throw new Exception("No cohort has been defined for this ExtractionConfiguration");

        //We are generating what the extraction SQL will be like, that only requires the dataset so empty bundle is fine
        _request = new ExtractDatasetCommand(ec, new ExtractableDatasetBundle(SelectedDataset.ExtractableDataSet));
        _request.GenerateQueryBuilder();
        _request.QueryBuilder.RegenerateSQL();
    }

    public IDataAccessPoint GetDataAccessPoint()
    {
        BuildRequest();

        return _request?.QueryBuilder?.TablesUsedInQuery?.FirstOrDefault();
        //return null;
    }

    public IEnumerable<DatabaseEntity> GetToolStripObjects()
    {
        yield return (DatabaseEntity)SelectedDataset;
    }

    public string GetTabName() => $"Merge SQL for {SelectedDataset}";

    public void AdjustAutocomplete(IAutoCompleteProvider autoComplete)
    {
    }

    public IQuerySyntaxHelper GetQuerySyntaxHelper()
    {
        BuildRequest();

        return _request.QueryBuilder.QuerySyntaxHelper;
    }
}
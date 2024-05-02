// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System.Collections.Generic;
using FAnsi.Discovery.QuerySyntax;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Curation.Data.Dashboarding;
using Rdmp.Core.Logging;
using Rdmp.Core.ReusableLibraryCode.DataAccess;

namespace Rdmp.Core.DataViewing;

/// <summary>
///     Collection that builds SQL for querying the logging database tables
/// </summary>
public class ViewLogsCollection : PersistableObjectCollection, IViewSQLAndResultsCollection
{
    private readonly ExternalDatabaseServer _loggingServer;
    private readonly LogViewerFilter _filter;

    public ViewLogsCollection(ExternalDatabaseServer loggingServer, LogViewerFilter filter)
    {
        _loggingServer = loggingServer;
        _filter = filter;
    }

    public void AdjustAutocomplete(IAutoCompleteProvider autoComplete)
    {
    }

    public IDataAccessPoint GetDataAccessPoint()
    {
        return _loggingServer;
    }

    public IQuerySyntaxHelper GetQuerySyntaxHelper()
    {
        return _loggingServer.GetQuerySyntaxHelper();
    }

    public string GetSql()
    {
        return $@"Select * from {_filter.LoggingTable}

{_filter.GetWhereSql()}";
    }

    public string GetTabName()
    {
        return _filter.ToString();
    }

    public IEnumerable<DatabaseEntity> GetToolStripObjects()
    {
        yield return _loggingServer;
    }
}
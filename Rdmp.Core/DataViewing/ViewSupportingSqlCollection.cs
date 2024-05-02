// Copyright (c) The University of Dundee 2018-2021
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System.Collections.Generic;
using System.Linq;
using FAnsi;
using FAnsi.Discovery.QuerySyntax;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Curation.Data.Dashboarding;
using Rdmp.Core.ReusableLibraryCode.DataAccess;

namespace Rdmp.Core.DataViewing;

internal class ViewSupportingSqlCollection : PersistableObjectCollection, IViewSQLAndResultsCollection
{
    public SupportingSQLTable SupportingSQLTable => DatabaseObjects.OfType<SupportingSQLTable>().FirstOrDefault();

    public ViewSupportingSqlCollection(SupportingSQLTable supportingSql)
    {
        DatabaseObjects.Add(supportingSql);
    }

    /// <summary>
    ///     Persistence constructor
    /// </summary>
    public ViewSupportingSqlCollection()
    {
    }

    public void AdjustAutocomplete(IAutoCompleteProvider autoComplete)
    {
    }

    public IDataAccessPoint GetDataAccessPoint()
    {
        return SupportingSQLTable.ExternalDatabaseServer;
    }

    public IQuerySyntaxHelper GetQuerySyntaxHelper()
    {
        var syntax = SupportingSQLTable.ExternalDatabaseServer?.DatabaseType ?? DatabaseType.MicrosoftSQLServer;
        return QuerySyntaxHelperFactory.Create(syntax);
    }

    public string GetSql()
    {
        return SupportingSQLTable.SQL;
    }

    public string GetTabName()
    {
        return SupportingSQLTable.Name;
    }

    public IEnumerable<DatabaseEntity> GetToolStripObjects()
    {
        yield return SupportingSQLTable;
    }
}
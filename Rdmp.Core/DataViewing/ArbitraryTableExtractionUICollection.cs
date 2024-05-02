// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using FAnsi;
using FAnsi.Discovery;
using FAnsi.Discovery.QuerySyntax;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Curation.Data.Dashboarding;
using Rdmp.Core.ReusableLibraryCode.DataAccess;

namespace Rdmp.Core.DataViewing;

/// <summary>
///     <see cref="IViewSQLAndResultsCollection" /> for querying samples of arbitrary tables / columns
/// </summary>
public class ArbitraryTableExtractionUICollection : PersistableObjectCollection, IViewSQLAndResultsCollection,
    IDataAccessPoint, IDataAccessCredentials
{
    private DiscoveredTable _table;

    public DatabaseType DatabaseType { get; set; }

    private Dictionary<string, string> _arguments = new();
    private const string DatabaseKey = "Database";
    private const string ServerKey = "Server";
    private const string TableKey = "Table";
    private const string DatabaseTypeKey = "DatabaseType";

    public string Username { get; set; }
    public string Password { get; set; }

    public string GetDecryptedPassword()
    {
        return Password ?? "";
    }

    public string OverrideSql { get; set; }

    /// <summary>
    ///     Needed for deserialization
    /// </summary>
    public ArbitraryTableExtractionUICollection()
    {
    }

    public ArbitraryTableExtractionUICollection(DiscoveredTable table) : this()
    {
        _table = table;
        _arguments.Add(ServerKey, _table.Database.Server.Name);
        _arguments.Add(DatabaseKey, _table.Database.GetRuntimeName());
        _arguments.Add(TableKey, _table.GetRuntimeName());
        DatabaseType = table.Database.Server.DatabaseType;

        _arguments.Add(DatabaseTypeKey, DatabaseType.ToString());


        Username = table.Database.Server.ExplicitUsernameIfAny;
        Password = table.Database.Server.ExplicitPasswordIfAny;
    }

    /// <nheritdoc />
    public override string SaveExtraText()
    {
        return PersistStringHelper.SaveDictionaryToString(_arguments);
    }

    public override void LoadExtraText(string s)
    {
        _arguments = PersistStringHelper.LoadDictionaryFromString(s);

        DatabaseType = (DatabaseType)Enum.Parse(typeof(DatabaseType), _arguments[DatabaseTypeKey]);

        var server = new DiscoveredServer(Server, Database, DatabaseType, null, null);
        _table = server.ExpectDatabase(Database).ExpectTable(_arguments[TableKey]);
    }

    public IEnumerable<DatabaseEntity> GetToolStripObjects()
    {
        yield break;
    }

    public IDataAccessPoint GetDataAccessPoint()
    {
        return this;
    }

    public string GetSql()
    {
        if (!string.IsNullOrWhiteSpace(OverrideSql))
            return OverrideSql;

        var response = _table.GetQuerySyntaxHelper().HowDoWeAchieveTopX(100);

        return response.Location switch
        {
            QueryComponent.SELECT => $"Select {response.SQL} * from {_table.GetFullyQualifiedName()}",
            QueryComponent.WHERE => $"Select * from {_table.GetFullyQualifiedName()} WHERE {response.SQL}",
            QueryComponent.Postfix => $"Select * from {_table.GetFullyQualifiedName()} {response.SQL}",
            _ => throw new ArgumentOutOfRangeException()
        };
    }

    public string GetTabName()
    {
        return $"View {_table.GetRuntimeName()}";
    }

    public void AdjustAutocomplete(IAutoCompleteProvider autoComplete)
    {
        autoComplete.Add(_table);
    }

    public string Server
    {
        get => _arguments[ServerKey];
        set => _arguments[ServerKey] = value;
    }

    public string Database
    {
        get => _arguments[DatabaseKey];
        set => _arguments[DatabaseKey] = value;
    }


    public IDataAccessCredentials GetCredentialsIfExists(DataAccessContext context)
    {
        //we have our own credentials if we do
        return string.IsNullOrWhiteSpace(Username) ? null : this;
    }

    public IQuerySyntaxHelper GetQuerySyntaxHelper()
    {
        return _table.GetQuerySyntaxHelper();
    }

    public bool DiscoverExistence(DataAccessContext context, out string reason)
    {
        if (_table.Exists())
        {
            reason = null;
            return true;
        }

        reason = $"Table {_table} did not exist";
        return false;
    }
}
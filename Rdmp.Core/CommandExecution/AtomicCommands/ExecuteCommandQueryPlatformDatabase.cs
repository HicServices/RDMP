// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.IO;
using System.Linq;
using FAnsi.Discovery;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Databases;
using Rdmp.Core.DataViewing;
using Rdmp.Core.MapsDirectlyToDatabaseTable;
using Rdmp.Core.MapsDirectlyToDatabaseTable.Versioning;
using Rdmp.Core.Repositories;
using Rdmp.Core.Repositories.Construction;
using Rdmp.Core.ReusableLibraryCode.DataAccess;

namespace Rdmp.Core.CommandExecution.AtomicCommands;

/// <summary>
///     Runs a query on a one of the RDMP platform databases and returns the results
/// </summary>
public class ExecuteCommandQueryPlatformDatabase : ExecuteCommandViewDataBase
{
    private readonly string _query;
    private readonly FileInfo _toFile;
    private DiscoveredTable _table;

    [UseWithObjectConstructor]
    public ExecuteCommandQueryPlatformDatabase(IBasicActivateItems activator,
        [DemandsInitialization(
            "Database type e.g. DataExport, Catalogue, QueryCaching, LoggingDatabase etc (See all IPatcher implementations)")]
        string databaseType,
        [DemandsInitialization(
            "Optional SQL query to execute on the database.  Or null to query the first table in the db.")]
        string query = null,
        [DemandsInitialization(ToFileDescription)]
        FileInfo toFile = null) : base(activator, toFile)
    {
        _query = query;
        _toFile = toFile;

        var patcherType = MEF.GetTypes<IPatcher>().FirstOrDefault(t => t.Name.Equals(databaseType) || t.Name.Equals(
            $"{databaseType}Patcher"));

        if (patcherType == null)
        {
            SetImpossible($"Could not find Type called {databaseType} or {databaseType}Patcher");
            return;
        }

        DiscoveredDatabase db;

        if (patcherType == typeof(DataExportPatcher))
        {
            db = GetDatabase(BasicActivator.RepositoryLocator.DataExportRepository);

            _query ??= "Select * from Project";
            _table = db?.ExpectTable("Project");
            return;
        }

        if (patcherType == typeof(CataloguePatcher))
        {
            db = GetDatabase(BasicActivator.RepositoryLocator.CatalogueRepository);

            _query ??= "Select * from Catalogue";
            _table = db?.ExpectTable("Catalogue");
            return;
        }

        var eds = BasicActivator.RepositoryLocator.CatalogueRepository.GetAllObjects<ExternalDatabaseServer>();

        var patcher = (IPatcher)Activator.CreateInstance(patcherType);
        db = GetDatabase(eds.Where(e => e.WasCreatedBy(patcher)).ToArray());

        if (db == null) return;

        SetTargetDatabase(db);
    }

    public ExecuteCommandQueryPlatformDatabase(IBasicActivateItems activator,
        ExternalDatabaseServer eds) : base(activator, null)
    {
        DiscoveredDatabase db;

        try
        {
            db = eds.Discover(DataAccessContext.InternalDataProcessing);
        }
        catch (Exception)
        {
            SetImpossible("Not a queryable SQL database");
            return;
        }

        SetTargetDatabase(db);
    }


    private DiscoveredDatabase GetDatabase(IRepository repository)
    {
        if (repository is TableRepository tableRepo) return tableRepo.DiscoveredServer?.GetCurrentDatabase();

        SetImpossible("Repository was not a database repo");
        return null;
    }

    private DiscoveredDatabase GetDatabase(ExternalDatabaseServer[] eds)
    {
        if (eds.Length == 0)
        {
            SetImpossible("Could not find any databases of the requested Type");
            return null;
        }

        if (eds.Length > 1)
        {
            SetImpossible($"Found {eds.Length} databases of the requested Type");
            return null;
        }

        return eds[0].Discover(DataAccessContext.InternalDataProcessing);
    }


    private void SetTargetDatabase(DiscoveredDatabase database)
    {
        _table = database.DiscoverTables(false).FirstOrDefault();

        if (_table == null) SetImpossible("Database was empty");
    }


    protected override IViewSQLAndResultsCollection GetCollection()
    {
        return new ArbitraryTableExtractionUICollection(_table)
        {
            OverrideSql = _query
        };
    }
}
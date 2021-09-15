// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using FAnsi.Discovery;
using MapsDirectlyToDatabaseTable;
using MapsDirectlyToDatabaseTable.Versioning;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Databases;
using Rdmp.Core.DataExport.DataExtraction;
using Rdmp.Core.DataViewing;
using Rdmp.Core.Repositories.Construction;
using ReusableLibraryCode.DataAccess;
using System;
using System.IO;
using System.Linq;

namespace Rdmp.Core.CommandExecution.AtomicCommands
{
    /// <summary>
    /// Runs a query on a one of the RDMP platform databases and returns the results
    /// </summary>
    public class ExecuteCommandQueryPlatformDatabase : BasicCommandExecution
    {
        private string _query;
        private readonly FileInfo _toFile;
        private DiscoveredTable _table;

        [UseWithObjectConstructor]
        public ExecuteCommandQueryPlatformDatabase(IBasicActivateItems activator,
            
            [DemandsInitialization("Database type e.g. DataExport, Catalogue, QueryCaching, LoggingDatabase etc (See all IPatcher implementations)")]
            string databaseType,
            
            [DemandsInitialization("Optional SQL query to execute on the database.  Or null to query the first table in the db.")]
            string query = null,

            [DemandsInitialization("Optional file to output the results of the query to.  Or null")]
            FileInfo toFile = null):base(activator)
        {
            _query = query;
            _toFile = toFile;

            var patcherType = activator.RepositoryLocator.CatalogueRepository.MEF.
            // find the database type the user wants to query (the Patcher suffix is optional)
                GetTypes<IPatcher>().FirstOrDefault(t=>t.Name.Equals(databaseType) || t.Name.Equals(databaseType + "Patcher"));
            
            if(patcherType == null)
            {
                SetImpossible($"Could not find Type called {databaseType} or {databaseType}Patcher");
                return;
            }

            DiscoveredDatabase db;

            if(patcherType == typeof(DataExportPatcher))
            {
                db = SetDatabase(BasicActivator.RepositoryLocator.DataExportRepository);
                _query = _query ?? "Select * from Project";
                _table = db.ExpectTable("Project");
                return;
            }
            else if (patcherType == typeof(CataloguePatcher))
            {
                db = SetDatabase(BasicActivator.RepositoryLocator.CatalogueRepository);
                _query = _query ?? "Select * from Catalogue";
                _table = db.ExpectTable("Catalogue");
                return;
            }
            else
            {
                var eds = BasicActivator.RepositoryLocator.CatalogueRepository.GetAllObjects<ExternalDatabaseServer>();

                var patcher = (IPatcher)Activator.CreateInstance(patcherType);
                db = SetDatabase(eds.Where(e => e.WasCreatedBy(patcher)).ToArray());
                
            }

            if(db == null)
            {
                return;
            }

            _table = db.DiscoverTables(false).FirstOrDefault();

            if(_table == null)
            {
                SetImpossible("Database was empty");
            }
        }

        private DiscoveredDatabase SetDatabase(IRepository repository)
        {
                if (repository is TableRepository tableRepo)
                {
                    return tableRepo.DiscoveredServer?.GetCurrentDatabase();
                }

                SetImpossible("Repository was not a database repo");
                return null;
        }

        private DiscoveredDatabase SetDatabase(ExternalDatabaseServer[] eds)
        {
            if(eds.Length == 0)
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
        public override void Execute()
        {
            base.Execute();

            var collection = new ArbitraryTableExtractionUICollection(_table){
                OverrideSql = _query 
            };

            if (_toFile != null)
            {
                ExtractTableVerbatim.ExtractDataToFile(collection, _toFile);
            }
            else
            {
                BasicActivator.ShowData(collection);
            }
        }
    }
}

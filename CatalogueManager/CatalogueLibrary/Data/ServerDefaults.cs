// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.Data.Common;
using CatalogueLibrary.Data;
using CatalogueLibrary.Repositories;
using MapsDirectlyToDatabaseTable;
using ReusableLibraryCode;

namespace CatalogueLibrary.Data
{
    /// <inheritdoc cref="IServerDefaults"/>
    public class ServerDefaults : IServerDefaults
    {
        /// <inheritdoc/>
        public ICatalogueRepository Repository { get; private set; }

        

        /// <summary>
        /// The value that will actually be stored in the ServerDefaults table as a dictionary (see constructor for population
        /// </summary>
        private readonly Dictionary<PermissableDefaults, string> StringExpansionDictionary = new Dictionary<PermissableDefaults, string>();

        /// <summary>
        /// Creates a new reader for the defaults configured in the <paramref name="repository"/> platform database
        /// </summary>
        /// <param name="repository"></param>
        public ServerDefaults(ICatalogueRepository repository)
        {
            Repository = repository;
            StringExpansionDictionary.Add(PermissableDefaults.LiveLoggingServer_ID, "Catalogue.LiveLoggingServer_ID");
            StringExpansionDictionary.Add(PermissableDefaults.TestLoggingServer_ID, "Catalogue.TestLoggingServer_ID");
            StringExpansionDictionary.Add(PermissableDefaults.IdentifierDumpServer_ID, "TableInfo.IdentifierDumpServer_ID");
            StringExpansionDictionary.Add(PermissableDefaults.CohortIdentificationQueryCachingServer_ID, "CIC.QueryCachingServer_ID");
            StringExpansionDictionary.Add(PermissableDefaults.ANOStore, "ANOTable.Server_ID");

            StringExpansionDictionary.Add(PermissableDefaults.WebServiceQueryCachingServer_ID, "WebServiceQueryCache");
            
            //this doesn't actually map to a field in the database, it is a bit of an abuse fo the defaults system
            StringExpansionDictionary.Add(PermissableDefaults.DQE, "DQE");
            StringExpansionDictionary.Add(PermissableDefaults.RAWDataLoadServer, "RAWDataLoadServer");

            
        }

        /// <inheritdoc/>
        public IExternalDatabaseServer GetDefaultFor(PermissableDefaults field)
        {
            if (field == PermissableDefaults.None)
                return null;

            using(var con = Repository.GetConnection())
            {
                var cmd = DatabaseCommandHelper.GetCommand("SELECT dbo.GetDefaultExternalServerIDFor('" + StringExpansionDictionary[field] + "')", con.Connection,con.Transaction);
                var executeScalar = cmd.ExecuteScalar();

                if (executeScalar == DBNull.Value)
                    return null;

                return Repository.GetObjectByID<ExternalDatabaseServer>(Convert.ToInt32(executeScalar));
            }
        }

        private int CountDefaults(PermissableDefaults type)
        {
            if (type == PermissableDefaults.None)
                return 0;

            using (var con = Repository.GetConnection())
            {
                var cmd = DatabaseCommandHelper.GetCommand("SELECT COUNT(*) AS NumDefaults FROM ServerDefaults WHERE DefaultType=@DefaultType", con.Connection, con.Transaction);
                DatabaseCommandHelper.AddParameterWithValueToCommand("@DefaultType", cmd, StringExpansionDictionary[type]);
                var result = cmd.ExecuteScalar();
                return Convert.ToInt32(result);
            }
        }

        /// <inheritdoc/>
        public void ClearDefault(PermissableDefaults toDelete)
        {
            // Repository strictly complains if there is nothing to delete, so we'll check first (probably good for the repo to be so strict?)
            if (CountDefaults(toDelete) == 0)
                return;

            Repository.Delete("DELETE FROM ServerDefaults WHERE DefaultType=@DefaultType",
                new Dictionary<string, object>()
                {
                    {"DefaultType",StringExpansionDictionary[toDelete]}
                });
        }

        /// <summary>
        /// Changes the database <paramref name="toChange"/> default to the specified server
        /// </summary>
        /// <param name="toChange"></param>
        /// <param name="externalDatabaseServer"></param>
        public void SetDefault(PermissableDefaults toChange, IExternalDatabaseServer externalDatabaseServer)
        {
            if(toChange == PermissableDefaults.None)
                throw new ArgumentException("toChange cannot be None","toChange");

            var oldValue = GetDefaultFor(toChange);

            if (oldValue == null)
                InsertNewValue(toChange, externalDatabaseServer);
            else
                UpdateExistingValue(toChange, externalDatabaseServer);

        }

        private void UpdateExistingValue(PermissableDefaults toChange, IExternalDatabaseServer externalDatabaseServer)
        {
            if (toChange == PermissableDefaults.None)
                throw new ArgumentException("toChange cannot be None", "toChange");

            string sql =
                "UPDATE ServerDefaults set ExternalDatabaseServer_ID  = @ExternalDatabaseServer_ID where DefaultType=@DefaultType";

                int affectedRows = Repository.Update(sql, new Dictionary<string, object>()
                {
                    {"DefaultType",StringExpansionDictionary[toChange]},
                    {"ExternalDatabaseServer_ID",externalDatabaseServer.ID}
                });

                if(affectedRows != 1)
                    throw new Exception("We were asked to update default for " + toChange + " but the query '" + sql + "' did not result in 1 affected rows (it resulted in " + affectedRows + ")");
        }

        private void InsertNewValue(PermissableDefaults toChange, IExternalDatabaseServer externalDatabaseServer)
        {
            if (toChange == PermissableDefaults.None)
                throw new ArgumentException("toChange cannot be None", "toChange");

            Repository.Insert(
                "INSERT INTO ServerDefaults(DefaultType,ExternalDatabaseServer_ID) VALUES (@DefaultType,@ExternalDatabaseServer_ID)",
                new Dictionary<string, object>()
                {
                    {"DefaultType",StringExpansionDictionary[toChange]},
                    {"ExternalDatabaseServer_ID",externalDatabaseServer.ID}
                });
        }

        /// <summary>
        /// Translates the given <see cref="PermissableDefaults"/> (a default that can be set) to a <see cref="Tier2DatabaseType"/> (identifies what type of database it is).
        /// </summary>
        /// <param name="permissableDefault"></param>
        /// <returns></returns>
        public static Tier2DatabaseType? PermissableDefaultToTier2DatabaseType(PermissableDefaults permissableDefault)
        {
            switch (permissableDefault)
            {
                case PermissableDefaults.LiveLoggingServer_ID:
                    return Tier2DatabaseType.Logging;
                case PermissableDefaults.TestLoggingServer_ID:
                    return Tier2DatabaseType.Logging;
                case PermissableDefaults.IdentifierDumpServer_ID:
                    return Tier2DatabaseType.IdentifierDump;
                case PermissableDefaults.DQE:
                    return Tier2DatabaseType.DataQuality;
                case PermissableDefaults.WebServiceQueryCachingServer_ID:
                    return Tier2DatabaseType.QueryCaching;
                case PermissableDefaults.CohortIdentificationQueryCachingServer_ID:
                    return Tier2DatabaseType.QueryCaching;
                case PermissableDefaults.RAWDataLoadServer:
                    return null;
                case PermissableDefaults.ANOStore:
                    return Tier2DatabaseType.ANOStore;
                default:
                    throw new ArgumentOutOfRangeException("permissableDefault");
            }
        }
    }
}

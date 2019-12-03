// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using Rdmp.Core.Repositories;
using ReusableLibraryCode;

namespace Rdmp.Core.Curation.Data.Defaults
{
    /// <inheritdoc cref="IServerDefaults"/>
    class ServerDefaults : IServerDefaults
    {
        /// <summary>
        /// The source repository from which the defaults were read
        /// </summary>
        private readonly CatalogueRepository _repository;
        
        /// <summary>
        /// The value that will actually be stored in the ServerDefaults table as a dictionary (see constructor for population
        /// </summary>
        private readonly Dictionary<PermissableDefaults, string> StringExpansionDictionary = new Dictionary<PermissableDefaults, string>();

        /// <summary>
        /// Creates a new reader for the defaults configured in the <paramref name="repository"/> platform database
        /// </summary>
        /// <param name="repository"></param>
        public ServerDefaults(CatalogueRepository repository)
        {
            _repository = repository;
            StringExpansionDictionary.Add(PermissableDefaults.LiveLoggingServer_ID, "Catalogue.LiveLoggingServer_ID");
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

            using (var con = _repository.GetConnection())
            {
                using (var cmd = DatabaseCommandHelper.GetCommand(
                    "SELECT ExternalDatabaseServer_ID FROM ServerDefaults WHERE DefaultType = @type", con.Connection,
                    con.Transaction))
                {
                    var p = cmd.CreateParameter();
                
                    p.ParameterName = "@type";
                    p.Value = StringExpansionDictionary[field];
                    cmd.Parameters.Add(p);

                    var executeScalar = cmd.ExecuteScalar();

                    if (executeScalar == DBNull.Value)
                        return null;

                    return _repository.GetObjectByID<ExternalDatabaseServer>(Convert.ToInt32(executeScalar));
                }
            }
        }

        private int CountDefaults(PermissableDefaults type)
        {
            if (type == PermissableDefaults.None)
                return 0;

            using (var con = _repository.GetConnection())
            {
                using(var cmd = DatabaseCommandHelper.GetCommand("SELECT COUNT(*) AS NumDefaults FROM ServerDefaults WHERE DefaultType=@DefaultType", con.Connection, con.Transaction))
                { 
                    DatabaseCommandHelper.AddParameterWithValueToCommand("@DefaultType", cmd, StringExpansionDictionary[type]);
                    var result = cmd.ExecuteScalar();
                    return Convert.ToInt32(result);
                }
            }
        }

        /// <inheritdoc/>
        public void ClearDefault(PermissableDefaults toDelete)
        {
            // Repository strictly complains if there is nothing to delete, so we'll check first (probably good for the repo to be so strict?)
            if (CountDefaults(toDelete) == 0)
                return;

            _repository.Delete("DELETE FROM ServerDefaults WHERE DefaultType=@DefaultType",
                new Dictionary<string, object>()
                {
                    {"DefaultType",StringExpansionDictionary[toDelete]}
                });
        }

        /// <inheritdoc/>
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

            int affectedRows = _repository.Update(sql, new Dictionary<string, object>()
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

            _repository.Insert(
                "INSERT INTO ServerDefaults(DefaultType,ExternalDatabaseServer_ID) VALUES (@DefaultType,@ExternalDatabaseServer_ID)",
                new Dictionary<string, object>()
                {
                    {"DefaultType",StringExpansionDictionary[toChange]},
                    {"ExternalDatabaseServer_ID",externalDatabaseServer.ID}
                });
        }

        
    }
}

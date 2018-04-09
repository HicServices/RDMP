using System;
using System.Collections.Generic;
using System.Data.Common;
using CatalogueLibrary.Data;
using CatalogueLibrary.Repositories;
using MapsDirectlyToDatabaseTable;
using ReusableLibraryCode;

namespace CatalogueLibrary.Data
{
    /// <summary>
    /// Server defaults let you identify a role a server plays (e.g. IdentifierDumpServer) and make it the default one of it's type for all rows created which have an IdentifierDump.
    /// For example TableInfo.IdentifierDumpServer_ID defaults to whichever IdentifierDump ExternalDatabaseServer is configured (can be DBNull.Value).
    /// 
    /// <para>A scalar valued function GetDefaultExternalServerIDFor is used to retrieve defaults so that even if the user creates a new record in the TableInfo table himself manually without
    /// using our library (very dangerous anyway btw) it will still have the default.</para>
    /// </summary>
    public class ServerDefaults : IServerDefaults
    {
        public CatalogueRepository Repository { get; private set; }

        /// <summary>
        /// Fields that can be set or fetched from the ServerDefaults table in the Catalogue Database
        /// </summary>
        public enum PermissableDefaults
        {
            None = 0,
            LiveLoggingServer_ID,
            TestLoggingServer_ID,
            IdentifierDumpServer_ID,
            DQE,
            WebServiceQueryCachingServer_ID,
            RAWDataLoadServer,
            ANOStore,
            CohortIdentificationQueryCachingServer_ID
        }

        /// <summary>
        /// The value that will actually be stored in the ServerDefaults table as a dictionary (see constructor for population
        /// </summary>
        private readonly Dictionary<PermissableDefaults, string> StringExpansionDictionary = new Dictionary<PermissableDefaults, string>();

        public ServerDefaults(CatalogueRepository repository)
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

        /// <summary>
        /// Pass in an enum to have it mapped to the scalar GetDefaultExternalServerIDFor function input that provides default values for columns that reference the given value - now note that this 
        /// might be a scalability issue at some point if there are multiple references from separate tables (or no references at all! like in DQE) 
        /// </summary>
        /// <param name="field"></param>
        /// <returns>the currently configured ExternalDatabaseServer the user wants to use as the default for the supplied role or null if no default has yet been picked</returns>
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

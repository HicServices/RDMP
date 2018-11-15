using System;
using System.Collections.Generic;
using System.Data.Common;
using CatalogueLibrary.Data;
using CatalogueLibrary.Data.Aggregation;
using CatalogueLibrary.Data.Cohort;
using CatalogueLibrary.Repositories;
using MapsDirectlyToDatabaseTable;
using ReusableLibraryCode;

namespace CatalogueLibrary.Data
{
    /// <inheritdoc cref="IServerDefaults"/>
    public class ServerDefaults : IServerDefaults
    {
        /// <inheritdoc/>
        public CatalogueRepository Repository { get; private set; }

        /// <summary>
        /// Fields that can be set or fetched from the ServerDefaults table in the Catalogue Database
        /// </summary>
        public enum PermissableDefaults
        {
            /// <summary>
            /// Null value/representation
            /// </summary>
            None = 0,

            /// <summary>
            /// Relational logging database to store logs in while loading, running DQE, extracting etc
            /// </summary>
            LiveLoggingServer_ID,

            /// <summary>
            /// Deprecated, use LiveLoggingServer_ID instead.
            /// </summary>
            TestLoggingServer_ID,

            /// <summary>
            /// Server to split sensitive identifiers off to during load (e.g. IdentifierDumper)
            /// </summary>
            IdentifierDumpServer_ID,

            /// <summary>
            /// Server to store the results of running the DQE on datasets over time
            /// </summary>
            DQE,

            /// <summary>
            /// Server to store cached results of <see cref="AggregateConfiguration"/> which are not sensitive and could be shown on a website etc
            /// </summary>
            WebServiceQueryCachingServer_ID,

            /// <summary>
            /// The RAW bubble server in data loads
            /// </summary>
            RAWDataLoadServer,

            /// <summary>
            /// Server to store substituted ANO/Identifiable mappings for sensitive fields during data load e.g. GPCode, CHI, etc.
            /// </summary>
            ANOStore,

            /// <summary>
            /// Server to store cached identifier lists of <see cref="AggregateConfiguration"/>  which are part of a <see cref="CohortIdentificationConfiguration"/> in order
            /// to speed up performance of UNION/INTERSECT/EXCEPT section of the cohort building query.
            /// </summary>
            CohortIdentificationQueryCachingServer_ID
        }

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

        /// <summary>
        /// Sets the database <paramref name="toDelete"/> default to null (not configured)
        /// </summary>
        /// <param name="toDelete"></param>
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

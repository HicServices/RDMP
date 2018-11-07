using System.Collections.Generic;
using System.Data.Common;
using CatalogueLibrary.Data;
using CatalogueLibrary.Data.Aggregation;
using CatalogueLibrary.Data.Cache;
using CatalogueLibrary.Data.Cohort;
using CatalogueLibrary.Data.Referencing;
using HIC.Logging;
using MapsDirectlyToDatabaseTable;
using ReusableLibraryCode.Comments;

namespace CatalogueLibrary.Repositories
{
    /// <summary>
    /// See CatalogueRepository
    /// </summary>
    public interface ICatalogueRepository : ITableRepository
    {
        AggregateForcedJoin AggregateForcedJoiner { get; set; }
        TableInfoToCredentialsLinker TableInfoToCredentialsLinker { get; set; }
        PasswordEncryptionKeyLocation PasswordEncryptionKeyLocation { get; set; }
        JoinInfoFinder JoinInfoFinder { get; set; }
        MEF MEF { get; set; }
        IEnumerable<CatalogueItem> GetAllCatalogueItemsNamed(string name, bool ignoreCase);

        CommentStore CommentStore { get; }

        /// <summary>
        /// If the configuration is part of any aggregate container anywhere this method will return the order within that container
        /// </summary>
        /// <param name="configuration"></param>
        /// <returns></returns>
        int? GetOrderIfExistsFor(AggregateConfiguration configuration);

        /// <summary>
        /// Returns a new <see cref="HIC.Logging.LogManager"/> that audits in the default logging server specified by <see cref="ServerDefaults"/>
        /// </summary>
        /// <returns></returns>
        LogManager GetDefaultLogManager();

        Catalogue[] GetAllCatalogues(bool includeDeprecatedCatalogues = false);
        Catalogue[] GetAllCataloguesWithAtLeastOneExtractableItem();
        IEnumerable<CohortIdentificationConfiguration> GetAllCohortIdentificationConfigurationsWithDependencyOn(AggregateConfiguration aggregate);
        IEnumerable<AnyTableSqlParameter> GetAllParametersForParentTable(IMapsDirectlyToDatabaseTable parent);
        ColumnInfo[] GetColumnInfosWithNameExactly(string name);
        TicketingSystemConfiguration GetTicketingSystem();
        IEnumerable<CacheProgress> GetAllCacheProgressWithoutAPermissionWindow();
        TableInfo GetTableWithNameApproximating(string tableName, string database);

        /// <summary>
        /// This method is used to allow you to clone an IMapsDirectlyToDatabaseTable object into a DIFFERENT database.  You should use DbCommandBuilder
        /// and "SELECT * FROM TableX" in order to get the Insert command and then pass in a corresponding wrapper object which must have properties
        /// that exactly match the underlying table, these will be populated into insertCommand ready for you to use
        /// </summary>
        /// <param name="insertCommand"></param>
        /// <param name="oTableWrapperObject"></param>
        void PopulateInsertCommandValuesWithCurrentState(DbCommand insertCommand, IMapsDirectlyToDatabaseTable oTableWrapperObject);

        T CloneObjectInTable<T>(T oToClone, TableRepository destinationRepository) where T : IMapsDirectlyToDatabaseTable;
        
        T[] GetAllObjectsWhere<T>(string whereSQL, Dictionary<string, object> parameters = null)
            where T : IMapsDirectlyToDatabaseTable;
        
        DbCommand PrepareCommand(string sql, Dictionary<string, object> parameters, DbConnection con, DbTransaction transaction = null);

        T[] GetReferencesTo<T>(IMapsDirectlyToDatabaseTable o) where T : ReferenceOtherObjectDatabaseEntity;

    }
}
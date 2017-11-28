using System;
using System.Collections.Generic;
using System.Data.Common;
using CatalogueLibrary.Data;
using CatalogueLibrary.Data.Aggregation;
using CatalogueLibrary.Data.Cache;
using CatalogueLibrary.Data.Cohort;
using CatalogueLibrary.Repositories.Sharing;
using MapsDirectlyToDatabaseTable;

namespace CatalogueLibrary.Repositories
{
    public interface ICatalogueRepository : ITableRepository
    {
        AggregateForcedJoin AggregateForcedJoiner { get; set; }
        TableInfoToCredentialsLinker TableInfoToCredentialsLinker { get; set; }
        PasswordEncryptionKeyLocation PasswordEncryptionKeyLocation { get; set; }
        JoinInfoFinder JoinInfoFinder { get; set; }
        MEF MEF { get; set; }
        ShareManager ShareManager { get; set; }
        IEnumerable<CatalogueItem> GetAllCatalogueItemsNamed(string name, bool ignoreCase);

        /// <summary>
        /// If the configuration is part of any aggregate container anywhere this method will return the order within that container
        /// </summary>
        /// <param name="configuration"></param>
        /// <returns></returns>
        int? GetOrderIfExistsFor(AggregateConfiguration configuration);

        /// <summary>
        /// Returns Catalogue1.CatalogueItem1, Catalogue1.CatalogueItem2 etc, a CatalogueItem does not know the name of it's parent so 
        /// for performance reasons this is a big saver it means we only have database query instead of having to construct and dereference
        /// every CatalogueItem and Every Catalogue in the database.
        /// </summary>
        /// <returns></returns>
        List<FriendlyNamedCatalogueItem> GetFullNameOfAllCatalogueItems();

        Catalogue[] GetAllCatalogues(bool includeDeprecatedCatalogues = false);
        Catalogue[] GetAllCataloguesWithAtLeastOneExtractableItem();
        IEnumerable<CohortIdentificationConfiguration> GetAllCohortIdentificationConfigurationsWithDependencyOn(AggregateConfiguration aggregate);
        IEnumerable<AnyTableSqlParameter> GetAllParametersForParentTable(IMapsDirectlyToDatabaseTable parent);
        ColumnInfo GetColumnInfoWithNameExactly(string name);
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
        /// <param name="insertIdentity">Pass true to also add and populate the ID part of the insert command (including the IDENTITY_INSERT command that allows INSERT of identity columns) </param>
        void PopulateInsertCommandValuesWithCurrentState(DbCommand insertCommand, IMapsDirectlyToDatabaseTable oTableWrapperObject, bool insertIdentity);

        T CloneObjectInTable<T>(T oToClone, TableRepository destinationRepository) where T : IMapsDirectlyToDatabaseTable;
        T CloneObjectInTable<T>(T oToClone, TableRepository sourceRepository, TableRepository destinationRepository) where T:IMapsDirectlyToDatabaseTable;

        T[] GetAllObjectsWhere<T>(string whereSQL, Dictionary<string, object> parameters = null)
            where T : IMapsDirectlyToDatabaseTable;
        
        DbCommand PrepareCommand(string sql, Dictionary<string, object> parameters, DbConnection con, DbTransaction transaction = null);

        Catalogue[] GetAllAutomationLockedCatalogues();
        
    }
}
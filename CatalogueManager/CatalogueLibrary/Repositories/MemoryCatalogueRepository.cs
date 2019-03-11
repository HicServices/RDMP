using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using CatalogueLibrary.Data;
using CatalogueLibrary.Data.Aggregation;
using CatalogueLibrary.Data.Cohort;
using CatalogueLibrary.Data.Referencing;
using FAnsi.Connections;
using FAnsi.Discovery;
using HIC.Logging;
using MapsDirectlyToDatabaseTable;
using ReusableLibraryCode.Comments;

namespace CatalogueLibrary.Repositories
{
    class MemoryCatalogueRepository : MemoryRepository, ICatalogueRepository
    {
        public AggregateForcedJoin AggregateForcedJoiner { get; set; }
        public TableInfoToCredentialsLinker TableInfoToCredentialsLinker { get; set; }
        public PasswordEncryptionKeyLocation PasswordEncryptionKeyLocation { get; set; }
        public JoinInfoFinder JoinInfoFinder { get; set; }
        public MEF MEF { get; set; }
        public CommentStore CommentStore { get; private set; }

        public IObscureDependencyFinder ObscureDependencyFinder { get; set; }
        public string ConnectionString { get { return null; } }
        public DbConnectionStringBuilder ConnectionStringBuilder { get { return null; } }
        public DiscoveredServer DiscoveredServer { get { return null; }}

        public void PopulateUpdateCommandValuesWithCurrentState(DbCommand cmd, IMapsDirectlyToDatabaseTable oTableWrapperObject)
        {
            throw new NotImplementedException();
        }

        public Dictionary<string, int> GetObjectCountByVersion(Type type)
        {
            throw new NotImplementedException();
        }

        public IManagedConnection GetConnection()
        {
            throw new NotImplementedException();
        }

        public IManagedConnection BeginNewTransactedConnection()
        {
            throw new NotImplementedException();
        }

        public void EndTransactedConnection(bool commit)
        {
            throw new NotImplementedException();
        }

        public void ClearUpdateCommandCache()
        {
            throw new NotImplementedException();
        }

        public int? ObjectToNullableInt(object o)
        {
            throw new NotImplementedException();
        }

        public DateTime? ObjectToNullableDateTime(object o)
        {
            throw new NotImplementedException();
        }

        public void TestConnection()
        {
            throw new NotImplementedException();
        }

        public bool SupportsObjectType(Type type)
        {
            throw new NotImplementedException();
        }

        public int? GetOrderIfExistsFor(AggregateConfiguration configuration)
        {
            throw new NotImplementedException();
        }

        public LogManager GetDefaultLogManager()
        {
            throw new NotImplementedException();
        }

        public Catalogue[] GetAllCatalogues(bool includeDeprecatedCatalogues = false)
        {
            throw new NotImplementedException();
        }

        public Catalogue[] GetAllCataloguesWithAtLeastOneExtractableItem()
        {
            throw new NotImplementedException();
        }

        public IEnumerable<AnyTableSqlParameter> GetAllParametersForParentTable(IMapsDirectlyToDatabaseTable parent)
        {
            return GetAllObjects<AnyTableSqlParameter>().Where(o => o.IsReferenceTo(parent));
        }

        public TicketingSystemConfiguration GetTicketingSystem()
        {
            throw new NotImplementedException();
        }

        public void PopulateInsertCommandValuesWithCurrentState(DbCommand insertCommand,
            IMapsDirectlyToDatabaseTable oTableWrapperObject)
        {
            throw new NotImplementedException();
        }

        public T CloneObjectInTable<T>(T oToClone, TableRepository destinationRepository) where T : IMapsDirectlyToDatabaseTable
        {
            throw new NotImplementedException();
        }

        public T[] GetAllObjectsWhere<T>(string whereSQL, Dictionary<string, object> parameters = null) where T : IMapsDirectlyToDatabaseTable
        {
            throw new NotImplementedException();
        }

        public DbCommand PrepareCommand(string sql, Dictionary<string, object> parameters, DbConnection con, DbTransaction transaction = null)
        {
            throw new NotImplementedException();
        }

        public T[] GetReferencesTo<T>(IMapsDirectlyToDatabaseTable o) where T : ReferenceOtherObjectDatabaseEntity
        {
            throw new NotImplementedException();
        }
    }
}
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Diagnostics;
using System.Linq;
using CatalogueLibrary.Data;
using CatalogueLibrary.Data.Aggregation;
using CatalogueLibrary.Data.Cohort;
using CatalogueLibrary.Data.Defaults;
using CatalogueLibrary.Data.Referencing;
using FAnsi.Connections;
using FAnsi.Discovery;
using HIC.Logging;
using MapsDirectlyToDatabaseTable;
using ReusableLibraryCode.Comments;

namespace CatalogueLibrary.Repositories
{
    class MemoryCatalogueRepository : MemoryRepository, ICatalogueRepository, IServerDefaults
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

        readonly Dictionary<PermissableDefaults, IExternalDatabaseServer> _defaults = new Dictionary<PermissableDefaults, IExternalDatabaseServer>();

        public MemoryCatalogueRepository(IServerDefaults currentDefaults = null)
        {
            //we need to know what the default servers for stuff are
            foreach (PermissableDefaults value in Enum.GetValues(typeof (PermissableDefaults)))
                if(currentDefaults == null)
                    _defaults.Add(value, null); //we have no defaults to import
                else
                {
                    //we have defaults to import so get the default
                    var defaultServer = currentDefaults.GetDefaultFor(value);

                    //if it's not null we must be able to return it with GetObjectByID
                    if(defaultServer != null)
                    {
                        AddType(typeof (ExternalDatabaseServer));
                        Objects[typeof(ExternalDatabaseServer)].Add(defaultServer);
                    }
                    
                    _defaults.Add(value,defaultServer);
                }

            if(Objects.Any())
                NextObjectId = Objects.Max(kvp => kvp.Value.Max(o => o.ID)) + 1;
        }


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

        public IServerDefaults GetServerDefaults()
        {
            return this;
        }
        
        public IExternalDatabaseServer GetDefaultFor(PermissableDefaults field)
        {
            return _defaults[field];
        }

        public void ClearDefault(PermissableDefaults toDelete)
        {
            _defaults[toDelete] = null;
        }

        public void SetDefault(PermissableDefaults toChange, IExternalDatabaseServer externalDatabaseServer)
        {
            _defaults[toChange] = externalDatabaseServer;
        }

        public override void InsertAndHydrate<T>(T toCreate, Dictionary<string, object> constructorParameters)
        {
            base.InsertAndHydrate(toCreate, constructorParameters);

            var v = toCreate as VersionedDatabaseEntity;
            if(v != null)
                v.SoftwareVersion = FileVersionInfo.GetVersionInfo(GetType().Assembly.Location).FileVersion;
        }
    }
}
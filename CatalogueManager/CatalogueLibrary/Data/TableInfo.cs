using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text.RegularExpressions;
using CatalogueLibrary.Data.DataLoad;
using CatalogueLibrary.Data.DataLoad.Extensions;
using CatalogueLibrary.Data.EntityNaming;
using CatalogueLibrary.DataHelper;
using CatalogueLibrary.QueryBuilding;
using CatalogueLibrary.Repositories;
using CatalogueLibrary.Triggers;
using FAnsi;
using FAnsi.Discovery;
using FAnsi.Discovery.QuerySyntax;
using FAnsi.Naming;
using MapsDirectlyToDatabaseTable;
using MapsDirectlyToDatabaseTable.Attributes;
using MapsDirectlyToDatabaseTable.Injection;
using ReusableLibraryCode;
using ReusableLibraryCode.Annotations;
using ReusableLibraryCode.Checks;
using ReusableLibraryCode.DataAccess;


namespace CatalogueLibrary.Data
{
    /// <summary>
    /// Describes an sql table (or table valued function) on a given Server from which you intend to either extract and/or load / curate data.
    /// These can be created most easily by using TableInfoImporter.  This entity is the hanging off point for PreLoadDiscardedColumn, ColumnInfo etc
    /// 
    /// <para>This class represents a constant for the RDMP and allows the system to detect when data analysts have randomly dropped/renamed columns without 
    /// telling anybody and present this information in a rational context to the systems user (see TableInfoSynchronizer).</para>
    /// </summary>
    public class TableInfo : VersionedDatabaseEntity,ITableInfo,INamed, IHasFullyQualifiedNameToo, IInjectKnown<ColumnInfo[]>,ICheckable
    {

        ///<inheritdoc cref="IRepository.FigureOutMaxLengths"/>
        public static int Name_MaxLength = -1;
        ///<inheritdoc cref="IRepository.FigureOutMaxLengths"/>
        public static int Server_MaxLength = -1;
        ///<inheritdoc cref="IRepository.FigureOutMaxLengths"/>
        public static int Database_MaxLength = -1;
        ///<inheritdoc cref="IRepository.FigureOutMaxLengths"/>
        public static int State_MaxLength = -1;
        ///<inheritdoc cref="IRepository.FigureOutMaxLengths"/>
        public static int ValidationXml_MaxLength = -1;

        #region Database Properties
        private string _name;
        private DatabaseType _databaseType;
        private string _server;
        private string _database;
        private string _state;
        private string _validationXml;
        private bool _isPrimaryExtractionTable;
        private int? _identifierDumpServer_ID;
        private bool _isTableValuedFunction;
        private string _schema;

        /// <summary>
        /// Fully specified table name
        /// </summary>
        [Sql]
        [NotNull]
        public string Name
        {
            get { return _name; }
            set { SetField(ref _name, value); }
        }

        /// <inheritdoc/>
        public DatabaseType DatabaseType
        {
            get { return _databaseType; }
            set { SetField(ref _databaseType, value); }
        }

        /// <inheritdoc/>
        public string Server
        {
            get { return _server; }
            set { SetField(ref _server, value); }
        }

        /// <inheritdoc/>
        [Sql]
        public string Database
        {
            get { return _database; }
            set { SetField(ref _database, value); }
        }

        /// <summary>
        /// Obsolete
        /// </summary>
        [Obsolete("Not used for anything")]
        public string State
        {
            get { return _state; }
            set { SetField(ref _state, value); }
        }

        /// <summary>
        /// Obsolete
        /// </summary>
        [Obsolete("Not used for anything")]
        [DoNotExtractProperty]
        public string ValidationXml
        {
            get { return _validationXml; }
            set { SetField(ref _validationXml, value); }
        }

        /// <summary>
        /// <para>Indicates that this TableInfo should be the first table joined in any query that has multiple other TableInfos</para>
        /// 
        /// <para>When determining how to join a collection of TableInfos the <see cref="QueryBuilder"/> will attempt to find <see cref="JoinInfo"/> pairings between <see cref="ColumnInfo"/> in
        /// the tables.  If it cannot work out how to resolve the join order (e.g. if there are 3+ tables and joins going in both directions) then it will demand that one of the 
        /// <see cref="TableInfo"/> be picked as the first table from which all other tables should then be joined.</para>
        /// </summary>
        public bool IsPrimaryExtractionTable
        {
            get { return _isPrimaryExtractionTable; }
            set { SetField(ref _isPrimaryExtractionTable, value); }
        }

        /// <inheritdoc/>
        public int? IdentifierDumpServer_ID
        {
            get { return _identifierDumpServer_ID; }
            set { SetField(ref _identifierDumpServer_ID, value); }
        }

        /// <inheritdoc/>
        public bool IsTableValuedFunction
        {
            get { return _isTableValuedFunction; }
            set { SetField(ref _isTableValuedFunction, value); }
        }

        /// <inheritdoc/>
        public string Schema
        {
            get { return _schema; }
            set { SetField(ref _schema, value); }
        }

        #endregion
        
        // Temporary fix to remove downcasts to CatalogueRepository when using CatalogueRepository specific classes etc.
        // Need to fix underlying design issue of having an IRepository in the base when this class requires an ICatalogueRepository
        private readonly ICatalogueRepository _catalogueRepository;
        private Lazy<ColumnInfo[]> _knownColumnInfos;
        private Lazy<bool> _knownIsLookup;
        

        #region Relationships
        /// <inheritdoc/>
        [NoMappingToDatabase]
        public ColumnInfo[] ColumnInfos { get
        {
            return _knownColumnInfos.Value;
        }}

        /// <inheritdoc/>
        [NoMappingToDatabase]
        public PreLoadDiscardedColumn[] PreLoadDiscardedColumns { get
        {
            return Repository.GetAllObjectsWithParent<PreLoadDiscardedColumn>(this);
        }}
        
        /// <inheritdoc cref="IdentifierDumpServer_ID"/>
        [NoMappingToDatabase]
        public ExternalDatabaseServer IdentifierDumpServer { get
        {
            return IdentifierDumpServer_ID == null
                ? null
                : Repository.GetObjectByID<ExternalDatabaseServer>((int) IdentifierDumpServer_ID);
        }}

        #endregion

        /// <summary>
        /// Defines a new table reference in the platform database <paramref name="repository"/>.  
        /// <para>Usually you should use <see cref="TableInfoImporter"/> instead</para>
        /// </summary>
        /// <param name="repository"></param>
        /// <param name="name"></param>
        public TableInfo(ICatalogueRepository repository, string name)
        {
            _catalogueRepository = repository;
            repository.InsertAndHydrate(this, new Dictionary<string, object>
            {
                {"Name", name}
            });

            ClearAllInjections();
        }

        internal TableInfo(ICatalogueRepository repository, DbDataReader r)
            : base(repository, r)
        {
            _catalogueRepository = repository;

            Name =r["Name"].ToString();
            DatabaseType = (DatabaseType)Enum.Parse(typeof(DatabaseType), r["DatabaseType"].ToString());
            Server = r["Server"].ToString();
            Database = r["Database"].ToString();
            State = r["State"].ToString();
            Schema = r["Schema"].ToString();
            ValidationXml = r["ValidationXml"].ToString();
            
            IsTableValuedFunction = r["IsTableValuedFunction"] != DBNull.Value && Convert.ToBoolean(r["IsTableValuedFunction"]);
            
            if(r["IsPrimaryExtractionTable"] == DBNull.Value)
                IsPrimaryExtractionTable = false;
            else 
                IsPrimaryExtractionTable = Convert.ToBoolean(r["IsPrimaryExtractionTable"]);

            if (r["IdentifierDumpServer_ID"] == DBNull.Value)
                IdentifierDumpServer_ID = null;
            else
                IdentifierDumpServer_ID = (int)r["IdentifierDumpServer_ID"];

            ClearAllInjections();
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return Name;
        }

        /// <inheritdoc/>
        public ISqlParameter[] GetAllParameters()
        {
            return _catalogueRepository.GetAllParametersForParentTable(this).ToArray();
        }

        /// <summary>
        /// Sorts two <see cref="TableInfo"/> alphabetically
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public int CompareTo(object obj)
        {
            if (obj is TableInfo)
            {
                return -(obj.ToString().CompareTo(this.ToString())); //sort alphabetically (reverse)
            }

            throw new Exception("Cannot compare " + this.GetType().Name + " to " + obj.GetType().Name);
        }


        /// <inheritdoc/>
        public string GetRuntimeName()
        {
            return GetQuerySyntaxHelper().GetRuntimeName(Name);
        }

        /// <inheritdoc/>
        public string GetFullyQualifiedName()
        {
            return GetQuerySyntaxHelper().EnsureFullyQualified(Database, Schema, GetRuntimeName());
        }

        /// <summary>
        /// Returns <see cref="Database"/> trimmed of any database qualifiers (e.g. square brackets)
        /// </summary>
        /// <returns></returns>
        public string GetDatabaseRuntimeName()
        {
            return Database.Trim(QuerySyntaxHelper.TableNameQualifiers);
        }


        /// <inheritdoc/>
        public string GetDatabaseRuntimeName(LoadStage loadStage,INameDatabasesAndTablesDuringLoads namer = null)
        {
            var baseName = GetDatabaseRuntimeName();
            
            if(namer == null)
                namer = new FixedStagingDatabaseNamer(baseName);

            return namer.GetDatabaseName(baseName, loadStage.ToLoadBubble());
        }

        /// <inheritdoc/>
        public string GetRuntimeName(LoadBubble bubble, INameDatabasesAndTablesDuringLoads tableNamingScheme = null)
        {
            // If no naming scheme is specified, the default 'FixedStaging...' prepends the database name and appends '_STAGING'
            if (tableNamingScheme == null)
                tableNamingScheme = new FixedStagingDatabaseNamer(Database);

            string baseName = GetQuerySyntaxHelper().GetRuntimeName(Name);

            return tableNamingScheme.GetName(baseName, bubble);
        }

        /// <inheritdoc/>
        public string GetRuntimeName(LoadStage stage, INameDatabasesAndTablesDuringLoads tableNamingScheme = null)
        {
            return GetRuntimeName(stage.ToLoadBubble(), tableNamingScheme);
        }
        
        /// <inheritdoc/>
        public IDataAccessCredentials GetCredentialsIfExists(DataAccessContext context)
        {
            if (context == DataAccessContext.Any)
                throw new Exception("You cannot ask for any credentials, you must supply a usage context.");

            return _catalogueRepository.TableInfoToCredentialsLinker.GetCredentialsIfExistsFor(this, context);
        }

        /// <summary>
        /// Declares that the given <paramref name="credentials"/> should be used to access the data table referenced by this 
        /// <see cref="TableInfo"/> under the given <see cref="DataAccessContext"/> (loading data etc).
        /// </summary>
        /// <param name="credentials">Credentials to use (username / encrypted password)</param>
        /// <param name="context">When the credentials can be used (Use Any for any case)</param>
        /// <param name="allowOverwritting">False will throw if there is already credentials declared for the table/context</param>
        public void SetCredentials(DataAccessCredentials credentials, DataAccessContext context, bool allowOverwritting = false)
        {
            var existingCredentials = _catalogueRepository.TableInfoToCredentialsLinker.GetCredentialsIfExistsFor(this, context);
            
            //if user told us to set credentials to null complain
            if(credentials == null)
                throw new Exception("Credentials was null, to remove a credential use TableInfoToCredentialsLinker.BreakLinkBetween instead");
            
            //if there are existing credentialoguememls already
            if (existingCredentials != null)
            {

                //user is trying to set the same credentials again
                if(existingCredentials.Equals(credentials))
                    return;//dont bother

                if(!allowOverwritting)
                    throw new Exception("Cannot overwrite existing credentials " + existingCredentials.Name + " with new credentials " + credentials.Name + " with context " + context + " because allowOverwritting was false");
            
                //allow overwritting is on
                //remove the existing link
                _catalogueRepository.TableInfoToCredentialsLinker.BreakLinkBetween(existingCredentials, this, context);
            }
            //create a new one to the new credentials
            _catalogueRepository.TableInfoToCredentialsLinker.CreateLinkBetween(credentials, this, context);
        }

        /// <inheritdoc/>
        public IHasDependencies[] GetObjectsThisDependsOn()
        {
            return 
                _catalogueRepository.TableInfoToCredentialsLinker.GetCredentialsIfExistsFor(this)
                .Select(kvp => kvp.Value)
                .Cast<IHasDependencies>()
                .ToArray();
        }
        /// <inheritdoc/>
        public IHasDependencies[] GetObjectsDependingOnThis()
        {
            return ColumnInfos.ToArray();
        }


        /// <summary>
        /// Checks that the table referenced exists on the database server and that it's properties and <see cref="ColumnInfo"/> etc are synchronized with the live
        /// table as it exists on the server.
        /// </summary>
        /// <param name="notifier"></param>
        public void Check(ICheckNotifier notifier)
        {
            if (IsLookupTable())
                if (IsPrimaryExtractionTable)
                    notifier.OnCheckPerformed(new CheckEventArgs("Table is both a Lookup table AND is marked IsPrimaryExtractionTable",CheckResult.Fail));

            try
            {
                TableInfoSynchronizer synchronizer = new TableInfoSynchronizer(this);
                synchronizer.Synchronize(notifier);
            }
            catch (Exception e)
            {
                notifier.OnCheckPerformed(new CheckEventArgs("Synchronization failed on TableInfo " + this,CheckResult.Fail, e));
            }
        }

        /// <inheritdoc/>
        public bool IsLookupTable()
        {
            return _knownIsLookup.Value;
        }

        private bool FetchIsLookup()
        {
            using (var con = _catalogueRepository.GetConnection())
            {
                DbCommand cmd = DatabaseCommandHelper.GetCommand(
@"if exists (select 1 from Lookup join ColumnInfo on Lookup.Description_ID = ColumnInfo.ID where TableInfo_ID = @tableInfoID)
select 1
else
select 0", con.Connection, con.Transaction);

                DatabaseCommandHelper.AddParameterWithValueToCommand("@tableInfoID", cmd, ID);
                return Convert.ToBoolean(cmd.ExecuteScalar());
            }
        }

        /// <summary>
        /// Returns all Catalogues which have any CatalogueItems which are associated with any of the ColumnInfos of this TableInfo.  If this is a lookup table then expect to get back 
        /// a whole bunch of catalogues.  If you have multiple extractable catalogues that all present different views of a single TableInfo then they will all be returned.  The normal
        /// behaviour though for a regular data table with one catalogue used for extraction would be for a single Catalogue to get returned.
        /// </summary>
        /// <returns></returns>
        public Catalogue[] GetAllRelatedCatalogues()
        {
            return Repository.GetAllObjects<Catalogue>(
                string.Format(@"Where
  Catalogue.ID in (Select CatalogueItem.Catalogue_ID from
  CatalogueItem join
  ColumnInfo on ColumnInfo_ID = ColumnInfo.ID
  where
  TableInfo_ID = {0} )", ID)).ToArray();

        }

        /// <inheritdoc/>
        public IEnumerable<IHasStageSpecificRuntimeName> GetColumnsAtStage(LoadStage loadStage)
        {
            //if it is AdjustRaw then it will also have the pre load discarded columns
            if (loadStage <= LoadStage.AdjustRaw)
                foreach (PreLoadDiscardedColumn discardedColumn in PreLoadDiscardedColumns.Where(c => c.Destination != DiscardedColumnDestination.Dilute))
                    yield return discardedColumn;

            //also add column infos
            foreach (ColumnInfo c in ColumnInfos)
                if (loadStage <= LoadStage.AdjustRaw && c.GetRuntimeName().StartsWith("hic_"))
                    continue;
                else
                if(loadStage == LoadStage.AdjustStaging && 
                    //these two do not appear in staging
                    (c.GetRuntimeName().Equals(SpecialFieldNames.DataLoadRunID)  || c.GetRuntimeName().Equals(SpecialFieldNames.ValidFrom))
                    )
                    continue;
                else
                    yield return c;
        }

        /// <inheritdoc/>
        public IQuerySyntaxHelper GetQuerySyntaxHelper()
        {
            return new QuerySyntaxHelperFactory().Create(DatabaseType);
        }

        /// <inheritdoc/>
        public void InjectKnown(ColumnInfo[] instance)
        {
            _knownColumnInfos = new Lazy<ColumnInfo[]>(() => instance);
        }

        /// <inheritdoc/>
        public void ClearAllInjections()
        {
            _knownColumnInfos = new Lazy<ColumnInfo[]>(FetchColumnInfos);
            _knownIsLookup = new Lazy<bool>(FetchIsLookup);
        }

        private ColumnInfo[] FetchColumnInfos()
        {
            return Repository.GetAllObjectsWithParent<ColumnInfo,TableInfo>(this);
        }

        /// <inheritdoc/>
        public DiscoveredTable Discover(DataAccessContext context)
        {
            var db = DataAccessPortal.GetInstance().ExpectDatabase(this, context);

            if (IsTableValuedFunction)
                return db.ExpectTableValuedFunction(GetRuntimeName(), Schema);

            return db.ExpectTable(GetRuntimeName(),Schema);
        }

        /// <summary>
        /// Returns true if the TableInfo is a reference to the discovered live table (same database, same table name, same server)
        /// <para>By default servername is not checked since you can have server aliases e.g. localhost\sqlexpress could be the same as 127.0.0.1\sqlexpress</para>
        /// </summary>
        /// <param name="discoveredTable">Pass true to also check the servername is EXACTLY the same (dangerous due to the fact that servers can be accessed by hostname or IP etc)</param>
        /// <returns></returns>
        public bool Is(DiscoveredTable discoveredTable,bool alsoCheckServer = false)
        {
            return GetRuntimeName().Equals(discoveredTable.GetRuntimeName(),StringComparison.CurrentCultureIgnoreCase) &&
                   GetDatabaseRuntimeName().Equals(discoveredTable.Database.GetRuntimeName(), StringComparison.CurrentCultureIgnoreCase) &&
                   DatabaseType == discoveredTable.Database.Server.DatabaseType &&
                   (!alsoCheckServer || discoveredTable.Database.Server.Name.Equals(Server,StringComparison.CurrentCultureIgnoreCase));
        }
    }
}

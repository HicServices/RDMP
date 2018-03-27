using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text.RegularExpressions;
using CatalogueLibrary.Data.DataLoad;
using CatalogueLibrary.Data.EntityNaming;
using CatalogueLibrary.DataHelper;
using CatalogueLibrary.QueryBuilding;
using CatalogueLibrary.Repositories;
using CatalogueLibrary.Triggers;
using MapsDirectlyToDatabaseTable;

using ReusableLibraryCode;
using ReusableLibraryCode.Checks;
using ReusableLibraryCode.DataAccess;
using ReusableLibraryCode.DatabaseHelpers;
using ReusableLibraryCode.DatabaseHelpers.Discovery;
using ReusableLibraryCode.DatabaseHelpers.Discovery.QuerySyntax;


namespace CatalogueLibrary.Data
{
    /// <summary>
    /// Describes an sql table (or table valued function) on a given Server from which you intend to either extract and/or load / curate data.
    /// These can be created most easily by using TableInfoImporter.  This entity is the hanging off point for PreLoadDiscardedColumn, ColumnInfo etc
    /// 
    /// This class represents a constant for the RDMP and allows the system to detect when data analysts have randomly dropped/renamed columns without 
    /// telling anybody and present this information in a rational context to the systems user (see TableInfoSynchronizer).
    /// </summary>
    public class TableInfo : VersionedDatabaseEntity,ITableInfo,INamed, IHasFullyQualifiedNameToo
    {

        public static int Name_MaxLength = -1;
        public static int Server_MaxLength = -1;
        public static int Database_MaxLength = -1;
        public static int State_MaxLength = -1;
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

        public string Name
        {
            get { return _name; }
            set { SetField(ref _name, value); }
        }
        public DatabaseType DatabaseType
        {
            get { return _databaseType; }
            set { SetField(ref _databaseType, value); }
        }
        public string Server
        {
            get { return _server; }
            set { SetField(ref _server, value); }
        }
        public string Database
        {
            get { return _database; }
            set { SetField(ref _database, value); }
        }
        public string State
        {
            get { return _state; }
            set { SetField(ref _state, value); }
        }

        [DoNotExtractProperty]
        public string ValidationXml
        {
            get { return _validationXml; }
            set { SetField(ref _validationXml, value); }
        }
        public bool IsPrimaryExtractionTable
        {
            get { return _isPrimaryExtractionTable; }
            set { SetField(ref _isPrimaryExtractionTable, value); }
        }
        public int? IdentifierDumpServer_ID
        {
            get { return _identifierDumpServer_ID; }
            set { SetField(ref _identifierDumpServer_ID, value); }
        }
        public bool IsTableValuedFunction
        {
            get { return _isTableValuedFunction; }
            set { SetField(ref _isTableValuedFunction, value); }
        }

        #endregion

        

        // Temporary fix to remove downcasts to CatalogueRepository when using CatalogueRepository specific classes etc.
        // Need to fix underlying design issue of having an IRepository in the base when this class requires an ICatalogueRepository
        private readonly ICatalogueRepository _catalogueRepository;

        #region Relationships
        [NoMappingToDatabase]
        public ColumnInfo[] ColumnInfos { get
        {
            return Repository.GetAllObjectsWithParent<ColumnInfo>(this);
        }}

        [NoMappingToDatabase]
        public PreLoadDiscardedColumn[] PreLoadDiscardedColumns { get
        {
            return Repository.GetAllObjectsWithParent<PreLoadDiscardedColumn>(this);
        }}

        [NoMappingToDatabase]
        public ExternalDatabaseServer IdentifierDumpServer { get
        {
            return IdentifierDumpServer_ID == null
                ? null
                : Repository.GetObjectByID<ExternalDatabaseServer>((int) IdentifierDumpServer_ID);
        }}

        #endregion

        public TableInfo(ICatalogueRepository repository, string name)
        {
            _catalogueRepository = repository;
            repository.InsertAndHydrate(this, new Dictionary<string, object>
            {
                {"Name", name}
            });
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
        }

        public override string ToString()
        {
            return Name;
        }

        public override int GetHashCode()
        {
            return ID.GetHashCode();
        }

        public ISqlParameter[] GetAllParameters()
        {
            return _catalogueRepository.GetAllParametersForParentTable(this).ToArray();
        }

        public int CompareTo(object obj)
        {
            if (obj is TableInfo)
            {
                return -(obj.ToString().CompareTo(this.ToString())); //sort alphabetically (reverse)
            }

            throw new Exception("Cannot compare " + this.GetType().Name + " to " + obj.GetType().Name);
        }

        public string GetRuntimeName()
        {
            return GetQuerySyntaxHelper().GetRuntimeName(Name);
        }

        public string GetFullyQualifiedName()
        {
            return Name;
        }

        public string GetDatabaseRuntimeName()
        {
            return Database.Trim(new[] { '[', ']' });
        }

        public string GetDatabaseRuntimeName(LoadStage loadStage,INameDatabasesAndTablesDuringLoads namer = null)
        {
            var baseName = GetDatabaseRuntimeName();
            
            if(namer == null)
                namer = new FixedStagingDatabaseNamer(baseName);

            return namer.GetDatabaseName(baseName, loadStage.ToLoadBubble());
        }

        /// <summary>
        /// Get the runtime name of the table for a particular load stage, as the table name may be modified depending on the stage.
        /// </summary>
        /// <param name="bubble"></param>
        /// <param name="tableNamingScheme"></param>
        /// <returns></returns>
        public string GetRuntimeName(LoadBubble bubble, INameDatabasesAndTablesDuringLoads tableNamingScheme = null)
        {
            // If no naming scheme is specified, the default 'FixedStaging...' prepends the database name and appends '_STAGING'
            if (tableNamingScheme == null)
                tableNamingScheme = new FixedStagingDatabaseNamer(Database);

            string baseName = GetQuerySyntaxHelper().GetRuntimeName(Name);

            return tableNamingScheme.GetName(baseName, bubble);
        }

        public string GetRuntimeName(LoadStage stage, INameDatabasesAndTablesDuringLoads tableNamingScheme = null)
        {
            return GetRuntimeName(stage.ToLoadBubble(), tableNamingScheme);
        }
        
        IDataAccessCredentials IDataAccessPoint.GetCredentialsIfExists(DataAccessContext context)
        {
            return GetCredentialsIfExists(context);
        }

        public DataAccessCredentials GetCredentialsIfExists(DataAccessContext context)
        { 
            if(context == DataAccessContext.Any)
                throw new Exception("You cannot ask for any credentials, you must supply a usage context.");

            return _catalogueRepository.TableInfoToCredentialsLinker.GetCredentialsIfExistsFor(this, context);
        }

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

        public IHasDependencies[] GetObjectsThisDependsOn()
        {
            return 
                _catalogueRepository.TableInfoToCredentialsLinker.GetCredentialsIfExistsFor(this)
                .Select(kvp => kvp.Value)
                .Cast<IHasDependencies>()
                .ToArray();
        }

        public IHasDependencies[] GetObjectsDependingOnThis()
        {
            return ColumnInfos.ToArray();
        }


        public void Check(ICheckNotifier notifier)
        {
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

        public bool IsLookupTable()
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

        public IQuerySyntaxHelper GetQuerySyntaxHelper()
        {
            return new QuerySyntaxHelperFactory().Create(DatabaseType);
        }

    }
}
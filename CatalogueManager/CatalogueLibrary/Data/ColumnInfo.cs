using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text.RegularExpressions;
using CatalogueLibrary.Data.DataLoad;
using CatalogueLibrary.Repositories;
using MapsDirectlyToDatabaseTable;
using MapsDirectlyToDatabaseTable.Attributes;
using ReusableLibraryCode;
using ReusableLibraryCode.Checks;
using ReusableLibraryCode.DataAccess;
using ReusableLibraryCode.DatabaseHelpers.Discovery;
using ReusableLibraryCode.DatabaseHelpers.Discovery.QuerySyntax;


namespace CatalogueLibrary.Data
{
    /// <summary>
    /// References an SQL column in a TableInfo (which itself references an SQL table).  This is the RDMP's awareness of the state of your database.  You can
    /// synchronize against the underlying sql server state using TableInfoSynchronizer.
    /// 
    /// <para>A ColumnInfo can belong to an anonymisation group (ANOTable) e.g. ANOGPCode, in this case it will be aware not only of it's name and datatype in LIVE
    /// but also it's unanonymised name/datatype (see method GetRuntimeName(LoadStage stage)).</para>
    /// 
    /// <para>A ColumnInfo may seem superfluous since you can query much of it's information at runtime but consider the situation where TableInfo is a table valued 
    /// function or it is a view and someone deletes a column from the view without telling anyone.  The ColumnInfo ensures a standard unchanging representation
    /// for the RDMP so that it can rationalize and inform the system user of disapearing columns etc and let the user make decisions about how to resolve it 
    /// (which might be as simple as deleting the ColumnInfos although that will have knock on effects for extraction logic etc).</para>
    /// </summary>
    public class ColumnInfo : VersionedDatabaseEntity, IComparable, IColumnInfo, IResolveDuplication, IHasDependencies, ICheckable, IHasQuerySyntaxHelper, IHasFullyQualifiedNameToo, ISupplementalColumnInformation
    {
        ///<inheritdoc cref="IRepository.FigureOutMaxLengths"/>
        public static int Name_MaxLength;
        ///<inheritdoc cref="IRepository.FigureOutMaxLengths"/>
        public static int Data_type_MaxLength;
        ///<inheritdoc cref="IRepository.FigureOutMaxLengths"/>
        public static int Format_MaxLength;
        ///<inheritdoc cref="IRepository.FigureOutMaxLengths"/>
        public static int Digitisation_specs_MaxLength;
        ///<inheritdoc cref="IRepository.FigureOutMaxLengths"/>
        public static int Source_MaxLength;
        ///<inheritdoc cref="IRepository.FigureOutMaxLengths"/>
        public static int Description_MaxLength;
        ///<inheritdoc cref="IRepository.FigureOutMaxLengths"/>
        public static int RegexPattern_MaxLength;
        ///<inheritdoc cref="IRepository.FigureOutMaxLengths"/>
        public static int ValidationRules_MaxLength;

        #region Database Properties

        private int _tableInfoID;
        private int? _anoTableID;
        private string _name;
        private string _dataType;
        private string _format;
        private string _digitisationSpecs;
        private string _source;
        private string _description;
        private ColumnStatus? _status;
        private string _regexPattern;
        private string _validationRules;
        private bool _isPrimaryKey;
        private bool _isAutoIncrement;
        private int? _duplicateRecordResolutionOrder;
        private bool _duplicateRecordResolutionIsAscending;
        private string _collation;

        /// <summary>
        /// ID of the <see cref="TableInfo"/> that this <see cref="ColumnInfo"/> belongs to.
        /// </summary>
        public int TableInfo_ID
        {
            get { return _tableInfoID; }
            private set { SetField(ref  _tableInfoID, value); }
        }

        /// <summary>
        /// ID of the <see cref="ANOTable"/> transform that is applied to this <see cref="ColumnInfo"/> during
        /// data loads e.g. swap chi for anochi.
        /// </summary>
        public int? ANOTable_ID
        {
            get { return _anoTableID; }
            set { SetField(ref  _anoTableID, value); }
        }

        /// <summary>
        /// The fully specified name of the column in the underlying database table this record points at.
        /// </summary>
        [Sql]
        public string Name
        {
            get { return _name; }
            set { SetField(ref  _name, value); }
        }

        /// <summary>
        /// The proprietary SQL datatype of the column in the underlying database table this record points at.  
        /// <para>E.g. datetime2 or varchar2 (Oracle) or int etc</para>
        /// </summary>
        public string Data_type
        {
            get { return _dataType; }
            set { SetField(ref  _dataType, value); }
        }

        /// <summary>
        ///  User specified free text field.  Not used for anything by RDMP. 
        /// <para> Use <see cref="Collation"/> instead</para>
        /// </summary>
        public string Format
        {
            get { return _format; }
            set { SetField(ref  _format, value); }
        }

        /// <summary>
        /// User specified free text field.  Not used for anything by RDMP.
        /// </summary>
        public string Digitisation_specs
        {
            get { return _digitisationSpecs; }
            set { SetField(ref  _digitisationSpecs, value); }
        }

        /// <summary>
        ///  User specified free text field.  Not used for anything by RDMP.
        /// </summary>
        public string Source
        {
            get { return _source; }
            set { SetField(ref  _source, value); }
        }

        /// <summary>
        ///  User specified free text field.  Not used for anything by RDMP.
        /// </summary>
        public string Description
        {
            get { return _description; }
            set { SetField(ref  _description, value); }
        }

        /// <summary>
        ///  User specified free text field.  Not used for anything by RDMP.
        /// </summary>
        public ColumnStatus? Status
        {
            get { return _status; }
            set { SetField(ref  _status, value); }
        }

        /// <summary>
        /// Validation Regex that could be used to assess the cleanliness of data in the column.   Not used for anything by RDMP.
        /// <para>Use the data quality engine instead (See <see cref="Catalogue.ValidatorXML"/>)</para>
        /// </summary>
        public string RegexPattern
        {
            get { return _regexPattern; }
            set { SetField(ref  _regexPattern, value); }
        }

        /// <summary>
        /// Not used for anything by RDMP.
        /// <para>Use the data quality engine instead (See <see cref="Catalogue.ValidatorXML"/>)</para>
        /// </summary>
        public string ValidationRules
        {
            get { return _validationRules; }
            set { SetField(ref  _validationRules, value); }
        }

        /// <summary>
        /// Records whether the column in the underlying database table this record points at is part of the primary key or not.
        /// </summary>
        public bool IsPrimaryKey
        {
            get { return _isPrimaryKey; }
            set { SetField(ref  _isPrimaryKey, value); }
        }

        /// <summary>
        /// Records whether the column in the underlying database table this record points at is an anto increment identity column
        /// </summary>
        public bool IsAutoIncrement
        {
            get { return _isAutoIncrement; }
            set { SetField(ref  _isAutoIncrement, value); }
        }

        /// <summary>
        /// Records the collation of the column in the underlying database table this record points at if explicitly declared by dbms (only applicable for char datatypes)
        /// </summary>
        public string Collation
        {
            get { return _collation; }
            set { SetField(ref  _collation, value); }
        }
        
        /// <summary>
        /// The importance of this column in resolving primary key collisions during data loads (in RAW).  Columns with a lower number are consulted first when resolving
        /// collisions.  E.g. are the colliding records different on this column? if yes use <see cref="DuplicateRecordResolutionIsAscending"/> to pick which to delete
        /// otherwise move onto the next (non primary key) column.
        /// <para>Only applies if you have a PrimaryKeyCollisionResolverMutilation in the data load</para>
        /// </summary>
        public int? DuplicateRecordResolutionOrder
        {
            get { return _duplicateRecordResolutionOrder; }
            set { SetField(ref  _duplicateRecordResolutionOrder, value); }
        }

        /// <summary>
        /// Used in primary key collision resolution during data loads (in RAW).  If two records differ on this field (and <see cref="IsPrimaryKey"/> is false) then the order
        /// (<see cref="DuplicateRecordResolutionIsAscending"/>) will be used to decide which is deleted.
        /// <para>Only applies if you have a PrimaryKeyCollisionResolverMutilation in the data load</para>
        /// </summary>
        public bool DuplicateRecordResolutionIsAscending
        {
            get { return _duplicateRecordResolutionIsAscending; }
            set { SetField(ref  _duplicateRecordResolutionIsAscending, value); }
        }

        #endregion

        #region Relationships

        /// <inheritdoc cref="TableInfo_ID"/>
        [NoMappingToDatabase]
        public TableInfo TableInfo
        {
            get
            {
                return Repository.GetObjectByID<TableInfo>(TableInfo_ID);
            }
        }
        
        /// <inheritdoc cref="ANOTable_ID"/>
        [NoMappingToDatabase]
        public ANOTable ANOTable {
            get { return ANOTable_ID == null ? null : Repository.GetObjectByID<ANOTable>((int) ANOTable_ID); }
        }

        [NoMappingToDatabase]
        public IEnumerable<ExtractionInformation> ExtractionInformations {
            get { return CatalogueItems.Select(e=>e.ExtractionInformation).Where(o=>o != null); }
        }

        [NoMappingToDatabase]
        public IEnumerable<CatalogueItem> CatalogueItems
        {
            get { return Repository.GetAllObjectsWithParent<CatalogueItem>(this); }
        }

        #endregion
        
        /// <summary>
        /// Notional usage status of a <see cref="ColumnInfo"/>.  Not used for anything by RDMP.
        /// </summary>
        public enum ColumnStatus
        {
            /// <summary>
            /// Should not be used anymore.  Not used for anything by RDMP.
            /// </summary>
            Deprecated,

            /// <summary>
            /// Notional usage state of a <see cref="ColumnInfo"/>.  Not used for anything by RDMP.
            /// </summary>
            Inactive,

            /// <summary>
            /// Notional usage state of a <see cref="ColumnInfo"/>.  Not used for anything by RDMP.
            /// </summary>
            Archived,

            /// <summary>
            /// Normal state for columns.  Not used for anything by RDMP.
            /// </summary>
            Active
        }

        /// <summary>
        /// Creates a new record of a column found on a database server in the table referenced by <see cref="TableInfo"/>.  This constructor will be used
        /// when first importing a table reference (See <see cref="CatalogueLibrary.DataHelper.TableInfoImporter"/>)  and again whenever there are new columns
        /// discovered during table sync (See <see cref="TableInfoSynchronizer"/>)
        /// </summary>
        /// <param name="repository"></param>
        /// <param name="name"></param>
        /// <param name="type"></param>
        /// <param name="parent"></param>
        public ColumnInfo(ICatalogueRepository repository, string name, string type, TableInfo parent)
        {
            repository.InsertAndHydrate(this,new Dictionary<string, object>
            {
                {"Name", name != null ? (object) name : DBNull.Value},
                {"Data_type", type != null ? (object) type : DBNull.Value},
                {"TableInfo_ID", parent.ID}
            });
        }

        internal ColumnInfo(ICatalogueRepository repository, DbDataReader r)
            : base(repository, r)
        {
            TableInfo_ID = int.Parse(r["TableInfo_ID"].ToString());
            Name =r["Name"].ToString();
            Data_type = r["Data_type"].ToString();
            Format = r["Format"].ToString();
            Digitisation_specs = r["Digitisation_specs"].ToString();
            Source = r["Source"].ToString();
            Description = r["Description"].ToString();
            Collation = r["Collation"] as string;

            //try to turn string value in database into enum value
            ColumnStatus dbStatus;
            if (ColumnStatus.TryParse(r["Status"].ToString(), out dbStatus))
                Status = dbStatus;

            RegexPattern = r["RegexPattern"].ToString();
            ValidationRules = r["ValidationRules"].ToString();
            IsPrimaryKey = Boolean.Parse(r["IsPrimaryKey"].ToString());
            IsAutoIncrement = Boolean.Parse(r["IsAutoIncrement"].ToString());

            if (r["ANOTable_ID"] != DBNull.Value)
                ANOTable_ID = int.Parse(r["ANOTable_ID"].ToString());
            else
                ANOTable_ID = null;

            if (r["DuplicateRecordResolutionOrder"] != DBNull.Value)
                DuplicateRecordResolutionOrder = int.Parse(r["DuplicateRecordResolutionOrder"].ToString());
            else
                DuplicateRecordResolutionOrder = null;

            DuplicateRecordResolutionIsAscending = Convert.ToBoolean(r["DuplicateRecordResolutionIsAscending"]);

        }

        /// <summary>
        /// Returns the fully qualified <see cref="Name"/> of the <see cref="ColumnInfo"/>
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return Name;
        }

        /// <summary>
        /// Allows sorting by fully qualified <see cref="Name"/>.
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public int CompareTo(object obj)
        {
            if (obj is ColumnInfo)
            {
                return - (obj.ToString().CompareTo(this.ToString())); //sort alphabetically (reverse)
            }
            
            throw new Exception("Cannot compare " + this.GetType().Name + " to " + obj.GetType().Name);
            
        }
        
        ///<inheritdoc/>
        public string GetRuntimeName()
        {
            if (Name == null)
                return null;

            return GetQuerySyntaxHelper().GetRuntimeName(Name);
        }

        ///<inheritdoc/>
        public string GetFullyQualifiedName()
        {
            return Name;
        }

        private IQuerySyntaxHelper _cachedQuerySyntaxHelper;

        ///<inheritdoc/>
        public IQuerySyntaxHelper GetQuerySyntaxHelper()
        {
            if (_cachedQuerySyntaxHelper == null)
                _cachedQuerySyntaxHelper = TableInfo.GetQuerySyntaxHelper();

            return _cachedQuerySyntaxHelper;
        }

        ///<inheritdoc/>
        public string GetRuntimeName(LoadStage stage)
        {
            string finalName = this.GetRuntimeName();

            if (stage <= LoadStage.AdjustRaw)
            {
                //see if it has an ANO Transform on it
                if (ANOTable_ID != null && finalName.StartsWith("ANO"))
                    return finalName.Substring("ANO".Length);
            }

            //any other stage will be the regular final name
            return finalName;
        }

        /// <summary>
        /// Gets the DataType adjusted for the stage at which the ColumnInfo is at, this is almost always the same as Data_type.  The only
        /// time it is different is when there is an ANOTable involved e.g. ANOLocation could be a varchar(6) like 'AB10_L' after anonymisation
        /// but if the LoadStage is AdjustRaw then it would have a value like 'NH10' (varchar(4) - the unanonymised state).
        /// </summary>
        /// <param name="loadStage"></param>
        /// <returns></returns>
        public string GetRuntimeDataType(LoadStage loadStage)
        {
            if (loadStage <= LoadStage.AdjustRaw)
            {
                //if it has an ANO transform
                if (ANOTable_ID != null)
                    return ANOTable.GetRuntimeDataType(loadStage);    //get the datatype from the ANOTable because ColumnInfo is of mutable type depending on whether it has been anonymised yet 

                //it doesn't have an ANOtransform but it might be the subject of dilution
                var discard = TableInfo.PreLoadDiscardedColumns.SingleOrDefault(c=>c.GetRuntimeName().Equals(GetRuntimeName(),StringComparison.InvariantCultureIgnoreCase));

                //The column exists both in the live database and in the identifier dump.  This is because it goes through horrendous bitcrushing operations e.g. Load RAW with full
                //postcode varchar(8) and ship postcode off to identifier dump but also let it go through to live but only as the first 4 letters varchar(4).  so the datatype of the column
                //in RAW is varchar(8) but in Live is varchar(4)
                if (discard != null)
                    return discard.Data_type;
                    
               return Data_type;
                
            }

            //The user is asking about a stage other than RAW so tell them about the final column type state
            return Data_type;
        }
        
        /// <summary>
        /// Connects to the live database referenced by this <seealso cref="ColumnInfo"/> and discovers the column returning the
        /// live state of the column.
        /// <para>If the database/table/column doesn't exist or is inaccessible then this method will throw</para>
        /// </summary>
        /// <param name="context">Determines which credentials (if any) to use to perform the connection operation</param>
        /// <returns>The live state of the column</returns>
        public DiscoveredColumn Discover(DataAccessContext context)
        {
            var ti = TableInfo;
            var db = DataAccessPortal.GetInstance().ExpectDatabase(ti, context);
            return db.ExpectTable(ti.GetRuntimeName()).DiscoverColumn(GetRuntimeName());
        }

        ///<inheritdoc/>
        public IHasDependencies[] GetObjectsThisDependsOn()
        {
            List<IHasDependencies> iDependOn = new List<IHasDependencies>();

            iDependOn.AddRange(CatalogueItems);
            iDependOn.Add(TableInfo);

            if (ANOTable_ID != null)
              iDependOn.Add(ANOTable);
            
            return iDependOn.ToArray();
        }


        ///<inheritdoc/>
        public IHasDependencies[] GetObjectsDependingOnThis()
        {
            List<IHasDependencies> dependantObjects = new List<IHasDependencies>();
            
            //also any CatalogueItems that reference us
            dependantObjects.AddRange(CatalogueItems);
            
            //also lookups are dependent on us
            dependantObjects.AddRange(GetAllLookupForColumnInfoWhereItIsA(LookupType.AnyKey));

            //also join infoslookups are dependent on us
            dependantObjects.AddRange(
                ((CatalogueRepository) Repository).JoinInfoFinder.GetAllJoinInfos().Where(j =>
                j.ForeignKey_ID == ID ||
                j.PrimaryKey_ID == ID));

            return dependantObjects.ToArray(); //dependantObjects.ToArray();
        }

        /// <summary>
        /// Checks the ANO status of the column is valid (and matching on datatypes etc).  
        /// <para>Does not check for synchronization against the underlying database.</para>
        /// </summary>
        /// <param name="notifier"></param>
        public void Check(ICheckNotifier notifier)
        {
            //if it does not have an ANO transform it should not start with ANO
            if (ANOTable_ID == null)
            {
                //the column has no anotable
                //make sure it doesn't start with ANO (if it does it should have an ANOTable, maybe the user is calling his column ANOUNCEMENT or something (not permitted)
                if (GetRuntimeName().StartsWith(ANOTable.ANOPrefix))
                    notifier.OnCheckPerformed(new CheckEventArgs("ColumnInfo " + this + " (ID=" + ID + ") begins with " + ANOTable.ANOPrefix + " but does not have an ANOTable configured for it", CheckResult.Warning, null));
            }
            else//if it does have an ANO transform it must start with ANO
                if(!GetRuntimeName().StartsWith(ANOTable.ANOPrefix))
                    notifier.OnCheckPerformed(new CheckEventArgs("ColumnInfo " + this + " (ID=" + ID + ") has an ANOTable configured but does not start with " + ANOTable.ANOPrefix + " (All anonymised columns must start with "+ANOTable.ANOPrefix+")", CheckResult.Fail, null));
        }

        /// <summary>
        /// Determines whether the <see cref="ColumnInfo"/> is involved in <see cref="Lookup"/> declarations (either as a foreign key, or as part of a lookup <see cref="TableInfo"/>).
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public Lookup[] GetAllLookupForColumnInfoWhereItIsA(LookupType type)
        {
            string sql;
            if (type == LookupType.Description)
                sql = "SELECT * FROM Lookup WHERE Description_ID=" + ID;
            else if (type == LookupType.AnyKey)
                sql = "SELECT * FROM Lookup WHERE ForeignKey_ID=" + ID + " OR PrimaryKey_ID=" + ID;
            else if (type == LookupType.ForeignKey)
                sql = "SELECT * FROM Lookup WHERE ForeignKey_ID=" + ID;
            else
                throw new NotImplementedException("Unrecognised LookupType " + type);

            var lookups = Repository.SelectAll<Lookup>(sql, "ID").ToArray();

            if (lookups.Select(l => l.PrimaryKey_ID).Distinct().Count() > 1 && type == LookupType.ForeignKey)
                throw new Exception("Column " + this + " is configured as a foreign key to more than 1 primary key (only 1 is allowed), the Lookups are:" + string.Join(",", lookups.Select(l => l.PrimaryKey)));

            return lookups.ToArray();
        }

        public bool CouldSupportConvertingToANOColumnInfo(out string reason)
        {
            if (GetRuntimeName().StartsWith(ANOTable.ANOPrefix))
            {
                reason =
                    "Column " + this + " begins with '" + ANOTable.ANOPrefix +
                    "' it cannot be converted into an ANO version because it is assumed to already be in the anonymous structure, you should instead identify the relevant ANOTable and configure it in the main ConfigureANOForTableInfo dialog";
                
                return false;
            }

            if (ANOTable_ID != null)
            {
                reason =
                    "Column " + this +
                    " ALREADY HAS an ANOTable associated with it! the whole purpose of this UI is to convert a column to an ANO versions (and handle migrating of identifiable data if existing), if the column already has an ANOTable associated with it then this is impossible";
                return false;
            }

            reason = null;
            return true;
        }
    }
}

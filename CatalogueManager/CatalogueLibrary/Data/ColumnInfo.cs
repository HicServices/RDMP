using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text.RegularExpressions;
using CatalogueLibrary.Data.DataLoad;
using CatalogueLibrary.Repositories;
using MapsDirectlyToDatabaseTable;
using ReusableLibraryCode;
using ReusableLibraryCode.Checks;
using ReusableLibraryCode.DataAccess;
using ReusableLibraryCode.DatabaseHelpers.Discovery.QuerySyntax;


namespace CatalogueLibrary.Data
{
    /// <summary>
    /// References an SQL column in a TableInfo (which itself references an SQL table).  This is the RDMP's awareness of the state of your database.  You can
    /// synchronize against the underlying sql server state using TableInfoSynchronizer.
    /// 
    /// A ColumnInfo can belong to an anonymisation group (ANOTable) e.g. ANOGPCode, in this case it will be aware not only of it's name and datatype in LIVE
    /// but also it's unanonymised name/datatype (see method GetRuntimeName(LoadStage stage)).
    /// 
    /// A ColumnInfo may seem superfluous since you can query much of it's information at runtime but consider the situation where TableInfo is a table valued 
    /// function or it is a view and someone deletes a column from the view without telling anyone.  The ColumnInfo ensures a standard unchanging representation
    /// for the RDMP so that it can rationalize and inform the system user of disapearing columns etc and let the user make decisions about how to resolve it 
    /// (which might be as simple as deleting the ColumnInfos although that will have knock on effects for extraction logic etc).
    /// </summary>
    public class ColumnInfo : VersionedDatabaseEntity, IDeleteable, IComparable, IColumnInfo, IResolveDuplication, IHasDependencies, ICheckable, IHasQuerySyntaxHelper, IHasFullyQualifiedNameToo
    {
        public static int Name_MaxLength;
        public static int Data_type_MaxLength;
        public static int Format_MaxLength;
        public static int Digitisation_specs_MaxLength;
        public static int Source_MaxLength;
        public static int Description_MaxLength;
        public static int RegexPattern_MaxLength;
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
        private int? _duplicateRecordResolutionOrder;
        private bool _duplicateRecordResolutionIsAscending;
        public int TableInfo_ID
        {
            get { return _tableInfoID; }
            private set { SetField(ref  _tableInfoID, value); }
        }

        public int? ANOTable_ID
        {
            get { return _anoTableID; }
            set { SetField(ref  _anoTableID, value); }
        }

        [Sql]
        public string Name
        {
            get { return _name; }
            set { SetField(ref  _name, value); }
        }

        public string Data_type
        {
            get { return _dataType; }
            set { SetField(ref  _dataType, value); }
        }

        public string Format
        {
            get { return _format; }
            set { SetField(ref  _format, value); }
        }

        public string Digitisation_specs
        {
            get { return _digitisationSpecs; }
            set { SetField(ref  _digitisationSpecs, value); }
        }

        public string Source
        {
            get { return _source; }
            set { SetField(ref  _source, value); }
        }

        public string Description
        {
            get { return _description; }
            set { SetField(ref  _description, value); }
        }

        public ColumnStatus? Status
        {
            get { return _status; }
            set { SetField(ref  _status, value); }
        }

        public string RegexPattern
        {
            get { return _regexPattern; }
            set { SetField(ref  _regexPattern, value); }
        }

        public string ValidationRules
        {
            get { return _validationRules; }
            set { SetField(ref  _validationRules, value); }
        }

        public bool IsPrimaryKey
        {
            get { return _isPrimaryKey; }
            set { SetField(ref  _isPrimaryKey, value); }
        }

        public int? DuplicateRecordResolutionOrder
        {
            get { return _duplicateRecordResolutionOrder; }
            set { SetField(ref  _duplicateRecordResolutionOrder, value); }
        }

        public bool DuplicateRecordResolutionIsAscending
        {
            get { return _duplicateRecordResolutionIsAscending; }
            set { SetField(ref  _duplicateRecordResolutionIsAscending, value); }
        }

        #endregion

        #region Relationships

        [NoMappingToDatabase]
        public TableInfo TableInfo
        {
            get
            {
                return Repository.GetObjectByID<TableInfo>(TableInfo_ID);
            }
        }
        
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
        
        public enum ColumnStatus
        {
            Deprecated,
            Inactive,
            Archived,
            Active
        }

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

            //try to turn string value in database into enum value
            ColumnStatus dbStatus;
            if (ColumnStatus.TryParse(r["Status"].ToString(), out dbStatus))
                Status = dbStatus;

            RegexPattern = r["RegexPattern"].ToString();
            ValidationRules = r["ValidationRules"].ToString();
            IsPrimaryKey = Boolean.Parse(r["IsPrimaryKey"].ToString());

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

        public override string ToString()
        {
            return Name;
        }

        public int CompareTo(object obj)
        {
            if (obj is ColumnInfo)
            {
                return - (obj.ToString().CompareTo(this.ToString())); //sort alphabetically (reverse)
            }
            
            throw new Exception("Cannot compare " + this.GetType().Name + " to " + obj.GetType().Name);
            
        }


        public string GetRuntimeName()
        {
            if (Name == null)
                return null;

            return GetQuerySyntaxHelper().GetRuntimeName(Name);
        }

        public string GetFullyQualifiedName()
        {
            return Name;
        }

        public override int GetHashCode()
        {
            return Repository.GetHashCode(this);
        }

        private IQuerySyntaxHelper _cachedQuerySyntaxHelper;
        public IQuerySyntaxHelper GetQuerySyntaxHelper()
        {
            if (_cachedQuerySyntaxHelper == null)
                _cachedQuerySyntaxHelper = TableInfo.GetQuerySyntaxHelper();

            return _cachedQuerySyntaxHelper;
        }


        public override bool Equals(object obj)
        {
            return Repository.AreEqual(this, obj);
        }

        
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

        public int? GetColumnLengthIfAny()
        {
            Regex r = new Regex(@"\(\d+\)");

            if (string.IsNullOrWhiteSpace(Data_type))
                return null;
            Match match = r.Match(Data_type);
            if (match.Success)
                return int.Parse(match.Value.TrimStart(new[] {'('}).TrimEnd(new[] {')'}));

            return null;
        }

        public IHasDependencies[] GetObjectsThisDependsOn()
        {
            List<IHasDependencies> iDependOn = new List<IHasDependencies>();

            iDependOn.AddRange(CatalogueItems);
            iDependOn.Add(TableInfo);

            if (ANOTable_ID != null)
              iDependOn.Add(ANOTable);


            return iDependOn.ToArray();
        }

        

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

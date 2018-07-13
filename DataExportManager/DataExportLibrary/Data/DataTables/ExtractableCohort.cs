using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Diagnostics;
using System.Linq;
using CatalogueLibrary.Data;
using CatalogueLibrary.QueryBuilding;
using CatalogueLibrary.Repositories;
using DataExportLibrary.Interfaces.Data;
using DataExportLibrary.Interfaces.Data.DataTables;
using MapsDirectlyToDatabaseTable;
using MapsDirectlyToDatabaseTable.Injection;
using ReusableLibraryCode;
using ReusableLibraryCode.Checks;
using ReusableLibraryCode.DatabaseHelpers.Discovery;
using ReusableLibraryCode.DatabaseHelpers.Discovery.QuerySyntax;
using ReusableLibraryCode.Progress;
using DataTable = System.Data.DataTable;

namespace DataExportLibrary.Data.DataTables
{

    /// <summary>
    /// While actual patient identifiers are stored in an external database (referenced by a ExternalCohortTable), the RDMP still needs to have a reference to each cohort for extaction.
    /// The ExtractableCohort object is a record that documents the location and ID of a cohort in your ExternalCohortTable.  This record means that the RDMP can record which cohorts
    /// are part of which ExtractionConfiguration in a Project without ever having to move the identifiers into the RDMP application database.
    /// 
    /// <para>The most important field in ExtractableCohort is the OriginID, this field represents the id of the cohort in the CohortDefinition table of the ExternalCohortTable.  Effectively
    /// this number is the id of the cohort in your cohort database while the ID property of the ExtractableCohort (as opposed to OriginID) is the RDMP ID assigned to the cohort.  This
    /// allows you to have two different cohort sources both of which have a cohort id 10 but the RDMP software is able to tell the difference.  In addition it allows for the unfortunate
    /// situation in which you delete a cohort in your cohort database and leave the ExtractableCohort orphaned - under such circumstances you will at least still have your RDMP configuration
    /// and know the location of the original cohort even if it doesn't exist anymore. </para>
    /// </summary>
    public class ExtractableCohort : VersionedDatabaseEntity, IExtractableCohort, IInjectKnown<IExternalCohortDefinitionData>, IInjectKnown<ExternalCohortTable>, IHasDependencies
    {
        #region Database Properties
        private int _externalCohortTable_ID;
        private string _overrideReleaseIdentifierSQL;
        private string _auditLog;

        public int ExternalCohortTable_ID
        {
            get { return _externalCohortTable_ID; }
            set { SetField(ref _externalCohortTable_ID, value); }
        }
        public string OverrideReleaseIdentifierSQL
        {
            get { return _overrideReleaseIdentifierSQL; }
            set { SetField(ref _overrideReleaseIdentifierSQL, value); }
        }
        public string AuditLog
        {
            get { return _auditLog; }
            set { SetField(ref _auditLog, value); }
        }

        /// <summary>
        /// The cohortDefinition_id used to identify this cohort in the external cohort server/database that this ExtractableCohort comes from.
        /// 
        /// <para>Because there can be multiple Cohort sources there can be overlap in these i.e. cohort 1 from source 1 is not the same as cohort 1 from source 2</para>
        /// 
        /// <para>Therefore this is completely different from the ID of this ExtractableCohort - which is unique within the DataExportManager database</para>
        /// </summary>
        public int OriginID
        {
            get { return _originID; }
            set { SetField(ref _originID, value); }
        }


        #endregion

        public const string CohortLoggingTask = "CohortManagement";

        
        private int _count = -1;


        [NoMappingToDatabase]
        public int Count
        {
            get
            {
                if (_count != -1)
                    return _count;
                else
                {
                    _count = CountCohortInDatabase();
                    return _count;
                }
            }
        }

        private int _countDistinct = -1;
        [NoMappingToDatabase]
        public int CountDistinct
        {
            get
            {
                if (_countDistinct != -1)
                    return _countDistinct;
                else
                {
                    _countDistinct = CountDISTINCTCohortInDatabase();
                    return _countDistinct;
                }
            }
        }

        ///<inheritdoc cref="IRepository.FigureOutMaxLengths"/>
        public static int OverrideReleaseIdentifierSQL_MaxLength = -1;

        private Dictionary<string, string> _releaseToPrivateKeyDictionary;
        
        #region Relationships
        /// <inheritdoc cref="ExternalCohortTable_ID"/>
        [NoMappingToDatabase]
        public IExternalCohortTable ExternalCohortTable
        {
            get { return _knownExternalCohortTable.Value;}
        }

        #endregion



        internal ExtractableCohort(IDataExportRepository repository, DbDataReader r)
            : base(repository, r)
        {
            OverrideReleaseIdentifierSQL = r["OverrideReleaseIdentifierSQL"] as string;
            OriginID = Convert.ToInt32(r["OriginID"]);
            ExternalCohortTable_ID = Convert.ToInt32(r["ExternalCohortTable_ID"]);
            AuditLog = r["AuditLog"] as string;

            ClearAllInjections();
        }

        public IExternalCohortDefinitionData GetExternalData()
        {
            return ExternalCohortTable.GetExternalData(this);
        }

        
        private Lazy<IExternalCohortDefinitionData> _cacheData;
        private Lazy<IExternalCohortTable> _knownExternalCohortTable;
        private int _originID;

        public ExtractableCohort(IDataExportRepository repository, ExternalCohortTable externalSource, int originalId)
        {
            Repository = repository;

            if (!externalSource.IDExistsInCohortTable(originalId))
                throw new Exception("ID " + originalId + " does not exist in Cohort Definitions (Referential Integrity Problem)");

            Repository.InsertAndHydrate(this, new Dictionary<string, object>
            {
                {"OriginID", originalId},
                {"ExternalCohortTable_ID", externalSource.ID}
            });

            ClearAllInjections();
        }

        public override string ToString()
        {
            IExternalCohortDefinitionData v = null;

            try
            {
                v = _cacheData.Value;
            }
            catch (Exception e)
            {
                _cacheData = new Lazy<IExternalCohortDefinitionData>(()=>null);    
            }
            if (v == null)
                return "Broken Cohort";

            return v.ExternalProjectNumber + "_" + v.ExternalDescription + "_V" + v.ExternalVersion;
        }

        private IQuerySyntaxHelper _cachedQuerySyntaxHelper;
        
        public IQuerySyntaxHelper GetQuerySyntaxHelper()
        {
            if (_cachedQuerySyntaxHelper == null)
                _cachedQuerySyntaxHelper = ExternalCohortTable.GetQuerySyntaxHelper();

            return _cachedQuerySyntaxHelper;
        }

        #region Stuff for executing the actual queries described by this class (generating cohorts etc)
        
        public DataTable FetchEntireCohort()
        {
            var ect = ExternalCohortTable;

            var db = ect.Discover();
            using (var con = db.Server.GetConnection())
            {
                con.Open();
                string sql = "SELECT * FROM " + ect.TableName + " WHERE " + this.WhereSQL();

                var da = db.Server.GetDataAdapter(sql, con);
                var dtReturn = new DataTable();
                da.Fill(dtReturn);

                dtReturn.TableName = ect.GetQuerySyntaxHelper().GetRuntimeName(ect.TableName);
                
                return dtReturn;
            }
        }

        [Obsolete("Use FetchEntireCohort instea")]
        public DataTable GetReleaseIdentifierMap(IDataLoadEventListener listener)
        {
            listener.OnNotify(this, new NotifyEventArgs(ProgressEventType.Information, "About to fetch release map as data table"));
            DataTable toReturn = new DataTable();

            var db = ExternalCohortTable.Discover();
            using (var con = db.Server.GetConnection())
            {
                con.Open();

                string sql =
                    string.Format(
                        "SELECT {0},{1} FROM {2} where " + ExternalCohortTable.DefinitionTableForeignKeyField + "=" +
                        OriginID
                        , GetPrivateIdentifier()
                        , GetReleaseIdentifier()
                        , ExternalCohortTable.TableName);

                var da = db.Server.GetDataAdapter(sql, con);
                da.Fill(toReturn);
            }

            listener.OnNotify(this, new NotifyEventArgs(ProgressEventType.Information, "Release map data table fetched, it has " + toReturn.Rows.Count + " rows"));
            return toReturn;
        }

        /// <summary>
        /// Gets the comparison check to use as part of a Where query, this does not include the WHERE section so that you can nest it deep inside giaganot OR AND trees if you feel like it
        /// </summary>
        /// <returns></returns>
        public string WhereSQL()
        {
            var ect = ExternalCohortTable;
            var querySyntaxHelper = ect.GetQuerySyntaxHelper();

            return querySyntaxHelper.EnsureFullyQualified(ect.Database, null, ect.TableName, ect.DefinitionTableForeignKeyField) + "=" + OriginID;
        }

        private int CountCohortInDatabase()
        {
            var ect = ExternalCohortTable;

            var db = ect.Discover();
            using (var con = db.Server.GetConnection())
            {
                con.Open();

                return Convert.ToInt32(db.Server.GetCommand("SELECT count(*) FROM " + ect.TableName + " WHERE " + WhereSQL(), con).ExecuteScalar());
            }
        }

        private int CountDISTINCTCohortInDatabase()
        {
            return Convert.ToInt32(ExecuteScalar("SELECT count(DISTINCT "+GetReleaseIdentifier(true)+") FROM " + ExternalCohortTable.TableName + " WHERE " + WhereSQL()));
        }

        private object ExecuteScalar(string sql)
        {
            var ect = ExternalCohortTable;

            var db = ect.Discover();
            using (var con = db.Server.GetConnection())
            {
                con.Open();

                return db.Server.GetCommand(sql, con).ExecuteScalar();
            }
        }

        [Obsolete("This is super HIC specific, the first 3 digits of every release identifier is a project code")]
        public string GetFirstProCHIPrefix()
        {
            var ect = ExternalCohortTable;

            if (ect.DatabaseType != DatabaseType.MicrosoftSQLServer)
                return "";

            return (string)ExecuteScalar("SELECT  TOP 1 LEFT(" + GetReleaseIdentifier() + ",3) FROM " + ect.TableName + " WHERE " +WhereSQL());
        }
        
        #endregion
        
        public static IEnumerable<CohortDefinition> GetImportableCohortDefinitions(ExternalCohortTable externalSource)
        {
            string displayMemberName, valueMemberName, versionMemberName,projectNumberMemberName;
            DataTable dt = GetImportableCohortDefinitionsTable(externalSource, out displayMemberName, out valueMemberName, out versionMemberName,out projectNumberMemberName);

            foreach (DataRow r in dt.Rows)
            {
                yield return
                    new CohortDefinition(
                        Convert.ToInt32(r[valueMemberName]),
                        r[displayMemberName].ToString(),
                        Convert.ToInt32(r[versionMemberName]),
                        Convert.ToInt32(r[projectNumberMemberName])
                        ,externalSource);
           }
        }

        public static DataTable GetImportableCohortDefinitionsTable(ExternalCohortTable externalSource, out string displayMemberName, out string valueMemberName, out string versionMemberName, out string projectNumberMemberName)
        {
            var server = externalSource.GetExpectDatabase().Server;
            using (var con = server.GetConnection())
            {
                con.Open();
                string sql = string.Format(
                    "Select description,id,version,projectNumber from {0} where exists (Select 1 from {1} WHERE {2}=id)"
                    , externalSource.DefinitionTableName,
                     externalSource.TableName,
                     externalSource.DefinitionTableForeignKeyField);

                var da = server.GetDataAdapter(sql, con);

                displayMemberName = "description";
                valueMemberName = "id";
                versionMemberName = "version";
                projectNumberMemberName = "projectNumber";

                DataTable toReturn = new DataTable();
                da.Fill(toReturn);
                return toReturn;
                
            }
        }
        
        public string GetReleaseIdentifier(bool runtimeName = false)
        {
            var fullName = ExternalCohortTable.GetReleaseIdentifier(this);

            return runtimeName ? GetQuerySyntaxHelper().GetRuntimeName(fullName) : fullName;
        }

        public string GetPrivateIdentifier(bool runtimeName = false)
        {
            //cannot be overwritten by ExtractableCohort but for ease we can return the value from the cached (during constructor) version of this entity
            var fullName = ExternalCohortTable.PrivateIdentifierField;

            return runtimeName ? GetQuerySyntaxHelper().GetRuntimeName(fullName) : fullName;

        }

        public string GetPrivateIdentifierDataType()
        {
            DiscoveredTable table = ExternalCohortTable.Discover().ExpectTable(ExternalCohortTable.TableName);
            
            //discover the column
            return table.DiscoverColumn(GetPrivateIdentifier(true))
                .DataType.SQLType; //and return it's datatype
            
        }

        public string GetReleaseIdentifierDataType()
        {
            DiscoveredTable table = ExternalCohortTable.Discover().ExpectTable(ExternalCohortTable.TableName);

            //discover the column
            return table.DiscoverColumn(GetReleaseIdentifier(true))
                .DataType.SQLType; //and return it's datatype
        }
        
        public DiscoveredDatabase GetDatabaseServer()
        {
            return ExternalCohortTable.Discover();
        }

        //these need to be private since ReverseAnonymiseDataTable will likely be called in batch
        private int _reverseAnonymiseProgressFetchingMap = 0;
        private int _reverseAnonymiseProgressReversing = 0;

        public void ReverseAnonymiseDataTable(DataTable toProcess, IDataLoadEventListener listener,bool allowCaching)
        {
            int haveWarnedAboutTop1AlreadyCount = 10;

            var syntax = ExternalCohortTable.GetQuerySyntaxHelper();
            
            string privateIdentifier = syntax.GetRuntimeName(GetPrivateIdentifier());
            string releaseIdentifier = syntax.GetRuntimeName(GetReleaseIdentifier());

            //if we don't want to support caching or there is no cached value yet
            if (!allowCaching || _releaseToPrivateKeyDictionary == null)
            {

                DataTable map = FetchEntireCohort();

                
                Stopwatch sw = new Stopwatch();
                sw.Start();
                //dictionary of released values (for the cohort) back to private values
                _releaseToPrivateKeyDictionary = new Dictionary<string, string>();
                foreach (DataRow r in map.Rows)
                {
                    if (_releaseToPrivateKeyDictionary.Keys.Contains(r[releaseIdentifier]))
                    {
                        if (haveWarnedAboutTop1AlreadyCount >0)
                        {
                            haveWarnedAboutTop1AlreadyCount--;
                            listener.OnNotify(this, new NotifyEventArgs(ProgressEventType.Warning,"Top 1-ing will occur for release identifier " + r[releaseIdentifier] + " because it maps to multiple private identifiers"));
                            
                        }
                        else
                        {
                            if (haveWarnedAboutTop1AlreadyCount == 0)
                            {
                                haveWarnedAboutTop1AlreadyCount = -1;
                                listener.OnNotify(this, new NotifyEventArgs(ProgressEventType.Warning, "Top 1-ing error message disabled due to flood of messages"));
                            }
                        }
                    }
                    else
                        _releaseToPrivateKeyDictionary.Add(r[releaseIdentifier].ToString().Trim(), r[privateIdentifier].ToString().Trim());

                    _reverseAnonymiseProgressFetchingMap++;

                    if(_reverseAnonymiseProgressFetchingMap%500 == 0)
                        listener.OnProgress(this, new ProgressEventArgs("Assembling Release Map Dictionary", new ProgressMeasurement(_reverseAnonymiseProgressFetchingMap, ProgressType.Records), sw.Elapsed));
                }

                listener.OnProgress(this, new ProgressEventArgs("Assembling Release Map Dictionary", new ProgressMeasurement(_reverseAnonymiseProgressFetchingMap, ProgressType.Records), sw.Elapsed));
            }
            int nullsFound = 0;
            int substitutions = 0;
            
            Stopwatch sw2 = new Stopwatch();
            sw2.Start();

            //fix values
            foreach (DataRow row in toProcess.Rows)
            {
                try
                {
                    object value = row[releaseIdentifier];

                    if(value == null || value == DBNull.Value)
                    {
                        nullsFound++;
                        continue;
                    }

                    row[releaseIdentifier] = _releaseToPrivateKeyDictionary[value.ToString().Trim()].Trim();//swap release value for private value (reversing the anonymisation)
                    substitutions++;

                    _reverseAnonymiseProgressReversing++;

                    if (_reverseAnonymiseProgressReversing % 500 == 0)
                        listener.OnProgress(this, new ProgressEventArgs("Substituting Release Identifiers For Private Identifiers", new ProgressMeasurement(_reverseAnonymiseProgressReversing, ProgressType.Records), sw2.Elapsed));
                }
                catch (KeyNotFoundException e)
                {
                    throw new Exception("Could not find private identifier (" + privateIdentifier + ") for the release identifier (" + releaseIdentifier + ") with value '" +row[releaseIdentifier] +"' in cohort with cohortDefinitionID " + OriginID ,e);
                }
            }
            
            //final value
            listener.OnProgress(this, new ProgressEventArgs("Substituting Release Identifiers For Private Identifiers", new ProgressMeasurement(_reverseAnonymiseProgressReversing, ProgressType.Records), sw2.Elapsed));
            
            if(nullsFound > 0)
                listener.OnNotify(this, new NotifyEventArgs(ProgressEventType.Warning,"Found " + nullsFound + " null release identifiers amongst the " + toProcess.Rows.Count + " rows of the input data table (on which we were attempting to reverse annonymise)"));

            listener.OnNotify(this, new NotifyEventArgs(substitutions >0?ProgressEventType.Information : ProgressEventType.Error,"Substituted " + substitutions + " release identifiers for private identifiers in input data table (input data table contained " + toProcess.Rows.Count +" rows)"));

            toProcess.Columns[releaseIdentifier].ColumnName = privateIdentifier;
        
        }

        public void AppendToAuditLog(string s)
        {
            if (AuditLog == null)
                AuditLog = "";

            AuditLog += Environment.NewLine + DateTime.Now + " " + Environment.UserName  + " " + s;
            SaveToDatabase();
        }

        public void InjectKnown(IExternalCohortDefinitionData instance)
        {
            _cacheData = new Lazy<IExternalCohortDefinitionData>(() => instance);
        }

        public void InjectKnown(ExternalCohortTable instance)
        {
            _knownExternalCohortTable = new Lazy<IExternalCohortTable>(() => instance);
        }

        public void ClearAllInjections()
        {
            _cacheData = new Lazy<IExternalCohortDefinitionData>(GetExternalData);
            _knownExternalCohortTable = new Lazy<IExternalCohortTable>(()=>Repository.GetObjectByID<ExternalCohortTable>(ExternalCohortTable_ID));
        }

        public IHasDependencies[] GetObjectsThisDependsOn()
        {
            return new[] { ExternalCohortTable };
        }

        public IHasDependencies[] GetObjectsDependingOnThis()
        {
            return Repository.GetAllObjects<ExtractionConfiguration>("WHERE Cohort_ID = " + ID);
        }
    }
        public enum OneToMErrorResolutionStrategy
    {
        TriggerFatalCrash,
        Top1,
        ExhaustivelyRecordDuplication
    }
}


// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Diagnostics;
using System.Linq;
using FAnsi.Discovery;
using FAnsi.Discovery.QuerySyntax;
using MapsDirectlyToDatabaseTable;
using MapsDirectlyToDatabaseTable.Attributes;
using MapsDirectlyToDatabaseTable.Injection;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Repositories;
using ReusableLibraryCode;
using ReusableLibraryCode.Progress;
using ReusableLibraryCode.Settings;

namespace Rdmp.Core.DataExport.Data
{

    /// <inheritdoc cref="IExtractableCohort"/>
    public class ExtractableCohort : DatabaseEntity, IExtractableCohort, IInjectKnown<IExternalCohortDefinitionData>, IInjectKnown<ExternalCohortTable>,  ICustomSearchString
    {
        /// <summary>
        /// Logging entry in the RDMP central relational log under which to record all activities that relate to creating cohorts
        /// </summary>
        public const string CohortLoggingTask = "CohortManagement";

        #region Database Properties
        private int _externalCohortTable_ID;
        private string _overrideReleaseIdentifierSQL;
        private string _auditLog;
        private bool _isDeprecated;

        /// <inheritdoc/>
        public int ExternalCohortTable_ID
        {
            get { return _externalCohortTable_ID; }
            set { SetField(ref _externalCohortTable_ID, value); }
        }

        /// <inheritdoc/>
        public string OverrideReleaseIdentifierSQL
        {
            get { return _overrideReleaseIdentifierSQL; }
            set { SetField(ref _overrideReleaseIdentifierSQL, value); }
        }

        /// <inheritdoc/>
        public string AuditLog
        {
            get { return _auditLog; }
            set { SetField(ref _auditLog, value); }
        }

        /// <inheritdoc/>
        public int OriginID
        {
            get { return _originID; }
            set { SetField(ref _originID, value); }
        }

        /// <summary>
        /// True if the cohort has been replaced by another cohort or otherwise should not be used
        /// </summary>
        public bool IsDeprecated
        {
            get { return _isDeprecated; }
            set { SetField(ref _isDeprecated, value); }
        }

        #endregion


        
        private int _count = -1;


        /// <inheritdoc/>
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
        /// <inheritdoc/>
        [NoMappingToDatabase]
        public int CountDistinct
        {
            get
            {
                if (_countDistinct != -1)
                    return _countDistinct;
                else
                {
                    _countDistinct = GetCountDistinctFromDatabase();
                    return _countDistinct;
                }
            }
        }

        private Dictionary<string, string> _releaseToPrivateKeyDictionary;
        
        #region Relationships
        /// <inheritdoc cref="ExternalCohortTable_ID"/>
        [NoMappingToDatabase]
        public IExternalCohortTable ExternalCohortTable
        {
            get { return _knownExternalCohortTable.Value;}
        }

        #endregion

        /// <summary>
        /// Alias field, returns <see cref="INamed.Name"/>
        /// </summary>
        [NoMappingToDatabase]
        [UsefulProperty(DisplayName = "Source")]
        public string Source { get { return ExternalCohortTable.Name; } }

        /// <summary>
        /// Fetches and returns the project number listed in the remote cohort database for this cohort (results are cached)
        /// </summary>
        [NoMappingToDatabase]
        [UsefulProperty(DisplayName = "P")]
        public int ExternalProjectNumber
        {
            get { return (int?)GetFromCacheData(x => x.ExternalProjectNumber) ?? -1; }
        }

        /// <summary>
        /// Fetches and returns the version number listed in the remote cohort database for this cohort (results are cached)
        /// </summary>
        [NoMappingToDatabase]
        [UsefulProperty(DisplayName = "V")]
        public int ExternalVersion
        {
            get { return (int?)GetFromCacheData(x => x.ExternalVersion) ?? -1; }
        }

        private object GetFromCacheData(Func<IExternalCohortDefinitionData, object> func)
        {
            if (_broken)
                return null;

            try
            {
                var v = _cacheData.Value;
                return func(v);
            }
            catch (Exception)
            {
                _broken = true;
                _cacheData = new Lazy<IExternalCohortDefinitionData>(() => null);
            }
                
            return null;
        }

        internal ExtractableCohort(IDataExportRepository repository, DbDataReader r)
            : base(repository, r)
        {
            OverrideReleaseIdentifierSQL = r["OverrideReleaseIdentifierSQL"] as string;
            OriginID = Convert.ToInt32(r["OriginID"]);
            ExternalCohortTable_ID = Convert.ToInt32(r["ExternalCohortTable_ID"]);
            AuditLog = r["AuditLog"] as string;
            IsDeprecated = (bool)r["IsDeprecated"];

            ClearAllInjections();
        }

        /// <inheritdoc/>
        public IExternalCohortDefinitionData GetExternalData(int timeout = -1)
        {
            var db = ExternalCohortTable.Discover();

            var syntax = db.Server.GetQuerySyntaxHelper();

            string sql =
                $@"Select 
{syntax.EnsureWrapped("projectNumber")},
{syntax.EnsureWrapped("description")},
{syntax.EnsureWrapped("version")},
{syntax.EnsureWrapped("dtCreated")}
from {ExternalCohortTable.DefinitionTableName} 
where 
    {syntax.EnsureWrapped("id")} = {OriginID}";

            
            using (var con = db.Server.GetConnection())
            {
                con.Open();
                using (var getDescription = db.Server.GetCommand(sql, con))
                {
                    if(timeout != -1)
                        getDescription.CommandTimeout = timeout;

                    using (var r = getDescription.ExecuteReader())
                    {
                        if (!r.Read())
                            return ExternalCohortDefinitionData.Orphan;

                        return new ExternalCohortDefinitionData(r, ExternalCohortTable.Name);
                    }
                }
            }
        }

        
        private Lazy<IExternalCohortDefinitionData> _cacheData;
        private Lazy<IExternalCohortTable> _knownExternalCohortTable;
        private int _originID;

        /// <summary>
        /// Creates a new cohort reference in the data export database.  This must resolve (via <paramref name="originalId"/>) to 
        /// a row in the external cohort database (<paramref name="externalSource"/>).
        /// </summary>
        /// <param name="repository"></param>
        /// <param name="externalSource"></param>
        /// <param name="originalId"></param>
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

        /// <summary>
        /// Returns the external description of the cohort (held in the remote cohort database <see cref="ExternalCohortTable"/>) or
        /// "Broken Cohort" if that database is unreachable
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return GetFromCacheData(x => x.ExternalDescription) as string ?? "Broken Cohort";
        }

        /// <inheritdoc/>
        public string GetSearchString()
        {
            return ToString() + " " + ExternalProjectNumber + " " + ExternalVersion;
        }

        private IQuerySyntaxHelper _cachedQuerySyntaxHelper;
        
        /// <inheritdoc/>
        public IQuerySyntaxHelper GetQuerySyntaxHelper()
        {
            if (_cachedQuerySyntaxHelper == null)
                _cachedQuerySyntaxHelper = ExternalCohortTable.GetQuerySyntaxHelper();

            return _cachedQuerySyntaxHelper;
        }

        #region Stuff for executing the actual queries described by this class (generating cohorts etc)
        
        /// <inheritdoc/>
        public DataTable FetchEntireCohort()
        {
            var ect = ExternalCohortTable;

            var cohortTable = ect.DiscoverCohortTable();

            using (var con = cohortTable.Database.Server.GetConnection())
            {
                con.Open();
                string sql = "SELECT DISTINCT * FROM " + cohortTable.GetFullyQualifiedName() + " WHERE " + this.WhereSQL();

                var da = cohortTable.Database.Server.GetDataAdapter(sql, con);
                var dtReturn = new DataTable();
                da.Fill(dtReturn);

                dtReturn.TableName = cohortTable.GetRuntimeName();
                
                return dtReturn;
            }
        }
        
        /// <inheritdoc/>
        public string WhereSQL()
        {
            var ect = ExternalCohortTable;
            var syntax = ect.GetQuerySyntaxHelper();
            
            return syntax.EnsureFullyQualified(
                syntax.GetRuntimeName(ect.Database ?? string.Empty),
                /* no schema*/
                null, 
                syntax.GetRuntimeName(ect.TableName ?? string.Empty),
                syntax.GetRuntimeName(ect.DefinitionTableForeignKeyField ?? string.Empty)) + "=" + OriginID;
        }

        private int CountCohortInDatabase()
        {
            var ect = ExternalCohortTable;

            var db = ect.Discover();
            using (var con = db.Server.GetConnection())
            {
                con.Open();

                using(var cmd = db.Server.GetCommand("SELECT count(*) FROM " + ect.TableName + " WHERE " + WhereSQL(), con))
                    return Convert.ToInt32(cmd.ExecuteScalar());
            }
        }

        /// <inheritdoc/>
        public int GetCountDistinctFromDatabase(int timeout = -1)
        {
            var syntax = GetQuerySyntaxHelper();
            
            return Convert.ToInt32(ExecuteScalar("SELECT count(DISTINCT "+
                                                 syntax.EnsureWrapped(GetReleaseIdentifier(true))+") FROM " + ExternalCohortTable.TableName + " WHERE " + WhereSQL(),timeout));
        }

        private object ExecuteScalar(string sql, int timeout = -1)
        {
            var ect = ExternalCohortTable;

            var db = ect.Discover();
            using (var con = db.Server.GetConnection())
            {
                con.Open();

                using(var cmd = db.Server.GetCommand(sql, con))
                {
                    if(timeout != -1)
                        cmd.CommandTimeout = timeout;

                    return cmd.ExecuteScalar();
                }
                    
            }
        }
        
        #endregion
        
        /// <summary>
        /// Returns details of all cohorts held in <paramref name="externalSource"/> (that have at least one identifier mapping).
        /// </summary>
        /// <param name="externalSource"></param>
        /// <returns></returns>
        public static IEnumerable<CohortDefinition> GetImportableCohortDefinitions(ExternalCohortTable externalSource)
        {
            using (DataTable dt = GetImportableCohortDefinitionsTable(externalSource,
                out string displayMemberName,
                out string valueMemberName,
                out string versionMemberName,
                out string projectNumberMemberName))
            {
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
        }

        /// <summary>
        /// Returns the remote DataTable row held in <paramref name="externalSource"/> that describes all cohorts held in it (that have at least one identifier mapping).
        /// </summary>
        /// <param name="externalSource"></param>
        /// <param name="displayMemberName"></param>
        /// <param name="valueMemberName"></param>
        /// <param name="versionMemberName"></param>
        /// <param name="projectNumberMemberName"></param>
        /// <returns></returns>
        public static DataTable GetImportableCohortDefinitionsTable(ExternalCohortTable externalSource, out string displayMemberName, out string valueMemberName, out string versionMemberName, out string projectNumberMemberName)
        {
            var server = externalSource.Discover().Server;
            var syntax = server.GetQuerySyntaxHelper();

            using (var con = server.GetConnection())
            {
                con.Open();
                string sql =
                    $@"Select 
{syntax.EnsureWrapped("description")},
{syntax.EnsureWrapped("id")},
{syntax.EnsureWrapped("version")},
{syntax.EnsureWrapped("projectNumber")}
from {externalSource.DefinitionTableName} 
where 
    exists (Select 1 from {externalSource.TableName} WHERE {externalSource.DefinitionTableForeignKeyField}=id)";

                using (var da = server.GetDataAdapter(sql, con))
                {
                    displayMemberName = "description";
                    valueMemberName = "id";
                    versionMemberName = "version";
                    projectNumberMemberName = "projectNumber";

                    DataTable toReturn = new DataTable();
                    da.Fill(toReturn);
                    return toReturn;
                }
            }
        }
        
        /// <inheritdoc/>
        public string GetReleaseIdentifier(bool runtimeName = false)
        {
            //respect override
            string toReturn = string.IsNullOrWhiteSpace(OverrideReleaseIdentifierSQL)
                ? ExternalCohortTable.ReleaseIdentifierField
                : OverrideReleaseIdentifierSQL;

            if (toReturn.Equals(ExternalCohortTable.PrivateIdentifierField) && !UserSettings.AllowIdentifiableExtractions)
                throw new Exception("ReleaseIdentifier for cohort " + ID +
                                    " is the same as the PrivateIdentifierSQL, this is forbidden");

            var syntaxHelper = GetQuerySyntaxHelper();

            if (syntaxHelper.GetRuntimeName(toReturn).Equals(syntaxHelper.GetRuntimeName(ExternalCohortTable.PrivateIdentifierField)) && !UserSettings.AllowIdentifiableExtractions)
                throw new Exception("ReleaseIdentifier for cohort " + ID +
                                    " is the same as the PrivateIdentifierSQL, this is forbidden");

            return runtimeName ? GetQuerySyntaxHelper().GetRuntimeName(toReturn) : toReturn;
        }

        /// <inheritdoc/>
        public string GetPrivateIdentifier(bool runtimeName = false)
        {
            return runtimeName ? GetQuerySyntaxHelper().GetRuntimeName(ExternalCohortTable.PrivateIdentifierField) : ExternalCohortTable.PrivateIdentifierField;
        }

        /// <inheritdoc/>
        public string GetPrivateIdentifierDataType()
        {
            return ExternalCohortTable.DiscoverPrivateIdentifier().DataType.SQLType;
            
        }

        /// <inheritdoc/>
        public string GetReleaseIdentifierDataType()
        {
            return ExternalCohortTable.DiscoverReleaseIdentifier().DataType.SQLType;
        }
        

        /// <inheritdoc/>
        public DiscoveredDatabase GetDatabaseServer()
        {
            return ExternalCohortTable.Discover();
        }

        //these need to be private since ReverseAnonymiseDataTable will likely be called in batch
        private int _reverseAnonymiseProgressFetchingMap = 0;
        private int _reverseAnonymiseProgressReversing = 0;
        
        /// <summary>
        /// Indicates whether the database described in ExternalCohortTable is unreachable or if the cohort has since been deleted etc.
        /// </summary>
        private bool _broken;

        /// <inheritdoc/>
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

        /// <summary>
        /// Appends the <paramref name="s"/> to the <see cref="AuditLog"/> prefixed by the DateTime and Username of the caller and then saves to database
        /// </summary>
        /// <param name="s"></param>
        public void AppendToAuditLog(string s)
        {
            if (AuditLog == null)
                AuditLog = "";

            AuditLog += Environment.NewLine + DateTime.Now + " " + Environment.UserName  + " " + s;
            SaveToDatabase();
        }

        /// <inheritdoc/>
        public void InjectKnown(IExternalCohortDefinitionData instance)
        {
            if (instance == null)
                _broken = true;

            _cacheData = new Lazy<IExternalCohortDefinitionData>(() => instance);
        }

        /// <inheritdoc/>
        public void InjectKnown(ExternalCohortTable instance)
        {
            _knownExternalCohortTable = new Lazy<IExternalCohortTable>(() => instance);
        }
        /// <inheritdoc/>
        public void ClearAllInjections()
        {
            _cacheData = new Lazy<IExternalCohortDefinitionData>(()=>GetExternalData());
            _knownExternalCohortTable = new Lazy<IExternalCohortTable>(()=>Repository.GetObjectByID<ExternalCohortTable>(ExternalCohortTable_ID));
        }

        /// <inheritdoc/>
        public IHasDependencies[] GetObjectsThisDependsOn()
        {
            return new[] { ExternalCohortTable };
        }

        /// <inheritdoc/>
        public IHasDependencies[] GetObjectsDependingOnThis()
        {
            return Repository.GetAllObjectsWhere<ExtractionConfiguration>("Cohort_ID " , ID);
        }
    }
}


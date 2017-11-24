using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Common;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using CatalogueLibrary.Data.Cohort;
using CatalogueLibrary.Repositories;
using MapsDirectlyToDatabaseTable;
using MapsDirectlyToDatabaseTable.Revertable;
using ReusableLibraryCode;
using ReusableLibraryCode.Checks;
using ReusableLibraryCode.DatabaseHelpers.Discovery;

namespace CatalogueLibrary.Data.DataLoad
{
    /// <summary>
    /// Describes a specific operation carried out at a specific step of a LoadMetadata.  This could be 'unzip all files called *.zip in for loading' or
    /// 'after loading the data to live, call sp_clean_table1' or 'Connect to webservice X and download 1,000,000 records which will be serialized into XML'
    /// 
    /// The class achieves this wide ranging functionality through the interaction of ProcessTaskType and Path.  e.g. when ProcessTaskType is Attacher then
    /// Path functions as the Type name of a class that implements IAttacher e.g. 'LoadModules.Generic.Attachers.AnySeparatorFileAttacher'.  
    /// 
    /// Each ProcessTask can have one or more strongly typed arguments (see entity ProcessTaskArgument), these are discovered at design time by using
    /// reflection to query the Path e.g. 'AnySeparatorFileAttacher' for all properties marked with [DemandsInitialization] attribute.  This allows for 3rd party developers
    /// to write plugin classes to easily handle freaky source file types or complex/bespoke data load requirements.
    /// </summary>
    public class ProcessTask : VersionedDatabaseEntity, IProcessTask, IArgumentHost, ITableInfoCollectionHost, ILoadProgressHost, IOrderable,INamed, ICheckable
    {
        #region Database Properties

        private int _loadMetadataID;
        private int? _relatesSolelyToCatalogueID;
        private int _order;
        private string _path;
        private string _name;
        private LoadStage _loadStage;
        private ProcessTaskType _processTaskType;
        private bool _isDisabled;


        public int LoadMetadata_ID
        {
            get { return _loadMetadataID; }
            set { SetField(ref  _loadMetadataID, value); }
        }

        public int? RelatesSolelyToCatalogue_ID
        {
            get { return _relatesSolelyToCatalogueID; }
            set { SetField(ref  _relatesSolelyToCatalogueID, value); }
        }

        public int Order
        {
            get { return _order; }
            set { SetField(ref  _order, value); }
        }

        [AdjustableLocation]
        public string Path
        {
            get { return _path; }
            set { SetField(ref  _path, value); }
        }

        public string Name
        {
            get { return _name; }
            set { SetField(ref  _name, value); }
        }

        public LoadStage LoadStage
        {
            get { return _loadStage; }
            set { SetField(ref  _loadStage, value); }
        }

        public ProcessTaskType ProcessTaskType
        {
            get { return _processTaskType; }
            set { SetField(ref  _processTaskType, value); }
        }

        public bool IsDisabled
        {
            get { return _isDisabled; }
            set { SetField(ref  _isDisabled, value); }
        }

        #endregion
        #region Relationships
        [NoMappingToDatabase]
        public LoadMetadata LoadMetadata {
            get { return Repository.GetObjectByID<LoadMetadata>(LoadMetadata_ID); }
        }

        [NoMappingToDatabase]
        public IEnumerable<ProcessTaskArgument> ProcessTaskArguments { get { return Repository.GetAllObjectsWithParent<ProcessTaskArgument>(this);} }

        [NoMappingToDatabase]
        public Catalogue RelatesSolelyToCatalogue {
            get
            {
                return RelatesSolelyToCatalogue_ID == null
                    ? null
                    : Repository.GetObjectByID<Catalogue>((int) RelatesSolelyToCatalogue_ID);
            }
        }

        #endregion



        public ProcessTask(ICatalogueRepository repository, ILoadMetadata parent, LoadStage stage)
        {
            repository.InsertAndHydrate(this,new Dictionary<string, object>
            {
                {"LoadMetadata_ID", parent.ID},
                {"ProcessTaskType", ProcessTaskType.Executable.ToString()},
                {"LoadStage", stage},
                {"Name", "New Process" + Guid.NewGuid()}
            });
        }

        public ProcessTask(ICatalogueRepository repository, DbDataReader r)
            : base(repository, r)
        {
            LoadMetadata_ID = int.Parse(r["LoadMetaData_ID"].ToString());

            if (r["RelatesSolelyToCatalogue_ID"] != DBNull.Value)
                RelatesSolelyToCatalogue_ID = int.Parse(r["RelatesSolelyToCatalogue_ID"].ToString());

            Path = r["Path"] as string;
            Name = r["Name"] as string;
            Order = int.Parse(r["Order"].ToString());

            ProcessTaskType processTaskType;

            if (ProcessTaskType.TryParse(r["ProcessTaskType"] as string, out processTaskType))
                ProcessTaskType = processTaskType;
            else
                throw new Exception("Could not parse ProcessTaskType:" + r["ProcessTaskType"]);

            LoadStage loadStage;
            if (LoadStage.TryParse(r["LoadStage"] as string, out loadStage))
                LoadStage = loadStage;
            else
                throw new Exception("Could not parse LoadStage:" + r["LoadStage"]);

            IsDisabled = Convert.ToBoolean(r["IsDisabled"]);
        }

        public override string ToString()
        {
            return Name;
        }

        public void Check(ICheckNotifier notifier)
        {
            switch (ProcessTaskType)
            {
                case ProcessTaskType.Executable:
                    CheckFileExistenceAndUniqueness(notifier);
                    break;
                case ProcessTaskType.SQLFile:
                    CheckFileExistenceAndUniqueness(notifier);
                    CheckForProblemsInSQLFile(notifier);

                    break;
                case ProcessTaskType.StoredProcedure:
                    break;
                case ProcessTaskType.Attacher:
                    break;
                case ProcessTaskType.DataProvider:
                    break;
                case ProcessTaskType.MutilateDataTable:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void CheckForProblemsInSQLFile(ICheckNotifier notifier)
        {
            try
            {
                var sql = File.ReadAllText(Path);

                //lets check for any SQL that indicates user is trying to modify a STAGING table in a RAW script (for example)
                foreach (TableInfo tableInfo in LoadMetadata.GetDistinctTableInfoList(false))
                    //for each stage get all the object names that are in that stage
                    foreach (var stage in new[]{LoadStage.AdjustRaw, LoadStage.AdjustStaging, LoadStage.PostLoad})
                    {
                        //process task belongs in that stage anyway so nothing is prohibited
                        if (stage == (LoadStage == LoadStage.Mounting? LoadStage.AdjustRaw:LoadStage))
                            continue;

                        //figure out what is prohibited
                        string prohibitedSql = SqlSyntaxHelper.EnsureFullyQualifiedMicrosoftSQL(tableInfo.GetDatabaseRuntimeName(stage),tableInfo.GetRuntimeName(stage));

                        //if we reference it, complain
                        if (sql.Contains(prohibitedSql))
                            notifier.OnCheckPerformed(
                                new CheckEventArgs(
                                    "Sql in file '" + Path + "' contains a reference to '" + prohibitedSql +
                                    "' which is prohibited since the ProcessTask ('" + Name + "') runs in LoadStage " +
                                    LoadStage, CheckResult.Warning));
                    }
            }
            catch (Exception e)
            {
                notifier.OnCheckPerformed(new CheckEventArgs("Failed to check the contents of the SQL file '" + Path + "'", CheckResult.Fail,e));
            }
        }

        private void CheckFileExistenceAndUniqueness(ICheckNotifier notifier)
        {
            if (string.IsNullOrWhiteSpace(Path))
            {
                notifier.OnCheckPerformed(new CheckEventArgs("No Path specified for ProcessTask '" + Name + "'",CheckResult.Fail));
                return;
            }

            if (!File.Exists(Path))
                notifier.OnCheckPerformed(new CheckEventArgs("Could not find File '" + Path + "' for ProcessTask '" + Name + "'", CheckResult.Fail));
            else
                notifier.OnCheckPerformed(new CheckEventArgs("Found File '" + Path + "'", CheckResult.Success));


            var matchingPaths = Repository.GetAllObjects<ProcessTask>().Where(pt => pt.Path.Equals(Path));
            foreach (var duplicate in matchingPaths.Except(new[] {this}))
                notifier.OnCheckPerformed(
                    new CheckEventArgs(
                        "ProcessTask '" + duplicate + "' (ID=" + duplicate.ID + ") also uses file '" +
                        System.IO.Path.GetFileName(Path) + "'", CheckResult.Warning));

            //conflicting tokens in Name string
            foreach (Match match in Regex.Matches(Name, @"'.*((\.exe')|(\.sql'))"))
            {
                if (match.Success)
                {

                    var referencedFile = System.IO.Path.GetFileName(match.Value.Trim('\''));
                    var actualFile = System.IO.Path.GetFileName(Path);

                    if (referencedFile != actualFile)
                        notifier.OnCheckPerformed(
                            new CheckEventArgs(
                                "Name of ProcessTask '" + Name + "' (ID=" + ID + ") references file '" + match.Value +
                                "' but the Path of the ProcessTask is '" + Path + "'", CheckResult.Fail));
                }
                
            }
        }

        public IEnumerable<TableInfo> GetTableInfos()
        {
            return LoadMetadata.GetDistinctTableInfoList(true);
        }

        public IEnumerable<ILoadProgress> GetLoadProgresses()
        {
            return LoadMetadata.LoadProgresses;
        }

        public IEnumerable<IArgument> GetAllArguments()
        {
            return ProcessTaskArguments;
        }

        public IArgument CreateNewArgument()
        {
            return new ProcessTaskArgument((ICatalogueRepository) Repository,this);
        }

        public string GetClassNameWhoArgumentsAreFor()
        {
            return Path;
        }

        /// <summary>
        /// Creates a new copy of the processTask and all it's arguments in the database, this clone is then hooked up to the
        /// new LoadMetadata at the specified stage
        /// </summary>
        /// <param name="loadMetadata">The new LoadMetadata parent for the clone</param>
        /// <param name="loadStage">The new load stage to put the clone in </param>
        /// <returns>the new ProcessTask (the clone has a different ID to the parent)</returns>
        public ProcessTask CloneToNewLoadMetadataStage(LoadMetadata loadMetadata, LoadStage loadStage)
        {
            var cataRepository = ((CatalogueRepository) Repository);

            //clone only accepts sql connections so make sure we aren't in mysql land or something
            using (cataRepository.BeginNewTransactedConnection())
            {
                try
                {
                    //get list of arguments to also clone (will happen outside of transaction
                    ProcessTaskArgument[] toCloneArguments = ProcessTaskArguments.ToArray();

                    //create a new transaction for all the cloning - note that once all objects are cloned the transaction is committed then all the objects are adjusted outside the transaction
                    ProcessTask clone = Repository.CloneObjectInTable(this);

                    //foreach of our child arguments
                    foreach (ProcessTaskArgument argument in toCloneArguments)
                    {
                        //clone it but rewire it to the proper ProcessTask parent (the clone)
                        var arg = Repository.CloneObjectInTable(argument);
                        arg.ProcessTask_ID = clone.ID;
                        arg.SaveToDatabase();
                    }
            
                    //the values passed into parameter
                    clone.LoadMetadata_ID = loadMetadata.ID;
                    clone.LoadStage = loadStage;
                    clone.SaveToDatabase();
                    
                    //it worked
                    cataRepository.EndTransactedConnection(true);

                    //return the clone
                    return clone;
                }
                catch(Exception)
                {
                    cataRepository.EndTransactedConnection(false);
                    throw;
                }
            }
        }

        public IArgument[] CreateArgumentsForClassIfNotExists<T>()
        {
            var argFactory = new ArgumentFactory();
            return argFactory.CreateArgumentsForClassIfNotExistsGeneric<T>(

                //tell it how to create new instances of us related to parent
                this,

                //what arguments already exist
                GetAllArguments().ToArray())

                //convert the result back from generic to specific (us)
                .ToArray();
        }

        public static bool IsCompatibleStage(ProcessTaskType type, LoadStage stage)
        {
            switch (type)
            {
                case ProcessTaskType.Executable:
                    return true;
                case ProcessTaskType.SQLFile:
                    return stage != LoadStage.GetFiles;
                case ProcessTaskType.StoredProcedure:
                    return stage != LoadStage.GetFiles;
                case ProcessTaskType.Attacher:
                    return stage == LoadStage.Mounting;
                case ProcessTaskType.DataProvider:
                    return true;
                case ProcessTaskType.MutilateDataTable:
                    return stage != LoadStage.GetFiles;
                default:
                    throw new ArgumentOutOfRangeException("type");
            }
        }

        public bool IsPluginType()
        {
            return ProcessTaskType == ProcessTaskType.Attacher || ProcessTaskType == ProcessTaskType.MutilateDataTable || ProcessTaskType == ProcessTaskType.DataProvider;
        }

        public void SetArgumentValue(string parameterName, object o)
        {
            var matchingArgument = ProcessTaskArguments.SingleOrDefault(p => p.Name.Equals(parameterName));
            if (matchingArgument == null)
                throw new Exception("Could not find a ProcessTaskArgument called '" + parameterName + "', have you called CreateArgumentsForClassIfNotExists<T> yet?");

            matchingArgument.SetValue(o);
            matchingArgument.SaveToDatabase();
        }
    }

    public enum ProcessTaskType
    {
        Executable,
        SQLFile,
        StoredProcedure,
        Attacher,
        DataProvider,
        MutilateDataTable
    }

    public enum LoadStage
    {
        [Description("Processes in this category should result in the generation or modification of files (e.g." +
                     " FTP file download, unzip local file etc).  The data load engine will not provide processes " +
                     "in this stage with any information about the database being loaded (but it will provide " +
                     "the root project directory so that processes know where to generate files into)")]
        GetFiles,

        [Description("Processes in this category should be concerned with moving data from the project directory" +
                     " into the RAW database.  The data load engine will provide both the root directory and the " +
                     "location of the RAW database.")]
        Mounting,

        [Description("Processes in this category should be concerned with modifying the content/structure of the data" +
                     " in the RAW database.  This data will not be annonymised at this point.  After running all the" +
                     " processes in this category, the structure of the database must match the _STAGING database.  " +
                     "Assuming the RAW database structure matches _STAGING, the data load engine will then move the data " +
                     "(performing appropriate anonymisation steps on a column by column basis as defined in the Catalogue" +
                     " ColumnInfos)")]
        AdjustRaw,

        [Description("Processes in this category should be concerned with modifying the content (not structure) of the data" +
                " in the _STAGING database.  This data will be annonymous.  After all processes have been executed and assuming" +
                " the _STAGING database structure still matches the LIVE structure, the data load engine will use the primary " +
                "key informtion defined in the Catalogue ColumnInfos to merge the new data into the current LIVE database")]
        AdjustStaging,


        [Description("Processes in this category are executed after the new data has been merged into the LIVE database.  This" +
                     "is your opportunity to update dependent data, run longitudinal/dataset wide cleaning algorithms etc.")]
        PostLoad
    }

    public static class LoadStageToNamingConventionMapper
    {
        public static LoadBubble LoadStageToLoadBubble(LoadStage loadStage)
        {
            switch (loadStage)
            {
                case LoadStage.GetFiles:
                    return LoadBubble.Raw;
                case LoadStage.Mounting:
                    return LoadBubble.Raw;
                case LoadStage.AdjustRaw:
                    return LoadBubble.Raw;
                case LoadStage.AdjustStaging:
                    return LoadBubble.Staging;
                case LoadStage.PostLoad:
                    return LoadBubble.Live;
                default:
                    throw new ArgumentOutOfRangeException("Unknown value for LoadStage: " + loadStage);
            }
        }
        public static LoadStage LoadBubbleToLoadStage(LoadBubble bubble)
        {
            switch (bubble)
            {
                case LoadBubble.Raw:
                    return LoadStage.AdjustRaw;
                case LoadBubble.Staging:
                    return LoadStage.AdjustStaging;
                case LoadBubble.Live:
                    return LoadStage.PostLoad;
                case LoadBubble.Archive:
                    throw new Exception("LoadBubble.Archive refers to _Archive tables, therefore it cannot be translated into a LoadStage");
                default:
                    throw new ArgumentOutOfRangeException("bubble");
            }
        }

    }
}

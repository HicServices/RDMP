// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.Data.Common;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Rdmp.Core.Curation.Data.Cohort;
using Rdmp.Core.Curation.Data.ImportExport;
using Rdmp.Core.Curation.Data.Serialization;
using Rdmp.Core.MapsDirectlyToDatabaseTable;
using Rdmp.Core.MapsDirectlyToDatabaseTable.Attributes;
using Rdmp.Core.Repositories;
using Rdmp.Core.ReusableLibraryCode.Annotations;
using Rdmp.Core.ReusableLibraryCode.Checks;

namespace Rdmp.Core.Curation.Data.DataLoad;

/// <summary>
/// Describes a specific operation carried out at a specific step of a LoadMetadata.  This could be 'unzip all files called *.zip in for loading' or
/// 'after loading the data to live, call sp_clean_table1' or 'Connect to webservice X and download 1,000,000 records which will be serialized into XML'
/// 
/// <para>The class achieves this wide ranging functionality through the interaction of ProcessTaskType and Path.  e.g. when ProcessTaskType is Attacher then
/// Path functions as the Type name of a class that implements IAttacher e.g. 'LoadModules.Generic.Attachers.AnySeparatorFileAttacher'.  </para>
/// 
/// <para>Each ProcessTask can have one or more strongly typed arguments (see entity ProcessTaskArgument), these are discovered at design time by using
/// reflection to query the Path e.g. 'AnySeparatorFileAttacher' for all properties marked with [DemandsInitialization] attribute.  This allows for 3rd party developers
/// to write plugin classes to easily handle proprietary/bespoke source file types or complex data load requirements.</para>
/// </summary>
public class ProcessTask : DatabaseEntity, IProcessTask, IOrderable, INamed, ICheckable
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
#nullable enable
    private string? _SerialisableConfiguration;
#nullable disable

    /// <summary>
    /// The load the process task exists as part of
    /// </summary>
    [Relationship(typeof(LoadMetadata), RelationshipType.SharedObject)]
    public int LoadMetadata_ID
    {
        get => _loadMetadataID;
        set => SetField(ref _loadMetadataID, value);
    }

    /// <inheritdoc/>
    [Obsolete(
        "Since you can't change which Catalogues are loaded by a LoadMetadata at runtime, this property is now obsolete")]
    public int? RelatesSolelyToCatalogue_ID
    {
        get => _relatesSolelyToCatalogueID;
        set => SetField(ref _relatesSolelyToCatalogueID, value);
    }

    /// <inheritdoc/>
    public int Order
    {
        get => _order;
        set => SetField(ref _order, value);
    }

    /// <inheritdoc/>
    [AdjustableLocation]
    public string Path
    {
        get => _path;
        set => SetField(ref _path, value);
    }

    /// <inheritdoc cref="IProcessTask.Name"/>
    [NotNull]
    public string Name
    {
        get => _name;
        set => SetField(ref _name, value);
    }

    /// <inheritdoc/>
    public LoadStage LoadStage
    {
        get => _loadStage;
        set => SetField(ref _loadStage, value);
    }

    /// <inheritdoc/>
    public ProcessTaskType ProcessTaskType
    {
        get => _processTaskType;
        set => SetField(ref _processTaskType, value);
    }

    /// <inheritdoc/>
    public bool IsDisabled
    {
        get => _isDisabled;
        set => SetField(ref _isDisabled, value);
    }


    /// <inheritdoc/>
#nullable enable
    public string? SerialisableConfiguration
    {
        get => _SerialisableConfiguration;
        set => SetField(ref _SerialisableConfiguration, value);
    }


#nullable disable

    #endregion

    #region Relationships

    /// <inheritdoc cref="LoadMetadata_ID"/>
    [NoMappingToDatabase]
    public LoadMetadata LoadMetadata => Repository.GetObjectByID<LoadMetadata>(LoadMetadata_ID);

    /// <inheritdoc/>
    [NoMappingToDatabase]
    public IEnumerable<ProcessTaskArgument> ProcessTaskArguments =>
        Repository.GetAllObjectsWithParent<ProcessTaskArgument>(this);

    /// <summary>
    /// All <see cref="ILoadProgress"/> (if any) that can be advanced by executing this load.  This allows batch execution of large loads
    /// </summary>
    [NoMappingToDatabase]
    public ILoadProgress[] LoadProgresses => LoadMetadata.LoadProgresses;

    #endregion

    public ProcessTask()
    {
    }

    /// <summary>
    /// Creates a new operation in the data load (e.g. copy files from A to B, load all CSV files to RAW table B etc)
    /// </summary>
    /// <param name="repository"></param>
    /// <param name="parent"></param>
    /// <param name="stage"></param>
    public ProcessTask(ICatalogueRepository repository, ILoadMetadata parent, LoadStage stage)
    {
        var order =
            repository.GetAllObjectsWithParent<ProcessTask>(parent).Select(t => t.Order).DefaultIfEmpty().Max() + 1;

        repository.InsertAndHydrate(this, new Dictionary<string, object>
        {
            { "LoadMetadata_ID", parent.RootLoadMetadata_ID??parent.ID },
            { "ProcessTaskType", ProcessTaskType.Executable.ToString() },
            { "LoadStage", stage },
            { "Name", $"New Process{Guid.NewGuid()}" },
            { "Order", order },
        });
    }

    /// <summary>
    /// Creates a new operation in the data load (e.g. copy files from A to B, load all CSV files to RAW table B etc)
    /// </summary>
    /// <param name="repository"></param>
    /// <param name="parent"></param>
    /// <param name="stage"></param>
    /// <param name="serialisableConfiguration"></param>
    public ProcessTask(ICatalogueRepository repository, ILoadMetadata parent, LoadStage stage, string serialisableConfiguration = null)
    {
        var order =
            repository.GetAllObjectsWithParent<ProcessTask>(parent).Select(t => t.Order).DefaultIfEmpty().Max() + 1;

        repository.InsertAndHydrate(this, new Dictionary<string, object>
        {
             { "LoadMetadata_ID", parent.RootLoadMetadata_ID??parent.ID },
            { "ProcessTaskType", ProcessTaskType.Executable.ToString() },
            { "LoadStage", stage },
            { "Name", $"New Process{Guid.NewGuid()}" },
            { "Order", order },
            {"SerialisableConfiguration", serialisableConfiguration}
        });
    }

    internal ProcessTask(ICatalogueRepository repository, DbDataReader r)
        : base(repository, r)
    {
        LoadMetadata_ID = int.Parse(r["LoadMetaData_ID"].ToString());

        if (r["RelatesSolelyToCatalogue_ID"] != DBNull.Value)
            _relatesSolelyToCatalogueID = int.Parse(r["RelatesSolelyToCatalogue_ID"].ToString());

        Path = r["Path"] as string;
        Name = r["Name"] as string;
        Order = int.Parse(r["Order"].ToString());
        if (Enum.TryParse(r["ProcessTaskType"] as string, out ProcessTaskType processTaskType))
            ProcessTaskType = processTaskType;
        else
            throw new Exception($"Could not parse ProcessTaskType:{r["ProcessTaskType"]}");

        if (Enum.TryParse(r["LoadStage"] as string, out LoadStage loadStage))
            LoadStage = loadStage;
        else
            throw new Exception($"Could not parse LoadStage:{r["LoadStage"]}");

        IsDisabled = Convert.ToBoolean(r["IsDisabled"]);
        if (r["SerialisableConfiguration"] is not null)
            SerialisableConfiguration = r["SerialisableConfiguration"].ToString();
    }

    internal ProcessTask(ShareManager shareManager, ShareDefinition shareDefinition)
    {
        shareManager.UpsertAndHydrate(this, shareDefinition);
    }

    /// <inheritdoc/>
    public override string ToString() => Name;

    /// <inheritdoc/>
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
            case ProcessTaskType.SQLBakFile:
                CheckFileExistenceAndUniqueness(notifier);
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

            //let's check for any SQL that indicates user is trying to modify a STAGING table in a RAW script (for example)
            foreach (var tableInfo in LoadMetadata.GetDistinctTableInfoList(false))
                //for each stage get all the object names that are in that stage
                foreach (var stage in new[] { LoadStage.AdjustRaw, LoadStage.AdjustStaging, LoadStage.PostLoad })
                {
                    //process task belongs in that stage anyway so nothing is prohibited
                    if (stage == (LoadStage == LoadStage.Mounting ? LoadStage.AdjustRaw : LoadStage))
                        continue;

                    //figure out what is prohibited
                    var prohibitedSql = tableInfo.GetQuerySyntaxHelper()
                        .EnsureFullyQualified(tableInfo.GetDatabaseRuntimeName(stage), null,
                            tableInfo.GetRuntimeName(stage));

                    //if we reference it, complain
                    if (sql.Contains(prohibitedSql))
                        notifier.OnCheckPerformed(
                            new CheckEventArgs(
                                $"Sql in file '{Path}' contains a reference to '{prohibitedSql}' which is prohibited since the ProcessTask ('{Name}') runs in LoadStage {LoadStage}",
                                CheckResult.Warning));
                }
        }
        catch (Exception e)
        {
            notifier.OnCheckPerformed(new CheckEventArgs($"Failed to check the contents of the SQL file '{Path}'",
                CheckResult.Fail, e));
        }
    }

    private void CheckFileExistenceAndUniqueness(ICheckNotifier notifier)
    {
        if (string.IsNullOrWhiteSpace(Path))
        {
            notifier.OnCheckPerformed(new CheckEventArgs($"No Path specified for ProcessTask '{Name}'",
                CheckResult.Fail));
            return;
        }

        notifier.OnCheckPerformed(!File.Exists(Path)
            ? new CheckEventArgs($"Could not find File '{Path}' for ProcessTask '{Name}'", CheckResult.Fail)
            : new CheckEventArgs($"Found File '{Path}'", CheckResult.Success));


        var matchingPaths = Repository.GetAllObjects<ProcessTask>().Where(pt => pt.Path.Equals(Path));
        foreach (var duplicate in matchingPaths.Except(new[] { this }))
            notifier.OnCheckPerformed(
                new CheckEventArgs(
                    $"ProcessTask '{duplicate}' (ID={duplicate.ID}) also uses file '{System.IO.Path.GetFileName(Path)}'",
                    CheckResult.Warning));

        //conflicting tokens in Name string
        foreach (Match match in Regex.Matches(Name, @"'.*((\.exe')|(\.sql'))"))
            if (match.Success)
            {
                var referencedFile = System.IO.Path.GetFileName(match.Value.Trim('\''));
                var actualFile = System.IO.Path.GetFileName(Path);

                if (referencedFile != actualFile)
                    notifier.OnCheckPerformed(
                        new CheckEventArgs(
                            $"Name of ProcessTask '{Name}' (ID={ID}) references file '{match.Value}' but the Path of the ProcessTask is '{Path}'",
                            CheckResult.Fail));
            }
    }

    /// <summary>
    /// Returns all tables loaded by the parent <see cref="LoadMetadata"/>
    /// </summary>
    /// <returns></returns>
    public IEnumerable<TableInfo> GetTableInfos() => LoadMetadata.GetDistinctTableInfoList(true);

    /// <inheritdoc/>
    public IEnumerable<IArgument> GetAllArguments() => ProcessTaskArguments;

    /// <inheritdoc/>
    public IArgument CreateNewArgument() => new ProcessTaskArgument((ICatalogueRepository)Repository, this);

    /// <inheritdoc/>
    public string GetClassNameWhoArgumentsAreFor() => Path;

    /// <summary>
    /// Creates a new copy of the processTask and all its arguments in the database, this clone is then hooked up to the
    /// new LoadMetadata at the specified stage
    /// </summary>
    /// <param name="loadMetadata">The new LoadMetadata parent for the clone</param>
    /// <param name="loadStage">The new load stage to put the clone in </param>
    /// <returns>the new ProcessTask (the clone has a different ID to the parent)</returns>
    public ProcessTask CloneToNewLoadMetadataStage(LoadMetadata loadMetadata, LoadStage loadStage)
    {
        var cataRepository = (ICatalogueRepository)Repository;

        //clone only accepts sql connections so make sure we aren't in mysql land or something
        using (cataRepository.BeginNewTransaction())
        {
            try
            {
                //get list of arguments to also clone (will happen outside of transaction
                var toCloneArguments = ProcessTaskArguments.ToArray();

                //create a new transaction for all the cloning - note that once all objects are cloned the transaction is committed then all the objects are adjusted outside the transaction
                var clone = new ProcessTask(CatalogueRepository, LoadMetadata, loadStage);
                CopyShallowValuesTo(clone);

                //foreach of our child arguments
                foreach (var argument in toCloneArguments)
                    //clone it but rewire it to the proper ProcessTask parent (the clone)
                    argument.ShallowClone(clone);

                //the values passed into parameter
                clone.LoadMetadata_ID = loadMetadata.ID;
                clone.LoadStage = loadStage;
                clone.SaveToDatabase();

                //it worked
                cataRepository.EndTransaction(true);

                //return the clone
                return clone;
            }
            catch (Exception)
            {
                cataRepository.EndTransaction(false);
                throw;
            }
        }
    }

    /// <inheritdoc/>
    public IArgument[] CreateArgumentsForClassIfNotExists(Type t) =>
        ArgumentFactory.CreateArgumentsForClassIfNotExistsGeneric(
                t,

                //tell it how to create new instances of us related to parent
                this,

                //what arguments already exist
                GetAllArguments().ToArray())

            //convert the result back from generic to specific (us)
            .ToArray();

    /// <inheritdoc/>
    public IArgument[] CreateArgumentsForClassIfNotExists<T>() => CreateArgumentsForClassIfNotExists(typeof(T));

    /// <summary>
    /// Returns true if the <see cref="ProcessTaskType"/> is allowed to happen during the given <see cref="LoadStage"/>  (e.g. you can't use an IAttacher to
    /// load data into STAGING/LIVE - only RAW).
    /// </summary>
    /// <param name="type"></param>
    /// <param name="stage"></param>
    /// <returns></returns>
    public static bool IsCompatibleStage(ProcessTaskType type, LoadStage stage)
    {
        return type switch
        {
            ProcessTaskType.Executable => true,
            ProcessTaskType.SQLFile => stage != LoadStage.GetFiles,
            ProcessTaskType.SQLBakFile => stage != LoadStage.GetFiles,
            ProcessTaskType.Attacher => stage == LoadStage.Mounting,
            ProcessTaskType.DataProvider => true,
            ProcessTaskType.MutilateDataTable => stage != LoadStage.GetFiles,
            _ => throw new ArgumentOutOfRangeException(nameof(type))
        };
    }

    /// <summary>
    /// True if <see cref="Path"/> is the name of a C# class (as opposed to the path to an executable or SQL file etc)
    /// </summary>
    /// <returns></returns>
    public bool IsPluginType() => ProcessTaskType == ProcessTaskType.Attacher ||
                                  ProcessTaskType == ProcessTaskType.MutilateDataTable ||
                                  ProcessTaskType == ProcessTaskType.DataProvider;

    /// <summary>
    /// Sets the value of the corresponding <see cref="IArgument"/> (which must already exist) to the given value.  If your argument doesn't exist yet you
    /// can call <see cref="CreateArgumentsForClassIfNotExists"/>
    /// </summary>
    /// <param name="parameterName"></param>
    /// <param name="o"></param>
    public void SetArgumentValue(string parameterName, object o)
    {
        var matchingArgument = ProcessTaskArguments.SingleOrDefault(p => p.Name.Equals(parameterName)) ??
                               throw new Exception(
                                   $"Could not find a ProcessTaskArgument called '{parameterName}', have you called CreateArgumentsForClassIfNotExists<T> yet?");
        matchingArgument.SetValue(o);
        matchingArgument.SaveToDatabase();
    }


    public ProcessTask Clone(LoadMetadata lmd)
    {
        var pt = new ProcessTask(CatalogueRepository, lmd, LoadStage) {
            ProcessTaskType = ProcessTaskType,
            Order = Order,
            IsDisabled = IsDisabled,
            SerialisableConfiguration = SerialisableConfiguration,
            Path = Path,
            Name= Name,
        };
        pt.LoadMetadata_ID = lmd.ID;
        pt.SaveToDatabase();
        foreach(var pta in ProcessTaskArguments)
        {
            pta.ShallowClone(pt);
        }
        return pt;
    }
}
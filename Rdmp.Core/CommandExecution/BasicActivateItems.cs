// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FAnsi.Discovery;
using Rdmp.Core.CohortCommitting.Pipeline;
using Rdmp.Core.CommandExecution.AtomicCommands;
using Rdmp.Core.CommandLine.Runners;
using Rdmp.Core.Curation;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Curation.Data.Aggregation;
using Rdmp.Core.Curation.Data.Cohort;
using Rdmp.Core.Curation.Data.DataLoad;
using Rdmp.Core.Curation.Data.Defaults;
using Rdmp.Core.Curation.Data.ImportExport;
using Rdmp.Core.Curation.Data.Pipelines;
using Rdmp.Core.DataExport.Data;
using Rdmp.Core.DataViewing;
using Rdmp.Core.Icons.IconProvision;
using Rdmp.Core.Logging;
using Rdmp.Core.Logging.PastEvents;
using Rdmp.Core.MapsDirectlyToDatabaseTable;
using Rdmp.Core.MapsDirectlyToDatabaseTable.Versioning;
using Rdmp.Core.Providers;
using Rdmp.Core.Repositories;
using Rdmp.Core.Repositories.Construction;
using Rdmp.Core.ReusableLibraryCode.Checks;
using Rdmp.Core.ReusableLibraryCode.Comments;
using Rdmp.Core.ReusableLibraryCode.Settings;

namespace Rdmp.Core.CommandExecution;

public abstract class BasicActivateItems : IBasicActivateItems
{
    /// <summary>
    /// The maximum number of characters (exclusive) at which text input UI
    /// controls should assume single line only (i.e. no newlines allowed).
    /// </summary>
    public const int MultiLineLengthThreshold = 1000;


    /// <inheritdoc/>
    public bool IsWinForms { get; protected set; }

    /// <inheritdoc/>
    public virtual bool IsInteractive => true;

    /// <inheritdoc/>
    public bool InteractiveDeletes { get; set; }

    /// <inheritdoc/>
    public ICoreChildProvider CoreChildProvider { get; protected set; }

    /// <inheritdoc/>
    public IServerDefaults ServerDefaults { get; }

    /// <inheritdoc/>
    public FavouritesProvider FavouritesProvider { get; private set; }

    public ICoreIconProvider CoreIconProvider { get; private set; }

    public CommentStore CommentStore => RepositoryLocator.CatalogueRepository.CommentStore;


    /// <inheritdoc/>
    public bool YesNo(string text, string caption, out bool chosen) =>
        YesNo(new DialogArgs
        {
            WindowTitle = caption,
            TaskDescription = text
        }, out chosen);

    public abstract bool YesNo(DialogArgs args, out bool chosen);

    /// <inheritdoc/>
    public bool YesNo(string text, string caption) =>
        YesNo(new DialogArgs
        {
            WindowTitle = caption,
            TaskDescription = text
        });

    public bool YesNo(DialogArgs args) => YesNo(args, out var chosen) && chosen;

    public bool Confirm(string text, string caption) =>
        Confirm(new DialogArgs
        {
            WindowTitle = caption,
            TaskDescription = text
        });

    /// <inheritdoc/>
    public bool Confirm(DialogArgs args) =>
        // auto confirm if not in user interactive mode
        !IsInteractive || YesNo(args);

    /// <inheritdoc/>
    public ICheckNotifier GlobalErrorCheckNotifier { get; set; }

    /// <inheritdoc/>
    public IRDMPPlatformRepositoryServiceLocator RepositoryLocator { get; }

    /// <inheritdoc/>
    public event EmphasiseItemHandler Emphasise;

    /// <inheritdoc/>
    public List<IPluginUserInterface> PluginUserInterfaces { get; private set; } = new();

    /// <inheritdoc/>
    public bool HardRefresh { get; set; }

    public bool IsAbleToLaunchSubprocesses { get; protected set; }

    public BasicActivateItems(IRDMPPlatformRepositoryServiceLocator repositoryLocator,
        ICheckNotifier globalErrorCheckNotifier)
    {
        RepositoryLocator = repositoryLocator;
        GlobalErrorCheckNotifier = globalErrorCheckNotifier;

        ServerDefaults = RepositoryLocator.CatalogueRepository;

        //Shouldn't ever change externally to your session so doesn't need constantly refreshed
        FavouritesProvider = new FavouritesProvider(this);

        // This has to happen before we create the child providers
        ConstructPluginChildProviders();

        // Note that this is virtual so can return null e.g. if other stuff has to happen with the activator before a valid child provider can be built (e.g. loading plugin user interfaces)
        CoreChildProvider = GetChildProvider();

        //handle custom icons from plugin user interfaces in which
        CoreIconProvider = new DataExportIconProvider(repositoryLocator, PluginUserInterfaces.ToArray());
    }

    protected virtual ICoreChildProvider GetChildProvider()
    {
        // Build new CoreChildProvider in a temp then update to it to avoid stale references
        ICoreChildProvider temp = null;

        //prefer a linked repository with both
        if (RepositoryLocator.DataExportRepository != null)
            try
            {
                temp = new DataExportChildProvider(RepositoryLocator, PluginUserInterfaces.ToArray(),
                    GlobalErrorCheckNotifier, CoreChildProvider as DataExportChildProvider);
            }
            catch (Exception e)
            {
                ShowException("Error constructing DataExportChildProvider", e);
            }

        //there was an error generating a data export repository or there was no repository specified

        //so just create a catalogue one
        temp ??= new CatalogueChildProvider(RepositoryLocator.CatalogueRepository, PluginUserInterfaces.ToArray(),
            GlobalErrorCheckNotifier, CoreChildProvider as CatalogueChildProvider);

        // first time
        if (CoreChildProvider == null)
            CoreChildProvider = temp;
        else
            CoreChildProvider.UpdateTo(temp);

        return CoreChildProvider;
    }

    private void ConstructPluginChildProviders()
    {
        PluginUserInterfaces = new List<IPluginUserInterface>();

        foreach (var pluginType in MEF.GetTypes<IPluginUserInterface>())
            try
            {
                PluginUserInterfaces.Add((IPluginUserInterface)ObjectConstructor.Construct(pluginType, this, false));
            }
            catch (Exception e)
            {
                GlobalErrorCheckNotifier.OnCheckPerformed(new CheckEventArgs(
                    $"Problem occurred trying to load Plugin '{pluginType.Name}'", CheckResult.Fail, e));
            }
    }

    protected void OnEmphasise(object sender, EmphasiseEventArgs args)
    {
        Emphasise?.Invoke(sender, args);
    }

    /// <inheritdoc/>
    public virtual IEnumerable<Type> GetIgnoredCommands() => Type.EmptyTypes;

    /// <inheritdoc/>
    public virtual void Wait(string title, Task task, CancellationTokenSource cts)
    {
        task.Wait(cts.Token);
    }

    /// <inheritdoc/>
    public virtual void RequestItemEmphasis(object sender, EmphasiseRequest emphasiseRequest)
    {
        AdjustEmphasiseRequest(emphasiseRequest);

        OnEmphasise(sender, new EmphasiseEventArgs(emphasiseRequest));
    }

    /// <summary>
    /// Changes what object should be emphasised based on system state for specific types
    /// of objects.  For example if a request is to emphasise an <see cref="ExtractableCohort"/>
    /// then we might instead emphasise it under a <see cref="Project"/> (if it is only
    /// associated with a single Project).
    /// </summary>
    /// <param name="emphasiseRequest"></param>
    protected void AdjustEmphasiseRequest(EmphasiseRequest emphasiseRequest)
    {
        if (emphasiseRequest.ObjectToEmphasise is ExtractableCohort ec)
            if (CoreChildProvider is DataExportChildProvider dx)
            {
                var projects = dx.Projects.Where(p => p.ProjectNumber == ec.ExternalProjectNumber).ToArray();

                // If there is only one Project with the number of this cohort
                if (projects.Length == 1)
                {
                    // we can emphasise the cohort under that Project
                    var usage = dx.GetAllCohortProjectUsageNodesFor(projects[0])
                        .SelectMany(s => s.CohortsUsed)
                        .FirstOrDefault(k => k.ObjectBeingUsed.Equals(ec));

                    if (usage != null)
                        emphasiseRequest.ObjectToEmphasise = usage;
                }
            }
    }

    /// <summary>
    /// Returns the root tree object which hosts the supplied object.  If the supplied object has no known descendancy it is assumed
    /// to be the root object itself so it is returned
    /// </summary>
    /// <param name="objectToEmphasise"></param>
    /// <returns></returns>
    public object GetRootObjectOrSelf(object objectToEmphasise) =>
        CoreChildProvider?.GetRootObjectOrSelf(objectToEmphasise) ?? objectToEmphasise;

    /// <inheritdoc/>
    public bool SelectEnum(string prompt, Type enumType, out Enum chosen) =>
        SelectEnum(new DialogArgs
        {
            WindowTitle = prompt
        }, enumType, out chosen);

    public abstract bool SelectEnum(DialogArgs args, Type enumType, out Enum chosen);


    public bool SelectType(string prompt, Type baseTypeIfAny, out Type chosen) =>
        SelectType(new DialogArgs
        {
            WindowTitle = prompt
        }, baseTypeIfAny, out chosen);

    public bool SelectType(DialogArgs args, Type baseTypeIfAny, out Type chosen) =>
        SelectType(args, baseTypeIfAny, false, false, out chosen);

    public bool SelectType(string prompt, Type baseTypeIfAny, bool allowAbstract, bool allowInterfaces,
        out Type chosen) =>
        SelectType(new DialogArgs
        {
            WindowTitle = prompt
        }, baseTypeIfAny, allowAbstract, allowInterfaces, out chosen);

    public bool SelectType(DialogArgs args, Type baseTypeIfAny, bool allowAbstract, bool allowInterfaces,
        out Type chosen)
    {
        var available =
            MEF.GetAllTypes()
                .Where(t =>
                    (baseTypeIfAny == null || baseTypeIfAny.IsAssignableFrom(t)) &&
                    (allowAbstract || !t.IsAbstract) &&
                    (allowInterfaces || !t.IsInterface))
                .ToArray();

        return SelectType(args, available, out chosen);
    }

    public bool SelectType(string prompt, Type[] available, out Type chosen) =>
        SelectType(new DialogArgs
        {
            WindowTitle = prompt
        }, available, out chosen);

    public abstract bool SelectType(DialogArgs args, Type[] available, out Type chosen);

    public virtual bool CanActivate(object target) => false;

    public void Activate(object o)
    {
        if (o is IMapsDirectlyToDatabaseTable m)
            // if a plugin user interface exists to handle editing this then let them handle it instead of launching the
            // normal UI
            foreach (var pluginInterface in PluginUserInterfaces)
                if (pluginInterface.CustomActivate(m))
                    return;

        ActivateImpl(o);
    }

    /// <summary>
    /// override to provide custom activation logic.  Note that this will be called after
    /// first consulting plugins about new behaviours
    /// </summary>
    /// <param name="o"></param>
    protected virtual void ActivateImpl(object o)
    {
    }

    /// <inheritdoc/>
    public virtual IEnumerable<T> GetAll<T>() =>
        CoreChildProvider.GetAllSearchables()
            .Keys.OfType<T>();

    /// <inheritdoc/>
    public virtual IEnumerable<IMapsDirectlyToDatabaseTable> GetAll(Type t) =>
        CoreChildProvider.GetAllSearchables()
            .Keys.Where(t.IsInstanceOfType);

    /// <inheritdoc/>
    public abstract void ShowException(string errorText, Exception exception);

    /// <inheritdoc/>
    public void PublishNearest(object publish)
    {
        if (publish != null)
        {
            if (publish is DatabaseEntity d)
            {
                Publish(d);
            }
            else
            {
                var descendancy = CoreChildProvider.GetDescendancyListIfAnyFor(publish);

                var parent = descendancy?.Parents.OfType<DatabaseEntity>().LastOrDefault();

                if (parent != null)
                    Publish(parent);
            }
        }
    }

    /// <inheritdoc/>
    public virtual bool DeleteWithConfirmation(IDeleteable deletable)
    {
        if (IsInteractive && InteractiveDeletes)
        {
            var didDelete = InteractiveDelete(deletable);

            if (didDelete) PublishNearest(deletable);

            return didDelete;
        }

        deletable.DeleteInDatabase();
        PublishNearest(deletable);

        return true;
    }

    protected virtual bool InteractiveDelete(IDeleteable deletable)
    {
        var databaseObject = deletable as DatabaseEntity;

        switch (databaseObject)
        {
            case Catalogue c:
                {
                    if (c.GetExtractabilityStatus(RepositoryLocator.DataExportRepository).IsExtractable)
                    {
                        if (YesNo(
                                "Catalogue must first be made non extractable before it can be deleted, mark non extractable?",
                                "Make Non Extractable"))
                        {
                            var cmd = new ExecuteCommandChangeExtractability(this, c);
                            cmd.Execute();
                        }
                        else
                        {
                            return false;
                        }
                    }

                    break;
                }
            case ExtractionFilter f:
                {
                    var children = f.ExtractionFilterParameterSets;

                    if (children.Any())
                    {
                        if (!YesNo(
                                $"Filter has {children.Length} value sets defined.  Deleting filter will also delete these.  Confirm?",
                                "Delete"))
                            return false;

                        foreach (var child in children) child.DeleteInDatabase();

                        f.ClearAllInjections();

                        f.DeleteInDatabase();
                        return true;
                    }

                    break;
                }
            case AggregateConfiguration ac when ac.IsJoinablePatientIndexTable():
                {
                    var users = ac.JoinableCohortAggregateConfiguration?.Users?.Select(u => u.AggregateConfiguration);
                    if (users != null)
                    {
                        users = users.ToArray();
                        if (users.Any())
                        {
                            Show(
                                $"Cannot Delete '{ac.Name}' because it is linked to by the following AggregateConfigurations:{Environment.NewLine}{string.Join(Environment.NewLine, users)}");
                            return false;
                        }
                    }

                    break;
                }
        }

        //it has already been deleted before
        if (databaseObject != null && !databaseObject.Exists())
            return false;

        var idText = "";

        if (databaseObject != null)
            idText = $" ID={databaseObject.ID}";

        if (databaseObject != null)
        {
            var exports = RepositoryLocator.CatalogueRepository.GetReferencesTo<ObjectExport>(databaseObject).ToArray();
            if (exports.Any(e => e.Exists()))
                if (YesNo(
                        "This object has been shared as an ObjectExport.  Deleting it may prevent you loading any saved copies.  Do you want to delete the ObjectExport definition?",
                        "Delete ObjectExport"))
                    foreach (var e in exports)
                        e.DeleteInDatabase();
                else
                    return false;
        }

        var overrideConfirmationText =
            //If there is some special way of describing the effects of deleting this object e.g. Selected Datasets
            deletable is IDeletableWithCustomMessage customMessageDeletable
                ? $"Are you sure you want to {customMessageDeletable.GetDeleteMessage()}?"
                : $"Are you sure you want to delete '{deletable}'?{Environment.NewLine}({deletable.GetType().Name}{idText})";
        if (
            YesNo(
                overrideConfirmationText,
                $"Delete {deletable.GetType().Name}"))
        {
            deletable.DeleteInDatabase();

            if (databaseObject == null)
            {
                var descendancy = CoreChildProvider.GetDescendancyListIfAnyFor(deletable);
                if (descendancy != null)
                    databaseObject = descendancy.Parents.OfType<DatabaseEntity>().LastOrDefault();
            }

            if (deletable is IMasqueradeAs masqueradeAs)
                databaseObject ??= masqueradeAs.MasqueradingAs() as DatabaseEntity;

            return databaseObject == null
                ? throw new NotSupportedException(
                    $"IDeletable {deletable} was not a DatabaseObject and it did not have a Parent in its tree which was a DatabaseObject (DescendancyList)")
                : true;
        }

        return false;
    }

    public bool SelectValueType(string prompt, Type paramType, object initialValue, out object chosen) =>
        SelectValueType(
            new DialogArgs
            {
                WindowTitle = $"Enter value for {prompt}",
                EntryLabel = prompt
            }, paramType, initialValue, out chosen);

    /// <inheritdoc/>
    public bool SelectValueType(DialogArgs args, Type paramType, object initialValue, out object chosen)
    {
        var underlying = Nullable.GetUnderlyingType(paramType);
        if ((underlying ?? paramType).IsEnum)
        {
            var ok = SelectEnum(args, underlying ?? paramType, out var enumChosen);
            chosen = enumChosen;
            return ok;
        }

        if (paramType == typeof(bool) || paramType == typeof(bool?))
        {
            var ok = YesNo(args, out var boolChosen);
            chosen = boolChosen;
            return ok;
        }

        if (paramType == typeof(string))
        {
            var ok = TypeText(args, int.MaxValue, initialValue?.ToString(), out var stringChosen, false);
            chosen = stringChosen;
            return ok;
        }

        return SelectValueTypeImpl(args, paramType, initialValue, out chosen);
    }

    protected abstract bool
        SelectValueTypeImpl(DialogArgs args, Type paramType, object initialValue, out object chosen);

    /// <inheritdoc/>
    public virtual void Publish(IMapsDirectlyToDatabaseTable databaseEntity)
    {
        if (!HardRefresh && UserSettings.SelectiveRefresh && CoreChildProvider.SelectiveRefresh(databaseEntity)) return;

        var fresh = GetChildProvider();
        CoreChildProvider.UpdateTo(fresh);
        HardRefresh = false;
    }

    /// <inheritdoc/>
    public void Show(string message)
    {
        Show("Message", message);
    }

    /// <inheritdoc/>
    public abstract void ShowWarning(string message);

    public abstract void Show(string title, string message);

    /// <inheritdoc/>
    public bool TypeText(string header, string prompt, int maxLength, string initialText, out string text,
        bool requireSaneHeaderText) =>
        TypeText(new DialogArgs
        {
            WindowTitle = header,
            EntryLabel = prompt
        }, maxLength, initialText, out text, requireSaneHeaderText);

    public abstract bool TypeText(DialogArgs args, int maxLength, string initialText, out string text,
        bool requireSaneHeaderText);

    /// <inheritdoc/>
    public abstract DiscoveredDatabase SelectDatabase(bool allowDatabaseCreation, string taskDescription);


    /// <inheritdoc/>
    public abstract DiscoveredTable SelectTable(bool allowDatabaseCreation, string taskDescription);

    /// <inheritdoc/>
    public IMapsDirectlyToDatabaseTable[] SelectMany(string prompt, Type arrayElementType,
        IMapsDirectlyToDatabaseTable[] availableObjects, string initialSearchText = null) =>
        SelectMany(new DialogArgs
        {
            WindowTitle = prompt,
            InitialSearchText = initialSearchText
        }, arrayElementType, availableObjects);

    /// <inheritdoc/>
    public abstract IMapsDirectlyToDatabaseTable[] SelectMany(DialogArgs args, Type arrayElementType,
        IMapsDirectlyToDatabaseTable[] availableObjects);

    /// <inheritdoc/>
    public virtual IMapsDirectlyToDatabaseTable SelectOne(string prompt,
        IMapsDirectlyToDatabaseTable[] availableObjects,
        string initialSearchText = null, bool allowAutoSelect = false) =>
        SelectOne(new DialogArgs
        {
            WindowTitle = prompt,
            AllowAutoSelect = allowAutoSelect,
            InitialSearchText = initialSearchText
        }, availableObjects);

    /// <inheritdoc/>
    public abstract IMapsDirectlyToDatabaseTable SelectOne(DialogArgs args,
        IMapsDirectlyToDatabaseTable[] availableObjects);

    /// <inheritdoc/>
    public bool SelectObject<T>(string prompt, T[] available, out T selected, string initialSearchText = null,
        bool allowAutoSelect = false) where T : class =>
        SelectObject<T>(new DialogArgs
        {
            WindowTitle = prompt,
            InitialSearchText = initialSearchText,
            AllowAutoSelect = allowAutoSelect
        }, available, out selected);

    public abstract bool SelectObject<T>(DialogArgs args, T[] available, out T selected) where T : class;

    /// <inheritdoc/>
    public bool SelectObjects<T>(string prompt, T[] available, out T[] selected, string initialSearchText = null)
        where T : class =>
        SelectObjects<T>(new DialogArgs
        {
            WindowTitle = prompt,
            InitialSearchText = initialSearchText
        }, available, out selected);

    public abstract bool SelectObjects<T>(DialogArgs args, T[] available, out T[] selected) where T : class;

    /// <inheritdoc/>
    public abstract DirectoryInfo SelectDirectory(string prompt);

    /// <inheritdoc/>
    public abstract FileInfo SelectFile(string prompt);

    /// <inheritdoc/>
    public abstract FileInfo SelectFile(string prompt, string patternDescription, string pattern);

    /// <inheritdoc/>
    public abstract FileInfo[] SelectFiles(string prompt, string patternDescription, string pattern);

    /// <inheritdoc/>
    public virtual List<CommandInvokerDelegate> GetDelegates() => new();

    /// <inheritdoc/>
    public virtual IPipelineRunner GetPipelineRunner(DialogArgs args, IPipelineUseCase useCase, IPipeline pipeline) =>
        new PipelineRunner(useCase, pipeline);

    /// <inheritdoc/>
    public virtual CohortCreationRequest GetCohortCreationRequest(ExternalCohortTable externalCohortTable,
        IProject project, string cohortInitialDescription)
    {
        int version;
        var projectNumber = project?.ProjectNumber;

        if (!TypeText("Name", "Enter name for cohort", 255, null, out var name, false))
            throw new Exception("User chose not to enter a name for the cohort and none was provided");


        if (projectNumber == null)
            if (SelectValueType("enter project number", typeof(int), 0, out var chosen))
                projectNumber = (int)chosen;
            else
                throw new Exception("User chose not to enter a Project number and none was provided");


        if (SelectValueType("enter version number for cohort", typeof(int), 0, out var chosenVersion))
            version = (int)chosenVersion;
        else
            throw new Exception("User chose not to enter a version number and none was provided");


        return new CohortCreationRequest(project,
            new CohortDefinition(null, name, version, projectNumber.Value, externalCohortTable),
            RepositoryLocator.DataExportRepository, cohortInitialDescription);
    }

    /// <inheritdoc/>
    public virtual CohortHoldoutLookupRequest GetCohortHoldoutLookupRequest(ExternalCohortTable externalCohortTable, IProject project, CohortIdentificationConfiguration cic)
    {

        if (!TypeText("Name", "Enter name for cohort", 255, null, out var name, false))
            throw new Exception("User chose not to enter a name for the cohort and none was provided");

        return new CohortHoldoutLookupRequest(cic, "empty", 1,false,"","");
    }

    /// <inheritdoc/>
    public virtual ICatalogue CreateAndConfigureCatalogue(ITableInfo tableInfo,
        ColumnInfo[] extractionIdentifierColumns, string initialDescription, IProject projectSpecific,
        string catalogueFolder)
    {
        // Create a new Catalogue based on the table info
        var engineer = new ForwardEngineerCatalogue(tableInfo, tableInfo.ColumnInfos);
        engineer.ExecuteForwardEngineering(out var cata, out _, out var eis);

        // if we know the linkable private identifier column(s)
        if (extractionIdentifierColumns != null && extractionIdentifierColumns.Any())
        {
            // Make the Catalogue extractable
            var eds = new ExtractableDataSet(RepositoryLocator.DataExportRepository, cata);

            // Mark the columns specified IsExtractionIdentifier
            foreach (var col in extractionIdentifierColumns)
            {
                var match = eis.FirstOrDefault(ei => ei.ColumnInfo?.ID == col.ID) ??
                            throw new ArgumentException(
                                $"Supplied ColumnInfo {col.GetRuntimeName()} was not found amongst the columns created");
                match.IsExtractionIdentifier = true;
                match.SaveToDatabase();
            }

            // Catalogue must be extractable to be project specific
            if (projectSpecific != null)
            {
                var edsp = new ExtractableDataSetProject(RepositoryLocator.DataExportRepository, eds, projectSpecific);
                edsp.SaveToDatabase();
                eds.Projects.Add(projectSpecific);
                eds.SaveToDatabase();
            }
        }

        if (!string.IsNullOrWhiteSpace(catalogueFolder))
        {
            cata.Folder = catalogueFolder;
            cata.SaveToDatabase();
        }

        return cata;
    }

    public virtual ExternalDatabaseServer CreateNewPlatformDatabase(ICatalogueRepository catalogueRepository,
        PermissableDefaults defaultToSet, IPatcher patcher, DiscoveredDatabase db)
    {
        if (db == null && IsInteractive) db = SelectDatabase(true, "Select database");

        if (db == null)
            throw new ArgumentException(
                $"Database must be picked before calling {nameof(CreateNewPlatformDatabase)} when using {nameof(BasicActivateItems)}",
                nameof(db));

        var executor = new MasterDatabaseScriptExecutor(db);
        executor.CreateAndPatchDatabase(patcher, new AcceptAllCheckNotifier { WriteToConsole = true });

        var eds = new ExternalDatabaseServer(catalogueRepository,
            $"New {(defaultToSet == PermissableDefaults.None ? "" : defaultToSet.ToString())}Server", patcher);
        eds.SetProperties(db);

        if (defaultToSet != PermissableDefaults.None)
            catalogueRepository.SetDefault(defaultToSet, eds);

        return eds;
    }

    public virtual bool ShowCohortWizard(out CohortIdentificationConfiguration cic)
    {
        cic = null;
        return false;
    }

    public void SelectAnythingThen(string prompt, Action<IMapsDirectlyToDatabaseTable> callback)
    {
        SelectAnythingThen(new DialogArgs { WindowTitle = prompt }, callback);
    }

    public virtual void SelectAnythingThen(DialogArgs args, Action<IMapsDirectlyToDatabaseTable> callback)
    {
        var selected = SelectOne(args, CoreChildProvider.GetAllSearchables().Keys.ToArray());

        if (selected != null)
            callback(selected);
    }

    public abstract void ShowData(IViewSQLAndResultsCollection collection);
    public abstract void ShowData(DataTable collection);

    /// <summary>
    /// Presents user with log info about <paramref name="rootObject"/>.  Inheritors may wish to use <see cref="GetLogs(ILoggedActivityRootObject)"/>.
    /// </summary>
    /// <param name="rootObject"></param>
    public abstract void ShowLogs(ILoggedActivityRootObject rootObject);

    /// <summary>
    /// Presents user with top down view of logged activity across all objects
    /// </summary>
    /// <param name="loggingServer"></param>
    /// <param name="filter"></param>
    public virtual void ShowLogs(ExternalDatabaseServer loggingServer, LogViewerFilter filter)
    {
        ShowData(new ViewLogsCollection(loggingServer, filter));
    }

    /// <summary>
    /// Returns all logged activities for <paramref name="rootObject"/>
    /// </summary>
    /// <param name="rootObject"></param>
    /// <returns></returns>
    protected static IEnumerable<ArchivalDataLoadInfo> GetLogs(ILoggedActivityRootObject rootObject)
    {
        var db = rootObject.GetDistinctLoggingDatabase();
        var task = rootObject.GetDistinctLoggingTask();

        var lm = new LogManager(db);
        return rootObject.FilterRuns(lm.GetArchivalDataLoadInfos(task));
    }

    /// <inheritdoc/>
    public abstract void ShowGraph(AggregateConfiguration aggregate);

    /// <inheritdoc/>
    public IRepository GetRepositoryFor(Type type)
    {
        foreach (var repo in RepositoryLocator.GetAllRepositories())
            if (repo.SupportsObjectType(type))
                return repo;

        throw new ArgumentException($"Did not know what repository to use to fetch objects of Type '{type}'");
    }

    public abstract void LaunchSubprocess(ProcessStartInfo startInfo);


    /// <inheritdoc/>
    public bool UseCommits()
    {
        var repo = RepositoryLocator?.CatalogueRepository;

        // system is in a very bad state
        if (repo == null)
            return false;

        // does user want to do commits? and we have a db repo
        return UserSettings.EnableCommits && repo.SupportsCommits;
    }
}
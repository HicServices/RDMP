// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FAnsi.Discovery;
using MapsDirectlyToDatabaseTable;
using MapsDirectlyToDatabaseTable.Versioning;
using Rdmp.Core.CohortCommitting.Pipeline;
using Rdmp.Core.CohortCreation.Execution;
using Rdmp.Core.CommandExecution.AtomicCommands;
using Rdmp.Core.CommandLine.Interactive;
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
using Rdmp.Core.Providers;
using Rdmp.Core.Repositories;
using Rdmp.Core.Repositories.Construction;
using ReusableLibraryCode.Checks;

namespace Rdmp.Core.CommandExecution
{
    public abstract class BasicActivateItems : IBasicActivateItems
    {
        /// <inheritdoc/>
        public virtual bool IsInteractive => true;

        /// <inheritdoc/>
        public bool InteractiveDeletes {get;set;}

        /// <inheritdoc/>
        public ICoreChildProvider CoreChildProvider { get; protected set;}

        /// <inheritdoc/>
        public IServerDefaults ServerDefaults { get; }

        /// <inheritdoc/>
        public FavouritesProvider FavouritesProvider { get; private set; }
        public ICoreIconProvider CoreIconProvider { get; private set; }

        /// <inheritdoc/>
        public bool YesNo(string text, string caption, out bool chosen)
        {
            return YesNo(new DialogArgs
            {
                WindowTitle = caption,
                TaskDescription = text
            }, out chosen);
        }

        public abstract bool YesNo(DialogArgs args, out bool chosen);

        /// <inheritdoc/>
        public bool YesNo(string text, string caption)
        {
            return YesNo(new DialogArgs
            {
                WindowTitle = caption,
                TaskDescription = text
            });
        }
        public bool YesNo(DialogArgs args)
        {
            return YesNo(args, out bool chosen) && chosen;
        }

        public bool Confirm(string text, string caption)
        {
            return Confirm(new DialogArgs
            {
                WindowTitle = caption,
                TaskDescription = text
            });
        }

        /// <inheritdoc/>
        public bool Confirm(DialogArgs args)
        {
            // auto confirm if not in user interactive mode
            if (!IsInteractive)
                return true;

            return YesNo(args);
        }

        /// <inheritdoc/>
        public ICheckNotifier GlobalErrorCheckNotifier { get; set; }

        /// <inheritdoc/>
        public IRDMPPlatformRepositoryServiceLocator RepositoryLocator { get; }

        /// <inheritdoc/>
        public event EmphasiseItemHandler Emphasise;
        
        /// <inheritdoc/>
        public List<IPluginUserInterface> PluginUserInterfaces { get; private set; } = new List<IPluginUserInterface>();

        public BasicActivateItems(IRDMPPlatformRepositoryServiceLocator repositoryLocator, ICheckNotifier globalErrorCheckNotifier)
        {
            RepositoryLocator = repositoryLocator;
            GlobalErrorCheckNotifier = globalErrorCheckNotifier;

            ServerDefaults = RepositoryLocator.CatalogueRepository.GetServerDefaults();
            
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
                    temp = new DataExportChildProvider(RepositoryLocator, PluginUserInterfaces.ToArray(), GlobalErrorCheckNotifier, CoreChildProvider as DataExportChildProvider);
                }
                catch (Exception e)
                {
                    ShowException("Error constructing DataExportChildProvider",e);
                }

            //there was an error generating a data export repository or there was no repository specified

            //so just create a catalogue one
            if (temp == null)
                temp = new CatalogueChildProvider(RepositoryLocator.CatalogueRepository, PluginUserInterfaces.ToArray(), GlobalErrorCheckNotifier, CoreChildProvider as CatalogueChildProvider);

            // first time
            if (CoreChildProvider == null)
                CoreChildProvider = temp;
            else
                CoreChildProvider.UpdateTo(temp);

            return CoreChildProvider;
        }

        private void ConstructPluginChildProviders()
        {
            // if startup has not taken place then we won't have any plugins
            if (RepositoryLocator.CatalogueRepository.MEF == null)
                return;

            PluginUserInterfaces = new List<IPluginUserInterface>();

            var constructor = new ObjectConstructor();

            foreach (Type pluginType in RepositoryLocator.CatalogueRepository.MEF.GetTypes<IPluginUserInterface>())
            {
                try
                {
                    PluginUserInterfaces.Add((IPluginUserInterface)constructor.Construct(pluginType, this, false));
                }
                catch (Exception e)
                {
                    GlobalErrorCheckNotifier.OnCheckPerformed(new CheckEventArgs("Problem occured trying to load Plugin '" + pluginType.Name + "'", CheckResult.Fail, e));
                }
            }
        }

        protected void OnEmphasise(object sender, EmphasiseEventArgs args)
        {
            Emphasise?.Invoke(sender,args);
        }
        /// <inheritdoc/>
        public virtual IEnumerable<Type> GetIgnoredCommands()
        {
            return new Type[0];
        }

        /// <inheritdoc/>
        public virtual void Wait(string title, Task task, CancellationTokenSource cts)
        {
            task.Wait(cts.Token);
        }

        /// <inheritdoc/>
        public virtual void RequestItemEmphasis(object sender, EmphasiseRequest emphasiseRequest)
        {
            OnEmphasise(sender, new EmphasiseEventArgs(emphasiseRequest));
        }

        /// <summary>
        /// Returns the root tree object which hosts the supplied object.  If the supplied object has no known descendancy it is assumed
        /// to be the root object itself so it is returned
        /// </summary>
        /// <param name="objectToEmphasise"></param>
        /// <returns></returns>
        public object GetRootObjectOrSelf(IMapsDirectlyToDatabaseTable objectToEmphasise)
        {
            return CoreChildProvider?.GetRootObjectOrSelf(objectToEmphasise) ?? objectToEmphasise;
        }

        /// <inheritdoc/>
        public bool SelectEnum(string prompt, Type enumType, out Enum chosen)
        {
            return SelectEnum(new DialogArgs
            {
                WindowTitle = prompt
            }, enumType, out chosen);
        }

        public abstract bool SelectEnum(DialogArgs args, Type enumType, out Enum chosen);


        public bool SelectType(string prompt, Type baseTypeIfAny, out Type chosen)
        {
            return SelectType(new DialogArgs()
            {
                WindowTitle = prompt
            },baseTypeIfAny,out chosen);
        }

        public bool SelectType(DialogArgs args, Type baseTypeIfAny, out Type chosen)
        {
            return SelectType(args, baseTypeIfAny, false, false, out chosen);
        }

        public bool SelectType(string prompt, Type baseTypeIfAny,bool allowAbstract,bool allowInterfaces, out Type chosen)
        {
            return SelectType(new DialogArgs()
            {
                WindowTitle = prompt
            },baseTypeIfAny,allowAbstract,allowInterfaces,out chosen);
        }
        public bool SelectType(DialogArgs args, Type baseTypeIfAny, bool allowAbstract, bool allowInterfaces, out Type chosen)
        {
            Type[] available =
            RepositoryLocator.CatalogueRepository.MEF.GetAllTypes()
                .Where(t => 
                (baseTypeIfAny == null || baseTypeIfAny.IsAssignableFrom(t)) &&
                (allowAbstract || !t.IsAbstract) &&
                (allowInterfaces || !t.IsInterface))
                .ToArray();

            return SelectType(args, available, out chosen);
        }

        public bool SelectType(string prompt, Type[] available, out Type chosen)
        {
            return SelectType(new DialogArgs
            {
                WindowTitle = prompt
            }, available, out chosen);
        }

        public abstract bool SelectType(DialogArgs args, Type[] available, out Type chosen);

        public virtual bool CanActivate(object target)
        {
            return false;
        }

        public void Activate(object o)
        {
            if(o is IMapsDirectlyToDatabaseTable m)
            {
                // if a plugin user interface exists to handle editing this then let them handle it instead of launching the 
                // normal UI
                foreach (var pluginInterface in PluginUserInterfaces)
                {
                    if (pluginInterface.CustomActivate(m))
                    {
                        return;
                    }
                }
            }

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
        public virtual IEnumerable<T> GetAll<T>()
        {
            return CoreChildProvider.GetAllSearchables()
                .Keys.OfType<T>();
        }

        /// <inheritdoc/>
        public virtual IEnumerable<IMapsDirectlyToDatabaseTable> GetAll(Type t)
        {
            return CoreChildProvider.GetAllSearchables()
                .Keys.Where(t.IsInstanceOfType);
        }
        
        /// <inheritdoc/>
        public abstract void ShowException(string errorText, Exception exception);

        /// <inheritdoc/>
        public void PublishNearest(object publish)
        {
            if (publish != null)
            {
                if (publish is DatabaseEntity d)
                    Publish(d);
                else
                {
                    var descendancy = CoreChildProvider.GetDescendancyListIfAnyFor(publish);

                    if (descendancy != null)
                    {
                        var parent = descendancy.Parents.OfType<DatabaseEntity>().LastOrDefault();

                        if (parent != null)
                            Publish(parent);
                    }
                }
            }
        }

        /// <inheritdoc/>
        public virtual bool DeleteWithConfirmation(IDeleteable deleteable)
        {
            if (IsInteractive && InteractiveDeletes)
            {
                bool didDelete = InteractiveDelete(deleteable);

                if (didDelete)
                {
                    PublishNearest(deleteable);
                }   

                return didDelete;
            }
            else
            {
                deleteable.DeleteInDatabase();
                PublishNearest(deleteable);

                return true;
            }
        }

        protected virtual bool InteractiveDelete(IDeleteable deleteable)
        {
            var databaseObject = deleteable as DatabaseEntity;
                        
            //If there is some special way of describing the effects of deleting this object e.g. Selected Datasets
            var customMessageDeletable = deleteable as IDeletableWithCustomMessage;
            
            if(databaseObject is Catalogue c)
            {
                if(c.GetExtractabilityStatus(RepositoryLocator.DataExportRepository).IsExtractable)
                {
                    if(YesNo("Catalogue must first be made non extractable before it can be deleted, mark non extractable?","Make Non Extractable"))
                    {
                        var cmd = new ExecuteCommandChangeExtractability(this,c);
                        cmd.Execute();
                    }
                    else
                        return false;
                }
            }
            
            if(databaseObject is ExtractionFilter f)
            {
                var children = f.ExtractionFilterParameterSets;

                if (children.Any())
                {
                    if (!YesNo(
                        $"Filter has {children.Length} value sets defined.  Deleting filter will also delete these.  Confirm?",
                        "Delete"))
                        return false;
                    
                    foreach (var child in children)
                    {
                        child.DeleteInDatabase();
                    }

                    f.DeleteInDatabase();
                    return true;
                }
            }

            if( databaseObject is AggregateConfiguration ac && ac.IsJoinablePatientIndexTable())
            {
                var users = ac.JoinableCohortAggregateConfiguration?.Users?.Select(u=>u.AggregateConfiguration);
                if(users != null)
                {
                    users = users.ToArray();
                    if(users.Any())
                    {
                        Show($"Cannot Delete '{ac.Name}' because it is linked to by the following AggregateConfigurations:{Environment.NewLine}{string.Join(Environment.NewLine,users)}");
                        return false;
                    }                       
                }
            }

            string overrideConfirmationText = null;

            if (customMessageDeletable != null)
                overrideConfirmationText = "Are you sure you want to " +customMessageDeletable.GetDeleteMessage() +"?";

            //it has already been deleted before
            if (databaseObject != null && !databaseObject.Exists())
                return false;

            string idText = "";

            if (databaseObject != null)
                idText = " ID=" + databaseObject.ID;

            if (databaseObject != null)
            {
                var exports = RepositoryLocator.CatalogueRepository.GetReferencesTo<ObjectExport>(databaseObject).ToArray();
                if(exports.Any(e=>e.Exists()))
                    if(YesNo("This object has been shared as an ObjectExport.  Deleting it may prevent you loading any saved copies.  Do you want to delete the ObjectExport definition?","Delete ObjectExport"))
                    {
                        foreach(ObjectExport e in exports)
                            e.DeleteInDatabase(); 
                    }
                    else
                        return false;
            }
                        
            if (
                YesNo(
                    overrideConfirmationText?? ("Are you sure you want to delete '" + deleteable + "'?")
                +Environment.NewLine + "(" + deleteable.GetType().Name + idText +")",
                "Delete " + deleteable.GetType().Name))
            {
                deleteable.DeleteInDatabase();
                
                if (databaseObject == null)
                {
                    var descendancy = CoreChildProvider.GetDescendancyListIfAnyFor(deleteable);
                    if(descendancy != null)
                        databaseObject = descendancy.Parents.OfType<DatabaseEntity>().LastOrDefault();
                }

                if (deleteable is IMasqueradeAs)
                    databaseObject = databaseObject ?? ((IMasqueradeAs)deleteable).MasqueradingAs() as DatabaseEntity;

                if (databaseObject == null)
                    throw new NotSupportedException("IDeletable " + deleteable +
                                                    " was not a DatabaseObject and it did not have a Parent in it's tree which was a DatabaseObject (DescendancyList)");
                return true;
            }

            return false;
        }

        public bool SelectValueType(string prompt, Type paramType, object initialValue, out object chosen)
        {
            return SelectValueType(
                new DialogArgs
                {
                    WindowTitle = $"Enter value for {prompt}",
                    EntryLabel = prompt,
                },paramType,initialValue,out chosen);
        }

        /// <inheritdoc/>
        public bool SelectValueType(DialogArgs args, Type paramType, object initialValue, out object chosen)
        {
            var underlying = Nullable.GetUnderlyingType(paramType);
            if ((underlying ?? paramType).IsEnum)
            {
                bool ok = SelectEnum(args, underlying?? paramType, out Enum enumChosen);
                chosen = enumChosen;
                return ok;
            }

            if (paramType == typeof(bool) || paramType == typeof(bool?))
            {
                bool ok = YesNo(args, out bool boolChosen);
                chosen = boolChosen;
                return ok;
            }

            if (paramType == typeof(string))
            {
                bool ok = TypeText(args, int.MaxValue, initialValue?.ToString(),out string stringChosen,false);
                chosen = stringChosen;
                return ok;

            }

            return SelectValueTypeImpl(args, paramType, initialValue, out chosen);
        }

        protected abstract bool SelectValueTypeImpl(DialogArgs args, Type paramType, object initialValue, out object chosen);

        /// <inheritdoc/>
        public virtual void Publish(IMapsDirectlyToDatabaseTable databaseEntity)
        {
            var fresh = GetChildProvider();
            CoreChildProvider.UpdateTo(fresh);
        }

        /// <inheritdoc/>
        public void Show(string message)
        {
            Show("Message",message);
        }

        public abstract void Show(string title, string message);

        /// <inheritdoc/>
        public bool TypeText(string header, string prompt, int maxLength, string initialText, out string text,
            bool requireSaneHeaderText)
        {
            return TypeText(new DialogArgs
            {
                WindowTitle = header,
                EntryLabel = prompt
            },maxLength,initialText,out text,requireSaneHeaderText);
        }
        public abstract bool TypeText(DialogArgs args, int maxLength, string initialText, out string text,
            bool requireSaneHeaderText);

        /// <inheritdoc/>
        public abstract DiscoveredDatabase SelectDatabase(bool allowDatabaseCreation, string taskDescription);


        /// <inheritdoc/>
        public abstract DiscoveredTable SelectTable(bool allowDatabaseCreation, string taskDescription);
        
        /// <inheritdoc/>
        public IMapsDirectlyToDatabaseTable[] SelectMany(string prompt, Type arrayElementType,
            IMapsDirectlyToDatabaseTable[] availableObjects, string initialSearchText = null)
        {
            return SelectMany(new DialogArgs()
            {
                WindowTitle = prompt,
                InitialSearchText = initialSearchText,
            },arrayElementType,availableObjects);
        }

        /// <inheritdoc/>
        public abstract IMapsDirectlyToDatabaseTable[] SelectMany(DialogArgs args, Type arrayElementType,
            IMapsDirectlyToDatabaseTable[] availableObjects);

        /// <inheritdoc/>
        public virtual IMapsDirectlyToDatabaseTable SelectOne(string prompt, IMapsDirectlyToDatabaseTable[] availableObjects,
            string initialSearchText = null, bool allowAutoSelect = false)
        {
            return SelectOne(new DialogArgs()
            {
                WindowTitle = prompt,
                AllowAutoSelect = allowAutoSelect,
                InitialSearchText = initialSearchText
            }, availableObjects);
        }

        /// <inheritdoc/>
        public abstract IMapsDirectlyToDatabaseTable SelectOne(DialogArgs args, IMapsDirectlyToDatabaseTable[] availableObjects);

        /// <inheritdoc/>
        public bool SelectObject<T>(string prompt, T[] available, out T selected, string initialSearchText = null, bool allowAutoSelect = false) where T : class
        {
            return SelectObject<T>(new DialogArgs()
            {
                WindowTitle = prompt,
                InitialSearchText = initialSearchText,
                AllowAutoSelect = allowAutoSelect
            }, available, out selected);
        }

        public abstract bool SelectObject<T>(DialogArgs args, T[] available, out T selected) where T : class;

        /// <inheritdoc/>
        public bool SelectObjects<T>(string prompt, T[] available, out T[] selected, string initialSearchText = null) where T : class
        {
            return SelectObjects<T>(new DialogArgs
            {
                WindowTitle = prompt,
                InitialSearchText = initialSearchText,
            }, available, out selected);
        }
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
        public virtual List<CommandInvokerDelegate> GetDelegates()
        {
            return new List<CommandInvokerDelegate>();
        }
        
        /// <inheritdoc/>
        public virtual IPipelineRunner GetPipelineRunner(IPipelineUseCase useCase, IPipeline pipeline)
        {
            return new PipelineRunner(useCase,pipeline);
        }
        
        /// <inheritdoc/>
        public virtual CohortCreationRequest GetCohortCreationRequest(ExternalCohortTable externalCohortTable, IProject project, string cohortInitialDescription)
        {
            int version;
            var projectNumber = project?.ProjectNumber;
            string name;

            if(!this.TypeText("Name","Enter name for cohort",255,null,out name,false))
                throw new Exception("User chose not to enter a name for the cohortand none was provided");


            if(projectNumber == null)
                if(this.SelectValueType("enter project number",typeof(int),0,out object chosen))
                {
                    projectNumber = (int)chosen;
                }
                else
                    throw new Exception("User chose not to enter a Project number and none was provided");

            
            if(this.SelectValueType("enter version number for cohort",typeof(int),0,out object chosenVersion))
            {
                version = (int)chosenVersion;
            }
            else
                throw new Exception("User chose not to enter a version number and none was provided");


            return new CohortCreationRequest(project,new CohortDefinition(null,name,version,projectNumber.Value,externalCohortTable),RepositoryLocator.DataExportRepository,cohortInitialDescription);
        }
        
        /// <inheritdoc/>
        public virtual ICatalogue CreateAndConfigureCatalogue(ITableInfo tableInfo, ColumnInfo[] extractionIdentifierColumns, string initialDescription, IProject projectSpecific, CatalogueFolder catalogueFolder)
        {
            // Create a new Catalogue based on the table info
            var engineer = new ForwardEngineerCatalogue(tableInfo,tableInfo.ColumnInfos,true);
            engineer.ExecuteForwardEngineering(out ICatalogue cata, out _, out ExtractionInformation[] eis);

            // if we know the linkable private identifier column(s)
            if(extractionIdentifierColumns != null && extractionIdentifierColumns.Any())
            {
                // Make the Catalogue extractable
                var eds = new ExtractableDataSet(RepositoryLocator.DataExportRepository,cata);

                // Mark the columns specified IsExtractionIdentifier
                foreach(var col in extractionIdentifierColumns)
                {
                    var match = eis.FirstOrDefault(ei=>ei.ColumnInfo?.ID == col.ID);
                    if(match == null)
                        throw new ArgumentException($"Supplied ColumnInfo {col.GetRuntimeName()} was not found amongst the columns created");

                    match.IsExtractionIdentifier = true;
                    match.SaveToDatabase();
                }

                // Catalogue must be extractable to be project specific
                if(projectSpecific != null)
                {
                    eds.Project_ID = projectSpecific.ID;
                    eds.SaveToDatabase();
                }
            }

            if(catalogueFolder != null)
            {
                cata.Folder = catalogueFolder;
                cata.SaveToDatabase();
            }

            return cata;
        }

        public virtual ExternalDatabaseServer CreateNewPlatformDatabase(ICatalogueRepository catalogueRepository, PermissableDefaults defaultToSet, IPatcher patcher, DiscoveredDatabase db)
        {
            if(db == null)
                throw new ArgumentException($"Database must be picked before calling {nameof(CreateNewPlatformDatabase)} when using {nameof(BasicActivateItems)}",nameof(db));

            MasterDatabaseScriptExecutor executor = new MasterDatabaseScriptExecutor(db);
            executor.CreateAndPatchDatabase(patcher,new AcceptAllCheckNotifier());

            var eds = new ExternalDatabaseServer(catalogueRepository,"New " + (defaultToSet == PermissableDefaults.None ? "" :  defaultToSet.ToString()) + "Server",patcher);
            eds.SetProperties(db);
            
            return eds;
        }

        public virtual bool ShowCohortWizard(out CohortIdentificationConfiguration cic)
        {
            cic = null;
            return false;
        }

        public void SelectAnythingThen(string prompt, Action<IMapsDirectlyToDatabaseTable> callback)
        {
            SelectAnythingThen(new DialogArgs() { WindowTitle = prompt}, callback);
        }
        public virtual void SelectAnythingThen(DialogArgs args, Action<IMapsDirectlyToDatabaseTable> callback)
        {
            var selected = SelectOne(args, CoreChildProvider.GetAllSearchables().Keys.ToArray());

            if (selected != null)
                callback(selected);
        }

        public abstract void ShowData(IViewSQLAndResultsCollection collection);

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
            ShowData(new ViewLogsCollection(loggingServer,filter));
        }

        /// <summary>
        /// Returns all logged activities for <paramref name="rootObject"/>
        /// </summary>
        /// <param name="rootObject"></param>
        /// <returns></returns>
        protected IEnumerable<ArchivalDataLoadInfo> GetLogs(ILoggedActivityRootObject rootObject)
        {
            var db = rootObject.GetDistinctLoggingDatabase();
            var task = rootObject.GetDistinctLoggingTask();

            var lm = new LogManager(db);
            return rootObject.FilterRuns(lm.GetArchivalDataLoadInfos(task));
        }

        /// <inheritdoc/>
        public abstract void ShowGraph(AggregateConfiguration aggregate);

        public IRepository GetRepositoryFor(Type type)
        {
            foreach(var repo in RepositoryLocator.GetAllRepositories())
            {
                if(repo.SupportsObjectType(type))
                {
                    return repo;
                }
            }

            throw new ArgumentException("Did not know what repository to use to fetch objects of Type '" + type + "'");
        }
    }
}

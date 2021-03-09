// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FAnsi.Discovery;
using MapsDirectlyToDatabaseTable;
using MapsDirectlyToDatabaseTable.Versioning;
using Rdmp.Core.CohortCommitting.Pipeline;
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
using Rdmp.Core.Logging;
using Rdmp.Core.Logging.PastEvents;
using Rdmp.Core.Providers;
using Rdmp.Core.Repositories;
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

        /// <inheritdoc/>
        public abstract bool YesNo(string text, string caption, out bool chosen);

        /// <inheritdoc/>
        public bool YesNo(string text, string caption)
        {
            return YesNo(text, caption, out bool chosen) && chosen;
        }

        /// <inheritdoc/>
        public ICheckNotifier GlobalErrorCheckNotifier { get; set; }

        /// <inheritdoc/>
        public IRDMPPlatformRepositoryServiceLocator RepositoryLocator { get; }

        /// <inheritdoc/>
        public event EmphasiseItemHandler Emphasise;

        public BasicActivateItems(IRDMPPlatformRepositoryServiceLocator repositoryLocator, ICheckNotifier globalErrorCheckNotifier)
        {
            RepositoryLocator = repositoryLocator;
            GlobalErrorCheckNotifier = globalErrorCheckNotifier;

            ServerDefaults = RepositoryLocator.CatalogueRepository.GetServerDefaults();
            
            //Shouldn't ever change externally to your session so doesn't need constantly refreshed
            FavouritesProvider = new FavouritesProvider(this);

            // Note that this is virtual so can return null e.g. if other stuff has to happen with the activator before a valid child provider can be built (e.g. loading plugin user interfaces)
            CoreChildProvider = GetChildProvider();
        }

        protected virtual ICoreChildProvider GetChildProvider()
        {
            return RepositoryLocator.DataExportRepository != null?
                            new DataExportChildProvider(RepositoryLocator,null,GlobalErrorCheckNotifier, CoreChildProvider as DataExportChildProvider):
                            new CatalogueChildProvider(RepositoryLocator.CatalogueRepository,null,GlobalErrorCheckNotifier,CoreChildProvider as CatalogueChildProvider);
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

        public abstract bool SelectEnum(string prompt, Type enumType, out Enum chosen);
        public virtual bool SelectType(string prompt, Type baseTypeIfAny, out Type chosen)
        {
            Type[] available =
            RepositoryLocator.CatalogueRepository.MEF.GetAllTypes()
                .Where(t => baseTypeIfAny == null || baseTypeIfAny.IsAssignableFrom(t))
                .ToArray();

            return SelectType(prompt, available, out chosen);
        }

        public abstract bool SelectType(string prompt, Type[] available, out Type chosen);
        
        public virtual bool CanActivate(object target)
        {
            return false;
        }

        public virtual void Activate(object o)
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
        public virtual bool DeleteWithConfirmation(IDeleteable deleteable)
        {
            if (IsInteractive && InteractiveDeletes)
            {
                bool didDelete = InteractiveDelete(deleteable);
                
                if(didDelete && deleteable is IMapsDirectlyToDatabaseTable o)
                    Publish(o);

                return didDelete;
            }
            else
            {
                deleteable.DeleteInDatabase();

                if(deleteable is DatabaseEntity d)
                    Publish(d);

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

        /// <inheritdoc/>
        public bool SelectValueType(string prompt, Type paramType, object initialValue, out object chosen)
        {

            if ((Nullable.GetUnderlyingType(paramType) ?? paramType).IsEnum)
            {
                bool ok = SelectEnum(prompt, paramType, out Enum enumChosen);
                chosen = enumChosen;
                return ok;
            }

            if (paramType == typeof(bool) || paramType == typeof(bool?))
            {
                bool ok = YesNo(prompt, "Enter Value", out bool boolChosen);
                chosen = boolChosen;
                return ok;
            }

            if (paramType == typeof(string))
            {
                bool ok = TypeText("Enter Value",prompt,int.MaxValue, initialValue?.ToString(),out string stringChosen,false);
                chosen = stringChosen;
                return ok;

            }

            return SelectValueTypeImpl(prompt, paramType, initialValue, out chosen);
        }

        protected abstract bool SelectValueTypeImpl(string prompt, Type paramType, object initialValue, out object chosen);

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
        public abstract bool TypeText(string header, string prompt, int maxLength, string initialText, out string text,
            bool requireSaneHeaderText);

        /// <inheritdoc/>
        public abstract DiscoveredDatabase SelectDatabase(bool allowDatabaseCreation, string taskDescription);


        /// <inheritdoc/>
        public abstract DiscoveredTable SelectTable(bool allowDatabaseCreation, string taskDescription);
        
        /// <inheritdoc/>
        public abstract IMapsDirectlyToDatabaseTable[] SelectMany(string prompt, Type arrayElementType,
            IMapsDirectlyToDatabaseTable[] availableObjects, string initialSearchText = null);

        /// <inheritdoc/>
        public abstract IMapsDirectlyToDatabaseTable SelectOne(string prompt, IMapsDirectlyToDatabaseTable[] availableObjects,
            string initialSearchText = null, bool allowAutoSelect = false);

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

        public virtual void SelectAnythingThen(string prompt, Action<IMapsDirectlyToDatabaseTable> callback)
        {
            var selected = SelectOne(prompt, CoreChildProvider.GetAllSearchables().Keys.ToArray());

            if(selected != null)
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

    }
}
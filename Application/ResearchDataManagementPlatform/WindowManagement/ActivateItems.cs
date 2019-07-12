// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using MapsDirectlyToDatabaseTable;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Curation.Data.Aggregation;
using Rdmp.Core.Curation.Data.Dashboarding;
using Rdmp.Core.Curation.Data.DataLoad;
using Rdmp.Core.Curation.Data.Defaults;
using Rdmp.Core.Curation.Data.ImportExport;
using Rdmp.Core.Providers;
using Rdmp.Core.QueryBuilding;
using Rdmp.Core.Repositories;
using Rdmp.UI.Collections;
using Rdmp.UI.Collections.Providers;
using Rdmp.UI.CommandExecution;
using Rdmp.UI.CommandExecution.AtomicCommands;
using Rdmp.UI.Copying;
using Rdmp.UI.DataLoadUIs.LoadMetadataUIs.LoadDiagram;
using Rdmp.UI.ExtractionUIs.FilterUIs;
using Rdmp.UI.ExtractionUIs.JoinsAndLookups;
using Rdmp.UI.Icons.IconProvision;
using Rdmp.UI.ItemActivation;
using Rdmp.UI.ItemActivation.Arranging;
using Rdmp.UI.ItemActivation.Emphasis;
using Rdmp.UI.PluginChildProvision;
using Rdmp.UI.Refreshing;
using Rdmp.UI.Rules;
using Rdmp.UI.SimpleDialogs;
using Rdmp.UI.SubComponents.Graphs;
using Rdmp.UI.TestsAndSetup.ServicePropogation;
using ResearchDataManagementPlatform.WindowManagement.ContentWindowTracking.Persistence;
using ResearchDataManagementPlatform.WindowManagement.WindowArranging;
using ReusableLibraryCode.Checks;
using ReusableLibraryCode.Comments;
using ReusableUIComponents.CommandExecution;
using ReusableUIComponents.Dialogs;
using ReusableUIComponents.Theme;
using WeifenLuo.WinFormsUI.Docking;

namespace ResearchDataManagementPlatform.WindowManagement
{
    /// <summary>
    /// Central class for RDMP main application, this class provides acceess to all the main systems in RDMP user interface such as Emphasis, the RefreshBus, Child 
    /// provision etc.  See IActivateItems for full details
    /// </summary>
    public class ActivateItems : IActivateItems, IRefreshBusSubscriber
    {

        public event EmphasiseItemHandler Emphasise;

        private readonly DockPanel _mainDockPanel;
        private readonly WindowManager _windowManager;

        public IRDMPPlatformRepositoryServiceLocator RepositoryLocator { get; set; }
        public WindowFactory WindowFactory { get; private set; }

        public ICoreIconProvider CoreIconProvider { get; private set; }

        public ITheme Theme { get; private set; }
        public IServerDefaults ServerDefaults { get; private set; }
        public RefreshBus RefreshBus { get; private set; }
        public FavouritesProvider FavouritesProvider { get; private set; }

        public ICoreChildProvider CoreChildProvider { get; private set; }

        public List<IPluginUserInterface> PluginUserInterfaces { get; private set; }
        readonly UIObjectConstructor _constructor = new UIObjectConstructor();

        public IArrangeWindows WindowArranger { get; private set; }
        
        public ICheckNotifier GlobalErrorCheckNotifier { get; private set; }

        public ICommandFactory CommandFactory { get; private set; }
        public ICommandExecutionFactory CommandExecutionFactory { get; private set; }
        public CommentStore CommentStore { get { return RepositoryLocator.CatalogueRepository.CommentStore; } }

        public List<IProblemProvider> ProblemProviders { get; private set; }

        public ActivateItems(ITheme theme,RefreshBus refreshBus, DockPanel mainDockPanel, IRDMPPlatformRepositoryServiceLocator repositoryLocator, WindowFactory windowFactory, WindowManager windowManager, ICheckNotifier globalErrorCheckNotifier)
        {
            Theme = theme;
            WindowFactory = windowFactory;
            _mainDockPanel = mainDockPanel;
            _windowManager = windowManager;
            GlobalErrorCheckNotifier = globalErrorCheckNotifier;
            RepositoryLocator = repositoryLocator;

            ServerDefaults = RepositoryLocator.CatalogueRepository.GetServerDefaults();

            //Shouldn't ever change externally to your session so doesn't need constantly refreshed
            FavouritesProvider = new FavouritesProvider(this, repositoryLocator.CatalogueRepository);

            RefreshBus = refreshBus;

            ConstructPluginChildProviders();

            UpdateChildProviders();
            RefreshBus.BeforePublish += (s, e) => UpdateChildProviders();

            //handle custom icons from plugin user interfaces in which
            CoreIconProvider = new DataExportIconProvider(PluginUserInterfaces.ToArray());
            
            SelectIMapsDirectlyToDatabaseTableDialog.ImageGetter = (model)=> CoreIconProvider.GetImage(model);

            WindowArranger = new WindowArranger(this,_windowManager,_mainDockPanel);
            
            CommandFactory = new RDMPCommandFactory();
            CommandExecutionFactory = new RDMPCommandExecutionFactory(this);

            ProblemProviders = new List<IProblemProvider>();
            ProblemProviders.Add(new DataExportProblemProvider());
            ProblemProviders.Add(new CatalogueProblemProvider());
            RefreshProblemProviders();

            RefreshBus.Subscribe(this);
        }

        private void ConstructPluginChildProviders()
        {
            PluginUserInterfaces = new List<IPluginUserInterface>();

            foreach (Type pluginType in RepositoryLocator.CatalogueRepository.MEF.GetTypes<IPluginUserInterface>())
            {
                try
                {
                    
                    PluginUserInterfaces.Add((IPluginUserInterface) _constructor.Construct(pluginType,this,false));
                }
                catch (Exception e)
                {
                    GlobalErrorCheckNotifier.OnCheckPerformed(new CheckEventArgs("Problem occured trying to load Plugin '" + pluginType.Name +"'", CheckResult.Fail, e));
                }
            }
        }

        private void UpdateChildProviders()
        {
            //prefer a linked repository with both
            if(RepositoryLocator.DataExportRepository != null)
                try
                {
                    CoreChildProvider = new DataExportChildProvider(RepositoryLocator,PluginUserInterfaces.ToArray(),GlobalErrorCheckNotifier);
                }
                catch (Exception e)
                {
                    ExceptionViewer.Show(e);
                }
            
            //there was an error generating a data export repository or there was no repository specified

            //so just create a catalogue one
            if (CoreChildProvider == null)
                CoreChildProvider = new CatalogueChildProvider(RepositoryLocator.CatalogueRepository, PluginUserInterfaces.ToArray(),GlobalErrorCheckNotifier);


            CoreChildProvider.GetPluginChildren();
            RefreshBus.ChildProvider = CoreChildProvider;
        }

        public Form ShowRDMPSingleDatabaseObjectControl(IRDMPSingleDatabaseObjectControl control,DatabaseEntity objectOfTypeT)
        {
            var content = WindowFactory.Create(this,control,objectOfTypeT);
            content.Show(_mainDockPanel, DockState.Document);
            control.SetDatabaseObject(this,objectOfTypeT);

            return content;
        }


        public Form ShowWindow(Control singleControlForm, bool asDocument = false)
        {
            int width = singleControlForm.Size.Width + SystemInformation.BorderSize.Width;
            int height = singleControlForm.Size.Height + SystemInformation.BorderSize.Height;

            //use the .Text or fallback on .Name 
            string name = string.IsNullOrWhiteSpace(singleControlForm.Text)
                ? singleControlForm.Name ?? singleControlForm.GetType().Name//or worst case scenario use the type name!
                : singleControlForm.Text;

            if(singleControlForm is Form && asDocument)
                throw new Exception("Control '" + singleControlForm + "' is a Form and asDocument was passed as true.  When asDocument is true you must be a Control not a Form e.g. inherit from RDMPUserControl instead of RDMPForm");

            var c = singleControlForm as RDMPUserControl;
            
            if(c != null)
                c.SetItemActivator(this);

            var content = WindowFactory.Create(this,singleControlForm,name , null);
            
            if (asDocument)
                content.Show(_mainDockPanel,DockState.Document);
            else
                content.Show(_mainDockPanel,new Rectangle(0,0,width,height));
            
            return content;
        }


        public bool DeleteWithConfirmation(object sender, IDeleteable deleteable)
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
                        WideMessageBox.Show("Cannot Delete",$"Cannot Delete '{ac.Name}' because it is linked to by the following AggregateConfigurations:{Environment.NewLine}{string.Join(Environment.NewLine,users)}");
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
                    overrideConfirmationText?? ("Are you sure you want to delete '" + deleteable + "' from the database?")
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

                RefreshBus.Publish(this, new RefreshObjectEventArgs(databaseObject){DeletedObjectDescendancy = CoreChildProvider.GetDescendancyListIfAnyFor(databaseObject)});

                return true;
            }

            return false;
        }
        
        public void RequestItemEmphasis(object sender, EmphasiseRequest request)
        {
            //ensure a relevant Toolbox is available
            var descendancy = CoreChildProvider.GetDescendancyListIfAnyFor(request.ObjectToEmphasise);
            object root = null;

            if (descendancy != null)
                root = descendancy.Parents.FirstOrDefault();
            else
                root = request.ObjectToEmphasise; //assume maybe o is a root object itself?

            if (root != null)
                _windowManager.ShowCollectionWhichSupportsRootObjectType(root);

            //really should be a listener now btw since we just launched the relevant Toolbox if it wasn't there before
            var h = Emphasise;
            if (h != null)
            {
                var args = new EmphasiseEventArgs(request);
                h(this, args);

                var content = args.FormRequestingActivation as DockContent;

                if(content != null)
                    content.Activate();
            }
        }

        public void ActivateLookupConfiguration(object sender, Catalogue catalogue,TableInfo optionalLookupTableInfo=null)
        {
            var t = Activate<LookupConfigurationUI, Catalogue>(catalogue);
            
            if(optionalLookupTableInfo != null)
                t.SetLookupTableInfo(optionalLookupTableInfo);
        }
        
        public void ViewFilterGraph(object sender,FilterGraphObjectCollection collection)
        {
            var aggFilter = collection.GetFilter() as AggregateFilter;

            //if it's a cohort set
            if(aggFilter != null && aggFilter.GetAggregate().IsCohortIdentificationAggregate)
            {
                var cohortAggregate = aggFilter.GetAggregate();
                //use this instead
                new ExecuteCommandViewCohortAggregateGraph(this, 
                    new CohortSummaryAggregateGraphObjectCollection(cohortAggregate,collection.GetGraph(),CohortSummaryAdjustment.WhereRecordsIn,aggFilter))
                    .Execute();

                return;
            }

            Activate<FilterGraphUI>(collection);
        }

        public void ActivateViewLog(LoadMetadata loadMetadata)
        {
            new ExecuteCommandViewLoadMetadataLogs(this).SetTarget(loadMetadata).Execute();
        }

        public IRDMPSingleDatabaseObjectControl ActivateViewLoadMetadataDiagram(object sender, LoadMetadata loadMetadata)
        {
            return Activate<LoadDiagramUI, LoadMetadata>(loadMetadata);
        }



        public bool IsRootObjectOfCollection(RDMPCollection collection, object rootObject)
        {
            //if the collection an arbitrary one then it is definetly not the root collection for anyone
            if (collection == RDMPCollection.None)
                return false;

            return _windowManager.GetCollectionForRootObject(rootObject) == collection;
        }

        /// <summary>
        /// Consults all currently configured IProblemProviders and returns true if any report a problem with the object
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public bool HasProblem(object model)
        {
            return ProblemProviders.Any(p => p.HasProblem(model));
        }

        /// <summary>
        /// Consults all currently configured IProblemProviders and returns the first Problem reported by any about the object or null
        /// if there are no problems reported.
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public string DescribeProblemIfAny(object model)
        {
            return ProblemProviders.Select(p => p.DescribeProblem(model)).FirstOrDefault(desc => desc != null);
        }

        /// <summary>
        /// Returns the root tree object which hosts the supplied object.  If the supplied object has no known descendancy it is assumed
        /// to be the root object itself so it is returned
        /// </summary>
        /// <param name="objectToEmphasise"></param>
        /// <returns></returns>
        public object GetRootObjectOrSelf(IMapsDirectlyToDatabaseTable objectToEmphasise)
        {
            var descendancy = CoreChildProvider.GetDescendancyListIfAnyFor(objectToEmphasise);

            if (descendancy != null && descendancy.Parents.Any())
                return descendancy.Parents[0];

            return objectToEmphasise;
        }

        public string GetDocumentation(Type type)
        {
            return RepositoryLocator.CatalogueRepository.CommentStore.GetTypeDocumentationIfExists(type);
        }

        public string CurrentDirectory { get { return Environment.CurrentDirectory; }}
        public DialogResult ShowDialog(Form form)
        {
            return form.ShowDialog();
        }

        public void KillForm(Form f, Exception reason)
        {
            f.Close();
            ExceptionViewer.Show("Window Closed",reason);
        }
        public void KillForm(Form f, string reason)
        {
            f.Close();
            ExceptionViewer.Show("Window Closed",reason);
        }
        public void OnRuleRegistered(IBinderRule rule)
        {
            //no special action required
        }

        /// <summary>
        /// Asks the user if they want to reload a fresh copy with a Yes/No message box.
        /// </summary>
        /// <param name="databaseEntity"></param>
        /// <returns></returns>
        public bool ShouldReloadFreshCopy(DatabaseEntity databaseEntity)
        {
            return YesNo(databaseEntity + " is out of date with database, would you like to reload a fresh copy?",
                           "Object Changed");
        }

        public T Activate<T, T2>(T2 databaseObject)
            where T : RDMPSingleDatabaseObjectControl<T2>, new()
            where T2 : DatabaseEntity
        {
            return Activate<T, T2>(databaseObject, (Bitmap)CoreIconProvider.GetImage(databaseObject));
        }
        
        public T Activate<T>(IPersistableObjectCollection collection)
            where T: Control,IObjectCollectionControl,new()

        {
            Control existingHostedControlInstance;
            if (PopExisting(typeof(T), collection, out existingHostedControlInstance))
                return (T)existingHostedControlInstance;

            var uiInstance = new T();
            Activate(uiInstance, collection);
            return uiInstance;
        }


        private T Activate<T, T2>(T2 databaseObject, Bitmap tabImage)
            where T : RDMPSingleDatabaseObjectControl<T2>, new()
            where T2 : DatabaseEntity
        {
            Control existingHostedControlInstance;
            if (PopExisting(typeof(T), databaseObject, out existingHostedControlInstance))
                return (T)existingHostedControlInstance;

            var uiInstance = new T();
            var floatable = WindowFactory.Create(this,RefreshBus, uiInstance, tabImage, databaseObject);
            floatable.Show(_mainDockPanel, DockState.Document);
            uiInstance.SetDatabaseObject(this, databaseObject);
            floatable.TabText = uiInstance.GetTabName();

            return uiInstance;
        }

        private bool PopExisting(Type windowType, IMapsDirectlyToDatabaseTable databaseObject, out Control existingHostedControlInstance)
        {
            var existing = _windowManager.GetActiveWindowIfAnyFor(windowType, databaseObject);
            existingHostedControlInstance = null;

            if (existing != null)
            {
                existingHostedControlInstance = existing.GetControl();
                existing.Activate();
                existing.HandleUserRequestingTabRefresh(this);
            }

            return existing != null;
        }

        private bool PopExisting(Type windowType, IPersistableObjectCollection collection, out Control existingHostedControlInstance)
        {
            var existing = _windowManager.GetActiveWindowIfAnyFor(windowType, collection);
            existingHostedControlInstance = null;

            if (existing != null)
            {
                existingHostedControlInstance = existing.GetControl();
                existing.Activate();
                existing.HandleUserRequestingTabRefresh(this);
            }

            return existing != null;
        }
        public DockContent Activate(DeserializeInstruction instruction)
        {
            if (instruction.DatabaseObject != null && instruction.ObjectCollection != null)
                throw new ArgumentException("DeserializeInstruction cannot have both a DatabaseObject and an ObjectCollection");

            var c = _constructor.Construct(instruction.UIControlType);

            var uiInstance = c as IRDMPSingleDatabaseObjectControl;
            var uiCollection = c as IObjectCollectionControl;

            //it has a database object so call SetDatabaseObject
            if (uiCollection != null)
                //if we get here then Instruction wasn't for a 
                return Activate(uiCollection, instruction.ObjectCollection);
            else
            if (uiInstance != null)
            {
                var databaseObject = instruction.DatabaseObject;

                //the database object is gone? deleted maybe
                if (databaseObject == null)
                    return null;

                DockContent floatable = WindowFactory.Create(this,RefreshBus, uiInstance,CoreIconProvider.GetImage(databaseObject), databaseObject);

                floatable.Show(_mainDockPanel, DockState.Document);
                try
                {
                    uiInstance.SetDatabaseObject(this,(DatabaseEntity) databaseObject);
                    floatable.TabText = uiInstance.GetTabName();
                }
                catch (Exception e)
                {
                    floatable.Close();
                    throw new Exception("SetDatabaseObject failed on Control of Type '"+instruction.UIControlType.Name+"', control closed, see inner Exception for details",e);
                }

                return floatable;
            }
            else
                throw new PersistenceException("DeserializeInstruction must have either a DatabaseObject or an ObjectCollection");
        }

        public PersistableObjectCollectionDockContent Activate(IObjectCollectionControl collectionControl, IPersistableObjectCollection objectCollection)
        {
            var floatable = WindowFactory.Create(this,collectionControl,objectCollection, null);
            floatable.Show(_mainDockPanel, DockState.Document);
            return floatable;
        }

        public void RefreshBus_RefreshObject(object sender, RefreshObjectEventArgs e)
        {
            RefreshProblemProviders();
        }

        private void RefreshProblemProviders()
        {
            foreach (IProblemProvider p in ProblemProviders)
                p.RefreshProblems(CoreChildProvider);
        }

        /// <inheritdoc/>
        public bool YesNo(string text,string caption)
        {
            return MessageBox.Show(text,caption,MessageBoxButtons.YesNo) == DialogResult.Yes;
        }
    }
}

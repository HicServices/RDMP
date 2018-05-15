using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using CatalogueLibrary.Data;
using CatalogueLibrary.Data.Aggregation;
using CatalogueLibrary.Data.Cohort;
using CatalogueLibrary.Data.Dashboarding;
using CatalogueLibrary.Data.DataLoad;
using CatalogueLibrary.Providers;
using CatalogueLibrary.Repositories;
using CatalogueManager.Collections;
using CatalogueManager.Collections.Providers;
using CatalogueManager.CommandExecution;
using CatalogueManager.CredentialsUIs;
using CatalogueManager.DashboardTabs;
using CatalogueManager.DataLoadUIs.ANOUIs.ANOTableManagement;
using CatalogueManager.DataLoadUIs.ANOUIs.PreLoadDiscarding;
using CatalogueManager.DataLoadUIs.LoadMetadataUIs.LoadDiagram;
using CatalogueManager.DataLoadUIs.LoadMetadataUIs.LoadProgressAndCacheUIs;
using CatalogueManager.DataViewing;
using CatalogueManager.DataViewing.Collections;
using CatalogueManager.ExtractionUIs;
using CatalogueManager.ExtractionUIs.FilterUIs;
using CatalogueManager.ExtractionUIs.JoinsAndLookups;
using CatalogueManager.Icons.IconProvision;
using CatalogueManager.Issues;
using CatalogueManager.ItemActivation;
using CatalogueManager.ItemActivation.Arranging;
using CatalogueManager.ItemActivation.Emphasis;
using CatalogueManager.MainFormUITabs.SubComponents;
using CatalogueManager.PluginChildProvision;
using CatalogueManager.Refreshing;
using CatalogueManager.SimpleDialogs;
using CatalogueManager.TestsAndSetup.ServicePropogation;
using CatalogueManager.Validation;
using CohortManager.CommandExecution.AtomicCommands;
using CohortManager.SubComponents;
using CohortManager.SubComponents.Graphs;
using CohortManagerLibrary.QueryBuilding;
using Dashboard.Automation;
using DataExportLibrary.Providers;
using DataExportManager.Icons.IconProvision;
using MapsDirectlyToDatabaseTable;
using MapsDirectlyToDatabaseTableUI;
using RDMPObjectVisualisation.Copying;
using ResearchDataManagementPlatform.WindowManagement.ContentWindowTracking.Persistence;
using ResearchDataManagementPlatform.WindowManagement.WindowArranging;
using ReusableLibraryCode.Checks;
using ReusableUIComponents;
using ReusableUIComponents.CommandExecution;
using WeifenLuo.WinFormsUI.Docking;

namespace ResearchDataManagementPlatform.WindowManagement
{
    /// <summary>
    /// Central class for RDMP main application, this class provides acceess to all the main systems in RDMP user interface such as Emphasis, the RefreshBus, Child 
    /// provision etc.  See IActivateItems for full details
    /// </summary>
    public class ContentWindowManager : IActivateItems, IRefreshBusSubscriber
    {
        public event EmphasiseItemHandler Emphasise;

        private readonly DockPanel _mainDockPanel;
        private readonly ToolboxWindowManager _toolboxWindowManager;

        public IRDMPPlatformRepositoryServiceLocator RepositoryLocator { get; set; }
        public WindowFactory WindowFactory { get; private set; }

        public ICoreIconProvider CoreIconProvider { get; private set; }

        public RefreshBus RefreshBus { get; private set; }
        public FavouritesProvider FavouritesProvider { get; private set; }

        public ICoreChildProvider CoreChildProvider { get; private set; }

        public RDMPDocumentationStore DocumentationStore;
        public List<IPluginUserInterface> PluginUserInterfaces { get; private set; }
        readonly UIObjectConstructor _constructor = new UIObjectConstructor();

        public IArrangeWindows WindowArranger { get; private set; }
        
        public ICheckNotifier GlobalErrorCheckNotifier { get; private set; }

        public ICommandFactory CommandFactory { get; private set; }
        public ICommandExecutionFactory CommandExecutionFactory { get; private set; }

        public List<IProblemProvider> ProblemProviders { get; private set; }

        public ContentWindowManager(RefreshBus refreshBus, DockPanel mainDockPanel, IRDMPPlatformRepositoryServiceLocator repositoryLocator, WindowFactory windowFactory, ToolboxWindowManager toolboxWindowManager, ICheckNotifier globalErrorCheckNotifier)
        {
            WindowFactory = windowFactory;
            _mainDockPanel = mainDockPanel;
            _toolboxWindowManager = toolboxWindowManager;
            GlobalErrorCheckNotifier = globalErrorCheckNotifier;
            RepositoryLocator = repositoryLocator;

            //Shouldn't ever change externally to your session so doesn't need constantly refreshed
            FavouritesProvider = new FavouritesProvider(this, repositoryLocator.CatalogueRepository);

            RefreshBus = refreshBus;

            ConstructPluginChildProviders();

            UpdateChildProviders();
            RefreshBus.BeforePublish += (s, e) => UpdateChildProviders();

            //handle custom icons from plugin user interfaces in which
            CoreIconProvider = new DataExportIconProvider(PluginUserInterfaces.ToArray());
            KeywordHelpTextListbox.HelpKeywordsIconProvider = CoreIconProvider;

            SelectIMapsDirectlyToDatabaseTableDialog.ImageGetter = (model)=> CoreIconProvider.GetImage(model);

            DocumentationStore = new RDMPDocumentationStore(RepositoryLocator);

            WindowArranger = new WindowArranger(this,_toolboxWindowManager,_mainDockPanel);
            
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
            
            string overrideConfirmationText = null;

            if (customMessageDeletable != null)
                overrideConfirmationText = "Are you sure you want to " +customMessageDeletable.GetDeleteMessage() +"?";

            //it has already been deleted before
            if (databaseObject != null && !databaseObject.Exists())
                return false;

            string idText = "";

            if (databaseObject != null)
                idText = " ID=" + databaseObject.ID;

            DialogResult result = MessageBox.Show(
                (overrideConfirmationText?? ("Are you sure you want to delete '" + deleteable + "' from the database?")) +Environment.NewLine + "(" + deleteable.GetType().Name + idText +")",
                "Delete " + deleteable.GetType().Name,
                MessageBoxButtons.YesNo);
            
            if (result == DialogResult.Yes)
            {
                deleteable.DeleteInDatabase();
                
                if (databaseObject == null)
                {
                    var descendancy = CoreChildProvider.GetDescendancyListIfAnyFor(deleteable);
                    if(descendancy != null)
                        databaseObject = descendancy.Parents.OfType<DatabaseEntity>().LastOrDefault();
                }

                if(databaseObject == null)
                    throw new NotSupportedException("IDeletable " + deleteable + " was not a DatabaseObject and it did not have a Parent in it's tree which was a DatabaseObject (DescendancyList)");
                
                RefreshBus.Publish(this, new RefreshObjectEventArgs(databaseObject));

                return true;
            }

            return false;
        }

        public bool DeleteControlFromDashboardWithConfirmation(object sender, DashboardControl controlToDelete)
        {
            var layout = controlToDelete.ParentLayout;
            if (DeleteWithConfirmation(this, controlToDelete))
            {

                RefreshBus.Publish(sender,new RefreshObjectEventArgs(layout));
                return true;
            }
            return false;
        }
        
        public IFilter AdvertiseCatalogueFiltersToUser(IContainer containerToImportOneInto, IFilter[] filtersThatCouldBeImported)
        {
            var wizard = new FilterImportWizard();
            return wizard.ImportOneFromSelection(containerToImportOneInto, filtersThatCouldBeImported);
        }

        public void ActivateCatalogueItemIssue(object sender, CatalogueItemIssue catalogueItemIssue)
        {
            Activate<IssueUI,CatalogueItemIssue>(catalogueItemIssue);
        }

        public void ActivateConvertColumnInfoIntoANOColumnInfo(ColumnInfo columnInfo)
        {
            Activate<ColumnInfoToANOTableConverterUI, ColumnInfo>(columnInfo);
        }

        public void ActivateSupportingDocument(object sender, SupportingDocument supportingDocument)
        {
            Activate<SupportingDocumentUI, SupportingDocument>(supportingDocument);
        }

        public void ActivateSupportingSqlTable(object sender, SupportingSQLTable supportingSQLTable)
        {
            Activate<SupportingSQLTableUI, SupportingSQLTable>(supportingSQLTable);
        }

        public void ActivateDataAccessCredentials(object sender, DataAccessCredentials dataAccessCredentials)
        {
            Activate<DataAccessCredentialsUI, DataAccessCredentials>(dataAccessCredentials);
        }

        public void ViewDataSample(IViewSQLAndResultsCollection collection)
        {
            Activate<ViewSQLAndResultsWithDataGridUI>(collection);
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
                _toolboxWindowManager.ShowCollectionWhichSupportsRootObjectType(root);

            //really should be a listener now btw since we just launched the relevant Toolbox if it wasn't there before
            if (Emphasise != null)
            {
                var args = new EmphasiseEventArgs(request);
                Emphasise(this, args);

                var content = args.FormRequestingActivation as DockContent;

                if(content != null)
                    content.Activate();
            }
        }

        public void ActivateLookupConfiguration(object sender, Catalogue catalogue,TableInfo optionalLookupTableInfo=null)
        {
            var t = Activate<LookupConfiguration, Catalogue>(catalogue);
            
            if(optionalLookupTableInfo != null)
                t.SetLookupTableInfo(optionalLookupTableInfo);
        }

        public void ActivateReOrderCatalogueItems(Catalogue catalogue)
        {
            Activate<ReOrderCatalogueItems, Catalogue>(catalogue);
        }

        public void ActivateConfigureValidation(object sender, Catalogue catalogue)
        {
            Activate<ValidationSetupForm, Catalogue>(catalogue);
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

            Activate<FilterGraph>(collection);
        }

        public void ActivateViewCohortIdentificationConfigurationSql(object sender, CohortIdentificationConfiguration cic)
        {
            Activate<ViewCohortIdentificationConfigurationUI, CohortIdentificationConfiguration>(cic);
        }

        public void ActivateViewLog(ExternalDatabaseServer loggingServer, int dataLoadRunID)
        {
            var log = Activate<SingleDataLoadLogView, ExternalDatabaseServer>(loggingServer);
            log.ShowDataLoadRunID(dataLoadRunID);
        }

        public IRDMPSingleDatabaseObjectControl ActivateViewLoadMetadataDiagram(object sender, LoadMetadata loadMetadata)
        {
            return Activate<LoadDiagram, LoadMetadata>(loadMetadata);
        }

        public void ActivateExternalDatabaseServer(object sender, ExternalDatabaseServer externalDatabaseServer)
        {
            Activate<ExternalDatabaseServerUI,ExternalDatabaseServer>(externalDatabaseServer);
        }

        public void ActivateTableInfo(object sender, TableInfo tableInfo)
        {
            Activate<TableInfoUI, TableInfo>(tableInfo);
        }

        public void ActivatePreLoadDiscardedColumn(object sender, PreLoadDiscardedColumn preLoadDiscardedColumn)
        {
            Activate<PreLoadDiscardedColumnUI, PreLoadDiscardedColumn>(preLoadDiscardedColumn);
        }
        

        public bool IsRootObjectOfCollection(RDMPCollection collection, object rootObject)
        {
            //if the collection an arbitrary one then it is definetly not the root collection for anyone
            if (collection == RDMPCollection.None)
                return false;

            return _toolboxWindowManager.GetCollectionForRootObject(rootObject) == collection;
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

        public DashboardLayoutUI ActivateDashboard(object sender, DashboardLayout dashboard)
        {
            return Activate<DashboardLayoutUI, DashboardLayout>(dashboard);
        }

        public T Activate<T, T2>(T2 databaseObject)
            where T : RDMPSingleDatabaseObjectControl<T2>, new()
            where T2 : DatabaseEntity
        {
            return Activate<T, T2>(databaseObject, (Bitmap)CoreIconProvider.GetImage(databaseObject));
        }
        
        public T Activate<T>(IPersistableObjectCollection collection)
            where T: IObjectCollectionControl,new()

        {
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
            var existing = WindowFactory.WindowTracker.GetActiveWindowIfAnyFor(windowType, databaseObject);
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
                    var setDatabaseObjectMethod = uiInstance.GetType().GetMethods().Where(m => 
                        m.Name.Equals("SetDatabaseObject") 
                        && m.DeclaringType == uiInstance.GetType()).ToArray();
                    
                    if(setDatabaseObjectMethod.Length == 0)
                        throw new Exception("Class did not have a method called SetDatabaseObject");

                    if (setDatabaseObjectMethod.Length > 1)
                        throw new AmbiguousMatchException("Class had "+setDatabaseObjectMethod.Length+" Generic methods called SetDatabaseObject");

                    setDatabaseObjectMethod[0].Invoke(uiInstance, new object[] { this, databaseObject });

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
    }
}

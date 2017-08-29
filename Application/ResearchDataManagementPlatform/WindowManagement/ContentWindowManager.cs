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
using CatalogueLibrary.Nodes;
using CatalogueLibrary.Providers;
using CatalogueLibrary.Repositories;
using CatalogueManager.AggregationUIs;
using CatalogueManager.AggregationUIs.Advanced;
using CatalogueManager.Collections.Providers;
using CatalogueManager.CredentialsUIs;
using CatalogueManager.DashboardTabs;
using CatalogueManager.DataLoadUIs.ANOUIs.ANOTableManagement;
using CatalogueManager.DataLoadUIs.LoadMetadataUIs;
using CatalogueManager.DataLoadUIs.LoadMetadataUIs.LoadDiagram;
using CatalogueManager.DataLoadUIs.LoadMetadataUIs.LoadProgressAndCacheUIs;
using CatalogueManager.DataLoadUIs.LoadMetadataUIs.ProcessTasks;
using CatalogueManager.DataQualityUIs;
using CatalogueManager.DataViewing;
using CatalogueManager.DataViewing.Collections;
using CatalogueManager.ExtractionUIs;
using CatalogueManager.ExtractionUIs.FilterUIs;
using CatalogueManager.ExtractionUIs.FilterUIs.ParameterUIs;
using CatalogueManager.ExtractionUIs.FilterUIs.ParameterUIs.Options;
using CatalogueManager.ExtractionUIs.JoinsAndLookups;
using CatalogueManager.Icons.IconProvision;
using CatalogueManager.Issues;
using CatalogueManager.ItemActivation.Arranging;
using CatalogueManager.ItemActivation.Emphasis;
using CatalogueManager.MainFormUITabs;
using CatalogueManager.PluginChildProvision;
using CatalogueManager.Refreshing;
using CatalogueManager.SimpleDialogs;
using CatalogueManager.TestsAndSetup.ServicePropogation;
using CatalogueManager.Validation;
using CohortManager.ItemActivation;
using CohortManager.SubComponents;
using CohortManager.SubComponents.Graphs;
using CohortManagerLibrary.QueryBuilding;
using Dashboard.Automation;
using Dashboard.CatalogueSummary;
using Dashboard.CatalogueSummary.LoadEvents;
using DataExportLibrary.Data.DataTables;
using DataExportLibrary.Data.LinkCreators;
using DataExportManager.CohortUI;
using DataExportManager.CohortUI.CohortSourceManagement;
using DataExportManager.Collections.Providers;
using DataExportManager.DataRelease;
using DataExportManager.Icons.IconProvision;
using DataExportManager.ItemActivation;
using DataExportManager.ProjectUI;
using DataExportManager.ProjectUI.Graphs;
using DatasetLoaderUI;
using MapsDirectlyToDatabaseTable;
using MapsDirectlyToDatabaseTableUI;
using RDMPStartup;
using ResearchDataManagementPlatform.WindowManagement.ContentWindowTracking.Persistence;
using ResearchDataManagementPlatform.WindowManagement.WindowArranging;
using ReusableUIComponents;
using WeifenLuo.WinFormsUI.Docking;

namespace ResearchDataManagementPlatform.WindowManagement
{
    public class ContentWindowManager : IActivateDataExportItems, IActivateCohortIdentificationItems
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

        public ContentWindowManager(RefreshBus refreshBus, DockPanel mainDockPanel, IRDMPPlatformRepositoryServiceLocator repositoryLocator, WindowFactory windowFactory, ToolboxWindowManager toolboxWindowManager)
        {
            WindowFactory = windowFactory;
            _mainDockPanel = mainDockPanel;
            _toolboxWindowManager = toolboxWindowManager;
            RepositoryLocator = repositoryLocator;

            //Shouldn't ever change externally to your session so doesn't need constantly refreshed
            FavouritesProvider = new FavouritesProvider(this, repositoryLocator.CatalogueRepository);

            RefreshBus = refreshBus;

            ConstructPluginChildProviders();

            UpdateChildProviders();
            RefreshBus.BeforePublish += (s, e) => UpdateChildProviders();
            
            CoreIconProvider = new DataExportIconProvider(PluginUserInterfaces.Where(i=>!i.Exceptions.Any()).ToArray());
            SelectIMapsDirectlyToDatabaseTableDialog.ImageGetter = (model)=> CoreIconProvider.GetImage(model);

            DocumentationStore = new RDMPDocumentationStore(RepositoryLocator);

            WindowArranger = new WindowArranger(this,_toolboxWindowManager,_mainDockPanel);

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
                    ExceptionViewer.Show("Problem occured trying to load Plugin '" + pluginType.Name +"'",  e);
                }
            }
        }

        private void UpdateChildProviders()
        {   
            //prefer a linked repository with both
            if(RepositoryLocator.DataExportRepository != null)
                try
                {
                    CoreChildProvider = new DataExportChildProvider(RepositoryLocator, PluginUserInterfaces.ToArray());
                    if (CoreChildProvider.Exceptions.Any())
                        WideMessageBox.Show("The following Exceptions occurred during Child Finding:" + 
                            String.Join(Environment.NewLine,CoreChildProvider.Exceptions)
                            );

                    return;
                }
                catch (Exception e)
                {
                    ExceptionViewer.Show(e);
                }
            
            //there was an error generating a data export repository or there was no repository specified

            //so just create a catalogue one
            CoreChildProvider = new CatalogueChildProvider(RepositoryLocator.CatalogueRepository, PluginUserInterfaces.ToArray());

            if(CoreChildProvider.Exceptions.Any())
                ExceptionViewer.Show("The following Exceptions occurred during Child Finding", new AggregateException(CoreChildProvider.Exceptions.ToArray()));
            
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

        public bool AllowExecute { get { return true; }}

        public void ActivateCatalogue(object sender, Catalogue c)
        {
            Activate<CatalogueTab,Catalogue>(c);
        }

        public void ActivateViewCatalogueExtractionSql(object sender,Catalogue c)
        {
            Activate<ViewExtractionSql,Catalogue>(c, CatalogueIcons.SQL);
        }

        public void ActivateLoadMetadata(object sender, LoadMetadata lmd)
        {
            Activate<LoadMetadataUI,LoadMetadata>(lmd);
        }
        
        public void ExecuteLoadMetadata(object sender, LoadMetadata lmd)
        {
            Activate<DatasetLoadControl, LoadMetadata>(lmd, CatalogueIcons.ExecuteArrow);
        }

        public bool DeleteWithConfirmation(object sender, IDeleteable deleteable, string overrideConfirmationText = null)
        {

            DialogResult result = MessageBox.Show(
                overrideConfirmationText??
                ("Are you sure you want to delete " + deleteable + " from the database?"), "Delete Record", MessageBoxButtons.YesNo);
            
            if (result == DialogResult.Yes)
            {
                deleteable.DeleteInDatabase();

                var j = deleteable as JoinInfo;
                if (j != null)
                    RefreshBus.Publish(this, new RefreshObjectEventArgs(j.PrimaryKey));//JoinInfo is a special snowflake in that it is IDeletable and basically a database object but lacks an ID so isn't IMapsDirectlyToDatabaseTable
                else
                    RefreshBus.Publish(this,new RefreshObjectEventArgs((DatabaseEntity)deleteable));
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

        public void ActivateViewLoadMetadataLog(object sender, LoadMetadata loadMetadata)
        {
            Activate<AllLoadEventsUI,LoadMetadata>(loadMetadata, CatalogueIcons.Logging);
        }

        public void ActivateExtractionFilterParameterSet(object sender, ExtractionFilterParameterSet parameterSet)
        {
            Activate<ExtractionFilterParameterSetUI,ExtractionFilterParameterSet>(parameterSet);
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
            Activate<ConvertColumnInfoIntoANOColumnInfo, ColumnInfo>(columnInfo);
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

        public void ActivateParameterNode(object sender, ParametersNode parameters)
        {
            var parameterCollectionUI = new ParameterCollectionUI();

            ParameterCollectionUIOptionsFactory factory = new ParameterCollectionUIOptionsFactory();
            var options = factory.Create(parameters.Collector);
            parameterCollectionUI.SetUp(options);

            ShowWindow(parameterCollectionUI, true);
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

        public void ActivateJoinInfoConfiguration(object sender, TableInfo tableInfo)
        {
            Activate<JoinConfiguration, TableInfo>(tableInfo);
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
                ExecuteCohortSummaryGraph(sender, new CohortSummaryAggregateGraphObjectCollection(cohortAggregate,collection.GetGraph(),CohortSummaryAdjustment.WhereRecordsIn,aggFilter));
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


        public void ActivateCohortIdentificationConfiguration(object sender, CohortIdentificationConfiguration cic)
        {
            Activate<CohortIdentificationConfigurationUI,CohortIdentificationConfiguration>(cic);
        }

        public void ExecuteCohortIdentificationConfiguration(object sender, CohortIdentificationConfiguration cic)
        {
            Activate<ExecuteCohortIdentificationConfigurationUI,CohortIdentificationConfiguration>(cic,CatalogueIcons.ExecuteArrow);
        }

        public void ActivateProcessTask(object sender, ProcessTask processTask)
        {
            if(processTask.IsPluginType())
                Activate<PluginProcessTaskUI,ProcessTask>(processTask);

            if (processTask.ProcessTaskType == ProcessTaskType.Executable)
                Activate<ExeProcessTaskUI, ProcessTask>(processTask);

            if (processTask.ProcessTaskType == ProcessTaskType.SQLFile)
                Activate<SqlProcessTaskUI, ProcessTask>(processTask);
        }

        public void ActivateExecuteDQE(object sender, Catalogue catalogue)
        {
            Activate<DQEExecutionControl, Catalogue>(catalogue);
        }

        public void ActivateLoadProgress(object sender, LoadProgress loadProgress)
        {
            Activate<LoadProgressUI, LoadProgress>(loadProgress);
        }

        public IRDMPSingleDatabaseObjectControl ActivateViewLoadMetadataDiagram(object sender, LoadMetadata loadMetadata)
        {
            return Activate<LoadDiagram, LoadMetadata>(loadMetadata);
        }

        public void ExecuteCohortSummaryGraph(object sender,CohortSummaryAggregateGraphObjectCollection objectCollection)
        {
            Activate<CohortSummaryAggregateGraph>(objectCollection);
        }

        public void ExecuteExtractionExtractionAggregateGraph(object sender, ExtractionAggregateGraphObjectCollection objectCollection)
        {
            Activate<ExtractionAggregateGraph>(objectCollection);
        }

        public void ActivateDQEResultViewing(object sender, Catalogue c)
        {
            Activate<CatalogueSummaryScreen,Catalogue>(c,CatalogueIcons.DQE);
        }

        public void ActivateExternalCohortTable(object sender, ExternalCohortTable externalCohortTable)
        {
            Activate<ExternalCohortTableUI, ExternalCohortTable>(externalCohortTable);
        }

        public void ActivateCohort(object sender, ExtractableCohort cohort)
        {
            Activate<ExtractableCohortUI,ExtractableCohort>(cohort,CatalogueIcons.ExtractableCohort);
        }

        public void ActivateProject(object sender, Project project)
        {
            Activate<ProjectUI,Project>(project);
        }

        public void ActivateExtractionConfiguration(object sender, ExtractionConfiguration config)
        {
            Activate<ExtractionConfigurationUI,ExtractionConfiguration>(config);
        }

        public void ExecuteExtractionConfiguration(object sender, ExecuteExtractionUIRequest request)
        {
            var ui = Activate<ExecuteExtractionUI,ExtractionConfiguration>(request.ExtractionConfiguration, CatalogueIcons.ExecuteArrow);
            ui.SetRequest(request);
        }

        public void ExecuteRelease(object sender, Project project)
        {
            Activate<DataReleaseUI, Project>(project,CatalogueIcons.Release);
        }

        public void ActivateEditExtractionConfigurationDataset(SelectedDataSets selectedDataSets)
        {
            Activate<ConfigureDatasetUI,SelectedDataSets>(selectedDataSets);
        }

        public void ActivateViewExtractionSQL(object sender, SelectedDataSets selectedDataSet)
        {
            Activate<ViewExtractionConfigurationSQLUI, SelectedDataSets>(selectedDataSet);
        }

        public void ActivateCatalogueItem(object sender, CatalogueItem cataItem)
        {
            Activate<CatalogueItemTab,CatalogueItem>(cataItem);
        }

        public void ActivateExtractionInformation(object sender, ExtractionInformation extractionInformation)
        {
            Activate<ExtractionInformationUI, ExtractionInformation>(extractionInformation);
        }

        public void ActivateFilter(object sender, ConcreteFilter filter)
        {
            Activate<ExtractionFilterUI,ConcreteFilter>(filter,CatalogueIcons.Filter);
        }

        public void ActivateAggregate(object sender, AggregateConfiguration aggregate)
        {
            Activate<AggregateEditor,AggregateConfiguration>(aggregate);
        }

        public void ExecuteAggregate(object sender, AggregateConfiguration aggregate)
        {
            var graph = Activate<AggregateGraph, AggregateConfiguration>(aggregate,CatalogueIcons.Graph);
            graph.LoadGraphAsync();
        }

        public DashboardLayoutUI ActivateDashboard(object sender, DashboardLayout dashboard)
        {
            return Activate<DashboardLayoutUI, DashboardLayout>(dashboard);
        }

        private T Activate<T, T2>(T2 databaseObject)
            where T : RDMPSingleDatabaseObjectControl<T2>, new()
            where T2 : DatabaseEntity
        {
            return Activate<T, T2>(databaseObject, (Bitmap)CoreIconProvider.GetImage(databaseObject));
        }
        
        private T Activate<T>(IPersistableObjectCollection collection)
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

    }
}

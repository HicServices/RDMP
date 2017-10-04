using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using BrightIdeasSoftware;
using CatalogueLibrary.Data;
using CatalogueLibrary.Data.Aggregation;
using CatalogueLibrary.Data.Cohort;
using CatalogueLibrary.Data.Cohort.Joinables;
using CatalogueLibrary.Nodes;
using CatalogueManager.Icons.IconProvision;
using CatalogueManager.ItemActivation;
using CatalogueManager.ItemActivation.Emphasis;
using CatalogueManager.TestsAndSetup.ServicePropogation;
using CohortManager.ItemActivation;
using CohortManager.SubComponents.EmptyLineElements;
using CohortManager.SubComponents.Graphs;
using CohortManagerLibrary;
using CohortManagerLibrary.Execution;
using CohortManagerLibrary.Execution.Joinables;
using CohortManagerLibrary.QueryBuilding;
using MapsDirectlyToDatabaseTable;
using QueryCaching.Aggregation;
using QueryCaching.Aggregation.Arguments;
using ReusableLibraryCode.DatabaseHelpers.Discovery;
using ReusableUIComponents;
using ReusableUIComponents.SingleControlForms;


namespace CohortManager.SubComponents
{
    /// <summary>
    /// Cohort identification in the RDMP is done by assembling patient sets and applying set operations on these sets (See 'Cohort Generation' in UserManual.docx).  For a use case of
    /// cohort identification see CohortIdentificationConfigurationUI.
    /// 
    /// The cohort identification requirements of researchers can sometimes be very complicated and so the RDMP is designed to help you split down the requirements into manageable bite
    /// sized pieces (Sets).
    /// 
    /// Start by identifying the first dataset you will need to interrogate (e.g. if they want to know about diabetic medications drag in 'Prescribing').  Next double click the set
    /// and configure appropriate filters (See AggregateConfigurationUI) do not change the Dimension (this should already be the patient identifier).  Finally once you have configured
    /// the correct filters you should rename your set (AggregateConfiguration) to have a name that reflects the filters (e.g. 'People who have been prescribed a diabetic medication).
    /// 
    /// Next identify the next dataset you need to interrogate (e.g. if they want to exclude patients who have a 'Biochemistry' test result of 'CREATANINE' > 100)  create this set as 
    /// you did above.  
    /// 
    /// Then set the root container to EXCEPT such that your configuration is the first set of patients excluding the second set of patients.
    /// 
    /// There are 3 set operations:
    ///  
    /// UNION - All patients in any of the sets (e.g. patients prescribed opiates UNION patients who have attended a drug rehabilitation clinic outpatient appointment)
    /// INTERSECT - Only patients who are in all the sets (e.g. patients prescribed opiates WHO HAVE ALSO attended a drug rehabilitation clinic)
    /// EXCEPT - All patients in the first set throwing out any that are in subsequent sets (e.g. patients prescribed opiates EXCEPT those who have attended a drug rehabilitation clinic)
    /// 
    /// Once you have configured your sets / set operations click 'Start All Tasks' to launch the SQL queries in parallel to the server.  If a set or container fails you can right click
    /// it to view the SQL error message or just look at the SQL the system has generated and run that manually (e.g. in Sql Management Studio). 
    /// 
    /// Once some of your sets are executing correctly you can improve performance by caching the identifier lists 'Cache Selected' (See QueryCachingServerSelector for how this is 
    /// implemented).
    /// 
    /// You will see an Identifier Count for each set, this is the number of unique patient identifiers amongst all records returned by the query.  Selecting a set will allow you to
    /// see an extract of the rows that matched the filters (See CohortIdentificationExecutionResultsUI)
    /// 
    /// Ticking 'Include Cumulative Totals' will give you a second total for each set that is in a container with at least 1 other set, this is the number of unique identifiers after
    /// performing the set operation e.g.
    /// 
    /// Except
    /// 
    /// People in Tayside
    /// 
    /// Dead People
    /// 
    /// will give you 3 totals:
    /// 
    /// 1. Total number of people who live in Tayside
    /// 
    /// 2. Total number of people who are dead across all healthboards
    /// 
    /// 3. The number of people in set 1 that are not in set 2 (because of the EXCEPT)
    /// 
    /// 
    /// </summary>
    public partial class CohortCompilerUI : CohortCompilerUI_Design,IConsultableBeforeClosing
    {
        public event EventHandler SelectionChanged;
        private CohortIdentificationConfiguration _configuration;
        private CohortAggregateContainer _root;

        private CohortCompiler Compiler = new CohortCompiler(null);
        private ExternalDatabaseServer QueryCachingServer;
        
        private ISqlParameter[] _globals;

        public EventHandler ConfigurationChanged;

        public object SelectedObject
        {
            get { return tlvConfiguration.SelectedObject; }
        }

        public CohortIdentificationConfiguration Configuration
        {
            get { return _configuration; }
            private set
            {
                //they changed what we are pointing at so refresh from db
                _configuration = value;

                if (value != null && value.QueryCachingServer_ID != null)
                    QueryCachingServer =
                        value.QueryCachingServer;
                else
                    QueryCachingServer = null;

                Compiler.CohortIdentificationConfiguration = value;

                RefreshUIFromDatabase();
            }
        }


        public CohortCompilerUI()
        {
            InitializeComponent();

            if(VisualStudioDesignMode)
                return;

            Compiler.TaskCompleted += CompilerOnTaskCompleted;

            RefreshUIFromDatabase();

            tlvConfiguration.CanExpandGetter += CanExpandGetter;
            tlvConfiguration.ChildrenGetter += ChildrenGetter;
            olvAggregate.ImageGetter += ImageGetter;
            tlvConfiguration.RowFormatter += RowFormatter;
            
            refreshThreadCountPeriodically.Start();

            ddOptimisation.DataSource = Enum.GetValues(typeof(OptimisationStrategy));
            ddOptimisation.SelectedItem = OptimisationStrategy.BasicOptimisation;

            tlvConfiguration.RowHeight = 19;

            _cohortUnionImage = CatalogueIcons.UNIONCohortAggregate;
            _cohortIntersectImage = CatalogueIcons.INTERSECTCohortAggregate;
            _cohortExceptImage = CatalogueIcons.EXCEPTCohortAggregate;
        }


        private void RowFormatter(OLVListItem olvItem)
        {
            if(olvItem.RowObject is JoinableCollectionNode || olvItem.RowObject is CohortIdentificationHeader)
            {
                olvItem.BackColor = Color.Black;
                olvItem.ForeColor = Color.White;
            }

            var compileable = olvItem.RowObject as Compileable;
            var selectedContainer = tlvConfiguration.SelectedObject as AggregationContainerTask;
            if (compileable != null && selectedContainer != null && compileable.ParentContainerIfAny != null)
            {
                if (compileable.ParentContainerIfAny.Equals(selectedContainer.Container))
                {
                    olvItem.BackColor = Color.LightCyan;
                }

            }
        }
        private object ImageGetter(object rowObject)
        {
            if (CoreIconProvider == null)
                return null;

            if (rowObject is AggregationTask)
                return GetImageForCompileable((Compileable)rowObject, CoreIconProvider.GetImage(((AggregationTask)rowObject).Aggregate));

            if (rowObject is AggregationContainerTask)
                return GetImageForCompileable((Compileable)rowObject, CoreIconProvider.GetImage(((AggregationContainerTask) rowObject).Container));

            var joinable = rowObject as JoinableTaskExecution;
            if (joinable != null)
                return joinable.IsUnused ? CatalogueIcons.Warning : CatalogueIcons.CohortAggregate;

            return null;
        }

        public ICoreIconProvider CoreIconProvider { get; set; }

        private readonly Bitmap _cohortUnionImage;
        private readonly Bitmap _cohortIntersectImage;
        private readonly Bitmap _cohortExceptImage;

        private Bitmap GetImageForCompileable(Compileable compileable,Bitmap basicImage)
        {
            if (compileable.IsFirstInContainer.HasValue && !compileable.IsFirstInContainer.Value)
            {
                //we are not the first in our container 
                switch (compileable.ParentContainerIfAny.Operation)
                {
                    case SetOperation.UNION:
                        return CombineImages(_cohortUnionImage,basicImage);
                    case SetOperation.INTERSECT:
                        return CombineImages(_cohortIntersectImage,basicImage);
                    case SetOperation.EXCEPT:
                        return CombineImages(_cohortExceptImage,basicImage);
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            return basicImage;
        }

        private Bitmap CombineImages(Bitmap image1, Bitmap image2)
        {
            if (image1.Height != image2.Height)
                throw new Exception("Images must be the same height, image1 was height " + image1.Height +" and image2 height was " + image2.Height);

            var newImage = new Bitmap(image1.Width + image2.Width, image1.Height);
            var g = Graphics.FromImage(newImage);

            g.DrawImage(image1,0,0);
            g.DrawImage(image2,image1.Width,0);
            return newImage;
        }


        private void RefreshUIFromDatabase()
        {
            if (VisualStudioDesignMode || RepositoryLocator == null)
                return;

            var item = (OptimisationStrategy)ddOptimisation.SelectedItem;

            switch (item)
            {
                case OptimisationStrategy.NoOptimisation:
                    RecreateAllTasks();
                    break;
                case OptimisationStrategy.BasicOptimisation:
                    using (RepositoryLocator.CatalogueRepository.SuperCachingMode())//Use super caching mode on
                        RecreateAllTasks();
                    break;
                case OptimisationStrategy.PreCacheOptimisation:
                    using (RepositoryLocator.CatalogueRepository.SuperCachingMode(new[] { typeof(AggregateDimension), typeof(ColumnInfo), typeof(TableInfo), typeof(ExtractionInformation), typeof(AnyTableSqlParameter), typeof(AggregateConfiguration), typeof(AggregateFilterContainer), typeof(AggregateFilterParameter)}))//Use super caching mode on if user has ticked the optimise button
                        RecreateAllTasks();
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            if (ConfigurationChanged != null)
                ConfigurationChanged(this, new EventArgs());
        }

        private void RecreateAllTasks()
        {

            Compiler.CancelAllTasks(false);

            tlvConfiguration.ClearObjects();

            if (Configuration == null)
                return;

            //Do not allow autocaching if there isn't a cache server
            if (Configuration.QueryCachingServer_ID == null)
            {
                cbAutoCache.Checked = false;
                cbAutoCache.Enabled = false;
            }
            else
                cbAutoCache.Enabled = true;

            tlvConfiguration.Enabled = true;

            Configuration.CreateRootContainerIfNotExists();
            //if there is no root container,create one
            _root = Configuration.RootCohortAggregateContainer;
            _globals = Configuration.GetAllParameters();

            //Could have configured/unconfigured a joinable state
            foreach (var j in Compiler.Tasks.Keys.OfType<JoinableTaskExecution>())
                j.RefreshIsUsedState();

            try
            {
                tlvConfiguration.AddObject(new CohortIdentificationHeader());
                tlvConfiguration.AddObject(new JoinableCollectionNode(Configuration,Configuration.GetAllJoinables()));
                tlvConfiguration.ExpandAll();
            }
            catch (Exception e)
            {
                tlvConfiguration.Enabled = false;
                ExceptionViewer.Show("Failed to populate tree of Tasks", e);
            }
        }

        #region children getting
        private bool CanExpandGetter(object model)
        {
            var container = model as AggregationContainerTask;
            if (container != null)
                return container.SubContainers.Any() || container.ContainedConfigurations.Any();

            var joinCollection = model as JoinableCollectionNode;

            if (joinCollection != null)
                return joinCollection.Joinables.Any();

            if (model is CohortIdentificationHeader)
                return true;

            return false;
        }
        private IEnumerable ChildrenGetter(object model)
        {
            var containerTask = model as AggregationContainerTask;
            if (containerTask != null)
            {
                //ensure we listen for state change on the root
                return containerTask.Container.GetOrderedContents().Cast<IMapsDirectlyToDatabaseTable>().Select(c => Compiler.GetTask(c, _globals)).ToArray();
            }

            var joinableColection = model as JoinableCollectionNode;
            if (joinableColection != null)
                return Configuration.GetAllJoinables().Select(j=>Compiler.GetTask(j,_globals)).ToArray();

            if (model is CohortIdentificationHeader)
                return new[] {Compiler.GetTask(_root, _globals)};

            return null;
        }


        #endregion
        
        private void otvConfiguration_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Delete)
                DeleteSelectedItem();
        }

        private void DeleteSelectedItem()
        {

            try
            {
                var containerTask = tlvConfiguration.SelectedObject as AggregationContainerTask;

                if (containerTask != null)
                {
                    if (Configuration.RootCohortAggregateContainer_ID ==
                        ((IMapsDirectlyToDatabaseTable)containerTask.Container).ID)
                    {
                        MessageBox.Show("Cannot delete root container");
                        return;
                    }

                    DialogResult result =
                        MessageBox.Show("Are you sure you want to delete " + containerTask.Container + " from the database?",
                            "Delete Record",
                            MessageBoxButtons.YesNo);

                    if (result == DialogResult.Yes)
                    {
                        containerTask.Container.DeleteInDatabase();
                        RefreshUIFromDatabase();
                    }
                }

                var configurationTask = tlvConfiguration.SelectedObject as AggregationTask;

                if (configurationTask != null)
                {
                    //it is likely they don't actually want to nuke the entire aggregate but instead just stop using it in this particular cohort generation activity
                    containerTask = (AggregationContainerTask)tlvConfiguration.GetParent(configurationTask);
                    containerTask.Container.RemoveChild(configurationTask.Aggregate);

                    RefreshUIFromDatabase();
                }

                var joinableTask = tlvConfiguration.SelectedObject as JoinableTaskExecution;

                if (joinableTask != null)
                {
                    ((IDeleteable)joinableTask.Child).DeleteInDatabase(); //will probably fail if anyone is using it?
                    RefreshUIFromDatabase();
                }

            }
            catch (Exception ex)
            {
                ExceptionViewer.Show(ex);
            }
        }


        private void otvConfiguration_CellRightClick(object sender, CellRightClickEventArgs e)
        {
            try
            {
                var containerTask = e.HitTest.RowObject as AggregationContainerTask;
                var configurationTask = e.HitTest.RowObject as AggregationTask;
                var joinableTask = e.HitTest.RowObject as JoinableTaskExecution;

                ICompileable compileable = e.HitTest.RowObject as ICompileable;
                ICachableTask cachableTask = e.HitTest.RowObject as ICachableTask;

            
                //create right click context menu
                var RightClickMenu = new ContextMenuStrip();

                if (containerTask != null)
                    if (containerTask.State == CompilationState.Crashed)
                        RightClickMenu.Items.Add("View Crash Message", null,(o, args) => ExceptionViewer.Show(containerTask.CrashMessage));

                //if it's a joinable or regular aggregate
                if (configurationTask != null || joinableTask != null)
                {
                    if (compileable.State == CompilationState.Crashed)
                        RightClickMenu.Items.Add("View Crash Message", null,
                            (o, args) => ExceptionViewer.Show(compileable.CrashMessage));

                    if (configurationTask != null)
                        AddSummaryGraphOptions(configurationTask, RightClickMenu, cachableTask);
                }

                if (cachableTask != null)
                {
                    var cachingMenuItem = RightClickMenu.Items.Add("Save To Cache", null, (o, args) =>
                    {
                        //cache the task
                        RightClickMenu_SaveToCache(cachableTask);
                        //and refresh the interface
                        RefreshUIFromDatabase();
                    });
                
                    CachedAggregateConfigurationResultsManager manager = null;

                    if(QueryCachingServer != null)
                        manager = new CachedAggregateConfigurationResultsManager(QueryCachingServer);

                    var clearCachingMenuItem = RightClickMenu.Items.Add("Clear From Cache", null, (o, args) =>
                    {
                        //clear the one you right clicked
                        cachableTask.ClearYourselfFromCache(manager);
                        //and refresh the interface
                        RefreshUIFromDatabase();
                    });

                    cachingMenuItem.Enabled =
                        QueryCachingServer != null //there must be a caching server
                        && cachableTask.State == CompilationState.Finished //execution must have finished
                        && cachableTask.IsCacheableWhenFinished();//it can't already be cached

                    clearCachingMenuItem.Enabled = 
                        QueryCachingServer != null //there must be a caching server
                        && cachableTask.CanDeleteCache(); //it must have been cached
                }


                if (compileable != null)
                {
                    var startThisTaskOnly = RightClickMenu.Items.Add("Start This Task Only", null, (o, args) => StartThisTaskOnly(compileable));
                    startThisTaskOnly.Enabled = compileable.State == CompilationState.NotScheduled || compileable.State == CompilationState.Crashed;

                    //if it's a container task
                    if (containerTask != null)
                    {
                        AddSummaryGraphOptions(containerTask.Container,RightClickMenu,containerTask);
                    }

                }
            
                if(RightClickMenu.Items.Count >0)
                    RightClickMenu.Show(tlvConfiguration,e.Location);
            }
            catch (Exception exception)
            {
                ExceptionViewer.Show(exception);
            }
        }

        private void AddSummaryGraphOptions(CohortAggregateContainer cohortAggregateContainer, ContextMenuStrip rightClickMenu, AggregationContainerTask containerTask)
        {
            var repository = cohortAggregateContainer.Repository;
            var availableGraphs = repository.GetAllObjects<AggregateConfiguration>().Where(g => !g.IsCohortIdentificationAggregate).ToArray();
            var allCatalogues = repository.GetAllObjectsInIDList<Catalogue>(availableGraphs.Select(g => g.Catalogue_ID).Distinct()).ToArray();

            var heading1 = new ToolStripMenuItem("Generate Summary Graph (ExtractionIdentifier IN)",CatalogueIcons.Graph);

            if (availableGraphs.Any())
                foreach (var cata in allCatalogues.OrderBy(c => c.Name))
                {
                    var catalogueSubheading = new ToolStripMenuItem(cata.Name, CatalogueIcons.Catalogue);

                    int cId = cata.ID;
                    foreach (var graph in availableGraphs.Where(g => g.Catalogue_ID == cId))
                    {
                        AggregateConfiguration toExecute = graph;
                        var menuItem = new ToolStripMenuItem(graph.Name, null,
                            (s, e) => LaunchSummaryGraph(cohortAggregateContainer, toExecute));
                        catalogueSubheading.DropDownItems.Add(menuItem);
                    }

                    heading1.DropDownItems.Add(catalogueSubheading);
                }

            rightClickMenu.Items.Add(heading1);
            heading1.Enabled = containerTask.AreaAllQueriesCached();

            if (!heading1.Enabled)
                heading1.Text += " (Requires 100% cached)";


        }


        private void AddSummaryGraphOptions(AggregationTask configurationTask, ContextMenuStrip rightClickMenu, ICachableTask cachableTask)
        {
            var cohort = (AggregateConfiguration) configurationTask.Child;
            var compatibleAggregates = CohortSummaryQueryBuilder.GetAllCompatibleSummariesForCohort(cohort);

            if (compatibleAggregates.Any())
            {
                var heading1 = new ToolStripMenuItem("Generate Summary Graph (ExtractionIdentifier IN)",CatalogueIcons.Graph);
                var heading2 = new ToolStripMenuItem("Generate Summary Graph (Records IN)", CatalogueIcons.Graph);

                foreach (AggregateConfiguration aggregate in compatibleAggregates)
                {
                    AggregateConfiguration aggregate1 = aggregate;
                    var item = new ToolStripMenuItem(aggregate.ToString(), CatalogueIcons.CohortAggregate,
                        (o, args) =>
                            LaunchSummaryGraph(cohort,aggregate1, CohortSummaryAdjustment.WhereExtractionIdentifiersIn));
                    heading1.DropDownItems.Add(item);

                    var item2 = new ToolStripMenuItem(aggregate.ToString(), CatalogueIcons.CohortAggregate,
                        (o, args) => LaunchSummaryGraph(cohort, aggregate1, CohortSummaryAdjustment.WhereRecordsIn));
                    heading2.DropDownItems.Add(item2);
                }

                //LaunchAll
                if (compatibleAggregates.Length > 1)
                {
                    var item = new ToolStripMenuItem("Launch All Graphs", null,
                        (o, args) =>
                            LaunchAllSummaryGraphs(cohort,compatibleAggregates,CohortSummaryAdjustment.WhereExtractionIdentifiersIn));
                    heading1.DropDownItems.Add(item);

                    var item2 = new ToolStripMenuItem("Launch All Graphs", null,
                        (o, args) =>
                            LaunchAllSummaryGraphs(cohort,compatibleAggregates, CohortSummaryAdjustment.WhereRecordsIn));
                    heading2.DropDownItems.Add(item2);
                }

                rightClickMenu.Items.Add(heading1);
                rightClickMenu.Items.Add(heading2);


                if (!cachableTask.CanDeleteCache())
                {
                    heading1.Text += " (Requires Cached Query)";
                    heading1.Enabled = false;
                }
            }
        }

        private void LaunchAllSummaryGraphs(AggregateConfiguration cohort, AggregateConfiguration[] compatibleAggregates, CohortSummaryAdjustment adjustment)
        {
            foreach (AggregateConfiguration aggregateConfiguration in compatibleAggregates)
                LaunchSummaryGraph(cohort, aggregateConfiguration, adjustment);
        }

        private void LaunchSummaryGraph( AggregateConfiguration cohort,AggregateConfiguration aggregate, CohortSummaryAdjustment adjustment)
        {
            ((IActivateCohortIdentificationItems)_activator).ExecuteCohortSummaryGraph(this,new CohortSummaryAggregateGraphObjectCollection(cohort, aggregate,adjustment));
        }
        
        private void LaunchSummaryGraph(CohortAggregateContainer cohortAggregateContainer, AggregateConfiguration graph)
        {
            ((IActivateCohortIdentificationItems)_activator).ExecuteCohortSummaryGraph(this, new CohortSummaryAggregateGraphObjectCollection(cohortAggregateContainer, graph));
        }
        private void StartThisTaskOnly(ICompileable configurationTask)
        {

            //if it is in crashed state
            if(configurationTask.State == CompilationState.Crashed)
            {
                //Cancel the task and remove it from the Compilers task list - so it no longer knows about it
                Compiler.CancelTask(configurationTask,true);

                //refresh the task list, this will pick up the orphaned .Child and create a new task for it in the Compiler
                RefreshUIFromDatabase();

                //fetch the new task for the child and make that the one we start (below)
                configurationTask = Compiler.Tasks.Single(t => t.Key.Child.Equals(configurationTask.Child)).Key;
            }


            //Task is now in state NotScheduled so we can start it
            Compiler.LaunchSingleTask(configurationTask,_timeout);
        }


        private void RightClickMenu_SaveToCache(ICachableTask configurationTask)
        {
            try
            {
                CacheConfigurationResults(configurationTask);
            }
            catch (Exception exception)
            {
                ExceptionViewer.Show(exception);
            }
        }
        
        private void CacheConfigurationResults(ICachableTask cachable)
        {
            CachedAggregateConfigurationResultsManager manager = new CachedAggregateConfigurationResultsManager(QueryCachingServer);
            
            var explicitTypes = new List<DatabaseColumnRequest>();
            
            AggregateConfiguration configuration = cachable.GetAggregateConfiguration();
            try
            {
                ColumnInfo identifierColumnInfo = configuration.AggregateDimensions.Single(c=>c.IsExtractionIdentifier).ColumnInfo;
                explicitTypes.Add(new DatabaseColumnRequest(identifierColumnInfo.GetRuntimeName(), identifierColumnInfo.Data_type));
            }
            catch (Exception e)
            {
                throw new Exception("Error occurred trying to find the data type of the identifier column when attempting to submit the result data table to the cache",e);
            }

            CacheCommitArguments args = cachable.GetCacheArguments(Compiler.Tasks[cachable].CountSQL, Compiler.Tasks[cachable].Identifiers, explicitTypes.ToArray());

            manager.CommitResults(args);
        }
  

        private void otvConfiguration_ItemActivate(object sender, EventArgs e)
        {
            var containerTask = tlvConfiguration.SelectedObject as AggregationContainerTask;
            
            if(containerTask != null && containerTask.CrashMessage != null)
                ExceptionViewer.Show(containerTask.CrashMessage);

            var c = tlvConfiguration.SelectedObject as Compileable;

            if(c!= null)
            {
                var child = c.Child;

                var joinable = child as JoinableCohortAggregateConfiguration;
                if (joinable != null)
                    child = joinable.AggregateConfiguration;

                if (child != null)
                    _activator.RequestItemEmphasis(this,new EmphasiseRequest(child));
            }
        }


        private void otvConfiguration_SelectionChanged(object sender, EventArgs e)
        {
            UpdateRunnablesButtons();

            tlvConfiguration.RefreshObjects(tlvConfiguration.Objects.OfType<Compileable>().ToList());

        }

        private void UpdateRunnablesButtons()
        {
            if (Configuration == null)
                return;
            
            var selectedRunnables = GetRunableTasks(true).ToArray();
            //if user has selected at least 1 unscheduled task then we can schedule it
            btnStartSelected.Enabled = selectedRunnables.Any();
            btnStartSelected.Text = string.Format("Selected ({0})", selectedRunnables.Length);

            //if there is no caching 
            if (Configuration.QueryCachingServer_ID == null)
            {
                //disable the cache selected tasks button
                btnCacheSelected.Enabled = false;
                return;
            }

            //caching is supported but... are there any selected aggregates that could be cached
            var cachables = GetSelectedCachableTasks().ToArray();
            btnCacheSelected.Enabled = cachables.Any();
            btnCacheSelected.Text = string.Format("Cache ({0})", cachables.Length);

            var clearables = GetCacheClearableTasks(true).ToArray();
            btnClearCacheForSelected.Enabled = clearables.Any();
            btnClearCacheForSelected.Text = string.Format("Clear ({0})", clearables.Length);

            btnClearCacheAll.Enabled = GetCacheClearableTasks(false).Any();
        }

        private void otvConfiguration_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (SelectionChanged != null)
                SelectionChanged(sender, e);
        }

        private IEnumerable<ICachableTask> GetSelectedCachableTasks()
        {
            return
                tlvConfiguration.SelectedObjects.OfType<ICachableTask>()
                    .Where(t => t.State == CompilationState.Finished && t.IsCacheableWhenFinished());
        }
        private IEnumerable<ICachableTask> GetCacheClearableTasks(bool selectedOnly)
        {
            if (selectedOnly)
                return tlvConfiguration.SelectedObjects.OfType<ICachableTask>().Where(t =>!t.IsCacheableWhenFinished());

            return Compiler.Tasks.Keys.OfType<ICachableTask>().Where(t => !t.IsCacheableWhenFinished());
        }
        private IEnumerable<ICompileable> GetRunableTasks(bool selectedOnly)
        {
            if(selectedOnly)
                return tlvConfiguration.SelectedObjects.OfType<ICompileable>().Where(t => t.State == CompilationState.NotScheduled);

            return Compiler.Tasks.Keys.Where(t => t.State == CompilationState.NotScheduled);
        }
        public CohortIdentificationTaskExecution GetSelectedTaskExecutionResultsIfAny()
        {
            var task = SelectedObject as ICompileable;
            if(task != null)
                if (Compiler.Tasks.ContainsKey(task))
                    return Compiler.Tasks[task];

            return null;
        }

        public string GetSelectedTaskSQLIfAny()
        {
            var task = SelectedObject as ICompileable;
            if(task != null)
                if(Compiler.Tasks.ContainsKey(task))
                    return Compiler.Tasks[task].CountSQL;

            return null;
        }

        private int _timeout = 300;

        private void tbTimeout_TextChanged(object sender, EventArgs e)
        {
            try
            {
                _timeout = int.Parse(tbTimeout.Text);
                tbTimeout.ForeColor = Color.Black;
            }
            catch (Exception)
            {
                _timeout = 30;
                tbTimeout.ForeColor = Color.Red;
            }
        }

        private void btnStartAll_Click(object sender, EventArgs e)
        {
            Reset();

            Compiler.LaunchScheduledTasksAsync(_timeout);
        }


        private void btnCancel_Click(object sender, EventArgs e)
        {
            Compiler.CancelAllTasks(false);
        }

        private void Reset()
        {
            Compiler.CancelAllTasks(true);
            RefreshUIFromDatabase();
        }

        private void refreshThreadCountPeriodically_Tick(object sender, EventArgs e)
        {
            UpdateRunnablesButtons();
            tlvConfiguration.RebuildColumns();
            lblThreadCount.Text = "Thread Count:" + Compiler.GetAliveThreadCount();

            //If we are overdue for a reset due to AutoCache
            if (asyncRefreshIsOverdue && Compiler.GetAliveThreadCount() == 0)
            {
                asyncRefreshIsOverdue = false;
                RefreshUIFromDatabase();
            }
        }
        
        private void btnCacheSelected_Click(object sender, EventArgs e)
        {
            int successes = 0;
            foreach (ICachableTask t in GetSelectedCachableTasks())
                try
                {
                    CacheConfigurationResults(t);
                    successes ++;
                }
                catch (Exception exception)
                {
                    ExceptionViewer.Show("Could not cache task "+ t,exception);
                }

            RefreshUIFromDatabase();
        }
        
        private void btnStartSelected_Click(object sender, EventArgs e)
        {
            foreach (ICompileable runnable in GetRunableTasks(true))
                Compiler.LaunchSingleTask(runnable, _timeout);
        }

        private void btnClearCacheForSelected_Click(object sender, EventArgs e)
        {
            ClearCacheFor(GetCacheClearableTasks(true).ToArray());
        }

        private void btnClearCacheAll_Click(object sender, EventArgs e)
        {
            var toClear = GetCacheClearableTasks(false).ToArray();

            if(MessageBox.Show("Are you sure you want to clear " + toClear.Length + " cached results?","Confirm clearing cache",MessageBoxButtons.YesNo) == DialogResult.Yes)
                ClearCacheFor(toClear);
        }
        private void ClearCacheFor(ICachableTask[] tasks)
        {

            var manager = new CachedAggregateConfigurationResultsManager(QueryCachingServer);

            int successes = 0;
            foreach (ICachableTask t in tasks)
                try
                {
                    t.ClearYourselfFromCache(manager);
                    Compiler.CancelTask(t, true);
                    successes++;
                }
                catch (Exception exception)
                {
                    ExceptionViewer.Show("Could not clear cache for task " + t, exception);
                }

            RefreshUIFromDatabase();
        }



        private void cbIncludeCumulative_CheckedChanged(object sender, EventArgs e)
        {
            Compiler.IncludeCumulativeTotals = cbIncludeCumulative.Checked;
        }

        

        //Because AutoCache is an even handler for completion of Compiler tasks we are launching Cache Saves but these will que up potentially or come in as Tasks finish executing (possibly over as much time as half an hour).  This means that we need
        //to schedule a reset everything so that all Tasks repoint themselves at the cached results location but if we try to reset as we go along then it will result in the cancelling of other tasks executing within the Compiler.  So what we need to
        //do is set this flag to true which means in our Thread polling code we can see when there are 0 executing threads and trigger the reset to get everyone pointing at the cache.
        private bool asyncRefreshIsOverdue = false;
        

        private void CompilerOnTaskCompleted(object sender, ICompileable completedTask)
        {
            var cacheable = completedTask as ICachableTask;

            if (cbAutoCache.Checked && cacheable != null && cacheable.State == CompilationState.Finished && cacheable.IsCacheableWhenFinished())
            {
                RightClickMenu_SaveToCache(cacheable);
                asyncRefreshIsOverdue = true;
            }
        }

        enum OptimisationStrategy
        {
            NoOptimisation,
            BasicOptimisation,
            PreCacheOptimisation
        }

        public void Refresh(IMapsDirectlyToDatabaseTable triggeringObject)
        {
            RefreshUIFromDatabase();
        }

        public void RefreshAll()
        {
            RefreshUIFromDatabase();
        }

        public override void SetDatabaseObject(IActivateItems activator, CohortIdentificationConfiguration databaseObject)
        {
            base.SetDatabaseObject(activator,databaseObject);
            Configuration = databaseObject;
            CoreIconProvider = activator.CoreIconProvider;
        }

        public void ConsultAboutClosing(object sender, FormClosingEventArgs e)
        {
            if (Compiler != null)
            {
                var aliveCount = Compiler.GetAliveThreadCount();
                if (aliveCount > 0)
                {
                    MessageBox.Show("There are " + aliveCount +
                                    " Tasks currently executing, you must cancel them before closing");
                    e.Cancel = true;
                }
            }
                
            
        }
    }

    [TypeDescriptionProvider(typeof(AbstractControlDescriptionProvider<CohortCompilerUI_Design, UserControl>))]
    public abstract class CohortCompilerUI_Design : RDMPSingleDatabaseObjectControl<CohortIdentificationConfiguration>
    {
    }
}

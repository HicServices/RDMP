using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.Eventing.Reader;
using System.Drawing;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.VisualStyles;
using BrightIdeasSoftware;
using CatalogueLibrary.Data;
using CatalogueLibrary.Data.Aggregation;
using CatalogueLibrary.Data.Cohort;
using CatalogueLibrary.Data.Cohort.Joinables;
using CatalogueLibrary.Nodes;
using CatalogueManager.Icons.IconProvision;
using CatalogueManager.ItemActivation;
using CatalogueManager.ItemActivation.Emphasis;
using CatalogueManager.Refreshing;
using CatalogueManager.TestsAndSetup.ServicePropogation;
using CohortManager.CommandExecution.AtomicCommands;
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
    public partial class 
        CohortCompilerUI : CohortCompilerUI_Design, IRefreshBusSubscriber
    {
        private CohortAggregateContainer _root;
        private CohortIdentificationConfiguration _cic;
        private CohortCompiler Compiler = new CohortCompiler(null);
        private ExternalDatabaseServer _queryCachingServer;

        private int _timeout = 3000;

        private ISqlParameter[] _globals;
        
        public CohortCompilerUI()
        {
            InitializeComponent();

            if(VisualStudioDesignMode)
                return;

            tlvConfiguration.CanExpandGetter += CanExpandGetter;
            tlvConfiguration.ChildrenGetter += ChildrenGetter;
            olvAggregate.ImageGetter += ImageGetter;
            tlvConfiguration.RowFormatter += RowFormatter;
            olvIdentifierCount.AspectGetter += RowCountAspectGetter;
            refreshThreadCountPeriodically.Start();

            tlvConfiguration.RowHeight = 19;

            _cohortUnionImage = CatalogueIcons.UNIONCohortAggregate;
            _cohortIntersectImage = CatalogueIcons.INTERSECTCohortAggregate;
            _cohortExceptImage = CatalogueIcons.EXCEPTCohortAggregate;
        }

        #region Layout, Children Getting, Appearance etc
        private object RowCountAspectGetter(object rowObject)
        {
            var compileable = rowObject as Compileable;

            if (compileable != null && compileable.State == CompilationState.Finished)
                return compileable.FinalRowCount;

            return null;
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

            var joinable = rowObject as JoinableTask;
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
                return containerTask.Container.GetOrderedContents().Cast<IMapsDirectlyToDatabaseTable>().Select(c => Compiler.AddTask(c, _globals)).ToArray();
            }

            var joinableColection = model as JoinableCollectionNode;
            if (joinableColection != null)
                return _cic.GetAllJoinables().Select(j => Compiler.AddTask(j, _globals)).ToArray();

            if (model is CohortIdentificationHeader)
                return new[] { Compiler.AddTask(_root, _globals) };

            return null;
        }

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


        #endregion

        private bool _haveSubscribed = false;
        private CohortCompilerRunner _runner;

        public override void SetDatabaseObject(IActivateItems activator, CohortIdentificationConfiguration databaseObject)
        {
            _cic = databaseObject;

            base.SetDatabaseObject(activator, databaseObject);

            if (!_haveSubscribed)
            {
                activator.RefreshBus.Subscribe(this);
                _haveSubscribed = true;
            }

            _queryCachingServer = _cic.QueryCachingServer;
            Compiler.CohortIdentificationConfiguration = _cic;
            CoreIconProvider = activator.CoreIconProvider;
            RecreateAllTasks();
        }

        public void RefreshBus_RefreshObject(object sender, RefreshObjectEventArgs e)
        {
            var descendancy = _activator.CoreChildProvider.GetDescendancyListIfAnyFor(e.Object);

            //if publish event was for a child of the cic (_cic is in the objects descendancy i.e. it sits below our cic)
            if (descendancy != null && descendancy.Parents.Contains(_cic))
                RecreateAllTasks();
        }
        
        /// <summary>
        /// Rebuilds the CohortCompiler diagram which shows all the currently configured tasks
        /// </summary>
        /// <param name="cancelTasks"></param>
        private void RecreateAllTasks(bool cancelTasks = true)
        {
            if (cancelTasks)
                Compiler.CancelAllTasks(false);

            tlvConfiguration.ClearObjects();
            
            tlvConfiguration.Enabled = true;

            _cic.CreateRootContainerIfNotExists();
            //if there is no root container,create one
            _root = _cic.RootCohortAggregateContainer;
            _globals = _cic.GetAllParameters();

            //Could have configured/unconfigured a joinable state
            foreach (var j in Compiler.Tasks.Keys.OfType<JoinableTask>())
                j.RefreshIsUsedState();

            try
            {
                tlvConfiguration.AddObject(new CohortIdentificationHeader());
                tlvConfiguration.AddObject(new JoinableCollectionNode(_cic, _cic.GetAllJoinables()));
                tlvConfiguration.ExpandAll();
            }
            catch (Exception e)
            {
                tlvConfiguration.Enabled = false;
                ExceptionViewer.Show("Failed to populate tree of Tasks", e);
            }
        }
        
        public ICompileable GetTaskIfExists(IMapsDirectlyToDatabaseTable o)
        {
            return Compiler.Tasks.Keys.SingleOrDefault(t => t.Child.Equals(o));
        }

        public ICompileable[] GetAllTasks()
        {
            return Compiler.Tasks.Keys.ToArray();
        }


        public void StartThisTaskOnly(IMapsDirectlyToDatabaseTable configOrContainer)
        {
            var task = Compiler.AddTask(configOrContainer, _globals);

            //Cancel the task and remove it from the Compilers task list - so it no longer knows about it
            Compiler.CancelTask(task, true);
            
            RecreateAllTasks();

            task = Compiler.AddTask(configOrContainer, _globals);

            //Task is now in state NotScheduled so we can start it
            Compiler.LaunchSingleTask(task, _timeout);
        }

        public void CancelAll()
        {
            Compiler.CancelAllTasks(true);
            RecreateAllTasks();
        }
        
        public void StartAll()
        {
            //only allow starting all if we are not mid execution already
            if (IsExecutingGlobalOperations())
                return;
            
            _runner = new CohortCompilerRunner(Compiler,_timeout);
            _runner.PhaseChanged += RunnerOnPhaseChanged;
            new Task(() =>
            {
                try
                {
                    _runner.Run();
                }
                catch (Exception e)
                {
                    ExceptionViewer.Show(e);
                }

            }).Start();
        }

        private void RunnerOnPhaseChanged(object sender, EventArgs eventArgs)
        {
            if (InvokeRequired)
            {
                Invoke(new MethodInvoker(() => RunnerOnPhaseChanged(sender, eventArgs)));
                return;
            }
            
            lblExecuteAllPhase.Text = _runner.ExecutionPhase.ToString();
            RecreateAllTasks(false);
        }

        public bool IsExecutingGlobalOperations()
        {
            return _runner != null && _runner.ExecutionPhase != CohortCompilerRunner.Phase.None && _runner.ExecutionPhase != CohortCompilerRunner.Phase.Finished;
        }

        public void Cancel(IMapsDirectlyToDatabaseTable o)
        {
            var task = Compiler.Tasks.Single(t=>t.Key.Child.Equals(o));
            Compiler.CancelTask(task.Key,true);
        }

        private void refreshThreadCountPeriodically_Tick(object sender, EventArgs e)
        {
            tlvConfiguration.RebuildColumns();
            lblThreadCount.Text = "Thread Count:" + Compiler.GetAliveThreadCount();
        }

        public CompilationState GetState(IMapsDirectlyToDatabaseTable o)
        {
            var task = GetTaskIfExists(o);

            if (task == null)
                return CompilationState.NotScheduled;

            return task.State;
        }

        private void ClearCacheFor(ICacheableTask[] tasks)
        {
            var manager = new CachedAggregateConfigurationResultsManager(_queryCachingServer);

            int successes = 0;
            foreach (ICacheableTask t in tasks)
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

            RecreateAllTasks();
        }

        private void cbIncludeCumulative_CheckedChanged(object sender, EventArgs e)
        {
            Compiler.IncludeCumulativeTotals = cbIncludeCumulative.Checked;
        }
        
        public override void ConsultAboutClosing(object sender, FormClosingEventArgs e)
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
        
        public void Clear(IMapsDirectlyToDatabaseTable o)
        {
            var task = GetTaskIfExists(o);

            if(task == null)
                return;

            var c = task as CacheableTask;
            if(c != null)
                ClearCacheFor(new ICacheableTask[] { c });

            Compiler.CancelTask(task,true);
        }

        public bool AnyCachedTasks()
        {
            return Compiler.Tasks.Keys.OfType<ICacheableTask>().Any(t => !t.IsCacheableWhenFinished());
        }

        public void ClearAllCaches()
        {
            ClearCacheFor(Compiler.Tasks.Keys.OfType<ICacheableTask>().Where(t => !t.IsCacheableWhenFinished()).ToArray());
        }

        private void tlvConfiguration_ItemActivate(object sender, EventArgs e)
        {
            var o = tlvConfiguration.SelectedObject as ICompileable;
            if (o != null && o.CrashMessage != null)
            {
                ExceptionViewer.Show(o.CrashMessage);
            }
        }
    }

    [TypeDescriptionProvider(typeof(AbstractControlDescriptionProvider<CohortCompilerUI_Design, UserControl>))]
    public abstract class CohortCompilerUI_Design : RDMPSingleDatabaseObjectControl<CohortIdentificationConfiguration>
    {
    }
}

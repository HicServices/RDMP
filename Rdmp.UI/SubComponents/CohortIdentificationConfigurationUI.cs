// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using BrightIdeasSoftware;
using MapsDirectlyToDatabaseTable;
using Rdmp.Core.CohortCreation;
using Rdmp.Core.CohortCreation.Execution;
using Rdmp.Core.CohortCreation.Execution.Joinables;
using Rdmp.Core.CommandExecution.AtomicCommands;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Curation.Data.Aggregation;
using Rdmp.Core.Curation.Data.Cohort;
using Rdmp.Core.Curation.Data.Cohort.Joinables;
using Rdmp.Core.Icons.IconProvision;
using Rdmp.Core.Providers.Nodes;
using Rdmp.Core.QueryCaching.Aggregation;
using Rdmp.UI.Collections;
using Rdmp.UI.CommandExecution.AtomicCommands;
using Rdmp.UI.CommandExecution.AtomicCommands.CohortCreationCommands;
using Rdmp.UI.Icons.IconProvision;
using Rdmp.UI.ItemActivation;
using Rdmp.UI.SimpleControls;
using Rdmp.UI.SimpleDialogs;
using Rdmp.UI.SubComponents.EmptyLineElements;
using Rdmp.UI.TestsAndSetup.ServicePropogation;
using ReusableLibraryCode.Icons.IconProvision;


namespace Rdmp.UI.SubComponents
{
    /// <summary>
    /// Allows you to view/edit a CohortIdentificationConfiguration.  You should start by giving it a meaningful name e.g. 'Project 132 Cases - Deaths caused by diabetic medication' 
    /// and a comprehensive description e.g. 'All patients in Tayside and Fife who are over 16 at the time of their first prescription of a diabetic medication (BNF chapter 6.1) 
    /// and died within 6 months'.  An accurate up-to-date description will help future data analysts to understand the configuration.
    /// 
    /// <para>If you have a large data repository or plan to use lots of different datasets or complex filters in your CohortIdentificationCriteria you should configure a caching database
    /// from the dropdown menu.</para>
    /// 
    /// <para>Next you should add datasets and set operations (<see cref="CohortAggregateContainer"/>) either by right clicking or dragging and dropping into the tree view</para>
    /// 
    /// <para>In the above example you might have </para>
    /// 
    /// <para>Set 1 - Prescribing</para>
    /// 
    /// <para>    Filter 1 - Prescription is for a diabetic medication</para>
    /// 
    /// <para>    Filter 2 - Prescription is the first prescription of it's type for the patient</para>
    /// 
    /// <para>    Filter 3 - Patient died within 6 months of prescription</para>
    /// 
    /// <para>INTERSECT</para>
    /// 
    /// <para>Set 2 - Demography</para>
    ///     
    /// <para>    Filter 1 - Latest known healthboard is Tayside or Fife</para>
    /// 
    /// <para>    Filter 2 - Date of Death - Date of Birth > 16 years</para>
    ///  
    /// </summary>
    public partial class CohortIdentificationConfigurationUI : CohortIdentificationConfigurationUI_Design
    {
        private CohortIdentificationConfiguration _configuration;

        ToolStripMenuItem _miClearCache = new ToolStripMenuItem("Clear Cached Records");

        ToolStripMenuItem cbIncludeCumulative = new ToolStripMenuItem("Calculate Cumulative Totals") { CheckOnClick = true };


        private RDMPCollectionCommonFunctionality _commonFunctionality;
        private ExternalDatabaseServer _queryCachingServer;
        private CohortAggregateContainer _root;
        CancellationTokenSource _cancelGlobalOperations;
        private ISqlParameter[] _globals;
        private CohortCompilerRunner _runner;
        
        readonly ToolStripTimeout _timeoutControls = new ToolStripTimeout() { Timeout = 3000 };

        public CohortCompiler Compiler { get; }

        public CohortIdentificationConfigurationUI()
        {
            InitializeComponent();

            Compiler = new CohortCompiler(null);

            olvExecute.IsButton = true;
            olvExecute.ButtonSizing = OLVColumn.ButtonSizingMode.CellBounds;
            tlvCic.RowHeight = 19;
            olvExecute.AspectGetter += ExecuteAspectGetter;
            tlvCic.ButtonClick += tlvCic_ButtonClick;
            olvOrder.AspectGetter += (o)=> o is JoinableCollectionNode ? null : o is ParametersNode ? null : (o as IOrderable)?.Order;
            olvOrder.IsEditable = false;
            AssociatedCollection = RDMPCollection.Cohort;

            
            olvCount.AspectGetter += Count_AspectGetter;
            olvCached.AspectGetter = Cached_AspectGetter;

            _miClearCache.Click += MiClearCacheClick;
            _miClearCache.Image = CatalogueIcons.ExternalDatabaseServer_Cache;

            cbIncludeCumulative.CheckedChanged += (s, e) => Compiler.IncludeCumulativeTotals = cbIncludeCumulative.Checked;

            //This is important, OrderableComparer ensures IOrderable objects appear in the correct order but the comparator
            //doesn't get called unless the column has a sorting on it
            olvNameCol.Sortable = true;
            tlvCic.Sort(olvNameCol);
        }

        private object Cached_AspectGetter(object rowobject)
        {
            var key = Compiler?.Tasks?.Keys.FirstOrDefault(k => k.Child.Equals(rowobject));
            
            if (key != null)
                return _configuration.QueryCachingServer_ID == null ? "No Cache" : key.GetCachedQueryUseCount();
            
            return null;
        }

        private object Count_AspectGetter(object rowObject)
        {
            var key = Compiler?.Tasks?.Keys.FirstOrDefault(k => k.Child.Equals(rowObject));
            
            if (key != null && key.State == CompilationState.Finished)
                return key.FinalRowCount;

            return null;
        }


        void CohortCompilerUI1_SelectionChanged(IMapsDirectlyToDatabaseTable obj)
        {
            var joinable = obj as JoinableCohortAggregateConfiguration;
            
            tlvCic.SelectedObject = joinable != null ? joinable.AggregateConfiguration : obj;
        }

        public override void SetDatabaseObject(IActivateItems activator, CohortIdentificationConfiguration databaseObject)
        {
            base.SetDatabaseObject(activator,databaseObject);
            _configuration = databaseObject;

            lblFrozen.Visible = _configuration.Frozen;

            tbName.Text = _configuration.Name;
            tbDescription.Text = _configuration.Description;
            ticket.TicketText = _configuration.Ticket;
            tlvCic.Enabled = !databaseObject.Frozen;
            
            if (_commonFunctionality == null)
            {
                _commonFunctionality = new RDMPCollectionCommonFunctionality();

                _commonFunctionality.SetUp(RDMPCollection.Cohort, tlvCic, activator, olvNameCol, olvNameCol, new RDMPCollectionCommonFunctionalitySettings
                {
                    AddFavouriteColumn = false,
                    AddCheckColumn = false,
                    AllowPinning = false,
                    AllowSorting =  true, //important, we need sorting on so that we can override sort order with our OrderableComparer
                });

                tlvCic.AddObject(_configuration);
                tlvCic.ExpandAll();
            }

            CommonFunctionality.AddToMenu(cbIncludeCumulative);
            CommonFunctionality.AddToMenu(new ToolStripSeparator());
            CommonFunctionality.AddToMenu(_miClearCache);
            CommonFunctionality.AddToMenu(new ExecuteCommandSetQueryCachingDatabase(Activator, _configuration));
            CommonFunctionality.AddToMenu(new ExecuteCommandCreateNewQueryCacheDatabase(activator, _configuration));
            CommonFunctionality.AddToMenu(
                new ExecuteCommandSet(activator, _configuration, _configuration.GetType().GetProperty("Description"))
                {
                    OverrideCommandName = "Edit Description",
                    OverrideIcon =
                        Activator.CoreIconProvider.GetImage(RDMPConcept.CohortIdentificationConfiguration, OverlayKind.Edit)
                });
            CommonFunctionality.AddToMenu(new ToolStripSeparator());
            CommonFunctionality.AddToMenu(new ExecuteCommandShowXmlDoc(activator, "CohortIdentificationConfiguration.QueryCachingServer_ID", "Query Caching"), "Help (What is Query Caching)");

            CommonFunctionality.Add(new ExecuteCommandCreateNewCohortByExecutingACohortIdentificationConfiguration(activator, null).SetTarget(_configuration),
                "Commit Cohort",
                activator.CoreIconProvider.GetImage(RDMPConcept.ExtractableCohort,OverlayKind.Add));
            
            foreach (var c in _timeoutControls.GetControls())
                CommonFunctionality.Add(c);

            _queryCachingServer = _configuration.QueryCachingServer;
            Compiler.CohortIdentificationConfiguration = _configuration;
            Compiler.CoreChildProvider = activator.CoreChildProvider;
            RecreateAllTasks();
        }

        public override void SetItemActivator(IActivateItems activator)
        {
            base.SetItemActivator(activator);
            ticket.SetItemActivator(activator);
        }

        public override string GetTabName()
        {
            return "Execute:" + base.GetTabName();
        }
        
        private void ticket_TicketTextChanged(object sender, EventArgs e)
        {
            _configuration.Ticket = ticket.TicketText;
        }

        private object ExecuteAspectGetter(object rowObject)
        {
            //don't expose any buttons if global execution is in progress
            if (IsExecutingGlobalOperations())
                return null;

            if (rowObject is AggregateConfiguration || rowObject is CohortAggregateContainer)
            {
                var plannedOp = GetNextOperation(GetState((IMapsDirectlyToDatabaseTable)rowObject));

                if (plannedOp == Operation.None)
                    return null;

                return plannedOp;
            }

            return null;
        }
        public bool IsExecutingGlobalOperations()
        {
            return _runner != null && _runner.ExecutionPhase != CohortCompilerRunner.Phase.None && _runner.ExecutionPhase != CohortCompilerRunner.Phase.Finished;
        }


        /// <summary>
        /// Rebuilds the CohortCompiler diagram which shows all the currently configured tasks
        /// </summary>
        /// <param name="cancelTasks"></param>
        private void RecreateAllTasks(bool cancelTasks = true)
        {
            if (cancelTasks)
                Compiler.CancelAllTasks(false);
            
            _configuration.CreateRootContainerIfNotExists();
            //if there is no root container,create one
            _root = _configuration.RootCohortAggregateContainer;
            _globals = _configuration.GetAllParameters();

            //Could have configured/unconfigured a joinable state
            foreach (var j in Compiler.Tasks.Keys.OfType<JoinableTask>())
                j.RefreshIsUsedState();

        }
        private Operation GetNextOperation(CompilationState currentState)
        {
            switch (currentState)
            {
                case CompilationState.NotScheduled:
                    return Operation.Execute;
                case CompilationState.Scheduled:
                    return Operation.None;
                case CompilationState.Executing:
                    return Operation.Cancel;
                case CompilationState.Finished:
                    return Operation.Execute;
                case CompilationState.Crashed:
                    return Operation.Execute;
                default:
                    throw new ArgumentOutOfRangeException("currentState");
            }
        }

        #region Job control
        private enum Operation
        {
            Execute,
            Cancel,
            Clear,
            None
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
                else
                {
                    Compiler.CancelAllTasks(true);
                }
            }
        }

        private CompilationState GetState(IMapsDirectlyToDatabaseTable o)
        {
            var task = GetTaskIfExists(o);

            if (task == null)
                return CompilationState.NotScheduled;

            return task.State;
        }
        public ICompileable GetTaskIfExists(IMapsDirectlyToDatabaseTable o)
        {
            return Compiler.Tasks.Keys.SingleOrDefault(t => t.Child.Equals(o));
        }


        void tlvCic_ButtonClick(object sender, CellClickEventArgs e)
        {
            var o = e.Model;
            var aggregate = o as AggregateConfiguration;
            var container = o as CohortAggregateContainer;

            if (aggregate != null)
            {
                var joinable = aggregate.JoinableCohortAggregateConfiguration;
                                    
                if(joinable != null)
                    OrderActivity(GetNextOperation(GetState(joinable)), joinable);
                else
                    OrderActivity(GetNextOperation(GetState(aggregate)), aggregate);
            }
            if (container != null)
            {
                OrderActivity(GetNextOperation(GetState(container)),container);
            }
        }

        private void OrderActivity(Operation operation, IMapsDirectlyToDatabaseTable o)
        {
            switch (operation)
            {
                case Operation.Execute:
                    StartThisTaskOnly(o);
                    break;
                case Operation.Cancel:
                    Cancel(o);
                    break;
                case Operation.Clear:
                    Clear(o);
                    break;
                case Operation.None:
                    break;
                default:
                    throw new ArgumentOutOfRangeException("operation");
            }
        }

        private void StartThisTaskOnly(IMapsDirectlyToDatabaseTable configOrContainer)
        {
            var task = Compiler.AddTask(configOrContainer, _globals);
            if (task.State == CompilationState.Crashed)
            {
                ExceptionViewer.Show("Task failed to build",task.CrashMessage);
                return;
            }
            //Cancel the task and remove it from the Compilers task list - so it no longer knows about it
            Compiler.CancelTask(task, true);
            
            RecreateAllTasks(false);

            task = Compiler.AddTask(configOrContainer, _globals);

            //Task is now in state NotScheduled so we can start it
            Compiler.LaunchSingleTask(task, _timeoutControls.Timeout,true);
        }
        
        public void Cancel(IMapsDirectlyToDatabaseTable o)
        {
            var task = Compiler.Tasks.Single(t=>t.Key.Child.Equals(o));
            Compiler.CancelTask(task.Key,true);
        }

        public void CancelAll()
        {
            //don't start any more global operations if your midway through
            if(_cancelGlobalOperations != null)
                _cancelGlobalOperations.Cancel();

            Compiler.CancelAllTasks(true);
            RecreateAllTasks();
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
        
        public void ClearAllCaches()
        {
            ClearCacheFor(Compiler.Tasks.Keys.OfType<ICacheableTask>().Where(t => !t.IsCacheableWhenFinished()).ToArray());
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

        public void StartAll()
        {
            //only allow starting all if we are not mid execution already
            if (IsExecutingGlobalOperations())
                return;

            _cancelGlobalOperations = new CancellationTokenSource();


            _runner = new CohortCompilerRunner(Compiler, _timeoutControls.Timeout);
            _runner.PhaseChanged += RunnerOnPhaseChanged;
            new Task(() =>
            {
                try
                {
                    _runner.Run(_cancelGlobalOperations.Token);
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

        private void btnExecute_Click(object sender, EventArgs e)
        {
            StartAll();
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            UpdateButtonStates();
        }

        private void UpdateButtonStates()
        {
            var plan = PlanGlobalOperation();

            btnExecute.Enabled = plan == Operation.Execute;
            btnAbortLoad.Enabled = plan == Operation.Cancel;
            
            _miClearCache.Enabled = AnyCachedTasks();
        }
        public bool AnyCachedTasks()
        {
            return Compiler.Tasks.Keys.OfType<ICacheableTask>().Any(t => !t.IsCacheableWhenFinished());
        }

        private Operation PlanGlobalOperation()
        {
            var allTasks = GetAllTasks();

            //if any are still executing or scheduled for execution
            if (allTasks.Any(t => t.State == CompilationState.Executing || t.State == CompilationState.Scheduled))
                return Operation.Cancel;

            //if all are complete
            return Operation.Execute;
        }
        #endregion
        
        public ICompileable[] GetAllTasks()
        {
            return Compiler.Tasks.Keys.ToArray();
        }

        private void MiClearCacheClick(object sender, EventArgs e)
        {
            ClearAllCaches();
        }

        private void btnAbortLoad_Click(object sender, EventArgs e)
        {
            CancelAll();
        }
    }

    [TypeDescriptionProvider(typeof(AbstractControlDescriptionProvider<CohortIdentificationConfigurationUI_Design, UserControl>))]
    public abstract class CohortIdentificationConfigurationUI_Design : RDMPSingleDatabaseObjectControl<CohortIdentificationConfiguration>
    {
    }
}

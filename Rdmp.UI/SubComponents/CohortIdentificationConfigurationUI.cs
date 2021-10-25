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
using Rdmp.Core;
using Rdmp.Core.CohortCreation;
using Rdmp.Core.CohortCreation.Execution;
using Rdmp.Core.CohortCreation.Execution.Joinables;
using Rdmp.Core.CommandExecution.AtomicCommands;
using Rdmp.Core.CommandExecution.AtomicCommands.CohortCreationCommands;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Curation.Data.Aggregation;
using Rdmp.Core.Curation.Data.Cohort;
using Rdmp.Core.Curation.Data.Cohort.Joinables;
using Rdmp.Core.Icons.IconProvision;
using Rdmp.Core.Providers.Nodes;
using Rdmp.Core.QueryCaching.Aggregation;
using Rdmp.UI.CohortUI.ImportCustomData;
using Rdmp.UI.Collections;
using Rdmp.UI.CommandExecution.AtomicCommands;
using Rdmp.UI.ItemActivation;
using Rdmp.UI.Refreshing;
using Rdmp.UI.SimpleDialogs;
using Rdmp.UI.TestsAndSetup.ServicePropogation;
using ReusableLibraryCode.Icons.IconProvision;
using Timer = System.Windows.Forms.Timer;


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
    public partial class CohortIdentificationConfigurationUI : CohortIdentificationConfigurationUI_Design,IRefreshBusSubscriber
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
        Timer timer = new Timer();

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
            tlvCic.ItemActivate += TlvCic_ItemActivate;
            AssociatedCollection = RDMPCollection.Cohort;

            
            timer.Tick += refreshColumnValues;
            timer.Interval = 2000;
            timer.Start();
            
            olvCount.AspectGetter = Count_AspectGetter;
            olvCached.AspectGetter = Cached_AspectGetter;
            olvCumulativeTotal.AspectGetter = CumulativeTotal_AspectGetter;
            olvTime.AspectGetter = Time_AspectGetter;
            olvWorking.AspectGetter = Working_AspectGetter;
            olvCatalogue.AspectGetter = Catalogue_AspectGetter;

            _miClearCache.Click += MiClearCacheClick;
            _miClearCache.Image = CatalogueIcons.ExternalDatabaseServer_Cache;

            cbIncludeCumulative.CheckedChanged += (s, e) =>
            {
                Compiler.IncludeCumulativeTotals = cbIncludeCumulative.Checked;
                RecreateAllTasks();
            };

            //This is important, OrderableComparer ensures IOrderable objects appear in the correct order but the comparator
            //doesn't get called unless the column has a sorting on it
            olvNameCol.Sortable = true;
            tlvCic.Sort(olvNameCol);

            RDMPCollectionCommonFunctionality.SetupColumnTracking(tlvCic, olvCached, new Guid("59c6eda9-dcf3-4a24-801f-4c5467c76f94"));
            RDMPCollectionCommonFunctionality.SetupColumnTracking(tlvCic, olvCatalogue, new Guid("59c6f9a6-4a93-4167-a268-9ea755d0ad94"));
            RDMPCollectionCommonFunctionality.SetupColumnTracking(tlvCic, olvCount, new Guid("4ca6588f-2511-4082-addd-ec42e9d75b39"));
            RDMPCollectionCommonFunctionality.SetupColumnTracking(tlvCic, olvCumulativeTotal, new Guid("a3e901e2-c6b8-4365-bea8-5666b9b74821"));
            RDMPCollectionCommonFunctionality.SetupColumnTracking(tlvCic, olvExecute, new Guid("f8ad1751-b273-42d7-a6d1-0c580099ceee"));
            RDMPCollectionCommonFunctionality.SetupColumnTracking(tlvCic, olvNameCol, new Guid("63db1af5-061c-42b9-873c-7d3d3ac21cd8"));
            RDMPCollectionCommonFunctionality.SetupColumnTracking(tlvCic, olvOrder, new Guid("5be4e6e7-bad6-4bd5-821c-a235bc056053"));
            RDMPCollectionCommonFunctionality.SetupColumnTracking(tlvCic, olvTime, new Guid("88f88d4a-6204-4f83-b9a7-5421186808b7"));
            RDMPCollectionCommonFunctionality.SetupColumnTracking(tlvCic, olvWorking, new Guid("cfe55a4f-9e17-4205-9016-ae506667f22d"));
        }

        private object Working_AspectGetter(object rowobject)
        {
            return GetKey(rowobject)?.State;
        }

        private object Time_AspectGetter(object rowobject)
        {
            return GetKey(rowobject)?.ElapsedTime?.ToString( @"hh\:mm\:ss");
        }

        private object CumulativeTotal_AspectGetter(object rowobject)
        {
            return GetKey(rowobject)?.CumulativeRowCount?.ToString("N0");
        }

        private ICompileable GetKey(object rowobject)
        {
            lock(Compiler.Tasks)
            {
                return
                    Compiler?.Tasks?.Keys.FirstOrDefault(k =>

                        (rowobject is AggregateConfiguration ac && k.Child is JoinableCohortAggregateConfiguration j
                                                                && j.AggregateConfiguration_ID == ac.ID)
                        || k.Child.Equals(rowobject));
            }
        }

        private object Cached_AspectGetter(object rowobject)
        {
            var key = GetKey(rowobject);
            
            if (key != null)
                return _configuration.QueryCachingServer_ID == null ? "No Cache" : key.GetCachedQueryUseCount();
            
            return null;
        }

        private object Count_AspectGetter(object rowobject)
        {
            var key = GetKey(rowobject);
            
            if (key != null && key.State == CompilationState.Finished)
                return key.FinalRowCount.ToString("N0");

            return null;
        }
        private object Catalogue_AspectGetter(object rowobject)
        {
            return
                rowobject is AggregateConfiguration ac ? ac.Catalogue.Name : null;
        }

        
        public void RefreshBus_RefreshObject(object sender, RefreshObjectEventArgs e)
        {
            var descendancy = Activator.CoreChildProvider.GetDescendancyListIfAnyFor(e.Object);

            
            //if publish event was for a child of the cic (_cic is in the objects descendancy i.e. it sits below our cic)
            if (descendancy != null && descendancy.Parents.Contains(_configuration))
            {

                //Go up descendency list clearing out the tasks above (and including) e.Object because it has changed
                foreach (var o in descendancy.Parents.Union(new[] {e.Object}))
                {
                    var key = GetKey(o);
                    if(key != null)
                        Compiler.CancelTask(key,true);
                }
                //TODO: this doesn't clear the compiler
                RecreateAllTasks();
            }
        }
        
        private void refreshColumnValues(object sender, EventArgs e)
        {
            if(!tlvCic.IsDisposed)
                tlvCic.RefreshObjects(tlvCic.Objects.Cast<object>().ToArray());
        }
        
        public override void SetDatabaseObject(IActivateItems activator, CohortIdentificationConfiguration databaseObject)
        {
            base.SetDatabaseObject(activator,databaseObject);
            _configuration = databaseObject;
            
            lblName.Text = $"Name:{_configuration.Name}";
            lblDescription.Text = $"Description:{_configuration.Description}";
            ticket.TicketText = _configuration.Ticket;

            if (_commonFunctionality == null)
            {
                activator.RefreshBus.Subscribe(this);
                _commonFunctionality = new RDMPCollectionCommonFunctionality();

                _commonFunctionality.SetUp(RDMPCollection.Cohort, tlvCic, activator, olvNameCol, olvNameCol, new RDMPCollectionCommonFunctionalitySettings
                {
                    SuppressActivate = true,
                    AddFavouriteColumn = false,
                    AddCheckColumn = false,
                    AllowPinning = false,
                    AllowSorting =  true, //important, we need sorting on so that we can override sort order with our OrderableComparer
                });
                _commonFunctionality.MenuBuilt += MenuBuilt;
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

        private void TlvCic_ItemActivate(object sender, EventArgs e)
        {
            
            var o = tlvCic.SelectedObject;
            if (o != null)
            {
                var key = GetKey(o);
                if (key?.CrashMessage != null)
                {
                    ViewCrashMessage(key);
                    return;
                }
            }
                
            _commonFunctionality.CommonItemActivation(sender, e);
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
            return currentState switch
            {
                CompilationState.NotScheduled => Operation.Execute,
                CompilationState.Building => Operation.Cancel,
                CompilationState.Scheduled => Operation.None,
                CompilationState.Executing => Operation.Cancel,
                CompilationState.Finished => Operation.Execute,
                CompilationState.Crashed => Operation.Execute,
                _ => throw new ArgumentOutOfRangeException("currentState")
            };
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
            lock(Compiler.Tasks)
            {
                var task = GetTaskIfExists(o);

                if (task == null)
                    return CompilationState.NotScheduled;

                return task.State;
            }
        }

        public ICompileable GetTaskIfExists(IMapsDirectlyToDatabaseTable o)
        {
            lock (Compiler.Tasks)
            {
                var kvps = Compiler.Tasks.Where(t => t.Key.Child.Equals(o)).ToArray();

                if(kvps.Length == 0)
                {
                    return null;
                }
                
                if(kvps.Length == 1)
                {
                    return kvps[0].Key;
                }

                var running = kvps.FirstOrDefault(k => k.Value != null).Key;

                return running ?? kvps[0].Key;
            }
        }


        void tlvCic_ButtonClick(object sender, CellClickEventArgs e)
        {
            var o = e.Model;
            var aggregate = o as AggregateConfiguration;
            var container = o as CohortAggregateContainer;

            Task.Run(() =>
            {
                if (aggregate != null)
                {
                    var joinable = aggregate.JoinableCohortAggregateConfiguration;

                    if (joinable != null)
                        OrderActivity(GetNextOperation(GetState(joinable)), joinable);
                    else
                        OrderActivity(GetNextOperation(GetState(aggregate)), aggregate);
                }
                if (container != null)
                {
                    OrderActivity(GetNextOperation(GetState(container)), container);
                }
            });
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
            lock(Compiler.Tasks)
            {
                var task = GetTaskIfExists(o);

                if (task == null)
                    return;

                var c = task as CacheableTask;
                if (c != null)
                    ClearCacheFor(new ICacheableTask[] { c });

                Compiler.CancelTask(task, true);
            }
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
            lock(Compiler.Tasks)
            {
                return Compiler.Tasks.Keys.OfType<ICacheableTask>().Any(t => !t.IsCacheableWhenFinished());
            }
        }

        private Operation PlanGlobalOperation()
        {
            var allTasks = GetAllTasks();

            //if any are still executing or scheduled for execution
            if (allTasks.Any(t => t.State == CompilationState.Executing || t.State == CompilationState.Building || t.State == CompilationState.Scheduled))
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

        
        private void MenuBuilt(object sender, MenuBuiltEventArgs e)
        {
            var c = GetKey(e.Obj);

            if (c != null)
            {
                e.Menu.Items.Add(new ToolStripSeparator());

                e.Menu.Items.Add(
                    BuildItem("View Sql", c, a => !string.IsNullOrWhiteSpace(a.CountSQL),
                        a => WideMessageBox.Show($"Sql {c}", a.CountSQL, WideMessageBoxTheme.Help))
                );
                
                                
                e.Menu.Items.Add(
                    new ToolStripMenuItem("View Crash Message", null,
                        (s, ev) => ViewCrashMessage(c)){Enabled = c.CrashMessage != null});

                e.Menu.Items.Add(
                    new ToolStripMenuItem("View Build Log", null,
                        (s, ev) => WideMessageBox.Show($"Build Log {c}", c.Log, WideMessageBoxTheme.Help)));
                
                e.Menu.Items.Add(
                    BuildItem("View Results", c, a => a.Identifiers != null,
                        a =>
                        {
                            Activator.ShowWindow(new DataTableViewerUI(a.Identifiers, $"Results {c}"));
                        })
                );

                
                e.Menu.Items.Add(
                    BuildItem("Clear Cache", c, a => a.SubqueriesCached > 0,
                        a =>
                        {
                            if (c is ICacheableTask cacheable)
                                ClearCacheFor(new[] {cacheable});
                        })
                );
            }

        }
        private ToolStripMenuItem BuildItem(string title, ICompileable c,Func<CohortIdentificationTaskExecution,bool> enabledFunc, Action<CohortIdentificationTaskExecution> action)
        {
            var menuItem = new ToolStripMenuItem(title);

            if (Compiler.Tasks.ContainsKey(c))
            {
                var exe = Compiler.Tasks[c];
                if (exe != null && enabledFunc(exe))
                    menuItem.Click += (s, e) => action(exe);
                else
                    menuItem.Enabled = false;
            }
            else
                menuItem.Enabled = false;

            return menuItem;
        }
        private void ViewCrashMessage(ICompileable compileable)
        {
            ExceptionViewer.Show(compileable.CrashMessage);
        }
    }

    [TypeDescriptionProvider(typeof(AbstractControlDescriptionProvider<CohortIdentificationConfigurationUI_Design, UserControl>))]
    public abstract class CohortIdentificationConfigurationUI_Design : RDMPSingleDatabaseObjectControl<CohortIdentificationConfiguration>
    {
    }
}

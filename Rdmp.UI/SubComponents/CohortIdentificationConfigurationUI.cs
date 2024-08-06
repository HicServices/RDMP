// Copyright (c) The University of Dundee 2018-2024
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.ComponentModel;
using System.Linq;
using System.Windows.Forms;
using BrightIdeasSoftware;
using Rdmp.Core;
using Rdmp.Core.CohortCreation;
using Rdmp.Core.CohortCreation.Execution;
using Rdmp.Core.CommandExecution.AtomicCommands;
using Rdmp.Core.CommandExecution.AtomicCommands.CohortCreationCommands;
using Rdmp.Core.Curation.Data.Cohort;
using Rdmp.Core.Icons.IconProvision;
using Rdmp.Core.ReusableLibraryCode;
using Rdmp.Core.ReusableLibraryCode.Icons.IconProvision;
using Rdmp.Core.ReusableLibraryCode.Settings;
using Rdmp.UI.Collections;
using Rdmp.UI.CommandExecution.AtomicCommands;
using Rdmp.UI.ItemActivation;
using Rdmp.UI.Refreshing;
using Rdmp.UI.SimpleDialogs;
using Rdmp.UI.TestsAndSetup.ServicePropogation;
using static Rdmp.Core.CohortCreation.CohortIdentificationConfigurationUICommon;
using Timer = System.Windows.Forms.Timer;


namespace Rdmp.UI.SubComponents;

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
/// <para>    Filter 2 - Prescription is the first prescription of its type for the patient</para>
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
public partial class CohortIdentificationConfigurationUI : CohortIdentificationConfigurationUI_Design,
    IRefreshBusSubscriber
{
    private ToolStripMenuItem cbIncludeCumulative = new("Calculate Cumulative Totals") { CheckOnClick = true };
    private ToolTip tt = new();

    private readonly ToolStripTimeout _timeoutControls = new() { Timeout = 3000 };
    private RDMPCollectionCommonFunctionality _commonFunctionality;

    private Timer timer = new();

    private ExecuteCommandClearQueryCache _clearCacheCommand;

    private CohortIdentificationConfigurationUICommon Common = new();

    public CohortIdentificationConfigurationUI()
    {
        InitializeComponent();

        Common = new CohortIdentificationConfigurationUICommon();

        olvExecute.IsButton = true;
        olvExecute.ButtonSizing = OLVColumn.ButtonSizingMode.CellBounds;
        tlvCic.RowHeight = 19;
        olvExecute.AspectGetter += Common.ExecuteAspectGetter;
        tlvCic.ButtonClick += tlvCic_ButtonClick;
        olvOrder.AspectGetter += static o => o is IOrderable orderable ? orderable.Order : null;
        olvOrder.IsEditable = false;
        tlvCic.ItemActivate += TlvCic_ItemActivate;
        AssociatedCollection = RDMPCollection.Cohort;


        timer.Tick += refreshColumnValues;
        timer.Interval = 2000;
        timer.Start();

        olvCount.AspectGetter = Common.Count_AspectGetter;
        olvCached.AspectGetter = Common.Cached_AspectGetter;
        olvCumulativeTotal.AspectGetter = Common.CumulativeTotal_AspectGetter;
        olvTime.AspectGetter = Common.Time_AspectGetter;
        olvWorking.AspectGetter = Common.Working_AspectGetter;
        olvCatalogue.AspectGetter = Catalogue_AspectGetter;

        cbIncludeCumulative.CheckedChanged += (s, e) =>
        {
            Common.SetShowCumulativeTotals(cbIncludeCumulative.Checked);
        };

        //This is important, OrderableComparer ensures IOrderable objects appear in the correct order but the comparator
        //doesn't get called unless the column has a sorting on it
        olvNameCol.Sortable = true;
        tlvCic.Sort(olvNameCol);

        RDMPCollectionCommonFunctionality.SetupColumnTracking(tlvCic, olvCached,
            new Guid("59c6eda9-dcf3-4a24-801f-4c5467c76f94"));
        RDMPCollectionCommonFunctionality.SetupColumnTracking(tlvCic, olvCatalogue,
            new Guid("59c6f9a6-4a93-4167-a268-9ea755d0ad94"));
        RDMPCollectionCommonFunctionality.SetupColumnTracking(tlvCic, olvCount,
            new Guid("4ca6588f-2511-4082-addd-ec42e9d75b39"));
        RDMPCollectionCommonFunctionality.SetupColumnTracking(tlvCic, olvCumulativeTotal,
            new Guid("a3e901e2-c6b8-4365-bea8-5666b9b74821"));
        RDMPCollectionCommonFunctionality.SetupColumnTracking(tlvCic, olvExecute,
            new Guid("f8ad1751-b273-42d7-a6d1-0c580099ceee"));
        RDMPCollectionCommonFunctionality.SetupColumnTracking(tlvCic, olvNameCol,
            new Guid("63db1af5-061c-42b9-873c-7d3d3ac21cd8"));
        RDMPCollectionCommonFunctionality.SetupColumnTracking(tlvCic, olvOrder,
            new Guid("5be4e6e7-bad6-4bd5-821c-a235bc056053"));
        RDMPCollectionCommonFunctionality.SetupColumnTracking(tlvCic, olvTime,
            new Guid("88f88d4a-6204-4f83-b9a7-5421186808b7"));
        RDMPCollectionCommonFunctionality.SetupColumnTracking(tlvCic, olvWorking,
            new Guid("cfe55a4f-9e17-4205-9016-ae506667f22d"));

        tt.SetToolTip(btnExecute, "Starts running and caches all cohort sets and containers");
        tt.SetToolTip(btnAbortLoad, "Cancels execution of any running cohort sets");

    }


    public void RefreshBus_RefreshObject(object sender, RefreshObjectEventArgs e)
    {
        Common.Activator = Activator;
        var descendancy = Activator.CoreChildProvider.GetDescendancyListIfAnyFor(e.Object);

        //if publish event was for a child of the cic (_cic is in the objects descendancy i.e. it sits below our cic)
        if (descendancy != null && descendancy.Parents.Contains(Common.Configuration))
        {
            //Go up descendency list clearing out the tasks above (and including) e.Object because it has changed
            foreach (var o in descendancy.Parents.Union(new[] { e.Object }))
            {
                var key = Common.GetKey(o);
                if (key != null)
                    Common.Compiler.CancelTask(key, true);
            }

            //TODO: this doesn't clear the compiler
            Common.RecreateAllTasks();
        }
    }

    private void refreshColumnValues(object sender, EventArgs e)
    {
        if (!tlvCic.IsDisposed)
            tlvCic.RefreshObjects(tlvCic.Objects.Cast<object>().ToArray());
    }

    public override void SetDatabaseObject(IActivateItems activator, CohortIdentificationConfiguration databaseObject)
    {
        base.SetDatabaseObject(activator, databaseObject);
        version.Setup(databaseObject, activator);
        Common.Configuration = databaseObject;
        Common.Compiler.CohortIdentificationConfiguration = databaseObject;

        RebuildClearCacheCommand();

        gbCicInfo.Text = $"Name: {databaseObject.Name}";
        tbDescription.Text = $"Description: {databaseObject.Description}";
        ticket.TicketText = databaseObject.Ticket;

        if (_commonFunctionality == null)
        {
            activator.RefreshBus.Subscribe(this);
            _commonFunctionality = new RDMPCollectionCommonFunctionality();

            _commonFunctionality.SetUp(RDMPCollection.Cohort, tlvCic, activator, olvNameCol, olvNameCol,
              new RDMPCollectionCommonFunctionalitySettings
              {
                  SuppressActivate = true,
                  AddFavouriteColumn = false,
                  AddCheckColumn = false,
                  AllowSorting =
                      true //important, we need sorting on so that we can override sort order with our OrderableComparer
              });
            _commonFunctionality.MenuBuilt += MenuBuilt;
            tlvCic.Objects = null;
            tlvCic.AddObject(databaseObject);

            if (UserSettings.ExpandAllInCohortBuilder)
                tlvCic.ExpandAll();
            tlvCic.SelectedIndex = 0;
        }

        CommonFunctionality.AddToMenu(cbIncludeCumulative);
        CommonFunctionality.AddToMenu(new ToolStripSeparator());
        CommonFunctionality.AddToMenu(new ExecuteCommandSetQueryCachingDatabase(Activator, databaseObject));
        CommonFunctionality.AddToMenu(new ExecuteCommandClearQueryCache(Activator, databaseObject));
        CommonFunctionality.AddToMenu(new ExecuteCommandCreateNewQueryCacheDatabase(activator, databaseObject));
        CommonFunctionality.AddToMenu(
            new ExecuteCommandSet(activator, databaseObject, databaseObject.GetType().GetProperty("Description"))
            {
                OverrideIcon =
                    Activator.CoreIconProvider.GetImage(RDMPConcept.CohortIdentificationConfiguration, OverlayKind.Edit)
            });
        CommonFunctionality.AddToMenu(new ToolStripSeparator());
        CommonFunctionality.AddToMenu(
            new ExecuteCommandShowXmlDoc(activator, "CohortIdentificationConfiguration.QueryCachingServer_ID",
                "Query Caching"), "Help (What is Query Caching)");
        CommonFunctionality.Add(
            new ExecuteCommandCreateNewCohortByExecutingACohortIdentificationConfiguration(activator, null).SetTarget(
                databaseObject),
            "Commit Cohort",
            activator.CoreIconProvider.GetImage(RDMPConcept.ExtractableCohort, OverlayKind.Add));

        foreach (var c in _timeoutControls.GetControls())
            CommonFunctionality.Add(c);

        Common.QueryCachingServer = databaseObject.QueryCachingServer;
        Common.Compiler.CoreChildProvider = activator.CoreChildProvider;
        Common.RecreateAllTasks();
    }

    /// <summary>
    /// Resets the state of <see cref="btnClearCache"/> to reflect any changes in cached status
    /// </summary>
    private void RebuildClearCacheCommand()
    {
        if (InvokeRequired)
        {
            Invoke(new MethodInvoker(RebuildClearCacheCommand));
            return;
        }

        _clearCacheCommand = new ExecuteCommandClearQueryCache(Activator, Common.Configuration);
        btnClearCache.Enabled = !_clearCacheCommand.IsImpossible;
        btnClearCache.Image = _clearCacheCommand.GetImage(Activator.CoreIconProvider).ImageToBitmap();

        tt.SetToolTip(btnClearCache,
            _clearCacheCommand.IsImpossible
                ? _clearCacheCommand.ReasonCommandImpossible
                : "Clears any cached results (stale or otherwise) from the query cache");
    }

    private void TlvCic_ItemActivate(object sender, EventArgs e)
    {
        var o = tlvCic.SelectedObject;
        if (o != null)
        {
            var key = Common.GetKey(o);
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

    public override string GetTabName() => $"Execute:{base.GetTabName()}";

    private void ticket_TicketTextChanged(object sender, EventArgs e)
    {
        Common.Configuration.Ticket = ticket.TicketText;
    }


    public override void ConsultAboutClosing(object sender, FormClosingEventArgs e)
    {
        e.Cancel = Common.ConsultAboutClosing();
    }


    private void tlvCic_ButtonClick(object sender, CellClickEventArgs e)
    {
        Common.ExecuteOrCancel(e.Model, _timeoutControls.Timeout);
    }

    public void StartAll()
    {
        lblExecuteAllPhase.Enabled = true;

        Common.StartAll(RebuildClearCacheCommand, RunnerOnPhaseChanged, _timeoutControls.Timeout);
    }

    private void RunnerOnPhaseChanged(object sender, EventArgs eventArgs)
    {
        if (InvokeRequired)
        {
            Invoke(new MethodInvoker(() => RunnerOnPhaseChanged(sender, eventArgs)));
            return;
        }

        lblExecuteAllPhase.Text = UsefulStuff.PascalCaseStringToHumanReadable(Common.Runner.ExecutionPhase.ToString());
        Common.RecreateAllTasks(false);
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
        var plan = Common.PlanGlobalOperation();

        btnExecute.Enabled = plan == Operation.Execute;
        btnAbortLoad.Enabled = plan == Operation.Cancel;
    }


    private void MiClearCacheClick(object sender, EventArgs e)
    {
        Common.ClearAllCaches();
    }

    private void btnAbortLoad_Click(object sender, EventArgs e)
    {
        Common.CancelAll();
    }

    private void btnClearCache_Click(object sender, EventArgs e)
    {
        try
        {
            _clearCacheCommand.Execute();
        }
        catch (Exception ex)
        {
            ExceptionViewer.Show(ex);
        }

        RebuildClearCacheCommand();
    }

    private void MenuBuilt(object sender, MenuBuiltEventArgs e)
    {
        var c = Common.GetKey(e.Obj);

        if (c != null)
        {
            e.Menu.Items.Add(new ToolStripSeparator());

            e.Menu.Items.Add(
                BuildItem("View Results", c, a => a.Identifiers != null,
                    a => { Activator.ShowWindow(new DataTableViewerUI(a.Identifiers, $"Results {c}")); })
            );

            e.Menu.Items.Add(
                BuildItem("View SQL", c, a => !string.IsNullOrWhiteSpace(a.CountSQL),
                    a => WideMessageBox.Show($"Sql {c}", a.CountSQL, WideMessageBoxTheme.Help))
            );

            e.Menu.Items.Add(
                new ToolStripMenuItem("View Build Log", null,
                    (s, ev) => WideMessageBox.Show($"Build Log {c}", c.Log, WideMessageBoxTheme.Help)));

            e.Menu.Items.Add(
                new ToolStripMenuItem("View Crash Message", null,
                    (s, ev) => ViewCrashMessage(c))
                { Enabled = c.CrashMessage != null });

            e.Menu.Items.Add(
                BuildItem("Clear Object from Cache", c, a => a.SubqueriesCached > 0,
                    a =>
                    {
                        if (c is ICacheableTask cacheable)
                            Common.ClearCacheFor(new[] { cacheable });
                    })
            );
        }
    }

    private ToolStripMenuItem BuildItem(string title, ICompileable c,
        Func<CohortIdentificationTaskExecution, bool> enabledFunc, Action<CohortIdentificationTaskExecution> action)
    {
        var menuItem = new ToolStripMenuItem(title);

        if (Common.Compiler.Tasks.TryGetValue(c, out var task) && task != null && enabledFunc(task))
            menuItem.Click += (s, e) => action(task);
        else
            menuItem.Enabled = false;

        return menuItem;
    }

    private static void ViewCrashMessage(ICompileable compileable)
    {
        ExceptionViewer.Show(compileable.CrashMessage);
    }

    private void cbKnownVersions_SelectedIndexChanged(object sender, EventArgs e)
    {

    }

    private void version_Load(object sender, EventArgs e)
    {

    }
}

[TypeDescriptionProvider(
    typeof(AbstractControlDescriptionProvider<CohortIdentificationConfigurationUI_Design, UserControl>))]
public abstract class
    CohortIdentificationConfigurationUI_Design : RDMPSingleDatabaseObjectControl<CohortIdentificationConfiguration>
{
}
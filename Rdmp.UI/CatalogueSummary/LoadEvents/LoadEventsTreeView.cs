// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using BrightIdeasSoftware;
using Rdmp.Core.CommandExecution.AtomicCommands;
using Rdmp.Core.Curation.Data.Dashboarding;
using Rdmp.Core.Curation.Data.DataLoad;
using Rdmp.Core.Logging;
using Rdmp.Core.Logging.PastEvents;
using Rdmp.Core.ReusableLibraryCode;
using Rdmp.UI.Collections;
using Rdmp.UI.ItemActivation;
using Rdmp.UI.Menus.MenuItems;
using Rdmp.UI.Refreshing;
using Rdmp.UI.SimpleDialogs;
using Rdmp.UI.TestsAndSetup.ServicePropogation;

namespace Rdmp.UI.CatalogueSummary.LoadEvents;

/// <summary>
/// Shows the longitudinal history of all data loads of a given object (e.g. data load).  This is an expandable tree including all progress messages, errors, table load notifications
/// etc.
/// 
/// <para>Right clicking on red error messages will allow you to resolve them into yellow state (error has been investigated and did not result in any serious problems / data integrity loss etc).
/// This launches the ResolveFatalErrors dialog.  You can resolve multiple errors at the same time by selecting all the errors at once and then right clicking one of them.</para>
/// </summary>
public partial class LoadEventsTreeView : RDMPUserControl, IObjectCollectionControl
{
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public LoadEventsTreeViewObjectCollection Collection { get; set; }

    private BackgroundWorker _populateLoadHistory = new();
    private ArchivalDataLoadInfo[] _populateLoadHistoryResults = Array.Empty<ArchivalDataLoadInfo>();
    private CancellationTokenSource _populateLoadHistoryCancel;


    private readonly ToolStripTextBox _tbFilterBox = new();
    private readonly ToolStripButton _btnApplyFilter = new("Apply");
    private readonly ToolStripTextBox _tbToFetch = new() { Text = "1000" };
    private readonly ToolStripButton _btnFetch = new("Go");

    private int _toFetch = 1000;


    //constructor
    public LoadEventsTreeView()
    {
        InitializeComponent();

        _populateLoadHistory.DoWork += _populateLoadHistory_DoWork;
        _populateLoadHistory.WorkerSupportsCancellation = true;
        _populateLoadHistory.RunWorkerCompleted += _populateLoadHistory_RunWorkerCompleted;

        treeView1.CanExpandGetter += CanExpandGetter;
        treeView1.ChildrenGetter += ChildrenGetter;
        treeView1.FormatRow += treeView1_FormatRow;
        treeView1.UseFiltering = true;
        olvDescription.UseFiltering = true;

        olvDate.AspectGetter += olvDate_AspectGetter;
        olvDescription.AspectGetter += olvDescription_AspectGetter;

        //We will handle this ourselves because default behaviour is to limit the amount of text copied
        treeView1.CopySelectionOnControlC = false;

        _btnApplyFilter.Click += (s, e) => ApplyFilter(_tbFilterBox.Text);
        _tbToFetch.TextChanged += TbToFetchTextChanged;
        _btnFetch.Click += (s, e) => PopulateLoadHistory();

        RDMPCollectionCommonFunctionality.SetupColumnTracking(treeView1, olvDescription,
            new Guid("6b09f39c-2b88-41ed-a396-42a2d2288952"));
        RDMPCollectionCommonFunctionality.SetupColumnTracking(treeView1, olvDate,
            new Guid("d0caf588-cff8-4e49-b755-ed9aaf320f1a"));
    }

    private void TbToFetchTextChanged(object sender, EventArgs e)
    {
        try
        {
            _toFetch = int.Parse(_tbToFetch.Text);
            _tbToFetch.ForeColor = Color.Black;
        }
        catch (Exception)
        {
            _tbToFetch.ForeColor = Color.Red;
        }
    }


    private object olvDescription_AspectGetter(object rowObject)
    {
        if (rowObject is null) return null;
        return rowObject switch
        {
            ArchivalDataLoadInfo adi => adi.ToString(),
            LoadEventsTreeView_Category cat => cat.ToString(),
            ArchivalFatalError fe => fe.ToShortString(),
            ArchivalTableLoadInfo ti =>
                $"{ti.TargetTable}(I={WithCommas(ti.Inserts)} U={WithCommas(ti.Updates)} D={WithCommas(ti.Deletes)})",
            ArchivalProgressLog pr => pr.Description,
            _ => throw new NotSupportedException()
        };
    }

    private static string WithCommas(int? i) => !i.HasValue ? @"N\A" : i.Value.ToString("N0");

    private static object olvDate_AspectGetter(object rowObject)
    {
        if (rowObject is null) return null;
        return rowObject switch
        {
            ArchivalDataLoadInfo adi => adi.StartTime,
            LoadEventsTreeView_Category => null,
            ArchivalFatalError fe => fe.Date,
            ArchivalTableLoadInfo ti => ti.Start,
            ArchivalProgressLog pr => pr.Date,
            _ => throw new NotSupportedException()
        };
    }

    private static void treeView1_FormatRow(object sender, FormatRowEventArgs e)
    {
        // Only apply if it is a data load info thing
        if (e.Model is not ArchivalDataLoadInfo dli) return;

        if (dli.HasErrors)
            e.Item.ForeColor = Color.DarkOrange;
        else if (dli.EndTime == null) //did not end
            e.Item.ForeColor = Color.Purple;
        else
            e.Item.ForeColor = Color.Green; //was fine
    }

    private static IEnumerable ChildrenGetter(object model)
    {
        if (model is not ArchivalDataLoadInfo dli)
            return model is LoadEventsTreeView_Category category ? category.Children : Enumerable.Empty<object>();

        var children = new List<object>();

        if (dli.Errors.Any())
            children.Add(new LoadEventsTreeView_Category("Errors",
                dli.Errors.OrderByDescending(static d => d.Date).ToArray(), LoggingTables.FatalError, dli.ID));

        if (dli.Progress.Any())
            children.Add(new LoadEventsTreeView_Category("Progress Messages",
                dli.Progress.OrderByDescending(static d => d.Date).ToArray(), LoggingTables.ProgressLog, dli.ID));

        if (dli.TableLoadInfos.Any())
            children.Add(new LoadEventsTreeView_Category("Tables Loaded",
                dli.TableLoadInfos.OrderByDescending(static d => d.Start).ToArray(), LoggingTables.TableLoadRun,
                dli.ID));

        return children;
    }

    private class LoadEventsTreeView_Category
    {
        public object[] Children { get; }

        private readonly string _name;

        public readonly LoggingTables AssociatedTable;
        public readonly int RunId;

        public LoadEventsTreeView_Category(string name, object[] children, LoggingTables associatedTable, int runId)
        {
            Children = children;
            RunId = runId;
            _name = name;
            AssociatedTable = associatedTable;
        }

        public override string ToString() => $"{_name} ({Children.Length})";
    }

    private static bool CanExpandGetter(object model) => model is ArchivalDataLoadInfo or LoadEventsTreeView_Category;

    //it is a child of a thing in a category, so a leaf
    private void _populateLoadHistory_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
    {
        llLoading.Visible = false;
        pbLoading.Visible = false;

        if (e.Error != null)
            ExceptionViewer.Show(e.Error);

        if (e.Cancelled)
        {
            ClearObjects();
            return;
        }

        AddObjects(_populateLoadHistoryResults);
    }

    public void AddObjects(ArchivalDataLoadInfo[] archivalDataLoadInfos)
    {
        treeView1.AddObjects(archivalDataLoadInfos);
    }

    public void ClearObjects()
    {
        treeView1.ClearObjects();
    }

    private LogManager _logManager;

    private void _populateLoadHistory_DoWork(object sender, DoWorkEventArgs e)
    {
        try
        {
            ArchivalDataLoadInfo[] results;
            try
            {
                _logManager = new LogManager(Collection.RootObject.GetDistinctLoggingDatabase());
                var unfilteredResults = _logManager.GetArchivalDataLoadInfos(
                    Collection.RootObject.GetDistinctLoggingTask(), _populateLoadHistoryCancel.Token, null, _toFetch);
                results = Collection.RootObject.FilterRuns(unfilteredResults).ToArray();
            }
            catch (OperationCanceledException) //user cancels
            {
                results = Array.Empty<ArchivalDataLoadInfo>();
            }

            _populateLoadHistoryResults = results;
        }
        catch (Exception exception)
        {
            CommonFunctionality.Fatal("Failed to populate load history", exception);
        }
    }

    private void PopulateLoadHistory()
    {
        //it's already doing it...
        if (_populateLoadHistory.IsBusy)
            return;

        //clear the tree
        ClearObjects();

        if (Collection?.RootObject == null)
            return;


        //cancel any running workers
        AbortWorkers();

        //tell user that we are loading
        llLoading.Visible = true;
        pbLoading.Visible = true;

        //clear the results
        _populateLoadHistoryResults = Array.Empty<ArchivalDataLoadInfo>();

        _populateLoadHistoryCancel = new CancellationTokenSource();
        _populateLoadHistory.RunWorkerAsync();
    }


    private void AbortWorkers()
    {
        if (_populateLoadHistory.IsBusy)
            _populateLoadHistory.CancelAsync();

        _populateLoadHistoryCancel?.Cancel();
    }

    private void llLoading_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
    {
        AbortWorkers();
    }

    private class LogFilter : AbstractModelFilter
    {
        public LogFilter()
        {

        }

        public LogFilter(ObjectListView olv)
        {
            this.ListView = olv;
        }

        public LogFilter(ObjectListView olv, string text)
        {
            this.ListView = olv;
            this.Text = text;
        }

        public LogFilter(ObjectListView olv, string text, StringComparison comparison)
        {
            this.ListView = olv;
            this.Text = text;
            this.StringComparison = comparison;
        }

        public string Text;
        public StringComparison StringComparison = StringComparison.InvariantCultureIgnoreCase;

        protected ObjectListView ListView;

        private List<int> acceptableChildren = [];
        private List<int> acceptableRoots = [];

        public override bool Filter(object modelObject)
        {
            if (this.ListView == null || String.IsNullOrEmpty(this.Text))
                return true;

            foreach (OLVColumn column in this.ListView.Columns)
            {
                if (column.IsVisible)
                {
                    string cellText = column.GetStringValue(modelObject);
                    if (cellText.IndexOf(this.Text, this.StringComparison) != -1)
                    {
                        if (modelObject is ArchivalDataLoadInfo adli)
                        {
                            acceptableChildren.AddRange(adli.Progress.Select(p => p.ID));
                            acceptableRoots.Add(adli.ID);
                        }
                        return true;
                    }
                    else if (modelObject is ArchivalProgressLog log)
                    {
                        if (acceptableChildren.Contains(log.ID))
                        {
                            return true;
                        }
                    }
                    else if (modelObject is LoadEventsTreeView_Category lec)
                    {
                        if (acceptableRoots.Contains(lec.RunId))
                        {
                            return true;
                        }
                        if (lec.Children is ArchivalProgressLog[] lapl)
                        {
                            if (lapl.Any(pl => column.GetStringValue(pl).IndexOf(this.Text, this.StringComparison) != -1)) return true;
                        }
                        if (lec.Children is ArchivalTableLoadInfo[] latli)
                        {
                            if (latli.Any(pl => column.GetStringValue(pl).IndexOf(this.Text, this.StringComparison) != -1)) return true;
                        }
                        if (lec.Children is ArchivalFatalError[] lafe)
                        {
                            if (lafe.Any(pl => column.GetStringValue(pl).IndexOf(this.Text, this.StringComparison) != -1)) return true;
                        }
                    }
                    else if (modelObject is ArchivalTableLoadInfo dli)
                    {
                        if (acceptableRoots.Contains(dli.Parent.ID))
                        {
                            return true;
                        }
                    }
                    else if (modelObject is ArchivalDataLoadInfo adli)
                    {
                        if (adli.Progress.Any(p => column.GetStringValue(p).IndexOf(this.Text, this.StringComparison) != -1)) return true;
                        if (adli.TableLoadInfos.Any(p => column.GetStringValue(p).IndexOf(this.Text, this.StringComparison) != -1)) return true;
                        if (adli.Errors.Any(p => column.GetStringValue(p).IndexOf(this.Text, this.StringComparison) != -1)) return true;
                    }
                }
            }

            return false;
        }
    }

    public void ApplyFilter(string filter)
    {

        treeView1.ModelFilter = new LogFilter(treeView1, filter, StringComparison.CurrentCultureIgnoreCase);
        treeView1.UseFiltering = !string.IsNullOrWhiteSpace(filter);
        treeView1.DefaultRenderer = new HighlightTextRenderer(new TextMatchFilter(treeView1, filter));
    }

    private void treeView1_ColumnRightClick(object sender, CellRightClickEventArgs e)
    {
        var RightClickMenu = new ContextMenuStrip();


        if (e.Model is LoadEventsTreeView_Category category)
        {
            var cmd = new ExecuteCommandViewLogs(Activator,
                new LogViewerFilter(category.AssociatedTable) { Run = category.RunId });
            RightClickMenu.Items.Add(new AtomicCommandMenuItem(cmd, Activator));
        }

        if (e.Model is ArchivalTableLoadInfo tli && Collection?.RootObject is LoadMetadata lmd)
            //if it is not a freaky temp table
            if (!tli.TargetTable.EndsWith("_STAGING") && !tli.TargetTable.EndsWith("_RAW"))
            {
                var mi = new ToolStripMenuItem("View Inserts/Updates", null,
                    (a, b) => new ViewInsertsAndUpdatesDialog(tli, lmd.GetDistinctTableInfoList(true)).Show());

                //if there are inserts/updates
                if (tli.Inserts > 0 || tli.Updates > 0)
                {
                    mi.Enabled = true;
                }
                else
                {
                    mi.Enabled = false;
                    mi.ToolTipText = "No records were changed by this load";
                }

                RightClickMenu.Items.Add(mi);
            }

        if (e.Model is ArchivalFatalError fatalError && _logManager != null)
        {
            var toResolve = treeView1.SelectedObjects.OfType<ArchivalFatalError>().ToArray();
            RightClickMenu.Items.Add("Resolve Fatal Error(s)", null, (a, b) =>
            {
                var resolve = new ResolveFatalErrors(Activator, _logManager, toResolve);
                resolve.ShowDialog();
                treeView1.RefreshObjects(toResolve);
            });
        }

        if (RightClickMenu.Items.Count > 0)
            e.MenuStrip = RightClickMenu;
    }

    public void ExpandAll()
    {
        treeView1.ExpandAll();
    }

    private void treeView1_ItemActivate(object sender, EventArgs e)
    {
        var o = treeView1.SelectedObject;

        if (o == null)
            return;

        if (o is ArchivalDataLoadInfo dli)
            new ExecuteCommandViewLogs(Activator, new LogViewerFilter(LoggingTables.DataLoadRun) { Run = dli.ID })
                .Execute();
        else if (o is LoadEventsTreeView_Category cat)
            new ExecuteCommandViewLogs(Activator, new LogViewerFilter(cat.AssociatedTable) { Run = cat.RunId })
                .Execute();
        else if (o is IHasSummary s)
            WideMessageBox.Show(s);
    }

    private void treeView1_KeyUp(object sender, KeyEventArgs e)
    {
        if (e.KeyCode == Keys.C && e.Control)
        {
            var selectedObjects = treeView1.SelectedObjects;

            var sb = new StringBuilder();

            foreach (var o in selectedObjects)
                sb.AppendLine(o.ToString());

            //We manually implement this here because the default TreeView will only copy 340? characters... very weird but hey Windows Forms
            if (sb.Length != 0)
                Clipboard.SetText(sb.ToString());
        }
    }


    public IPersistableObjectCollection GetCollection() => Collection;

    public string GetTabName() => $"Logs:{Collection?.RootObject}";

    public string GetTabToolTip() => null;

    public void RefreshBus_RefreshObject(object sender, RefreshObjectEventArgs e)
    {
    }

    public void SetCollection(IActivateItems activator, IPersistableObjectCollection collection)
    {
        SetItemActivator(activator);

        Collection = (LoadEventsTreeViewObjectCollection)collection;

        RDMPCollectionCommonFunctionality.SetupColumnSortTracking(treeView1,
            new Guid("ccbea22e-a784-4968-a127-7c3a55b6d281"));

        CommonFunctionality.ClearToolStrip();

        CommonFunctionality.Add(new ToolStripLabel("Filter:"));
        CommonFunctionality.Add(_tbFilterBox);
        CommonFunctionality.Add(_btnApplyFilter);

        CommonFunctionality.Add(new ToolStripSeparator());
        CommonFunctionality.Add(new ToolStripLabel("Fetch:"));
        CommonFunctionality.Add(_tbToFetch);
        CommonFunctionality.Add(_btnFetch);

        PopulateLoadHistory();
    }
}
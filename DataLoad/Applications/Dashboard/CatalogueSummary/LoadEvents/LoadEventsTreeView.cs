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
using CatalogueLibrary.Data.DataLoad;
using CatalogueManager.Collections;
using CatalogueManager.CommandExecution.AtomicCommands;
using CatalogueManager.ItemActivation;
using CatalogueManager.Menus.MenuItems;
using CatalogueManager.TestsAndSetup.ServicePropogation;
using HIC.Logging;
using HIC.Logging.PastEvents;
using ReusableUIComponents;
using ReusableUIComponents.Dialogs;

namespace Dashboard.CatalogueSummary.LoadEvents
{
    /// <summary>
    /// Shows the longitudinal history of all data loads of the selected Catalogue (dataset).  This is an expandable tree including all progress messages, errors, table load notifications
    /// etc.
    /// 
    /// <para>Right clicking on red error messages will allow you to resolve them into yellow state (error has been investigated and did not result in any serious problems / data integrity loss etc).
    /// This launches the ResolveFatalErrors dialog.  You can resolve multiple errors at the same time by selecting all the errors at once and then right clicking one of them.</para>
    /// 
    /// <para>Right clicking a live table load message will let you view a sample of the the UPDATES / INSERTS that happened during the data load (launches ViewInsertsAndUpdatesDialog).</para>
    /// 
    /// </summary>
    public partial class LoadEventsTreeView : LoadEventsTreeView_Design
    {
        private LoadMetadata _loadMetadata;
        
        private BackgroundWorker _populateLoadHistory = new BackgroundWorker();
        private ArchivalDataLoadInfo[] _populateLoadHistoryResults = new ArchivalDataLoadInfo[0];
        private CancellationTokenSource _populateLoadHistoryCancel;
        

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public LoadMetadata LoadMetadata
        {
            get { return _loadMetadata; }
            private set
            {
                _loadMetadata = value;
                PopulateLoadHistory();
            }
        }

        //constructor
        public LoadEventsTreeView()
        {
            InitializeComponent();

            _populateLoadHistory.DoWork += _populateLoadHistory_DoWork;
            _populateLoadHistory.WorkerSupportsCancellation = true;
            _populateLoadHistory.RunWorkerCompleted += _populateLoadHistory_RunWorkerCompleted;

            treeView1.CanExpandGetter += CanExpandGetter;
            treeView1.ChildrenGetter+= ChildrenGetter;
            treeView1.FormatRow += treeView1_FormatRow;
            treeView1.UseFiltering = true;
            olvDescription.UseFiltering = true;

            olvDate.AspectGetter += olvDate_AspectGetter;
            olvDescription.AspectGetter += olvDescription_AspectGetter;

            //We will handle this ourselves because default behaviour is to limit the amount of text copied
            treeView1.CopySelectionOnControlC = false;

            AssociatedCollection = RDMPCollection.DataLoad;
        }

        private object olvDescription_AspectGetter(object rowObject)
        {
            var adi = rowObject as ArchivalDataLoadInfo;
            if (adi != null)
                return adi.ToString();

            var cat = rowObject as LoadEventsTreeView_Category;
            if (cat != null)
                return cat.ToString();

            var fe = rowObject as ArchivalFatalError;
            if (fe != null)
                return fe.ToShortString();

            var ti = rowObject as ArchivalTableLoadInfo;
            if (ti != null)
                return ti.TargetTable + "(I="+ WithCommas(ti.Inserts)  + " U=" + WithCommas(ti.Updates) + " D=" +WithCommas(ti.Deletes)+")";

            var pr = rowObject as ArchivalProgressLog;
            if (pr != null)
                return pr.Description;

            throw new NotSupportedException();
        }

        private string WithCommas(int? i)
        {
            if (!i.HasValue)
                return @"N\A";

            return i.Value.ToString("N0");

        }

        private object olvDate_AspectGetter(object rowObject)
        {
            var adi = rowObject as ArchivalDataLoadInfo;
            
            if(adi != null)
                return adi.StartTime;

            var cat = rowObject as LoadEventsTreeView_Category;
            if (cat != null)
                return null;

            var fe = rowObject as ArchivalFatalError;
            if (fe != null)
                return fe.Date;

            var ti = rowObject as ArchivalTableLoadInfo;
            if (ti != null)
                return ti.Start;

            var pr = rowObject as ArchivalProgressLog;
            if (pr != null)
                return pr.Date;
            
            throw new NotSupportedException();
        }

        void treeView1_FormatRow(object sender, FormatRowEventArgs e)
        {
            var dli = e.Model as ArchivalDataLoadInfo;

            //if it is a data load info thing
            if (dli != null)
                if (dli.HasErrors)
                    e.Item.ForeColor = Color.DarkOrange;
                else if (dli.EndTime == null) //did not end
                    e.Item.ForeColor = Color.Purple;
                else
                    e.Item.ForeColor = Color.Green; //was fine
        }

        private IEnumerable ChildrenGetter(object model)
        {
            List<object> children = new List<object>();

            var dli = model as ArchivalDataLoadInfo;

            if (dli != null)
            {

                if(dli.Errors.Any())
                    children.Add(new LoadEventsTreeView_Category("Errors", dli.Errors.ToArray(), LoggingTables.FatalError, dli.ID));

                if(dli.Progress.Any())
                    children.Add(new LoadEventsTreeView_Category("Progress Messages", dli.Progress.ToArray(),LoggingTables.ProgressLog, dli.ID));

                if(dli.TableLoadInfos.Any())
                    children.Add(new LoadEventsTreeView_Category("Tables Loaded", dli.TableLoadInfos.ToArray(),LoggingTables.TableLoadRun, dli.ID));
            }

            var category = model as LoadEventsTreeView_Category;
            if (category != null)
                return category.Children;

            return children;
        }

        private class LoadEventsTreeView_Category
        {
            public object[] Children { get; set; }
            
            private readonly string _name;
            
            public readonly LoggingTables AssociatedTable;
            public readonly int RunId;

            public LoadEventsTreeView_Category(string name, object[] children,LoggingTables associatedTable,int runId)
            {
                Children = children;
                RunId = runId;
                _name = name;
                AssociatedTable = associatedTable;
            }

            public bool IsTopXSampleOnly()
            {
                return Children.Length == ArchivalDataLoadInfo.MaxChildrenToFetch;
            }
            public override string ToString()
            {
                if(IsTopXSampleOnly())
                    return string.Format(_name + " (Top " + ArchivalDataLoadInfo.MaxChildrenToFetch +")", Children.Length);

                return string.Format(_name + " ({0})",Children.Length);
            }
        }

        private bool CanExpandGetter(object model)
        {
            if (model is ArchivalDataLoadInfo)
                return true;
            
            if (model is LoadEventsTreeView_Category)
                return true;

            //it is a child of a thing in a category
            return false;
        }

        void _populateLoadHistory_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            llLoading.Visible = false;
            pbLoading.Visible = false;

            if (e.Error != null)
                ExceptionViewer.Show(e.Error);

            if(e.Cancelled)
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

        LogManager _logManager;
        void _populateLoadHistory_DoWork(object sender, DoWorkEventArgs e)
        {
            ArchivalDataLoadInfo[] results;
            try
            {
                try
                {
                    _logManager = new LogManager(_loadMetadata.GetDistinctLoggingDatabase());
                    results = _logManager.GetArchivalDataLoadInfos(_loadMetadata.GetDistinctLoggingTask(), _populateLoadHistoryCancel.Token).ToArray();

                    if(results.Length == ArchivalDataLoadInfo.MaxChildrenToFetch)
                        ragSmiley1.Warning(new Exception("Only showing " + ArchivalDataLoadInfo.MaxChildrenToFetch + " most recent records"));
                }
                catch (OperationCanceledException)//user cancels
                {
                    results = new ArchivalDataLoadInfo[0];
                }

                _populateLoadHistoryResults = results;
            }
            catch (Exception exception)
            {
                ragSmiley1.Fatal(exception);
            }
        }

        private void PopulateLoadHistory()
        {
            //it's already doing it...
            if (_populateLoadHistory.IsBusy)
                return;

            //clear the tree
            ClearObjects();

            if (LoadMetadata == null)
                return;


            //cancel any running workers
            AbortWorkers();

            //tell user that we are loading
            llLoading.Visible = true;
            pbLoading.Visible = true;

            //clear the results
            _populateLoadHistoryResults = new ArchivalDataLoadInfo[0];

            _populateLoadHistoryCancel = new CancellationTokenSource();
            _populateLoadHistory.RunWorkerAsync();
        }


        private void AbortWorkers()
        {
            if (_populateLoadHistory.IsBusy)
                _populateLoadHistory.CancelAsync();

            if (_populateLoadHistoryCancel != null)
                _populateLoadHistoryCancel.Cancel();
        }

        private void llLoading_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            AbortWorkers();
        }


        public void ApplyFilter(string filter)
        {
            treeView1.ModelFilter = new TextMatchFilter(treeView1, filter,StringComparison.CurrentCultureIgnoreCase);
            
        }

        private void treeView1_ColumnRightClick(object sender, CellRightClickEventArgs e)
        {
            var RightClickMenu = new ContextMenuStrip();

            var tli = e.Model as ArchivalTableLoadInfo;
            var category = e.Model as LoadEventsTreeView_Category;

            if (category != null)
            {
                var cmd = new ExecuteCommandViewLoggedData(_activator,category.AssociatedTable,new LogViewerFilter(){Run = category.RunId});
                RightClickMenu.Items.Add(new AtomicCommandMenuItem(cmd, _activator));
            }

            if (tli != null && _loadMetadata != null)
            {
                //if it is not a freaky temp table
                if (!tli.TargetTable.EndsWith("_STAGING") && !tli.TargetTable.EndsWith("_RAW"))
                {
                    //if there are inserts
                    if (
                        (tli.Inserts != null && tli.Inserts > 0 )
                        ||
                        (tli.Updates != null && tli.Updates > 0 )
                        )
                        RightClickMenu.Items.Add("View Inserts/Updates", null, (a, b) => new ViewInsertsAndUpdatesDialog(tli, _loadMetadata.GetDistinctTableInfoList(true)).Show());
                }
            }

            var fatalError = e.Model as ArchivalFatalError;

            if (fatalError != null && _logManager != null)
            {
                var toResolve = treeView1.SelectedObjects.OfType<ArchivalFatalError>().ToArray();
                RightClickMenu.Items.Add("Resolve Fatal Error(s)", null, (a, b) =>
                {
                    var resolve = new ResolveFatalErrors(_logManager, toResolve);
                    resolve.RepositoryLocator = RepositoryLocator;
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
            var dli = o as ArchivalDataLoadInfo;
            var cat = o as LoadEventsTreeView_Category;

            if (o == null)
                return;
            
            if(dli != null)
                new ExecuteCommandViewLoggedData(_activator,LoggingTables.DataLoadRun,new LogViewerFilter(){Run = dli.ID}).Execute();
            else if (cat != null)
                new ExecuteCommandViewLoggedData(_activator, cat.AssociatedTable, new LogViewerFilter() { Run = cat.RunId}).Execute();
        }

        private void treeView1_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.C && e.Control)
            {
                var selectedObjects = treeView1.SelectedObjects;

                StringBuilder sb = new StringBuilder();

                foreach (var o in selectedObjects)
                    sb.AppendLine(o.ToString());

                //We manually implement this here because the default TreeView will only copy 340? characters... very weird but hey Windows Forms
                if (sb.Length != 0)
                    Clipboard.SetText(sb.ToString());
                
            }
        }

        public override void SetDatabaseObject(IActivateItems activator, LoadMetadata databaseObject)
        {
            base.SetDatabaseObject(activator,databaseObject);
            ragSmiley1.Reset();

            LoadMetadata = databaseObject;
        }

        private void tbFilter_TextChanged(object sender, EventArgs e)
        {
            ApplyFilter(tbFilter.Text);
        }
    }

    [TypeDescriptionProvider(typeof(AbstractControlDescriptionProvider<LoadEventsTreeView_Design, UserControl>))]
    public abstract class LoadEventsTreeView_Design : RDMPSingleDatabaseObjectControl<LoadMetadata>
    {
    }
}

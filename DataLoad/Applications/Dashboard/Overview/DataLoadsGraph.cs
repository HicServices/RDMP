using System;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Threading;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;
using BrightIdeasSoftware;
using CatalogueLibrary.Data;
using CatalogueLibrary.Data.Dashboarding;
using CatalogueLibrary.Data.DataLoad;
using CatalogueManager.DashboardTabs.Construction;
using CatalogueManager.Icons.IconProvision;
using CatalogueManager.ItemActivation;
using CatalogueManager.ItemActivation.Emphasis;
using CatalogueManager.Refreshing;
using CatalogueManager.TestsAndSetup.ServicePropogation;
using Dashboard.CommandExecution.AtomicCommands;
using HIC.Logging;
using HIC.Logging.PastEvents;
using ReusableUIComponents;

namespace Dashboard.Overview
{
    /// <summary>
    /// Displays a graph showing how many of your data loads passed the last time they were run and which data loads are currently failing.  If you have not configured any data
    /// loads yet then this control will be blank.
    /// </summary>
    public partial class DataLoadsGraph : RDMPUserControl, IDashboardableControl
    {
        private DataLoadsGraphObjectCollection _collection;
        private IActivateItems _activator;
        
        public DataLoadsGraph()
        {
            InitializeComponent();

            SetupOlvDelegates();

            olvDataLoads.AlwaysGroupByColumn = olvStatus;
            olvDataLoads.UseCellFormatEvents = true;
            olvDataLoads.FormatCell += olvDataLoads_FormatCell;

            olvViewLog.AspectGetter += (s) => "View Log";
        }

        private void SetupOlvDelegates()
        {
            olvName.ImageGetter = delegate
            {
                return _activator.CoreIconProvider.GetImage(RDMPConcept.LoadMetadata);
            };

            olvDataLoads.ButtonClick += delegate(object sender, CellClickEventArgs e)
            {
                var loadSummary = (DataLoadsGraphResult)e.Model;
                var metadata =
                    _activator.RepositoryLocator.CatalogueRepository.GetAllObjects<LoadMetadata>()
                        .SingleOrDefault(m => m.ID == loadSummary.ID);

                if (metadata != null)
                    new ExecuteCommandViewLoadMetadataLogs(_activator).SetTarget(metadata).Execute();
            };

            olvDataLoads.DoubleClick += delegate(object sender, EventArgs args)
            {
                var loadSummary = (DataLoadsGraphResult)olvDataLoads.GetItem(olvDataLoads.SelectedIndex).RowObject;
                if (loadSummary != null)
                {
                    var metadata =
                        _activator.RepositoryLocator.CatalogueRepository.GetAllObjects<LoadMetadata>()
                            .SingleOrDefault(m => m.ID == loadSummary.ID);

                    if (metadata != null)
                        _activator.RequestItemEmphasis(sender, new EmphasiseRequest(metadata));
                }
            };
        }

        void olvDataLoads_FormatCell(object sender, FormatCellEventArgs e)
        {
            if (e.ColumnIndex == olvStatus.Index)
                e.SubItem.ForeColor = ((DataLoadsGraphResult) e.Model).Status != DataLoadsGraphResultStatus.Succeeding
                    ? Color.Red
                    : Color.Black;
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            RefreshChartAsync();
        }

        public void RefreshChartAsync()
        {
            if (VisualStudioDesignMode)
                return;

            pbLoading.Visible = true;
            olvDataLoads.ClearObjects();
            ragSmiley1.Reset();
            ragSmiley1.SetVisible(false);

            Thread t = new Thread(() =>
            {
                try
                {
                    int countManualLoadsuccessful = 0;
                    int countManualLoadFailure = 0;
                    
                    foreach (LoadMetadata metadata in _activator.RepositoryLocator.CatalogueRepository.GetAllObjects<LoadMetadata>())
                    {
                        try
                        {
                            LogManager logManager;

                            try
                            {
                                //get the logging connection
                                logManager = new LogManager(metadata.GetDistinctLoggingDatabaseSettings());
                            }
                            catch (NotSupportedException e)
                            {
                                //sometimes a load metadata won't have any catalogues so we can't process it's log history
                                if(e.Message.Contains("does not have any Catalogues associated with it"))
                                    continue;
                                
                                throw;
                            }

                            ArchivalDataLoadInfo archivalDataLoadInfo = logManager.GetLoadStatusOf(PastEventType.MostRecent, metadata.GetDistinctLoggingTask());

                            bool lastLoadWasError;

                            var loadSummary = new DataLoadsGraphResult
                            {
                                ID = metadata.ID,
                                Name = metadata.Name,
                            };

                            if (archivalDataLoadInfo == null)
                            {
                                this.Invoke(new MethodInvoker(() =>
                                {
                                    loadSummary.Status = DataLoadsGraphResultStatus.NeverBeenRun;
                                    loadSummary.LastRun = "Never";
                                    olvDataLoads.AddObject(loadSummary);

                                    ResizeColumns();
                                }));
                                continue; //has never been run (or has had test runs only)
                            }
                            
                            lastLoadWasError = archivalDataLoadInfo.Errors.Any() || archivalDataLoadInfo.EndTime == null;

                            //while we were fetching data from database the form was closed
                            if (IsDisposed || !IsHandleCreated)
                                return;

                            if (lastLoadWasError)
                                countManualLoadFailure++;
                            else
                                countManualLoadsuccessful++;

                            this.Invoke(new MethodInvoker(() =>
                            {
                                loadSummary.Status = lastLoadWasError ? DataLoadsGraphResultStatus.Failing : DataLoadsGraphResultStatus.Succeeding;
                                loadSummary.LastRun = archivalDataLoadInfo.EndTime.ToString();
                                
                                olvDataLoads.AddObject(loadSummary);

                                ResizeColumns();
                            }));
                        }
                        catch (Exception e)
                        {
                            ragSmiley1.Fatal(e);
                            this.Invoke(new MethodInvoker(() =>
                            {
                                pbLoading.Visible = false;
                            }));
                        }
                    }


                    //if there have been no loads at all ever
                    if (countManualLoadsuccessful == 0 && countManualLoadFailure == 0)
                    {
                        this.Invoke(new MethodInvoker(() =>
                        {
                            lblNoDataLoadsFound.Visible = true;
                            chart1.Visible = false;
                            pbLoading.Visible = false;
                        }));
                        
                        return;
                    }

                    DataTable dt = new DataTable();
                    dt.Columns.Add("Category");
                    dt.Columns.Add("NumberOfDataLoadsAtStatus");

                    dt.Rows.Add(new object[] { "Manual Successful", countManualLoadsuccessful });
                    dt.Rows.Add(new object[] { "Manual Fail", countManualLoadFailure });


                    this.Invoke(new MethodInvoker(() =>
                    {
                        chart1.Series[0].XValueMember = "Category";
                        chart1.Series[0].YValueMembers = "NumberOfDataLoadsAtStatus";

                        chart1.DataSource = dt;
                        chart1.DataBind();

                        chart1.Series[0].Points[0].Color = Color.Green;
                        chart1.Series[0].Points[1].Color = Color.Red;

                        var max = new int[]
                        {
                            countManualLoadFailure,
                            countManualLoadsuccessful
                        }.Max();

                        int gridMarkEvery = max == 0 ? 1 : Math.Max(max/10, 1);


                        chart1.ChartAreas[0].AxisY.Interval = gridMarkEvery;

                        chart1.ChartAreas[0].AxisY.MajorGrid.Interval = gridMarkEvery;
                        chart1.ChartAreas[0].AxisY.MajorTickMark.Interval = gridMarkEvery;
                        chart1.ChartAreas[0].AxisY.MajorTickMark.IntervalOffset = 0;
                        
                        chart1.ChartAreas[0].AxisY.IsMarginVisible = false;


                        chart1.ChartAreas[0].AxisY.MinorGrid.Enabled = false;
                        chart1.ChartAreas[0].AxisY.MinorTickMark.Enabled = false;

                        pbLoading.Visible = false;
                    }));
                }
                catch (Exception e)
                {
                    ragSmiley1.Fatal(e);
                    this.Invoke(new MethodInvoker(() =>
                    {
                        pbLoading.Visible = false;
                    }));
                }

            });
            t.SetApartmentState(ApartmentState.STA);
            t.Start();
        }

        private void ResizeColumns()
        {
            foreach (ColumnHeader column in olvDataLoads.Columns)
                column.Width = -2; //magical (apparently it resizes to max width of content or header)
        }

        public void RefreshBus_RefreshObject(object sender, RefreshObjectEventArgs e)
        {
            
        }

        public string GetTabName()
        {
            return Text;
        }

        public void SetCollection(IActivateItems activator, IPersistableObjectCollection collection)
        {
            _activator = activator;
            _collection = (DataLoadsGraphObjectCollection)collection;

            if(IsHandleCreated && !IsDisposed)
                RefreshChartAsync();
        }

        public IPersistableObjectCollection GetCollection()
        {
            return _collection;
        }

        public void NotifyEditModeChange(bool isEditModeOn)
        {
            
        }

        public IPersistableObjectCollection ConstructEmptyCollection(DashboardControl databaseRecord)
        {
            return new DataLoadsGraphObjectCollection();
        }
    }
}

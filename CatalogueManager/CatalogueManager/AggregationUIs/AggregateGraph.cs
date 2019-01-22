using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.Common;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;
using CatalogueLibrary.Data;
using CatalogueLibrary.Data.Aggregation;
using CatalogueLibrary.QueryBuilding;
using CatalogueLibrary.Reports;
using CatalogueManager.CommandExecution.AtomicCommands;
using CatalogueManager.ItemActivation;
using CatalogueManager.TestsAndSetup.ServicePropogation;
using CsvHelper;
using FAnsi.Discovery.QuerySyntax.Aggregation;
using QueryCaching.Aggregation;
using QueryCaching.Aggregation.Arguments;
using ReusableLibraryCode;
using ReusableLibraryCode.Checks;
using ReusableLibraryCode.DataAccess;
using ReusableUIComponents;
using ReusableUIComponents.Dialogs;
using ReusableUIComponents.ScintillaHelper;
using ScintillaNET;

namespace CatalogueManager.AggregationUIs
{

    public delegate void DataTableHandler(object sender, DataTable dt);
    
    /// <summary>
    /// Executes and renders an AggregateConfiguration as a graph.  An AggregateConfiguration is a GROUP BY sql statement.  You can view the statement executed through the 
    /// 'SQL Code' tab.  In the Data tab you can view the raw data returned by the GROUP BY query (And also if applicable you can cache the results for displaying on the website).
    /// 
    /// <para>The Graph will do it's best to render something appropriate to the selected Dimensions, Pivot, Axis etc but there are limits.  If the graph doesn't look the way you want
    /// try putting a year/month axis onto the AggregateConfiguration.  Also make sure that your PIVOT Dimension doesn't have 1000 values or the graph will be completely incomprehensible.</para>
    /// </summary>
    public partial class AggregateGraph : AggregateGraph_Design
    {
        
        public Scintilla QueryEditor { get;private set; }
        ServerDefaults _defaults;

        public int Timeout { get; set; }

        public event DataTableHandler GraphTableRetrieved;

        private AggregateConfiguration _aggregateConfiguration;

        public AggregateGraph()
        {
            InitializeComponent();

            llCancel.Visible = false;

            #region Query Editor setup
            
            if(VisualStudioDesignMode)
                return;

            QueryEditor = new ScintillaTextEditorFactory().Create();

            QueryEditor.Text = "/*Graph load has not been attempted yet, wait till the system calls LoadGraphAsync() or LoadGraph()*/";

            QueryEditor.ReadOnly = true;
            
            tpCode.Controls.Add(QueryEditor);
            #endregion QueryEditor

            SetToolbarButtonsEnabled(true);

            tbTimeout.Text = "300";
            Timeout = 300;
        }

        private void SetToolbarButtonsEnabled(bool enabled)
        {
            if (InvokeRequired)
            {
                Invoke(new MethodInvoker(()=>SetToolbarButtonsEnabled(enabled)));
                return;
            }

            btnSaveImages.Enabled = enabled && _dt != null && _dt.Rows.Count>0;
            btnClipboard.Enabled = enabled && _dt != null && _dt.Rows.Count > 0;
            btnResendQuery.Enabled = enabled;
        }

        public AggregateConfiguration AggregateConfiguration
        {
            get { return _aggregateConfiguration; }
        }

        public void AbortLoadGraph()
        {
            if (InvokeRequired)
            {
                Invoke(new MethodInvoker(AbortLoadGraph));
                return;
            }

            if (_cmd != null && _cmd.Connection != null && _cmd.Connection.State != ConnectionState.Closed)
                _cmd.Cancel();

            pbLoading.Visible = false;
            llCancel.Visible = false;
        }

        public Exception Exception { get; private set; }
        public bool Crashed { get; private set; }
        public bool Done { get; private set; }
        
        private Task _loadTask;

        public void LoadGraphAsync()
        {
            //it is already executing
            if (_loadTask != null && !_loadTask.IsCompleted) 
                return;

            if (chart1.IsDisposed || chart1.Disposing)
                return;

            SetToolbarButtonsEnabled(false);
            Done = false;
            Crashed = false;
            Exception = null; 

            chart1.Visible = false;

            chart1.Series.Clear();

            ragSmiley1.Visible = false;
            chart1.DataSource = null;

            if (AggregateConfiguration == null)
                return;

            pbLoading.Visible = true;
            llCancel.Visible = true;

            btnCache.Enabled = false;

            label1.ForeColor = Color.Black;
            label1.Text = GetDescription();

            _loadTask = new Task(LoadGraph);
            _loadTask.Start();
        }

        protected virtual string GetDescription()
        {
            return string.IsNullOrWhiteSpace(AggregateConfiguration.Description)
                ? "No description"
                : AggregateConfiguration.Description;
        }

        DbCommand _cmd;

         ChartDashStyle[] StyleList = new ChartDashStyle[]
         {
             ChartDashStyle.Solid,
             ChartDashStyle.Dash,
             ChartDashStyle.Dot,
             ChartDashStyle.DashDot,
             ChartDashStyle.DashDotDot
         };

        private DataTable _dt;

        private void LoadGraph()
        {
            try
            {
                int timeout = 10000;

                while (!IsHandleCreated && timeout > 0)
                {
                    timeout -= 100;
                    Thread.Sleep(100);

                    if (timeout <= 0)
                        throw new TimeoutException(
                            "Window Handle was not created on AggregateGraph control after 10 seconds of calling LoadGraph!");
                }
                
                this.Invoke(new MethodInvoker(() =>
                {
                    lblLoadStage.Visible = true;
                    lblLoadStage.Text = "Generating Query...";
                }));

                AggregateContinuousDateAxis axis = AggregateConfiguration.GetAxisIfAny();
                AggregateBuilder builder = GetQueryBuilder(AggregateConfiguration);

                UpdateQueryViewerScreenWithQuery(builder.SQL);

                var countColumn = builder.SelectColumns.FirstOrDefault(c => c.IColumn is AggregateCountColumn);

                var server =
                    AggregateConfiguration.Catalogue.GetDistinctLiveDatabaseServer(
                        DataAccessContext.InternalDataProcessing,true);

                this.Invoke(new MethodInvoker(() => { lblLoadStage.Text = "Connecting To Server..."; }));

                using (var con = server.GetConnection())
                {
                    con.Open();
                    
                    _cmd = server.GetCommand(builder.SQL, con);
                    _cmd.CommandTimeout = Timeout;

                    _dt = new DataTable();

                    this.Invoke(new MethodInvoker(() => { lblLoadStage.Text = "Executing Query..."; }));

                    DbDataAdapter adapter = server.GetDataAdapter(_cmd);
                    adapter.Fill(_dt);
                    _cmd = null;



                    if (_dt.Rows.Count == 0)
                        throw new Exception("Query Returned No Rows");


                    //setup the heatmap if there is a pivot
                    if (_dt.Columns.Count > 2 && AggregateConfiguration.PivotOnDimensionID != null)
                        this.Invoke(new MethodInvoker(() => heatmapUI.SetDataTable(_dt)));
                    else
                        this.Invoke(new MethodInvoker(() => heatmapUI.Clear()));


                    if (GraphTableRetrieved != null)
                        GraphTableRetrieved(this, _dt);

                    if (_dt.Columns.Count < 2)
                        throw new NotSupportedException("Aggregates must have 2 columns at least");

                    //Invoke onto main UI thread so we can setup the chart
                    this.Invoke(new MethodInvoker(() =>
                    {
                        PopulateGraphResults(countColumn, axis);
                        Done = true;
                    }));
                }
             
                Invoke(new MethodInvoker(() => { lblLoadStage.Text = "Crashed"; }));
                
                ShowHeatmapTab(heatmapUI.HasDataTable());
            }
            catch (Exception e)
            {
                //don't bother if it is closing / closed
                if (IsDisposed)
                    return;

                Crashed = true;
                Exception = e; 

                ragSmiley1.SetVisible(true);
                ragSmiley1.Fatal(e);
                
                AbortLoadGraph();
                
                SetToolbarButtonsEnabled(true);
                Done = true;

            }
        }

        private void PopulateGraphResults(QueryTimeColumn countColumn, AggregateContinuousDateAxis axis)
        {
            bool haveSetSource = false;

            //last column is always the X axis, then for each column before it add a series with Y values coming from that column
            for (int i = 0; i < _dt.Columns.Count - 1; i++)
            {
                int index = i;


                if (!haveSetSource)
                {
                    try
                    {
                        dataGridView1.DataSource = _dt;
                        lblCannotLoadData.Visible = false;
                    }
                    catch (Exception e)
                    {
                        lblCannotLoadData.Visible = true;
                        lblCannotLoadData.Text = "Could not load data table:" + e.Message;
                        dataGridView1.DataSource = null;
                    }

                    chart1.DataSource = _dt;
                    haveSetSource = true;
                }

                if (countColumn != null)
                    try
                    {
                        chart1.ChartAreas[0].AxisY.Title = countColumn.IColumn.GetRuntimeName();
                    }
                    catch (Exception)
                    {
                        chart1.ChartAreas[0].AxisY.Title = "Count";
                        //sometimes you can't get a runtime name e.g. it is count(*) with no alias
                    }

                chart1.ChartAreas[0].AxisX.IsMarginVisible = false;
                chart1.ChartAreas[0].AxisY.IsMarginVisible = false;

                if (axis != null)
                {
                    switch (axis.AxisIncrement)
                    {
                        case AxisIncrement.Day:
                            chart1.ChartAreas[0].AxisX.Title = "Day";


                            if (_dt.Rows.Count <= 370)
                            {
                                //by two months
                                chart1.ChartAreas[0].AxisX.MinorGrid.Interval = 7;
                                chart1.ChartAreas[0].AxisX.MajorGrid.Interval = 14;
                                chart1.ChartAreas[0].AxisX.Interval = 14;
                            }
                            else
                            {
                                chart1.ChartAreas[0].AxisX.MajorGrid.Interval = 28;
                                chart1.ChartAreas[0].AxisX.MinorGrid.Interval = 7;
                                chart1.ChartAreas[0].AxisX.Interval = 28;
                            }


                            chart1.ChartAreas[0].AxisX.MinorGrid.LineWidth = 1;
                            chart1.ChartAreas[0].AxisX.MinorGrid.LineDashStyle = ChartDashStyle.Dot;
                            chart1.ChartAreas[0].AxisX.MinorGrid.Enabled = true;

                            break;
                        case AxisIncrement.Month:

                            //x axis is the number of rows in the data table
                            chart1.ChartAreas[0].AxisX.Title = "Month";


                            //if it is less than or equal to ~3 years at once - with 
                            if (_dt.Rows.Count <= 40)
                            {
                                //by month
                                chart1.ChartAreas[0].AxisX.MinorGrid.Interval = 1;
                                chart1.ChartAreas[0].AxisX.MajorGrid.Interval = 3;
                                chart1.ChartAreas[0].AxisX.Interval = 1;
                            }
                            else
                            {
                                //by year
                                chart1.ChartAreas[0].AxisX.MinorGrid.Interval = 6;
                                chart1.ChartAreas[0].AxisX.MajorGrid.Interval = 12;
                                chart1.ChartAreas[0].AxisX.Interval = 12;
                            }

                            chart1.ChartAreas[0].AxisX.MinorGrid.LineWidth = 1;
                            chart1.ChartAreas[0].AxisX.MinorGrid.LineDashStyle = ChartDashStyle.Dot;
                            chart1.ChartAreas[0].AxisX.MinorGrid.Enabled = true;


                            break;
                        case AxisIncrement.Year:

                            chart1.ChartAreas[0].AxisX.Title = "Year";

                            if (_dt.Rows.Count <= 10)
                            {
                                chart1.ChartAreas[0].AxisX.MinorGrid.Interval = 1;
                                chart1.ChartAreas[0].AxisX.MajorGrid.Interval = 1;
                                chart1.ChartAreas[0].AxisX.Interval = 1;
                            }
                            else
                            {
                                chart1.ChartAreas[0].AxisX.MinorGrid.Interval = 1;
                                chart1.ChartAreas[0].AxisX.MajorGrid.Interval = 5;
                                chart1.ChartAreas[0].AxisX.Interval = 5;
                            }

                            chart1.ChartAreas[0].AxisX.MinorGrid.LineWidth = 1;
                            chart1.ChartAreas[0].AxisX.MinorGrid.LineDashStyle = ChartDashStyle.Dot;
                            chart1.ChartAreas[0].AxisX.MinorGrid.Enabled = true;

                            break;

                        case AxisIncrement.Quarter:

                            //x axis is the number of rows in the data table
                            chart1.ChartAreas[0].AxisX.Title = "Quarter";


                            //if it is less than or equal to ~3 years at once - with 
                            if (_dt.Rows.Count <= 12)
                            {
                                //by quarter
                                chart1.ChartAreas[0].AxisX.MinorGrid.Interval = 1;
                                chart1.ChartAreas[0].AxisX.MajorGrid.Interval = 4;
                                chart1.ChartAreas[0].AxisX.Interval = 4;
                            }
                            else
                            {
                                //by year
                                chart1.ChartAreas[0].AxisX.MinorGrid.Interval = 1;
                                chart1.ChartAreas[0].AxisX.MajorGrid.Interval = 16;
                                chart1.ChartAreas[0].AxisX.Interval = 16;
                            }

                            chart1.ChartAreas[0].AxisX.MinorGrid.LineWidth = 1;
                            chart1.ChartAreas[0].AxisX.MinorGrid.LineDashStyle = ChartDashStyle.Dot;
                            chart1.ChartAreas[0].AxisX.MinorGrid.Enabled = true;


                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                }
                else
                    chart1.ChartAreas[0] = new ChartArea(); //reset it

                chart1.ChartAreas[0].AxisY.MinorGrid.Enabled = true;
                chart1.ChartAreas[0].AxisY.MinorGrid.LineDashStyle = ChartDashStyle.Dot;
                chart1.ChartAreas[0].AxisY.LabelStyle.Format = "{0:#,#}";

                //avoid buffer overun
                if (index > chart1.Series.Count - 1)
                    chart1.Series.Add(new Series());

                chart1.Series[index].XValueMember = _dt.Columns[0].ColumnName;
                chart1.Series[index].YValueMembers = _dt.Columns[index + 1].ColumnName;

                if (axis != null)
                {
                    chart1.Series[index].ChartType = SeriesChartType.Line;

                    //alternate in rotating style the various lines on the graph
                    chart1.Series[index].BorderDashStyle = StyleList[index%StyleList.Length];
                    chart1.Series[index].BorderWidth = 2;
                }
                else
                {
                    chart1.Series[index].ChartType = SeriesChartType.Column;


                    if (_dt.Columns[0].DataType == typeof(decimal) || _dt.Columns[0].DataType == typeof(int))
                    {
                        //by year
                        chart1.ChartAreas[0].AxisX.IntervalAutoMode = IntervalAutoMode.FixedCount;
                    }
                    else
                    {
                        chart1.ChartAreas[0].AxisX.Interval = 1;
                        chart1.ChartAreas[0].AxisX.LabelAutoFitMinFontSize = 8;
                    }
                    
                }

                //name series based on column 3 or the aggregate name
                chart1.Series[index].Name = _dt.Columns[index + 1].ColumnName;
            }

            lblLoadStage.Text = "Data Binding Chart (" + _dt.Columns.Count + " columns)";
            lblLoadStage.Refresh();

            chart1.DataBind();

            chart1.Visible = true;
            pbLoading.Visible = false;
            llCancel.Visible = false;
            lblLoadStage.Visible = false;

            //set publish enabledness to the enabledness of 
            btnCache.Enabled =
                _defaults.GetDefaultFor(
                    ServerDefaults.PermissableDefaults.WebServiceQueryCachingServer_ID) != null;
            btnClearFromCache.Enabled = false;

            //Make publish button enabledness be dependant on cache
            if (btnCache.Enabled)
            {
                //let them clear if there is a query caching server and the manager has cached results already
                btnClearFromCache.Enabled =
                    GetCacheManager()
                        .GetLatestResultsTableUnsafe(AggregateConfiguration,
                            AggregateOperation.ExtractableAggregateResults) != null;
            }

            SetToolbarButtonsEnabled(true);
        }

        private void ShowHeatmapTab(bool show)
        {
            if (InvokeRequired)
            {
                Invoke(new MethodInvoker(() => ShowHeatmapTab(show)));
                return;
            }

            if(show && !tabControl1.TabPages.Contains(tpHeatmap))
                    tabControl1.TabPages.Add(tpHeatmap);

            if(!show && tabControl1.TabPages.Contains(tpHeatmap))
                tabControl1.TabPages.Remove(tpHeatmap);
        }

        public void UpdateQueryViewerScreenWithQuery(string sql)
        {
            if (InvokeRequired)
            {
                Invoke(new MethodInvoker(() => UpdateQueryViewerScreenWithQuery(sql)));
                return;
            }

            QueryEditor.ReadOnly = false;
            QueryEditor.Text = sql;
            QueryEditor.ReadOnly = true;
        }

        protected virtual AggregateBuilder GetQueryBuilder(AggregateConfiguration aggregateConfiguration)
        {
            return aggregateConfiguration.GetQueryBuilder();
        }


        private string GetSeriesName(object o)
        {
            if (o == null || o == DBNull.Value)
                return "NULL";

            return o.ToString();
        }

        public void SaveTo(DirectoryInfo subdir, string nameOfFile, ICheckNotifier notifier, Dictionary<AggregateGraph, string> graphSaveLocations = null)
        {
            if (!Done)
            {
                notifier.OnCheckPerformed(
                    new CheckEventArgs(
                        "Graph could not be extracted to " + nameOfFile +
                        " because Done is false, possibly the graph has not been loaded yet or crashed?",
                        CheckResult.Fail));
                return;
            }

            string imgSavePath = Path.Combine(subdir.FullName, nameOfFile + ".png");
            string dataSavePath = Path.Combine(subdir.FullName, nameOfFile + ".csv");
            string querySavePath = Path.Combine(subdir.FullName, nameOfFile + ".sql");
            try
            {
                chart1.SaveImage(imgSavePath, ChartImageFormat.Png);
                notifier.OnCheckPerformed(new CheckEventArgs("Saved chart image to " + imgSavePath, CheckResult.Success));

                if(graphSaveLocations != null)
                    graphSaveLocations.Add(this,imgSavePath);
            }
            catch (Exception e)
            {
                notifier.OnCheckPerformed(new CheckEventArgs("Failed to save image to " + imgSavePath, CheckResult.Fail,e));
            }

            try
            {
                var dt = (DataTable) dataGridView1.DataSource;

                using (CsvWriter csvWriter = new CsvWriter(new StreamWriter(dataSavePath)))
                {
                    foreach (DataColumn column in dt.Columns)
                        csvWriter.WriteField(column.ColumnName);

                    csvWriter.NextRecord();

                    foreach (DataRow row in dt.Rows)
                    {
                        for (var i = 0; i < dt.Columns.Count; i++)
                            csvWriter.WriteField(row[i]);

                        csvWriter.NextRecord();
                    }
                }

                notifier.OnCheckPerformed(new CheckEventArgs("Saved chart data to " + dataSavePath, CheckResult.Success));
            }
            catch (Exception e)
            {
                notifier.OnCheckPerformed(new CheckEventArgs("Failed to save chart data to " + dataSavePath,CheckResult.Fail, e));
            }

            try
            {
                File.WriteAllText(querySavePath, QueryEditor.Text);
                notifier.OnCheckPerformed(new CheckEventArgs("Wrote SQL used to " + querySavePath, CheckResult.Success));
            }
            catch (Exception e)
            {
                notifier.OnCheckPerformed(new CheckEventArgs("Failed to save SQL query to " + querySavePath,CheckResult.Fail,e));
            }
        }

        private void llCancel_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            AbortLoadGraph();
        }

        private void btnClearFromCache_Click(object sender, EventArgs e)
        {
            try
            {
                GetCacheManager().DeleteCacheEntryIfAny(AggregateConfiguration,AggregateOperation.ExtractableAggregateResults);
                MessageBox.Show(
                    "Cached results deleted, they should no longer appear on the website (subject to website page level caching in IIS etc of course)");
                btnClearFromCache.Enabled = false;
            }
            catch (Exception exception)
            {
                ExceptionViewer.Show(exception);
            }

        }

        private CachedAggregateConfigurationResultsManager GetCacheManager()
        {
            return new CachedAggregateConfigurationResultsManager(_defaults.GetDefaultFor(ServerDefaults.PermissableDefaults.WebServiceQueryCachingServer_ID));
        }

        /// <summary>
        /// Normally you dont need to work about double subscriptions but this graph gets recycled during MetadataReport generation with different aggregates one
        /// after the other which violetates the 1 subscription per control rule (see base.SetDatabaseObject)
        /// </summary>
        private bool initialized = false;

        public override void SetDatabaseObject(IActivateItems activator, AggregateConfiguration databaseObject)
        {
            if (!initialized)
            {
                base.SetDatabaseObject(activator,databaseObject);
                initialized = true;
            }

            SetAggregate(activator,databaseObject);

            activator.Theme.ApplyTo(toolStrip1);
        }

        /// <summary>
        /// Loads the Graph without establishing a lifetime subscription to refresh events (use if you are a derrived class who has it's own subscription or if you plan
        /// to load multiple different graphs into this control one after the other).
        /// </summary>
        /// <param name="graph"></param>
        /// <param name="b"></param>
        public void SetAggregate(IActivateItems activator,AggregateConfiguration graph)
        {
            //graphs cant edit so no need to even record refresher/activator
            _aggregateConfiguration = graph;
            
            SetItemActivator(activator);

            SetupRibbon();
        }

        private void SetupRibbon()
        {
            ClearToolStrip();

            foreach (var o in GetRibbonObjects())
            {
                if(o is string)
                    Add((string)o);
                else
                if (o is DatabaseEntity)
                    Add(new ExecuteCommandShow(_activator,(DatabaseEntity)o,0,true));
                else
                    throw new NotSupportedException("GetRibbonObjects can only return strings or DatabaseEntity objects, object '" + o + "' is not valid because it is a '" + o.GetType().Name + "'");
            }
        }


        protected virtual object[] GetRibbonObjects()
        {
            return new[] {_aggregateConfiguration};
        }

        protected override void OnRepositoryLocatorAvailable()
        {
            base.OnRepositoryLocatorAvailable();
            
            if (VisualStudioDesignMode)
                return;

            _defaults = new ServerDefaults(RepositoryLocator.CatalogueRepository);
        }

        public IEnumerable<BitmapWithDescription> GetImages()
        {
            var b = new Bitmap(chart1.Width, chart1.Height);
            chart1.DrawToBitmap(b, new Rectangle(new Point(0, 0), new Size(chart1.Width, chart1.Height)));
            
            yield return new BitmapWithDescription(b,AggregateConfiguration.Name,AggregateConfiguration.Description);

            if (heatmapUI.HasDataTable())
                yield return new BitmapWithDescription(heatmapUI.GetImage(800),null,null);
        }
        
        private void btnSaveImages_Click(object sender, EventArgs e)
        {
            SaveFileDialog sfd = new SaveFileDialog();
            sfd.FileName = "Chart.jpg";
            sfd.Filter = "Jpeg|*.jpg";
            if (sfd.ShowDialog() == DialogResult.OK)
            {
                chart1.SaveImage(sfd.FileName, ChartImageFormat.Jpeg);

                if (heatmapUI.HasDataTable())
                {
                    var directory = Path.GetDirectoryName(sfd.FileName);
                    var filename = Path.GetFileNameWithoutExtension(sfd.FileName) + "_HeatMap.jpg";
                    string heatmapPath = Path.Combine(directory, filename);

                    heatmapUI.SaveImage(heatmapPath, ImageFormat.Jpeg);
                }
                
                UsefulStuff.GetInstance().ShowFileInWindowsExplorer(new FileInfo(sfd.FileName));
            }
        }

        private void btnClipboard_Click(object sender, EventArgs e)
        {
            string s = UsefulStuff.GetInstance().DataTableToHtmlDataTable(_dt);

            var formatted = UsefulStuff.GetInstance().GetClipboardFormatedHtmlStringFromHtmlString(s);
            
            Clipboard.SetText(formatted,TextDataFormat.Html);
        }

        private void btnResendQuery_Click(object sender, EventArgs e)
        {
            LoadGraphAsync();
        }

        private void tbTimeout_TextChanged(object sender, EventArgs e)
        {
            try
            {
                Timeout = int.Parse(tbTimeout.Text);
                tbTimeout.ForeColor = Color.Black;
            }
            catch (Exception)
            {
                tbTimeout.ForeColor = Color.Red;
            }
        }

        public override string GetTabName()
        {
            return "Graph:" + base.GetTabName();
        }

        private void btnCache_Click(object sender, EventArgs e)
        {
            try
            {
                CachedAggregateConfigurationResultsManager cacheManager = GetCacheManager();

                var args = new CacheCommitExtractableAggregate(AggregateConfiguration, QueryEditor.Text, (DataTable)dataGridView1.DataSource,Timeout);
                cacheManager.CommitResults(args);

                var result = cacheManager.GetLatestResultsTable(AggregateConfiguration,AggregateOperation.ExtractableAggregateResults, QueryEditor.Text);

                if(result == null)
                    throw new NullReferenceException("CommitResults passed but GetLatestResultsTable returned false (when we tried to refetch the table name from the cache)");

                MessageBox.Show("DataTable successfully submitted to:" + result.GetFullyQualifiedName());
                btnClearFromCache.Enabled = true;
            }
            catch (Exception exception)
            {
                ExceptionViewer.Show(exception);
            }
        }
    }

    [TypeDescriptionProvider(typeof(AbstractControlDescriptionProvider<AggregateGraph_Design, UserControl>))]
    public abstract class AggregateGraph_Design : RDMPSingleDatabaseObjectControl<AggregateConfiguration>
    {
    }
}

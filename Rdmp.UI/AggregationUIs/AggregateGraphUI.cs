// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.Common;
using System.Drawing;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;
using FAnsi.Discovery.QuerySyntax.Aggregation;
using Rdmp.Core.CommandExecution.AtomicCommands;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Curation.Data.Aggregation;
using Rdmp.Core.Curation.Data.Defaults;
using Rdmp.Core.Icons.IconProvision;
using Rdmp.Core.QueryBuilding;
using Rdmp.Core.QueryCaching.Aggregation;
using Rdmp.Core.QueryCaching.Aggregation.Arguments;
using Rdmp.Core.Reports;
using Rdmp.Core.ReusableLibraryCode;
using Rdmp.Core.ReusableLibraryCode.Checks;
using Rdmp.Core.ReusableLibraryCode.DataAccess;
using Rdmp.Core.ReusableLibraryCode.Extensions;
using Rdmp.UI.AggregationUIs.Advanced;
using Rdmp.UI.ItemActivation;
using Rdmp.UI.ScintillaHelper;
using Rdmp.UI.SimpleDialogs;
using Rdmp.UI.TestsAndSetup.ServicePropogation;
using ScintillaNET;

namespace Rdmp.UI.AggregationUIs;

public delegate void DataTableHandler(object sender, DataTable dt);

/// <summary>
/// Executes and renders an AggregateConfiguration as a graph.  An AggregateConfiguration is a GROUP BY sql statement.  You can view the statement executed through the
/// 'SQL Code' tab.  In the Data tab you can view the raw data returned by the GROUP BY query (And also if applicable you can cache the results for displaying on the website).
/// 
/// <para>The Graph will do it's best to render something appropriate to the selected Dimensions, Pivot, Axis etc but there are limits.  If the graph doesn't look the way you want
/// try putting a year/month axis onto the AggregateConfiguration.  Also make sure that your PIVOT Dimension doesn't have 1000 values or the graph will be completely incomprehensible.</para>
/// </summary>
public partial class AggregateGraphUI : AggregateGraph_Design
{
    /// <summary>
    /// The maximum number of cells in a DataTable before we warn the user that rendering it is likely to hang
    /// up System.Windows.Forms.DataVisualization.Charting for a minutes/hours
    /// </summary>
    public const int MAXIMUM_CELLS_BEFORE_WARNING = 1_000_000;

    /// <summary>
    /// Set to true to suppress yes/no dialogues from showing e.g. if there is too much data
    /// (see <see cref="MAXIMUM_CELLS_BEFORE_WARNING"/>) to sensibly render.  If true then
    /// the sensible decision is taken e.g. to not try to render.
    /// 
    /// </summary>
    public bool Silent { get; set; }

    public Scintilla QueryEditor { get; private set; }

    public int Timeout
    {
        get => _timeoutControls.Timeout;
        set => _timeoutControls.Timeout = value;
    }

    public event DataTableHandler GraphTableRetrieved;

    private AggregateConfiguration _aggregateConfiguration;
    private readonly ToolStripMenuItem _miSaveImages = new("Save Image", FamFamFamIcons.disk.ImageToBitmap());

    private readonly ToolStripMenuItem _miCopyToClipboard =
        new("Copy to Clipboard", CatalogueIcons.Clipboard.ImageToBitmap());

    private readonly ToolStripMenuItem _miClipboardWord = new("Word Format");
    private readonly ToolStripMenuItem _miClipboardCsv = new("Comma Separated Format");
    private readonly ToolStripMenuItem _btnCache = new("Cache", FamFamFamIcons.picture_save.ImageToBitmap());
    private readonly ToolStripButton _btnResendQuery = new("Send Query", FamFamFamIcons.arrow_refresh.ImageToBitmap());
    private readonly ToolStripButton _btnRefreshData = new("Refresh Data", FamFamFamIcons.arrow_refresh.ImageToBitmap());
    private readonly ToolStripTimeout _timeoutControls = new();

    public AggregateGraphUI()
    {
        InitializeComponent();

        llCancel.Visible = false;

        #region Query Editor setup

        if (VisualStudioDesignMode)
            return;

        QueryEditor = new ScintillaTextEditorFactory().Create();

        QueryEditor.Text =
            "/*Graph load has not been attempted yet, wait till the system calls LoadGraphAsync() or LoadGraph()*/";

        QueryEditor.ReadOnly = true;

        tpCode.Controls.Add(QueryEditor);

        #endregion QueryEditor

        SetToolbarButtonsEnabled(true);

        dataGridView1.ColumnAdded += (s, e) => e.Column.FillWeight = 1;

        Timeout = 300;

        _miCopyToClipboard.DropDownItems.Add(_miClipboardWord);
        _miCopyToClipboard.DropDownItems.Add(_miClipboardCsv);

        _miClipboardWord.Click += ClipboardClick;
        _miClipboardWord.ToolTipText = "Copies data as HTML formatted (for pasting into Word / Excel etc)";
        _miClipboardCsv.Click += ClipboardClick;
        _miClipboardCsv.ToolTipText = "Copies data as CSV formatted";

        _btnResendQuery.Click += btnResendQuery_Click;
        _btnRefreshData.Click += btnRefreshData_Click;
        _miSaveImages.Click += MiSaveImagesClick;
        _btnCache.Click += btnCache_Click;


        _btnCache.Enabled = false;
    }

    private void SetToolbarButtonsEnabled(bool enabled)
    {
        if (InvokeRequired)
        {
            Invoke(new MethodInvoker(() => SetToolbarButtonsEnabled(enabled)));
            return;
        }

        _miSaveImages.Enabled = enabled && _dt is { Rows.Count: > 0 };
        _miClipboardCsv.Enabled = enabled && _dt is { Rows.Count: > 0 };
        _miClipboardWord.Enabled = enabled && _dt is { Rows.Count: > 0 };
        _btnResendQuery.Enabled = enabled;
        _btnRefreshData.Enabled = enabled;
    }

    public AggregateConfiguration AggregateConfiguration => _aggregateConfiguration;

    public void AbortLoadGraph()
    {
        if (InvokeRequired)
        {
            Invoke(new MethodInvoker(AbortLoadGraph));
            return;
        }

        if (_cmd is { Connection: not null } && _cmd.Connection.State != ConnectionState.Closed)
            _cmd.Cancel();

        pbLoading.Visible = false;
        llCancel.Visible = false;
    }

    public Exception Exception { get; private set; }
    public bool Crashed { get; private set; }
    public bool Done { get; private set; }

    private Task _loadTask;

    public void ReloadDataBetweenDates(string startDate, string endDate)
    {
        _loadTask = new Task(() => LoadGraph(true, startDate, endDate));
        _loadTask.Start();
    }

    public void LoadGraphAsync()
    {
        //it is already executing
        if (_loadTask is { IsCompleted: false })
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

        _btnCache.Enabled = false;

        label1.ForeColor = Color.Black;
        label1.Text = GetDescription();

        _loadTask = new Task(() => LoadGraph());
        _loadTask.Start();
    }

    protected virtual string GetDescription() =>
        string.IsNullOrWhiteSpace(AggregateConfiguration.Description)
            ? "No description"
            : AggregateConfiguration.Description;

    private DbCommand _cmd;

    private readonly ChartDashStyle[] _styleList =
    {
        ChartDashStyle.Solid,
        ChartDashStyle.Dash,
        ChartDashStyle.Dot,
        ChartDashStyle.DashDot,
        ChartDashStyle.DashDotDot
    };

    private DataTable _dt;

    int GetQuarterName(DateTime myDate)
    {
        return (int)Math.Ceiling(myDate.Month / 3.0);
    }

    private DateTime ConvertAxisOverridetoDate(string axisOverride, AxisIncrement increment)
    {
        var overrideDate = DateTime.Parse(axisOverride);
        switch (increment)
        {
            case AxisIncrement.Day:
                return overrideDate.Date;
            case AxisIncrement.Month:
                return new DateTime(overrideDate.Year, overrideDate.Month, 1);
            case AxisIncrement.Quarter:
                return new DateTime(overrideDate.Year, (3 * GetQuarterName(overrideDate)) - 2, 1);
            case AxisIncrement.Year:
                return new DateTime(overrideDate.Year, 1, 1);
            default:
                return DateTime.Parse(axisOverride);
        }
    }

    private void LoadGraph(bool isRefresh = false, string startDateOverride = null, string endDateOverride = null)
    {
        try
        {
            var timeout = 10000;

            while (!IsHandleCreated && timeout > 0)
            {
                timeout -= 100;
                Thread.Sleep(100);

                if (timeout <= 0)
                    throw new TimeoutException(
                        "Window Handle was not created on AggregateGraph control after 10 seconds of calling LoadGraph!");
            }

            Invoke(new MethodInvoker(() =>
            {
                lblLoadStage.Visible = true;
                lblLoadStage.Text = "Generating Query...";
            }));

            var axis = AggregateConfiguration.GetAxisIfAny();
            var builder = GetQueryBuilder(AggregateConfiguration);
            if (startDateOverride != null)
                builder.AxisStartDateOverride = startDateOverride;
            if (endDateOverride != null)
                builder.AxisEndDateOverride = endDateOverride;

            if (isRefresh)
            {
                //wipe out data from dt that is between these dates
                var dateColumnName = "joinDt";// axis.AggregateDimension.ColumnInfo.Name;
                var columnIndex = _dt.Columns.IndexOf(dateColumnName);

                var incriment = axis.AxisIncrement;


                var startDate = ConvertAxisOverridetoDate(startDateOverride.Trim('\''), incriment);
                var endDate = ConvertAxisOverridetoDate(endDateOverride.Trim('\''), incriment);
                var rowsToDelete = _dt.Rows.Cast<DataRow>().Where(r =>
                {
                    //this could maybe be more efficient with some sort of lookup atfer the first time?
                    var currentDT = new DateTime();
                    if (incriment == AxisIncrement.Day)
                    {
                        DateTime.TryParseExact(r.ItemArray[columnIndex].ToString(), "yyyy-mm-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out currentDT);
                    }
                    if (incriment == AxisIncrement.Month)
                    {
                        DateTime.TryParseExact(r.ItemArray[columnIndex].ToString(), "yyyy-mm", CultureInfo.InvariantCulture, DateTimeStyles.None, out currentDT);
                    }
                    if (incriment == AxisIncrement.Quarter)
                    {
                        var year = r.ItemArray[columnIndex].ToString()[0..4];
                        var asString = r.ItemArray[columnIndex].ToString();
                        var quarter = Int32.Parse(asString.Substring(asString.Length - 1)) * 3;
                        DateTime.TryParseExact($"{year}-{quarter}", "yyyy-mm", CultureInfo.InvariantCulture, DateTimeStyles.None, out currentDT);

                    }
                    if (incriment == AxisIncrement.Year)
                    {
                        DateTime.TryParseExact(r.ItemArray[columnIndex].ToString(), "yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out currentDT);
                    }
                    return currentDT >= startDate && currentDT <= endDate;
                }).ToList();
                foreach (var row in rowsToDelete)
                {
                    _dt.Rows.Remove(row);
                }
                Console.WriteLine(dateColumnName);
            }
            UpdateQueryViewerScreenWithQuery(builder.SQL);

            var countColumn = builder.SelectColumns.FirstOrDefault(c => c.IColumn is AggregateCountColumn);

            var server =
                AggregateConfiguration.Catalogue.GetDistinctLiveDatabaseServer(
                    DataAccessContext.InternalDataProcessing, true);

            Invoke(new MethodInvoker(() => { lblLoadStage.Text = "Connecting To Server..."; }));

            using (var con = server.GetConnection())
            {
                con.Open();

                _cmd = server.GetCommand(builder.SQL, con);
                _cmd.CommandTimeout = Timeout;

                if (!isRefresh)
                {
                    _dt = new DataTable();
                }

                Invoke(new MethodInvoker(() => { lblLoadStage.Text = "Executing Query..."; }));

                var adapter = server.GetDataAdapter(_cmd);
                adapter.Fill(_dt);
                _cmd = null;

                //trim all leading/trailing whitespace from column
                if (!isRefresh)
                    foreach (DataColumn c in _dt.Columns)
                        c.ColumnName = c.ColumnName.Trim();

                if (_dt.Rows.Count == 0)
                    throw new Exception("Query Returned No Rows");

                AggregateConfiguration.AdjustGraphDataTable(_dt);

                //setup the heatmap if there is a pivot
                if (_dt.Columns.Count > 2 && AggregateConfiguration.PivotOnDimensionID != null)
                    Invoke(new MethodInvoker(() => heatmapUI.SetDataTable(_dt)));
                else
                    Invoke(new MethodInvoker(() => heatmapUI.Clear()));


                GraphTableRetrieved?.Invoke(this, _dt);

                if (_dt.Columns.Count < 2)
                    throw new NotSupportedException("Aggregates must have 2 columns at least");

                //Invoke onto main UI thread so we can setup the chart
                Invoke(new MethodInvoker(() =>
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
        var haveSetSource = false;
        if (chart1.Legends.Count == 0)
            chart1.Legends.Add(new Legend());

        chart1.Titles.Clear();
        chart1.Titles.Add(_aggregateConfiguration.Name);

        //last column is always the X axis, then for each column before it add a series with Y values coming from that column
        for (var i = 0; i < _dt.Columns.Count - 1; i++)
        {
            var index = i;

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
                    lblCannotLoadData.Text = $"Could not load data table:{e.Message}";
                    dataGridView1.DataSource = null;
                }

                chart1.DataSource = _dt;
                haveSetSource = true;
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
            {
                chart1.ChartAreas[0] = new ChartArea(); //reset it
                chart1.ChartAreas[0].AxisX.Title = _dt.Columns[0].ColumnName;
            }

            //Set the Y axis title
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

            chart1.ChartAreas[0].AxisY.MinorGrid.Enabled = true;
            chart1.ChartAreas[0].AxisY.MinorGrid.LineDashStyle = ChartDashStyle.Dot;
            chart1.ChartAreas[0].AxisY.LabelStyle.Format = "{0:#,#}";

            //avoid buffer overun
            if (index > chart1.Series.Count - 1)
                chart1.Series.Add(new Series());

            chart1.ChartAreas[0].AxisX.ScaleView.Zoomable = true;
            chart1.ChartAreas[0].CursorX.AutoScroll = true;
            chart1.ChartAreas[0].CursorX.IsUserSelectionEnabled = true;

            chart1.Series[index].XValueMember = _dt.Columns[0].ColumnName;
            chart1.Series[index].YValueMembers = _dt.Columns[index + 1].ColumnName;

            if (axis != null)
            {
                chart1.Series[index].ChartType = SeriesChartType.Line;

                //alternate in rotating style the various lines on the graph
                chart1.Series[index].BorderDashStyle = _styleList[index % _styleList.Length];
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

        //don't show legend if there's only one series
        if (chart1.Series.Count == 1)
            chart1.Legends.Clear();

        lblLoadStage.Text = $"Data Binding Chart ({_dt.Columns.Count} columns)";
        lblLoadStage.Refresh();

        var cells = _dt.Columns.Count * _dt.Rows.Count;

        var abandon = false;

        if (cells > MAXIMUM_CELLS_BEFORE_WARNING)
            if (Silent)
                throw new Exception($"Aborting data binding because there were {cells} cells in the graph data table");
            else
                abandon = !Activator.YesNo(
                    $"Data Table has {cells:n0} cells.  Are you sure you want to attempt to graph it?",
                    "Render Graph?");

        if (!abandon)
        {
            chart1.DataBind();
            chart1.Visible = true;
        }

        pbLoading.Visible = false;
        llCancel.Visible = false;
        lblLoadStage.Visible = false;

        //set publish enabledness to the enabledness of
        _btnCache.Enabled =
            Activator.RepositoryLocator.CatalogueRepository.GetDefaultFor(PermissableDefaults
                .WebServiceQueryCachingServer_ID) != null;
        btnClearFromCache.Enabled = false;

        //Make publish button enabledness be dependant on cache
        if (_btnCache.Enabled)
            //let them clear if there is a query caching server and the manager has cached results already
            btnClearFromCache.Enabled =
                GetCacheManager()
                    .GetLatestResultsTableUnsafe(AggregateConfiguration,
                        AggregateOperation.ExtractableAggregateResults) != null;

        SetToolbarButtonsEnabled(true);
    }

    private void ShowHeatmapTab(bool show)
    {
        if (InvokeRequired)
        {
            Invoke(new MethodInvoker(() => ShowHeatmapTab(show)));
            return;
        }

        if (show && !tabControl1.TabPages.Contains(tpHeatmap))
            tabControl1.TabPages.Add(tpHeatmap);

        if (!show && tabControl1.TabPages.Contains(tpHeatmap))
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

    protected virtual AggregateBuilder GetQueryBuilder(AggregateConfiguration aggregateConfiguration) =>
        aggregateConfiguration.GetQueryBuilder();


    private static string GetSeriesName(object o) => o == null || o == DBNull.Value ? "NULL" : o.ToString();

    public void SaveTo(DirectoryInfo subdir, string nameOfFile, ICheckNotifier notifier,
        Dictionary<AggregateGraphUI, string> graphSaveLocations = null)
    {
        if (!Done)
        {
            notifier.OnCheckPerformed(
                new CheckEventArgs(
                    $"Graph could not be extracted to {nameOfFile} because Done is false, possibly the graph has not been loaded yet or crashed?",
                    CheckResult.Fail));
            return;
        }

        var imgSavePath = Path.Combine(subdir.FullName, $"{nameOfFile}.png");
        var dataSavePath = Path.Combine(subdir.FullName, $"{nameOfFile}.csv");
        var querySavePath = Path.Combine(subdir.FullName, $"{nameOfFile}.sql");
        try
        {
            chart1.SaveImage(imgSavePath, ChartImageFormat.Png);
            notifier.OnCheckPerformed(new CheckEventArgs($"Saved chart image to {imgSavePath}", CheckResult.Success));

            graphSaveLocations?.Add(this, imgSavePath);
        }
        catch (Exception e)
        {
            notifier.OnCheckPerformed(new CheckEventArgs($"Failed to save image to {imgSavePath}", CheckResult.Fail,
                e));
        }

        try
        {
            var dt = (DataTable)dataGridView1.DataSource;
            using var dataSaveStream = new StreamWriter(dataSavePath);
            dt.SaveAsCsv(dataSaveStream);

            notifier.OnCheckPerformed(new CheckEventArgs($"Saved chart data to {dataSavePath}", CheckResult.Success));
        }
        catch (Exception e)
        {
            notifier.OnCheckPerformed(new CheckEventArgs($"Failed to save chart data to {dataSavePath}",
                CheckResult.Fail, e));
        }

        try
        {
            File.WriteAllText(querySavePath, QueryEditor.Text);
            notifier.OnCheckPerformed(new CheckEventArgs($"Wrote SQL used to {querySavePath}", CheckResult.Success));
        }
        catch (Exception e)
        {
            notifier.OnCheckPerformed(new CheckEventArgs($"Failed to save SQL query to {querySavePath}",
                CheckResult.Fail, e));
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
            GetCacheManager()
                .DeleteCacheEntryIfAny(AggregateConfiguration, AggregateOperation.ExtractableAggregateResults);
            MessageBox.Show(
                "Cached results deleted, they should no longer appear on the website (subject to website page level caching in IIS etc of course)");
            btnClearFromCache.Enabled = false;
        }
        catch (Exception exception)
        {
            ExceptionViewer.Show(exception);
        }
    }

    private CachedAggregateConfigurationResultsManager GetCacheManager() =>
        new(
            Activator.RepositoryLocator.CatalogueRepository.GetDefaultFor(PermissableDefaults
                .WebServiceQueryCachingServer_ID)
        );

    /// <summary>
    /// Normally you don't need to worry about double subscriptions but this graph gets recycled during MetadataReport generation with different aggregates one
    /// after the other which violates the 1 subscription per control rule (see base.SetDatabaseObject)
    /// </summary>
    private bool _menuInitialized;

    private bool _ribbonInitialized;

    public override void SetDatabaseObject(IActivateItems activator, AggregateConfiguration databaseObject)
    {
        base.SetDatabaseObject(activator, databaseObject);

        BuildMenu(activator);

        SetAggregate(activator, databaseObject);
    }

    protected void BuildMenu(IActivateItems activator)
    {
        if (!_menuInitialized)
        {
            _menuInitialized = true;

            if (DatabaseObject != null)
                CommonFunctionality.AddToMenu(new ExecuteCommandActivate(activator, DatabaseObject));

            CommonFunctionality.AddToMenu(new ToolStripSeparator());

            CommonFunctionality.AddToMenu(_miSaveImages);
            CommonFunctionality.AddToMenu(_miCopyToClipboard);
            CommonFunctionality.AddToMenu(_btnCache);

            CommonFunctionality.Add(_btnResendQuery);

            foreach (var c in _timeoutControls.GetControls())
                CommonFunctionality.Add(c);

            if (DatabaseObject.GetType() == typeof(AggregateConfiguration) && ((AggregateConfiguration)DatabaseObject).GetAxisIfAny() != null)
                CommonFunctionality.Add(_btnRefreshData);
        }
    }


    /// <summary>
    /// Loads the Graph without establishing a lifetime subscription to refresh events (use if you are a derived class who has it's own subscription or if you plan
    /// to load multiple different graphs into this control one after the other).
    /// </summary>
    /// <param name="activator"></param>
    /// <param name="graph"></param>
    public void SetAggregate(IActivateItems activator, AggregateConfiguration graph)
    {
        //graphs cant edit so no need to even record refresher/activator
        _aggregateConfiguration = graph;

        SetItemActivator(activator);

        SetupRibbon();
    }

    private void SetupRibbon()
    {
        if (_ribbonInitialized)
            return;

        _ribbonInitialized = true;

        foreach (var o in GetRibbonObjects())
            switch (o)
            {
                case string s:
                    CommonFunctionality.Add(s);
                    break;
                case DatabaseEntity entity:
                    CommonFunctionality.AddToMenu(new ExecuteCommandShow(Activator, entity, 0, true));
                    break;
                default:
                    throw new NotSupportedException(
                        $"GetRibbonObjects can only return strings or DatabaseEntity objects, object '{o}' is not valid because it is a '{o.GetType().Name}'");
            }
    }


    protected virtual object[] GetRibbonObjects() => Array.Empty<object>();


    public IEnumerable<BitmapWithDescription> GetImages()
    {
        var b = new Bitmap(chart1.Width, chart1.Height);
        chart1.DrawToBitmap(b, new Rectangle(new Point(0, 0), new Size(chart1.Width, chart1.Height)));

        yield return new BitmapWithDescription(b.LegacyToImage(), AggregateConfiguration.Name,
            AggregateConfiguration.Description);

        if (heatmapUI.HasDataTable())
            yield return new BitmapWithDescription(heatmapUI.GetImage(800).LegacyToImage(), null, null);
    }

    private void MiSaveImagesClick(object sender, EventArgs e)
    {
        using var sfd = new SaveFileDialog
        {
            FileName = "Chart.jpg",
            Filter = "Jpeg|*.jpg"
        };
        if (sfd.ShowDialog() != DialogResult.OK) return;

        chart1.SaveImage(sfd.FileName, ChartImageFormat.Jpeg);

        if (heatmapUI.HasDataTable())
        {
            var directory = Path.GetDirectoryName(sfd.FileName);
            var filename = $"{Path.GetFileNameWithoutExtension(sfd.FileName)}_HeatMap.jpg";
            var heatmapPath = Path.Combine(directory, filename);

            heatmapUI.SaveImage(heatmapPath, ImageFormat.Jpeg);
        }

        UsefulStuff.ShowPathInWindowsExplorer(new FileInfo(sfd.FileName));
    }

    private void ClipboardClick(object sender, EventArgs e)
    {
        if (sender == _miClipboardWord)
        {
            var s = UsefulStuff.DataTableToHtmlDataTable(_dt);

            var formatted = UsefulStuff.GetClipboardFormattedHtmlStringFromHtmlString(s);

            Clipboard.SetText(formatted, TextDataFormat.Html);
        }

        if (sender == _miClipboardCsv)
        {
            var s = UsefulStuff.DataTableToCsv(_dt);
            Clipboard.SetText(s);
        }
    }

    private void btnResendQuery_Click(object sender, EventArgs e)
    {
        LoadGraphAsync();
    }


    private void btnRefreshData_Click(object sender, EventArgs e)
    {
        var axis = AggregateConfiguration.GetAxisIfAny();
        var dialog = new AggregateGraphDateSelector(axis.StartDate, axis.EndDate);
        if (dialog.ShowDialog() == DialogResult.OK)
        {
            ReloadDataBetweenDates(dialog.StartDate, dialog.EndDate);
        }
    }

    public override string GetTabName() => $"Graph:{base.GetTabName()}";

    private void btnCache_Click(object sender, EventArgs e)
    {
        try
        {
            var cacheManager = GetCacheManager();

            var args = new CacheCommitExtractableAggregate(AggregateConfiguration, QueryEditor.Text,
                (DataTable)dataGridView1.DataSource, Timeout);
            cacheManager.CommitResults(args);

            var result =
                cacheManager.GetLatestResultsTable(AggregateConfiguration,
                    AggregateOperation.ExtractableAggregateResults, QueryEditor.Text) ??
                throw new NullReferenceException(
                    "CommitResults passed but GetLatestResultsTable returned false (when we tried to refetch the table name from the cache)");
            MessageBox.Show($"DataTable successfully submitted to:{result.GetFullyQualifiedName()}");
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
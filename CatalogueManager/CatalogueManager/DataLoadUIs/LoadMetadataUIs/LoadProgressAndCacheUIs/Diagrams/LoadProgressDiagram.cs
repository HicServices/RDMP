using System;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Forms.DataVisualization.Charting;
using CatalogueLibrary.Data;
using CatalogueManager.LocationsMenu;
using CatalogueManager.TestsAndSetup.ServicePropogation;
using DataQualityEngine.Reports;
using MapsDirectlyToDatabaseTable.Revertable;
using ReusableUIComponents;

namespace CatalogueManager.DataLoadUIs.LoadMetadataUIs.LoadProgressAndCacheUIs.Diagrams
{
    /// <summary>
    /// Replacement for LoadProgress 
    /// </summary>
    public partial class LoadProgressDiagram : RDMPUserControl
    {
        LoadProgressAnnotation _annotations;
        private LoadProgress _loadProgress;
        public event Action LoadProgressChanged;

        public LoadProgress LoadProgress
        {
            get
            {
                return _loadProgress;
            }
            set
            {
                _loadProgress = value;
                RefreshUIFromDatabase();

                DoTransparencyProperly.ThisHoversOver(pathLinkLabel1,cacheState);
            }
        }


        private void RefreshUIFromDatabase()
        {
            if (RepositoryLocator == null || LoadProgress == null)
                return;

            var report = new LoadProgressSummaryReport(LoadProgress);

            checksUIIconOnly1.Check(report);

            if (report.DQERepositoryExists)
                cataloguesRowCountChart.Visible = true;
            else
            {
                WideMessageBox.Show("You don't have any DQE server yet, you must create one in Manage External Servers (Ctrl + M)");
                cataloguesRowCountChart.Visible = false;
            }

            if (report.CataloguesPeriodictiyData == null)
            {
                cataloguesRowCountChart.Enabled = false;
                cacheState.Enabled = false;
                return;
            }
            else
            {
                cataloguesRowCountChart.Enabled = true;
                cacheState.Enabled = true;
            }

            cataloguesRowCountChart.Palette = ChartColorPalette.None;
            cataloguesRowCountChart.PaletteCustomColors = new[]
            {
                Color.FromArgb(160,0,65),
                Color.FromArgb(246,110,60),
                Color.FromArgb(255,175,89),
                Color.FromArgb(255,225,133),
                Color.FromArgb(255,255,188),
                Color.FromArgb(230,246,147),
                Color.FromArgb(170,222,162),
                Color.FromArgb(98,195,165),
                Color.FromArgb(44,135,191),
                Color.FromArgb(94,76,164)


            };

            //Catalogue periodicity chart
            PopulateChart(cataloguesRowCountChart, report.CataloguesPeriodictiyData, "Count of records");
            
            //Annotations
            _annotations = new LoadProgressAnnotation(LoadProgress, report.CataloguesPeriodictiyData,
                cataloguesRowCountChart);
            cataloguesRowCountChart.Annotations.Add(_annotations.LineAnnotationOrigin);
            cataloguesRowCountChart.Annotations.Add(_annotations.TextAnnotationOrigin);
            cataloguesRowCountChart.Annotations.Add(_annotations.LineAnnotationFillProgress);
            cataloguesRowCountChart.Annotations.Add(_annotations.TextAnnotationFillProgress);
            
            //Cache annotation (still on the Catalogue periodicity chart)
            if (_annotations.LineAnnotationCacheProgress != null)
            {
                cataloguesRowCountChart.Annotations.Add(_annotations.LineAnnotationCacheProgress);
                cataloguesRowCountChart.Annotations.Add(_annotations.TextAnnotationCacheProgress);
            }

            //Now onto the cache diagram which shows what files are in the cache directory and the failure states of old loads
            if (report.CachePeriodictiyData == null)
                splitContainer1.Panel2Collapsed = true;
            else
            {
                pathLinkLabel1.Text = report.ResolvedCachePath.FullName;

                cacheState.Palette = ChartColorPalette.None;
                cacheState.PaletteCustomColors = new[] { Color.Red,Color.Green };
                PopulateChart(cacheState, report.CachePeriodictiyData,"Fetch Failure/Success");
                splitContainer1.Panel2Collapsed = false;

                cacheState.Series[0].ChartType = SeriesChartType.Column;
                cacheState.Series[0]["DrawingStyle"] = "Cylinder";

                cacheState.Series[1].ChartType = SeriesChartType.Column;
                cacheState.Series[1]["DrawingStyle"] = "Cylinder";

            }
        }

        private void PopulateChart(Chart chart, DataTable dt,string yAxisTitle)
        {
            chart.DataSource = dt;
            chart.Annotations.Clear();
            chart.Series.Clear();

            chart.ChartAreas[0].AxisX.MinorGrid.LineWidth = 1;
            chart.ChartAreas[0].AxisX.MinorGrid.Enabled = true;
            chart.ChartAreas[0].AxisX.MinorGrid.LineDashStyle = ChartDashStyle.Dot;

            chart.ChartAreas[0].AxisX.LineColor = Color.LightGray;
            chart.ChartAreas[0].AxisX.MajorGrid.LineColor = Color.LightGray;
            chart.ChartAreas[0].AxisX.MinorGrid.LineColor = Color.LightGray;
            chart.ChartAreas[0].AxisX.TitleForeColor = Color.DarkGray;
            chart.ChartAreas[0].AxisX.LabelStyle.ForeColor = Color.DarkGray;


            chart.ChartAreas[0].AxisY.LineColor = Color.LightGray;
            chart.ChartAreas[0].AxisY.MajorGrid.LineColor = Color.LightGray;
            chart.ChartAreas[0].AxisY.MinorGrid.LineColor = Color.LightGray;
            chart.ChartAreas[0].AxisY.TitleForeColor = Color.DarkGray;
            chart.ChartAreas[0].AxisY.LabelStyle.ForeColor = Color.DarkGray;

            int rowsPerYear = 12;
            int datasetLifespanInYears = dt.Rows.Count / rowsPerYear;

            if (datasetLifespanInYears < 10)
            {
                chart.ChartAreas[0].AxisX.MinorGrid.Interval = 1;
                chart.ChartAreas[0].AxisX.MinorTickMark.Interval = 1;
                chart.ChartAreas[0].AxisX.MajorGrid.Interval = 12;
                chart.ChartAreas[0].AxisX.MajorTickMark.Interval = 4;
                chart.ChartAreas[0].AxisX.Interval = 4; //ever quarter

                //start at beginning of a quarter (%4)
                string monthYearStart = dt.Rows[0]["YearMonth"].ToString();
                int offset = GetOffset(monthYearStart);
                chart.ChartAreas[0].AxisX.IntervalOffset = offset % 4;
            }
            else if (datasetLifespanInYears < 100)
            {
                chart.ChartAreas[0].AxisX.MinorGrid.Interval = 4;
                chart.ChartAreas[0].AxisX.MinorTickMark.Interval = 4;
                chart.ChartAreas[0].AxisX.MajorGrid.Interval = 12;
                chart.ChartAreas[0].AxisX.MajorTickMark.Interval = 12;
                chart.ChartAreas[0].AxisX.Interval = 12; //every year

                //start at january
                string monthYearStart = dt.Rows[0]["YearMonth"].ToString();
                int offset = GetOffset(monthYearStart);
                chart.ChartAreas[0].AxisX.IntervalOffset = offset;
            }

            chart.ChartAreas[0].AxisX.IsMarginVisible = false;
            chart.ChartAreas[0].AxisX.Title = "Dataset Record Time (periodicity)";
            chart.ChartAreas[0].AxisY.Title = yAxisTitle;


            var cols = dt.Columns;

            foreach (var col in cols.Cast<DataColumn>().Skip(3))
            {
                var s = new Series();
                chart.Series.Add(s);
                s.ChartType = SeriesChartType.StackedArea;
                s.XValueMember = "YearMonth";
                s.YValueMembers = col.ToString();
                s.Name = col.ToString();
            }

            // Set gradient background of the chart area
            chart.ChartAreas[0].BackColor = Color.White;
            chart.ChartAreas[0].BackSecondaryColor = Color.FromArgb(255,230,230,230);
            chart.ChartAreas[0].BackGradientStyle = GradientStyle.DiagonalRight;

        }

        /// <summary>
        /// calculates how far offset the month is 
        /// </summary>
        /// <param name="yearMonth"></param>
        /// <returns></returns>
        private int GetOffset(string yearMonth)
        {
            var match = Regex.Match(yearMonth, @"\d{4}-(\d+)");

            if (!match.Success)
                throw new Exception("Regex did not match!");

            return int.Parse(match.Groups[1].Value);
        }
        public LoadProgressDiagram()
        {
            InitializeComponent();
        }

        private void cataloguesRowCountChart_AnnotationPositionChanged(object sender, EventArgs e)
        {
            if(_annotations != null)
                _annotations.OnAnnotationPositionChanged(sender, e);

            LoadProgressChanged();
        }

    }
}

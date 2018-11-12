using System;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Forms.DataVisualization.Charting;

namespace ReusableUIComponents
{
    public class ChartLookAndFeelSetter
    {
        /// <summary>
        /// Formats the X and Y axis of a <paramref name="chart"/> with sensible axis increments for the DataTable <paramref name="dt"/>. The 
        /// table must have a first column called YearMonth in the format YYYY-MM
        /// </summary>
        /// <param name="chart"></param>
        /// <param name="dt"></param>
        /// <param name="yAxisTitle"></param>
        public void PopulateYearMonthChart(Chart chart, DataTable dt, string yAxisTitle)
        {
            if(dt.Columns[0].ColumnName != "YearMonth")
                throw new ArgumentException("Expected a graph with a first column YearMonth containing values expressed as YYYY-MM");

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

            if (dt.Rows.Count == 0)
                throw new Exception("There are no rows in the DQE database (dataset is empty?)");

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
            chart.ChartAreas[0].BackSecondaryColor = Color.FromArgb(255, 230, 230, 230);
            chart.ChartAreas[0].BackGradientStyle = GradientStyle.DiagonalRight;

        }

        /// <summary>
        /// calculates how far offset the month is 
        /// </summary>
        /// <param name="yearMonth"></param>
        /// <returns></returns>
        private int GetOffset(string yearMonth)
        {
            var matchMonthName = Regex.Match(yearMonth, @"\d{4}-([A-Za-z]+)");

            if(matchMonthName.Success)
            {
                var monthName = matchMonthName.Groups[1].Value;
                return DateTime.ParseExact(monthName, "MMMM", CultureInfo.CurrentCulture).Month;
            }

            var matchMonthDigit = Regex.Match(yearMonth, @"\d{4}-(\d+)");

            if (!matchMonthDigit.Success)
                throw new Exception("Regex did not match expected YYYY-MM!");

            return int.Parse(matchMonthDigit.Groups[1].Value);
        }
    }
}

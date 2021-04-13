using FAnsi.Discovery.QuerySyntax.Aggregation;
using Rdmp.Core.CommandExecution;
using Rdmp.Core.Curation.Data.Aggregation;
using Rdmp.Core.DataViewing;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Terminal.Gui;
using Attribute = Terminal.Gui.Attribute;

namespace Rdmp.Core.CommandLine.Gui
{
    class ConsoleGuiViewGraph : ConsoleGuiSqlEditor
    {
        private readonly AggregateConfiguration aggregate;
        private GraphView graphView;
        private Tab graphTab;

        public ConsoleGuiViewGraph(IBasicActivateItems activator, AggregateConfiguration aggregate) :
            base
            (activator, new ViewAggregateExtractUICollection(aggregate) { TopX = null })
        {
            graphView = new GraphView()
            {
                Width = Dim.Fill(),
                Height = Dim.Fill()
            };
            TabView.AddTab(graphTab = new Tab("Graph",graphView), false);
            this.aggregate = aggregate;
        }

        /// <summary>
        /// Styles that will be used to render graph series
        /// </summary>
        public GraphCellToRender[] StyleList { get; private set; } = new GraphCellToRender[]
        { 
            new GraphCellToRender('x')
        };

        protected override void OnQueryCompleted(DataTable dt)
        {
            base.OnQueryCompleted(dt);

            TabView.SelectedTab = graphTab;

            string valueColumnName;
                        
            try
            {
                valueColumnName = aggregate.GetQuerySyntaxHelper().GetRuntimeName(aggregate.CountSQL);
            }
            catch (Exception)
            {
                valueColumnName = "Count";
            }

            PopulateGraphResults(dt, valueColumnName, aggregate.GetAxisIfAny());
        }

        private void PopulateGraphResults(DataTable dt, string countColumnName, AggregateContinuousDateAxis axis)
        {
            //       if (chart1.Legends.Count == 0)
            //         chart1.Legends.Add(new Legend());

            // clear any lingering settings
            graphView.Reset();

            // Work out how much screen real estate we have
            var boundsWidth = graphView.Bounds.Width;
            var boundsHeight = graphView.Bounds.Height;

            if (boundsWidth == 0)
            {
                boundsWidth = TabView.Bounds.Width - 4;
            }
            if (boundsHeight == 0)
            {
                boundsHeight = TabView.Bounds.Height - 4;
            }

            // if no time axis then we have a regular bar chart
            if (axis == null)
            {
                if(dt.Columns.Count == 2)
                {
                    SetupBarSeries(dt, countColumnName, boundsWidth, boundsHeight);
                }
                else
                {
                    SetupMultiBarSeries(dt, countColumnName, boundsWidth, boundsHeight);
                }
                
                return;
            }


            //last column is always the X axis, then for each column before it add a series with Y values coming from that column
            for (int i = 0; i < dt.Columns.Count - 1; i++)
            {
                if (axis != null)
                {
                    switch (axis.AxisIncrement)
                    {
                        
                        case AxisIncrement.Day:
                            graphView.AxisX.Text = "Day";


                            if (dt.Rows.Count <= 370)
                            {
                                //by two weeks
                                graphView.AxisX.Increment = 14;
                                graphView.AxisX.ShowLabelsEvery = 1;
                            }
                            else
                            {
                                graphView.AxisX.Increment = 28;
                                graphView.AxisX.ShowLabelsEvery = 1;
                            }

                            break;
                        case AxisIncrement.Month:

                            //x axis is the number of rows in the data table
                            graphView.AxisX.Text = "Month";


                            //if it is less than or equal to ~3 years at once - with 
                            if (dt.Rows.Count <= 40)
                            {
                                //by month
                                graphView.AxisX.Increment = 3;
                                graphView.AxisX.ShowLabelsEvery = 1;
                            }
                            else
                            {
                                //by year
                                graphView.AxisX.Increment = 12;
                                graphView.AxisX.ShowLabelsEvery = 1;
                            }

                            break;
                        case AxisIncrement.Year:

                            graphView.AxisX.Text = "Year";

                            if (dt.Rows.Count <= 10)
                            {
                                //by year
                                graphView.AxisX.Increment = 1;
                                graphView.AxisX.ShowLabelsEvery = 1;
                            }
                            else
                            {
                                graphView.AxisX.Increment = 5;
                                graphView.AxisX.ShowLabelsEvery = 1;
                            }

                            break;

                        case AxisIncrement.Quarter:

                            //x axis is the number of rows in the data table
                            graphView.AxisX.Text = "Quarter";


                            //if it is less than or equal to ~3 years at once - with 
                            if (dt.Rows.Count <= 12)
                            {
                                graphView.AxisX.Increment = 4;
                                graphView.AxisX.ShowLabelsEvery = 1;
                            }
                            else
                            {
                                //by year
                                graphView.AxisX.Increment = 16;
                                graphView.AxisX.ShowLabelsEvery = 1;
                            }

                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                }
                else
                {
                    graphView.AxisX.Text = dt.Columns[0].ColumnName;
                }

                //Set the Y axis title
                if (countColumnName != null)
                    try
                    {
                        graphView.AxisY.Text = aggregate.GetQuerySyntaxHelper().GetRuntimeName(countColumnName);
                    }
                    catch (Exception)
                    {
                        graphView.AxisY.Text = "Count";
                        //sometimes you can't get a runtime name e.g. it is count(*) with no alias
                    }

                // TODO: needs some kind of rounding probably
                graphView.AxisY.LabelGetter = (v)=>v.GraphSpace.Y.ToString();

                //avoid buffer overun
                if (i > graphView.Series.Count - 1)
                {
                    // if theres a continuous date axis use line chart
                    if(axis != null)
                    {
                        graphView.Series.Add(new ScatterSeries()
                        {
                            Fill = StyleList[i % StyleList.Length],
                        });
                    }
                }
            }

            int cells = dt.Columns.Count * dt.Rows.Count;
        }

        private void SetupBarSeries(DataTable dt,string countColumnName, int boundsWidth, int boundsHeight)
        {
            var barSeries = new BarSeries();

            var softStiple = new GraphCellToRender('\u2591');
            var mediumStiple = new GraphCellToRender('\u2592');

            int row = 0;
            int widestCategory = 0;

            decimal min = 0M;
            decimal max = 1M;

            foreach (DataRow dr in dt.Rows)
            {
                var label = dr[0].ToString();

                if (string.IsNullOrWhiteSpace(label))
                {
                    label = "<Null>";
                }

                var val = Convert.ToDecimal(dr[1]);

                min = Math.Min(min, val);
                max = Math.Max(max, val);

                widestCategory = Math.Max(widestCategory, label.Length);

                barSeries.Bars.Add(new BarSeries.Bar(label, row++ % 2 == 0 ? softStiple : mediumStiple, val));
            }

            // show bars alphabetically (graph is rendered y=0 at bottom)
            barSeries.Bars = barSeries.Bars.OrderByDescending(b => b.Text).ToList();


            // Configure Axis, Margins etc

            // make sure whole graph fits on axis
            decimal xIncrement = (max - min) / (boundsWidth);

            // 1 bar per row of console
            graphView.CellSize = new PointD(xIncrement, 1);

            graphView.Series.Add(barSeries);
            graphView.AxisY.LabelGetter = barSeries.GetLabelText;
            barSeries.Orientation = Orientation.Horizontal;
            graphView.MarginBottom = 2;
            graphView.MarginLeft = (uint)widestCategory + 1;

            // work out how to space x axis without scrolling
            graphView.AxisX.Increment = 10 * xIncrement;
            graphView.AxisX.ShowLabelsEvery = 1;
            graphView.AxisX.LabelGetter = (v) => FormatValue(v.GraphSpace.X, min, max);
            graphView.AxisX.Text = countColumnName;

            graphView.AxisY.Increment = 1;
            graphView.AxisY.ShowLabelsEvery = 1;

            // scroll to the top of the bar chart so that the natural scroll direction (down) is preserved
            graphView.ScrollOffset = new PointD(0, barSeries.Bars.Count - boundsHeight + 4);
        }

        private List<Attribute> GetColors(int numberNeeded)
        {
            var colors = new Attribute[15];

            colors[0] = Application.Driver.MakeAttribute(Color.Blue, Color.Black);
            colors[1] = Application.Driver.MakeAttribute(Color.Green, Color.Black);
            colors[2] = Application.Driver.MakeAttribute(Color.Cyan, Color.Black);
            colors[3] = Application.Driver.MakeAttribute(Color.Red, Color.Black);
            colors[4] = Application.Driver.MakeAttribute(Color.Magenta, Color.Black);
            colors[5] = Application.Driver.MakeAttribute(Color.Brown, Color.Black);
            colors[6] = Application.Driver.MakeAttribute(Color.Gray, Color.Black);
            colors[7] = Application.Driver.MakeAttribute(Color.DarkGray, Color.Black);
            colors[8] = Application.Driver.MakeAttribute(Color.BrightBlue, Color.Black);
            colors[9] = Application.Driver.MakeAttribute(Color.BrightGreen, Color.Black);
            colors[10] = Application.Driver.MakeAttribute(Color.BrighCyan, Color.Black);
            colors[11] = Application.Driver.MakeAttribute(Color.BrightRed, Color.Black);
            colors[12] = Application.Driver.MakeAttribute(Color.BrightMagenta, Color.Black);
            colors[13] = Application.Driver.MakeAttribute(Color.BrightYellow, Color.Black);
            colors[14] = Application.Driver.MakeAttribute(Color.White, Color.Black);

            var toReturn = new List<Attribute>();

            for (int i = 0; i < numberNeeded; i++)
            {
                toReturn.Add(colors[i % colors.Length]);
            }

            return toReturn;
        }

        private void SetupMultiBarSeries(DataTable dt, string countColumnName, int boundsWidth, int boundsHeight)
        {
            int numberOfBars = dt.Columns.Count - 1;
            var colors = GetColors(numberOfBars).ToArray();
            var mediumStiple = '\u2592';
            graphView.GraphColor = Driver.MakeAttribute(Color.White, Color.Black);

            var barSeries = new MultiBarSeries(numberOfBars, numberOfBars+1,1, colors);

            decimal min = 0M;
            decimal max = 1M;
            
            foreach (DataRow dr in dt.Rows)
            {
                var label = dr[0].ToString();

                if (string.IsNullOrWhiteSpace(label))
                {
                    label = "<Null>";
                }
                var vals = dr.ItemArray.Skip(1).Select(Convert.ToDecimal).ToArray();

                barSeries.AddBars(label, mediumStiple, vals);

                foreach(var val in vals)
                {
                    min = Math.Min(min, val);
                    max = Math.Max(max, val);
                }
            }

            // Configure Axis, Margins etc

            // make sure whole graph fits on axis
            decimal yIncrement = (max - min) / (boundsHeight);

            // 1 bar per row of console
            graphView.CellSize = new PointD(1, yIncrement);

            graphView.Series.Add(barSeries);
            graphView.MarginBottom = 2;
            graphView.MarginLeft = (uint)(Math.Max(FormatValue(max,min,max).Length, FormatValue(min, min, max).Length))+1;
            
            // work out how to space x axis without scrolling
            graphView.AxisY.Increment = yIncrement*5;
            graphView.AxisY.ShowLabelsEvery = 1;
            graphView.AxisY.LabelGetter = (v) => FormatValue(v.GraphSpace.Y, min, max);
            graphView.AxisY.Text = countColumnName;

            graphView.AxisX.Increment = numberOfBars+1;
            graphView.AxisX.ShowLabelsEvery = 1;
            graphView.AxisX.LabelGetter = (v) => barSeries.SubSeries.First().GetLabelText(v);
            graphView.AxisX.Text = dt.Columns[0].ColumnName;
        }
        private string FormatValue(decimal val, decimal min, decimal max)
        {
            if (val < min)
                return "";

            if (val > 1)
            {
                return val.ToString("N0");
            }

            if (val >= 0.01M)
                return val.ToString("N2");
            if (val > 0.0001M)
                return val.ToString("N4");
            if (val > 0.000001M)
                return val.ToString("N6");


            return val.ToString();
        }
    }
}

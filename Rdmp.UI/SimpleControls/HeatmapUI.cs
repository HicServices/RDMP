// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Forms;
using Rdmp.Core.ReusableLibraryCode.Settings;
using Rdmp.UI.SimpleDialogs;

namespace Rdmp.UI.SimpleControls;

/// <summary>
/// Displays complicated many dimension pivot Aggregate graphs in an understandable format.  Requires a result data table that contains an axis in the first column of the data table
/// followed by any number (usually high e.g. 500+) additional columns which contain values that correspond to the axis.  A typical usage of this control would be to display drug
/// prescriptions by month where there are thousands of different prescribeable drugs.
/// 
/// <para>The HeatmapUI renders each column as a row of heat map with each cell in the column as a 'pixel' (where the pixel width depends on the number of increments in the axis).  The color
/// of each pixel ranges from blue to red (with 0 appearing as black).  The effect of this is to show the distribution of popular vs rare pivot values across time (or whatever the axis is).</para>
/// 
/// <para>You can use this to visualise high dimensionality data that is otherwise incomprehensible in AggregateGraph</para>
/// </summary>
public partial class HeatmapUI : UserControl
{
    /*/////////////////////////////////////////EXPECTED DATA TABLE FORMAT/////////////////////////////
    * Date   | HeatLine1 | HeatLine2| HeatLine3 | HeatLine4 | etc
    * 2001   |    30     |   40     |    30     |   40      | ...
    * 2002   |    10     |   40     |    20     |   43      | ...
    * 2003   |    11     |   10     |    50     |   10      | ...
    * 2004   |    5      |   20     |    30     |   45      | ...
    * 2005   |    -3     |   10     |    30     |   44      | ...
    * 2006   |    17     |   99     |    10     |   45      | ...
    * 2007   |    19     |   40     |    30     |   40      | ...
    * ...    |   ...     |    ...   |   ...     |   ...     | ...
    *
    * */

    //Control Layout:

    //////////////////////////////////////////////////////////////////////////////////////////////
    //     <<axis labels on first visible line of control>>                      | Labels go here
    //                                                                           |
    //              Heat Lines                                                   |
    //              Heat Lines                                                   |
    //              Heat Lines                                                   |
    //              ...                        plot area                         |
    //                                                                           |
    //                                                                           |
    //                                                                           |
    //                                                                           |
    //////////////////////////////////////////////////////////////////////////////////////////////


    ///Table is interpreted in the following way:
    /// - First column is the axis in direction X (horizontally) containing (in order) the axis label values that will be each pixel in each heat lane
    /// - Each subsequent column (HeatLine1, HeatLine2 etc above) is a horizontal line of the heatmap with each pixel intensity being determined by the value on the corresponding date (in the first column)
    private RainbowColorPicker _rainbow = new(NumberOfColors);

    private const double MinPixelHeight = 15.0;
    private const double MaxPixelHeight = 20.0;

    private const double MaxLabelsWidth = 150;
    private const double LabelsHorizontalPadding = 10.0;

    private double _currentLabelsWidth;

    private readonly Lock _oDataTableLock = new();

    public HeatmapUI()
    {
        InitializeComponent();
    }

    public void SetDataTable(DataTable dataTable)
    {
        if (!string.IsNullOrWhiteSpace(UserSettings.HeatMapColours))
        {
            var colorRegex = new Regex("#([0-9A-F][0-9A-F])([0-9A-F][0-9A-F])([0-9A-F][0-9A-F])");

            var tokens = UserSettings.HeatMapColours.Split(new string[] { "->" }, StringSplitOptions.None);

            if (tokens.Length == 2)
            {
                var m1 = colorRegex.Match(tokens[0]);
                var m2 = colorRegex.Match(tokens[1]);

                if (m1.Success && m2.Success)
                {
                    var fromColor = Color.FromArgb(
                        (int)Convert.ToByte(m1.Groups[1].Value, 16),
                        (int)Convert.ToByte(m1.Groups[2].Value, 16),
                        (int)Convert.ToByte(m1.Groups[3].Value, 16)
                    );

                    var toColor = Color.FromArgb(
                        (int)Convert.ToByte(m2.Groups[1].Value, 16),
                        (int)Convert.ToByte(m2.Groups[2].Value, 16),
                        (int)Convert.ToByte(m2.Groups[3].Value, 16)
                    );

                    _rainbow = new RainbowColorPicker(fromColor, toColor, NumberOfColors);
                }
            }
        }

        lock (_oDataTableLock)
        {
            _dataTable = dataTable;

            //skip the first column (which will be the X axis values)  then compute the maximum value in any cell in the data table, this is the brightest pixel in heatmap
            //the minimum value will be the darkest pixel

            _maxValueInDataTable = double.MinValue;
            _minValueInDataTable = double.MaxValue;

            for (var x = 0; x < _dataTable.Rows.Count; x++)
                for (var y = 1; y < _dataTable.Columns.Count; y++)
                {
                    var cellValue = ToDouble(_dataTable.Rows[x][y]);

                    if (cellValue < _minValueInDataTable)
                        _minValueInDataTable = cellValue;

                    if (cellValue > _maxValueInDataTable)
                        _maxValueInDataTable = cellValue;
                }

            Height = (int)Math.Max(Height, _dataTable.Columns.Count * MinPixelHeight);
        }

        Invalidate();
    }

    private static double ToDouble(object o) => o == DBNull.Value ? 0 : Convert.ToDouble(o);

    private DataTable _dataTable;
    private double _maxValueInDataTable;
    private double _minValueInDataTable;
    private bool _crashedPainting;


    private const int NumberOfColors = 256;

    protected override void OnResize(EventArgs e)
    {
        base.OnResize(e);
        Invalidate();
    }


    private ToolTip tt = new();

    private int toolTipDelayInTicks = 500;
    private Point _lastHoverPoint;
    private int _lastHoverTickCount;
    private bool _useEntireControlAsVisibleArea;


    private void hoverToolTipTimer_Tick(object sender, EventArgs e)
    {
        var pos = PointToClient(Cursor.Position);

        //if we moved
        if (!_lastHoverPoint.Equals(pos))
        {
            _lastHoverPoint = pos;
            _lastHoverTickCount = Environment.TickCount;

            tt.Hide(this);
            return;
        }

        //we didn't move, have we been here a while?
        if (Environment.TickCount - _lastHoverTickCount < toolTipDelayInTicks)
            return; //no

        //yes we have been here a while so show the tool tip
        _lastHoverTickCount = Environment.TickCount;
        object value = null;

        lock (_oDataTableLock)
        {
            value = GetValueFromClientPosition(pos);
        }

        //there wasn't anything to display anyway
        if (value == null)
            return;

        if (Visible)
            //show the tool tip
            tt.Show(value.ToString(), this,
                new Point(pos.X + 20, pos.Y - 10)); //allow room for cusor to not overdraw the tool tip
    }

    private object GetValueFromClientPosition(Point pos)
    {
        if (_dataTable == null)
            return null;

        if (pos.X < 0 || pos.Y < 0)
            return null;

        //pointer is to the right of the entire control
        if (pos.X > Width)
            return null;

        var pixelHeight = GetHeatPixelHeight();
        var pixelWidth = GetHeatPixelWidth();


        var dataTableCol = (int)(pos.Y / pixelHeight); //heat map line number + 1 because first column is the axis label
        var dataTableRow =
            (int)(pos.X / pixelWidth); //the pixel width corresponds to the number of axis values in the first column

        if (dataTableCol >= _dataTable.Columns.Count)
            return null;

        //return the label since they are on the right of the control
        if (dataTableRow >= _dataTable.Rows.Count)
            return _dataTable.Columns[dataTableCol].ColumnName;

        return dataTableCol == 0
            ? _dataTable.Rows[dataTableRow][dataTableCol]
            : $"{_dataTable.Rows[dataTableRow][0]}:{_dataTable.Columns[dataTableCol].ColumnName}{Environment.NewLine}{_dataTable.Rows[dataTableRow][dataTableCol]}";
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);

        if (_dataTable == null)
            return;
        if (_crashedPainting)
            return;
        try
        {
            lock (_oDataTableLock)
            {
                //draw background
                e.Graphics.FillRectangle(Brushes.White, new Rectangle(0, 0, Width, Height));

                //decide how tall to make pixels
                var heatPixelHeight = GetHeatPixelHeight();

                //based on the height of the line what text font will fit into that line?
                var font = GetFontSizeThatWillFitPixelHeight(heatPixelHeight, e.Graphics);

                //now we know the Font to use, figure out the width of the longest piece of text when rendered with the Font (with a sensible max, we aren't allowing war and peace into this label)
                _currentLabelsWidth = GetLabelWidth(e.Graphics, font);

                //now that we know the width of the labels work out the width of each pixel to fill the rest of the controls area with heat pixels / axis
                var heatPixelWidth = GetHeatPixelWidth();

                var brush = new SolidBrush(Color.Black);

                //for each line of pixels in heatmap
                for (var x = 0; x < _dataTable.Rows.Count; x++)
                    //draw the line this way -------------> with pixels of width heatPixelWidth/Height
                    //skip the first y value which is the x axis value
                    for (var y = 1; y < _dataTable.Columns.Count; y++)
                    {
                        //the value we are drawing
                        var cellValue = ToDouble(_dataTable.Rows[x][y]);

                        //if the cell value is 0 render it as black
                        if (Math.Abs(cellValue - _minValueInDataTable) < 0.0000000001 &&
                            Math.Abs(_minValueInDataTable) < 0.0000000001)
                        {
                            brush.Color = Color.Black;
                        }
                        else
                        {
                            var brightness = (cellValue - _minValueInDataTable) /
                                             (_maxValueInDataTable - _minValueInDataTable);
                            var brightnessIndex = (int)(brightness * (NumberOfColors - 1));

                        brush.Color = _rainbow.Colors[brightnessIndex];
                    }

                        e.Graphics.FillRectangle(brush, (float)(x * heatPixelWidth), (float)(y * heatPixelHeight),
                            (float)heatPixelWidth, (float)heatPixelHeight);
                    }

                var labelStartX = Width - _currentLabelsWidth;


                //draw the labels
                for (var i = 1; i < _dataTable.Columns.Count; i++)
                {
                    var labelStartY = i * heatPixelHeight;

                    var name = _dataTable.Columns[i].ColumnName;

                    e.Graphics.DrawString(name, font, Brushes.Black,
                        new PointF((float)labelStartX, (float)labelStartY));
                }

                double lastAxisStart = -500;
                double lastAxisLabelWidth = -500;

                var visibleArea = _useEntireControlAsVisibleArea
                    ? new Rectangle(0, 0, Width, Height)
                    : this.GetVisibleArea();


                var visibleClipBoundsTop = visibleArea.Top;

                //now draw the axis
                //axis starts at the first visible pixel
                double axisYStart = Math.Max(0, visibleClipBoundsTop);

                e.Graphics.FillRectangle(Brushes.White, 0, (int)axisYStart, Width, (int)heatPixelHeight);

                //draw the axis labels
                for (var i = 0; i < _dataTable.Rows.Count; i++)
                {
                    var axisXStart = i * heatPixelWidth;

                    //skip labels if the axis would result in a label overdrawing its mate
                    if (axisXStart < lastAxisStart + lastAxisLabelWidth)
                        continue;

                    lastAxisStart = axisXStart;

                    var label = _dataTable.Rows[i][0].ToString();

                    //draw the axis label text with 1 pixel left and right so that there is space for the axis black line
                    e.Graphics.DrawString(label, font, Brushes.Black,
                        new PointF((float)axisXStart + 1, (float)axisYStart));
                    lastAxisLabelWidth = (int)e.Graphics.MeasureString(label, font).Width + 2;


                    //draw axis black line
                    e.Graphics.DrawLine(Pens.Black, new PointF((float)axisXStart, (float)axisYStart),
                        new PointF((float)axisXStart, Height));
                }
            }
        }
        catch (Exception exception)
        {
            _crashedPainting = true;
            ExceptionViewer.Show(exception);
        }
    }

    private double GetLabelWidth(Graphics g, Font font)
    {
        var nameStrings = _dataTable.Columns.Cast<DataColumn>().Select(c => c.ColumnName);
        var longestString = nameStrings.Aggregate("", (max, cur) => max.Length > cur.Length ? max : cur);
        var longestStringWidth = g.MeasureString(longestString, font).Width + LabelsHorizontalPadding;

        if (VerticalScroll.Visible)
            longestStringWidth += SystemInformation.VerticalScrollBarWidth;

        return Math.Min(MaxLabelsWidth, longestStringWidth);
    }

    private double GetHeatPixelWidth()
    {
        var plotAreaWidth = Width - _currentLabelsWidth;
        return plotAreaWidth / _dataTable.Rows.Count;
    }

    /// <summary>
    /// Gets a suitable size to render each heat line respecting the controls Height and the number of dimensions in the DataTable.  Bounded by MinPixelHeight and MaxPixelHeight
    /// (see consts)
    /// </summary>
    /// <returns></returns>
    private double GetHeatPixelHeight()
    {
        double plotAreaHeight = Height;
        double numberOfDimensions = _dataTable.Columns.Count; //first column is the X axis value

        return Math.Min(MaxPixelHeight, Math.Max(MinPixelHeight, plotAreaHeight / numberOfDimensions));
    }

    private static Font GetFontSizeThatWillFitPixelHeight(double heightInPixels, Graphics graphics)
    {
        Font font;
        var emSize = heightInPixels;
        do
        {
            font = new Font(new FontFamily("Tahoma"), (float)(emSize -= 0.5), FontStyle.Regular);
        } while (graphics.MeasureString("testing", font).Height > heightInPixels);

        return font;
    }


    public static void CalculateLayout()
    {
    }

    public void Clear()
    {
        lock (_oDataTableLock)
        {
            _dataTable = null;
        }
    }

    public bool HasDataTable() => _dataTable != null;

    public Bitmap GetImage(int maxHeight)
    {
        var h = Math.Min(maxHeight, Height);

        var isClipped = maxHeight < Height;

        var bmp = new Bitmap(Width, h);

        _useEntireControlAsVisibleArea = true;

        DrawToBitmap(bmp, new Rectangle(0, 0, Width, h));

        _useEntireControlAsVisibleArea = false;

        if (isClipped)
        {
            //number of heat map lines
            var numberOfHeatLinesVisible = (int)(h / GetHeatPixelHeight());

            //total number of heatmap lines
            var totalHeatMapLinesAvailable = _dataTable.Columns.Count - 1;

            if (numberOfHeatLinesVisible < totalHeatMapLinesAvailable)
            {
                //add a note saying to user data has been clipped
                var clippedRowsComment =
                    $"{totalHeatMapLinesAvailable - numberOfHeatLinesVisible} more rows clipped";
                var g = Graphics.FromImage(bmp);

                var fontSize = g.MeasureString(clippedRowsComment, Font);

                //centre it on the bottom of the image
                g.FillRectangle(Brushes.WhiteSmoke, 0, h - fontSize.Height, fontSize.Width, fontSize.Height);
                g.DrawString(clippedRowsComment, Font, Brushes.Black, 0, h - fontSize.Height);
            }
        }

        return bmp;
    }

    public void SaveImage(string heatmapPath, ImageFormat imageFormat)
    {
        var bmp = new Bitmap(Width, Height);

        _useEntireControlAsVisibleArea = true;

        DrawToBitmap(bmp, new Rectangle(0, 0, Width, Height));
        bmp.Save(heatmapPath, imageFormat);

        _useEntireControlAsVisibleArea = false;
    }
}
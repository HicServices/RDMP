// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using Rdmp.Core.CommandExecution;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.DataQualityEngine.Data;
using Rdmp.Core.Icons.IconProvision;
using Rdmp.UI.DashboardTabs.Construction;
using Rdmp.UI.ItemActivation;
using Rdmp.UI.SimpleControls;
using Timer = System.Windows.Forms.Timer;

namespace Rdmp.UI.Raceway;

/// <summary>
/// Part of DatasetRaceway, this control shows a miniature bar chart with 1 bar per month of data held within the dataset.  Bar colour indicates the proportion of records in that month
/// which are passing/failing validation (green = passing, red = failing).  The month axis is shared across all datasets in the DatasetRaceway meaning that you see a continuous axis
/// that spans the whole length of time you have been holding data for in any of your datasets.  The data for this graph comes from the Data Quality Engine evaluations database so if
/// you have never run the DQE on a given dataset it will not appear in the DatasetRaceway.
/// 
/// <para>In DatasetRaceway you can adjust the scope of the axis down from 'All Time' to 'Last Decade',  'Last Year' or 'Last 6 months' if you just want to see how up-to-date each dataset
/// is in finer detail.</para>
/// 
/// <para>The overall effect of this control is to allow you to rapidly identify any datasets that you host which have suddenly started failing validation, see where each dataset starts and
/// if there are any gaps or periods of duplication (e.g. where the bars double in height for a period).</para>
/// </summary>
[TechnicalUI]
public partial class RacewayRenderAreaUI : UserControl, INotifyMeOfEditState
{
    private DateTime[] _buckets;
    private Pen _verticalLinesPen = new(Color.FromArgb(150, Color.White));
    private Timer _mouseHeldDownTimer = new();
    private ScrollActionUnderway _currentScrollAction = ScrollActionUnderway.None;

    public RacewayRenderAreaUI()
    {
        InitializeComponent();

        var colorPicker = new RainbowColorPicker(Color.Red, Color.LawnGreen, 101);
        _brushes = colorPicker.Colors.Select(c => new SolidBrush(c)).ToArray();
        DoubleBuffered = true;

        _mouseHeldDownTimer.Interval = 100;
        _mouseHeldDownTimer.Tick += _mouseHeldDownTimer_Tick;
        _mouseHeldDownTimer.Start();
    }

    public event Action<Catalogue> RequestDeletion;

    public void NotifyEditModeChange(bool isEditmodeOn)
    {
        _isEditModeOn = isEditmodeOn;
        Invalidate();
    }

    private readonly Lock _oPeriodicityDictionaryLock = new();

    private bool _ignoreRowCounts;
    private bool _isEditModeOn;
    private const float MaximumRaceLaneRenderSpace = 30f;
    private SolidBrush[] _brushes;

    private Dictionary<Rectangle, Catalogue> rectNoDQE = new();
    private Dictionary<Rectangle, Catalogue> rectDeleteButtons = new();
    private IActivateItems _activator;

    private bool _allowScrollDown;
    private RectangleF _rectScrollDown;
    private int _scrollDownIndexOffset = 0;

    private bool _allowScrollUp;
    private RectangleF _rectScrollUp;
    private DateTime _currentScrollActionBegan;
    private Dictionary<Catalogue, Dictionary<DateTime, ArchivalPeriodicityCount>> _periodicityDictionary;
    private const float MinimumRowHeight = 20;

    public void AddTracks(IActivateItems activator,
        Dictionary<Catalogue, Dictionary<DateTime, ArchivalPeriodicityCount>> periodicityDictionary, DateTime[] buckets,
        bool ignoreRows)
    {
        _activator = activator;
        _buckets = buckets;
        _ignoreRowCounts = ignoreRows;
        lock (_oPeriodicityDictionaryLock)
        {
            _periodicityDictionary = periodicityDictionary;
        }
    }

    private long frameLimiter;

    protected override void OnMouseMove(MouseEventArgs e)
    {
        base.OnMouseMove(e);


        if (rectNoDQE.Any(r => r.Key.Contains(e.Location)) || rectDeleteButtons.Any(r => r.Key.Contains(e.Location)))
            Cursor = Cursors.Hand;
        else
            Cursor = Cursors.Arrow;

        //don't spam invalidate every time the mouse moves a pixel

        //Ticks is the number of 100-nanosecond intervals that have elapsed
        //Therefore this is 10 FPS
        if (DateTime.Now.Ticks - frameLimiter < 1000000)
            return;

        frameLimiter = DateTime.Now.Ticks;

        Invalidate();
    }


    protected override void OnMouseClick(MouseEventArgs e)
    {
        base.OnMouseClick(e);

        foreach (var kvp in rectNoDQE.Where(r => r.Key.Contains(e.Location)))
            _activator.RequestItemEmphasis(this, new EmphasiseRequest(kvp.Value));

        foreach (var kvp in rectDeleteButtons.Where(r => r.Key.Contains(e.Location)).ToList())
            if (RequestDeletion != null)
                lock (_oPeriodicityDictionaryLock)
                {
                    RequestDeletion(kvp.Value);
                    //stop rendering this Catalogue
                    _periodicityDictionary.Remove(kvp.Value);
                    Invalidate();
                }
    }


    protected override void OnMouseDown(MouseEventArgs e)
    {
        base.OnMouseDown(e);


        //scroll them down
        if (_rectScrollDown != RectangleF.Empty && _rectScrollDown.Contains(e.Location) && _allowScrollDown)
        {
            _currentScrollActionBegan = DateTime.Now;
            _currentScrollAction = ScrollActionUnderway.ScrollingDown;
            ScrollDown();
        }

        //scroll them down
        if (_rectScrollUp != RectangleF.Empty && _rectScrollUp.Contains(e.Location) && _allowScrollUp)
        {
            _currentScrollActionBegan = DateTime.Now;
            _currentScrollAction = ScrollActionUnderway.ScrollingUp;
            ScrollUp();
        }

        if (_allowScrollDown || _allowScrollUp)
        {
            var scrollBarMiddle = new RectangleF(_rectScrollUp.Left, _rectScrollUp.Bottom, 20,
                _rectScrollDown.Top - _rectScrollUp.Bottom);
            if (scrollBarMiddle.Contains(e.Location))
            {
                _currentScrollActionBegan = DateTime.Now;
                _currentScrollAction = ScrollActionUnderway.ScrollDragging;
                ScrollToMouseLocation();
            }
        }
    }

    protected override void OnMouseUp(MouseEventArgs e)
    {
        base.OnMouseUp(e);
        _currentScrollAction = ScrollActionUnderway.None;
    }

    private void _mouseHeldDownTimer_Tick(object sender, EventArgs e)
    {
        if (IsDisposed)
        {
            _mouseHeldDownTimer.Stop();
            _mouseHeldDownTimer.Dispose();
            return;
        }

        if (_currentScrollAction == ScrollActionUnderway.None ||
            DateTime.Now.Subtract(_currentScrollActionBegan).TotalMilliseconds < 200)
            return;

        if (_currentScrollAction == ScrollActionUnderway.ScrollingUp)
            ScrollUp();

        if (_currentScrollAction == ScrollActionUnderway.ScrollingDown)
            ScrollDown();

        if (_currentScrollAction == ScrollActionUnderway.ScrollDragging)
            ScrollToMouseLocation();

        Invalidate();
    }

    protected override void OnMouseWheel(MouseEventArgs e)
    {
        base.OnMouseWheel(e);

        if (e.Delta < 0)
            ScrollDown();
        else
            ScrollUp();

        Invalidate();
    }

    private void ScrollUp()
    {
        _scrollDownIndexOffset = Math.Max(0, _scrollDownIndexOffset - 1);
    }

    private void ScrollDown()
    {
        _scrollDownIndexOffset = Math.Min(GetMaxScrollDownToIndex() + 1, _scrollDownIndexOffset + 1);
    }

    private void ScrollToMouseLocation()
    {
        var mouseloc = PointToClient(Cursor.Position);
        var maxScroll = GetMaxScrollDownToIndex();

        var clickLocationY = mouseloc.Y - _rectScrollUp.Bottom;
        var fullDistanceBetweenUpAndDownArrows = _rectScrollDown.Top - _rectScrollUp.Bottom;
        var ratio = clickLocationY / fullDistanceBetweenUpAndDownArrows;

        _scrollDownIndexOffset = (int)(maxScroll * ratio);

        _scrollDownIndexOffset = Math.Max(0, _scrollDownIndexOffset);
        _scrollDownIndexOffset = Math.Min(maxScroll, _scrollDownIndexOffset);
    }

    private int GetMaxScrollDownToIndex()
    {
        var indexesVisibleOnScreen = Height / MinimumRowHeight;
        var indexes = _periodicityDictionary.Count + 1;

        return (int)Math.Max(0, indexes - indexesVisibleOnScreen);
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);

        e.Graphics.FillRectangle(Brushes.Black, 0, 0, Width, Height);

        //draw buckets
        if (_buckets == null)
            return;

        var eachBucketHasThisManyPixelsOfX = (float)Width / _buckets.Length;

        var heightReservedForAxis = e.Graphics.MeasureString("TEST", Font).Height + 2;

        var mousePosition = PointToClient(new Point(MousePosition.X, MousePosition.Y));

        rectNoDQE.Clear();
        rectDeleteButtons.Clear();

        string hoverLabel = null;
        string hoverValue = null;

        _allowScrollUp = _scrollDownIndexOffset > 0;


        //draw the tracks
        lock (_oPeriodicityDictionaryLock)
        {
            var eachRaceLaneHasThisMuchYSpace = Math.Max(MinimumRowHeight,
                Math.Min(MaximumRaceLaneRenderSpace, (float)Height / (_periodicityDictionary.Count + 1)));


            float startDrawingLaneAtY = 0;
            _allowScrollDown = false;

            var index = 0;
            foreach (var (catalogue, dictionary) in _periodicityDictionary.OrderBy(kvp =>
                         kvp.Value == null ? $"zzzzzz{kvp.Key.Name}" : kvp.Key.Name))
            {
                index++;

                //skip it because of scrolling
                if (index <= _scrollDownIndexOffset)
                    continue;

                //we have run out of space stop drawing
                if (startDrawingLaneAtY + eachRaceLaneHasThisMuchYSpace > Height - heightReservedForAxis)
                {
                    _allowScrollDown = true;
                    break;
                }

                var middleLineOfCatalogueLabelY =
                    eachRaceLaneHasThisMuchYSpace / 2 - Font.Height / 2.0 + startDrawingLaneAtY;

                if (!_buckets.Any())
                {
                    DrawErrorText("No DQE data exists for 'Show Period'", false, e, startDrawingLaneAtY,
                        eachRaceLaneHasThisMuchYSpace, middleLineOfCatalogueLabelY);
                }
                else if (dictionary == null)
                {
                    var textWidth = DrawErrorText($"No DQE Evaluation for {catalogue}", true, e, startDrawingLaneAtY,
                        eachRaceLaneHasThisMuchYSpace, middleLineOfCatalogueLabelY);
                    rectNoDQE.Add(
                        new Rectangle(0, (int)startDrawingLaneAtY, (int)textWidth, (int)eachRaceLaneHasThisMuchYSpace),
                        catalogue);
                }
                else if (!dictionary.Any())
                {
                    var textWidth = DrawErrorText($"Table(s) were empty for {catalogue}", true, e, startDrawingLaneAtY,
                        eachRaceLaneHasThisMuchYSpace, middleLineOfCatalogueLabelY);
                    rectNoDQE.Add(
                        new Rectangle(0, (int)startDrawingLaneAtY, (int)textWidth, (int)eachRaceLaneHasThisMuchYSpace),
                        catalogue);
                }
                else
                {
                    //get the maximum number of rows regardless of consequence found in any data month
                    var maxRowsInAnyMonth = dictionary.Max(r => r.Value.Total);

                    for (var i = 0; i < _buckets.Length; i++)
                    {
                        Brush brush;
                        float lineHeightPercentage;

                        var good = 0;
                        var total = 0;

                        if (dictionary.TryGetValue(_buckets[i], out var apcCount))
                        {
                            good = apcCount.CountGood;
                            total = apcCount.Total;

                            var ratioGood = (float)good / total;

                            brush = _brushes[(int)(ratioGood * 100)];

                            lineHeightPercentage = (float)total / maxRowsInAnyMonth;
                        }
                        else
                        {
                            brush = new SolidBrush(Color.Black);
                            lineHeightPercentage = 0;
                        }

                        //now draw the actual bar
                        //bar will occupy this much of the allowed Y space for the line e.g. 0.5 will be half way up the graph
                        var thicknessRatio = _ignoreRowCounts ? 1 : lineHeightPercentage;

                        var heightOfBarToDraw = eachRaceLaneHasThisMuchYSpace * thicknessRatio;
                        var emptyYSpace = eachRaceLaneHasThisMuchYSpace - heightOfBarToDraw;

                        var rectToFill = new RectangleF(i * eachBucketHasThisManyPixelsOfX, startDrawingLaneAtY + emptyYSpace,
                            eachBucketHasThisManyPixelsOfX, heightOfBarToDraw);
                        e.Graphics.FillRectangle(brush, rectToFill);

                        var fullRectHitbox = new RectangleF(rectToFill.X, startDrawingLaneAtY, eachBucketHasThisManyPixelsOfX,
                            eachRaceLaneHasThisMuchYSpace);
                        if (fullRectHitbox.Contains(mousePosition))
                        {
                            hoverLabel = _buckets[i].ToString("Y");
                            hoverValue =
                                $"{good:n0}/{total:n0}";
                        }
                    }

                    e.Graphics.DrawString(catalogue.Name, Font, Brushes.White,
                        new Point(0, (int)middleLineOfCatalogueLabelY));
                }

                if (hoverLabel != null)
                {
                    const float labelPadding = 3;
                    var labelSize = e.Graphics.MeasureString(hoverLabel, Font);
                    var valueSize = e.Graphics.MeasureString(hoverValue, Font);

                    //if there's a scroll down bar add 20 to the width to make space for the down arrow
                    if (_allowScrollDown)
                        valueSize.Width += 20;

                    var rectHoverLabel = new
                        RectangleF(Width - (labelSize.Width + valueSize.Width + 2 * labelPadding),
                            Height - (heightReservedForAxis + labelSize.Height),
                            labelSize.Width,
                            labelSize.Height);

                    var rectHoverValue = new
                        RectangleF(rectHoverLabel.Right + labelPadding,
                            rectHoverLabel.Top,
                            valueSize.Width,
                            valueSize.Height);

                    e.Graphics.DrawString(hoverLabel, Font, Brushes.White, rectHoverLabel);
                    e.Graphics.DrawString(hoverValue, Font, Brushes.White, rectHoverValue);
                }

                if (_isEditModeOn)
                {
                    var deleteIcon = FamFamFamIcons.delete.ImageToBitmap();
                    var middleLineOfDeleteButtonY = eachRaceLaneHasThisMuchYSpace / 2 - deleteIcon.Height / 2.0 +
                                                    startDrawingLaneAtY;
                    var buttonPoint = new Point(Width / 2, (int)middleLineOfDeleteButtonY);

                    e.Graphics.DrawImage(deleteIcon, buttonPoint);

                    rectDeleteButtons.Add(
                        new Rectangle(buttonPoint.X, buttonPoint.Y, deleteIcon.Width, deleteIcon.Height), catalogue);
                }

                //move to next lane on graph
                startDrawingLaneAtY += eachRaceLaneHasThisMuchYSpace;
            }
        }

        float currentX = 0;
        var startDrawingAxisAtY = Height - heightReservedForAxis;

        for (var i = 0; i < _buckets.Length; i++)
        {
            var bucket = _buckets[i];
            var bucketLabel = bucket.ToString("yyyy-MM");

            var idealBucketRenderLocation = i * eachBucketHasThisManyPixelsOfX;

            if (currentX > idealBucketRenderLocation)
                continue; //had to skip bucket because we don't have space to render it

            var requiredSpaceForLabel = e.Graphics.MeasureString(bucketLabel, Font).Width;

            //draw this label because there is space
            e.Graphics.DrawString(bucketLabel, Font, Brushes.White, idealBucketRenderLocation, startDrawingAxisAtY);

            //draw an axis line all the way up the graph
            e.Graphics.DrawLine(_verticalLinesPen, idealBucketRenderLocation, 0, idealBucketRenderLocation, Height);

            //record the space we occupied
            currentX += requiredSpaceForLabel;
        }


        if (_allowScrollUp || _allowScrollDown)
        {
            _rectScrollUp = new RectangleF(Width - 20, 0, 20, 20);
            e.Graphics.FillRectangle(Brushes.Green, _rectScrollUp);
            e.Graphics.FillRectangle(Brushes.Black, _rectScrollUp.X + 2, _rectScrollUp.Y + 2, 16, 16);
            Point[] points =
            {
                new((int)(_rectScrollUp.X + 5), (int)(_rectScrollUp.Y + 15)),
                new((int)(_rectScrollUp.X + 15), (int)(_rectScrollUp.Y + 15)),
                new((int)(_rectScrollUp.X + 10), (int)(_rectScrollUp.Y + 5))
            };

            e.Graphics.DrawPolygon(new Pen(_allowScrollUp ? Color.LawnGreen : Color.Green), points);

            _rectScrollDown = new RectangleF(Width - 20, startDrawingAxisAtY - 20, 20, 20);
            e.Graphics.FillRectangle(Brushes.Green, _rectScrollDown);
            e.Graphics.FillRectangle(Brushes.Black, _rectScrollDown.X + 2, _rectScrollDown.Y + 2, 16, 16);

            points = new[]
            {
                new Point((int)(_rectScrollDown.X + 5), (int)(_rectScrollDown.Y + 5)),
                new Point((int)(_rectScrollDown.X + 15), (int)(_rectScrollDown.Y + 5)),
                new Point((int)(_rectScrollDown.X + 10), (int)(_rectScrollDown.Y + 15))
            };
            e.Graphics.DrawPolygon(new Pen(_allowScrollDown ? Color.LawnGreen : Color.Green), points);
        }

        if (_allowScrollDown && _allowScrollUp && GetMaxScrollDownToIndex() != 0)
        {
            //draw the progress tab
            var scrollFloatOffset = (float)_scrollDownIndexOffset / GetMaxScrollDownToIndex();
            var progressScrolling = (_rectScrollDown.Top - _rectScrollUp.Bottom) * scrollFloatOffset;

            var yHeightOfScrollIndicator = (int)(progressScrolling + _rectScrollUp.Bottom);

            e.Graphics.FillRectangle(Brushes.LawnGreen, Width - 20, yHeightOfScrollIndicator, 20, 3);
            e.Graphics.DrawLine(Pens.Black, Width - 20, yHeightOfScrollIndicator + 1, Width - 1,
                yHeightOfScrollIndicator + 1);
        }
    }

    private float DrawErrorText(string text, bool underLine, PaintEventArgs e, float startDrawingLaneAtY,
        float eachRaceLaneHasThisMuchYSpace, double middleLineOfCatalogueLabelY)
    {
        var redGradientBrush = new LinearGradientBrush(
            new Point(0, (int)startDrawingLaneAtY),
            new Point((int)Width, (int)eachRaceLaneHasThisMuchYSpace)
            , Color.FromArgb(255, Color.Red), Color.FromArgb(50, Color.Red)
        );

        e.Graphics.FillRectangle(redGradientBrush, 0, startDrawingLaneAtY, Width, eachRaceLaneHasThisMuchYSpace);


        e.Graphics.DrawString(text, underLine ? new Font(Font, FontStyle.Underline) : Font, Brushes.White,
            new Point(0, (int)middleLineOfCatalogueLabelY));

        return e.Graphics.MeasureString(text, Font).Width;
    }

    protected override void OnResize(EventArgs e)
    {
        base.OnResize(e);
        Invalidate();
    }

    private enum ScrollActionUnderway
    {
        None,
        ScrollingUp,
        ScrollingDown,
        ScrollDragging
    }
}
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using CatalogueLibrary.Data;
using CatalogueLibrary.Data.Automation;
using CatalogueManager.Icons.IconProvision;
using CatalogueManager.ItemActivation;
using HIC.Logging;
using ReusableUIComponents;

namespace Dashboard.Automation
{

    /// <summary>
    /// TECHNICAL: Provides the rendering area (drawing functionality) for AutomationServerMonitorUI (See AutomationServerMonitorUI)
    /// </summary>
    [TechnicalUI]
    public partial class AutomationServerMonitorUIRenderArea : UserControl
    {
        object oCollectionLock = new object();
        private AutomationServerMonitorUIObjectCache _cache;
        private IActivateItems _activator;

        private Bitmap _consoleImage;
        private Bitmap _errorImage;

        private Bitmap _dqeImage;
        private Bitmap _dleImage;
        private Bitmap _cachingImage;
        private Bitmap _userPluginImage;

        private Bitmap _loggingImage;
        private Bitmap _cancelImage;
        private Bitmap _killImage;

        private Pen _goodPen;
        private Pen _goodPenDash;

        private Pen _badPen;
        private Brush _goodBrush;
        private Brush _badBrush;
        private const double ElapsedMinutesBeforeConsideredBadTime = 30;

        private const string EmptyTimeString = "00:00:00";
        private const string ErrorString = "99:99:99";

        Dictionary<Rectangle,AutomationServiceException> _globalExceptionsRendered = new Dictionary<Rectangle, AutomationServiceException>();
        private ToolTip _toolTip;
        private List<Rectangle> _timerTextRectangles = new List<Rectangle>();
        private Rectangle _killServerRect = Rectangle.Empty;

        private Dictionary<Rectangle,AutomationJob> _viewLoggingRectangles= new Dictionary<Rectangle, AutomationJob>();
        private Dictionary<Rectangle, AutomationJob> _stopRectangles = new Dictionary<Rectangle, AutomationJob>();

        private const int textHeightOffset = 3;

        bool _foundToolTipLocation;

        public AutomationServerMonitorUIRenderArea()
        {
            InitializeComponent();

            _goodBrush = Brushes.GreenYellow;
            _badBrush = Brushes.Red;

            _dqeImage = CatalogueIcons.DQE;
            _dleImage = CatalogueIcons.LoadMetadata;
            _cachingImage = CatalogueIcons.CacheProgress;
            _userPluginImage = CatalogueIcons.Plugin;

            _loggingImage = CatalogueIcons.Logging;
            _cancelImage = FamFamFamIcons.delete;
            _killImage = CatalogueIcons.Kill;
            
            _consoleImage = CatalogueIcons.AutomationServiceSlot;
            _errorImage = FamFamFamIcons.flag_red;

            _goodPen = new Pen(_goodBrush, 1);
            _badPen = new Pen(_badBrush, 1);
            
            _goodPenDash = new Pen(_goodBrush, 1);
            _goodPenDash.DashStyle = DashStyle.Dash;

            _toolTip = new ToolTip();
            DoubleBuffered = true;
        }

        #region Mouse Control
        protected override void OnMouseMove(MouseEventArgs e)
        {
            
            base.OnMouseMove(e);

            lock (oCollectionLock)
            {
                _foundToolTipLocation = false;
                   
                //if the location is right and we are not currently showing a tool tip
                if( _globalExceptionsRendered.Any(kvp=>kvp.Key.Contains(e.Location)))
                    SetToolTip("Left Click to View, Right Click to Dismiss (permenantly)");

                if(_viewLoggingRectangles.Any(kvp=>kvp.Key.Contains(e.Location)))
                    SetToolTip("View Log");
                
                if (_killServerRect.Contains(e.Location))
                    SetToolTip("Unlock the Server Slot (Only use if you think the server exe has stopped responding)");

                if (!_foundToolTipLocation)
                {
                    _toolTip.Hide(this);
                    Cursor = Cursors.Arrow;
                }
                else
                    Cursor = Cursors.Hand;
            }
        }

        private void SetToolTip(string msg)
        {
            if (string.IsNullOrEmpty(_toolTip.GetToolTip(this)))
                _toolTip.Show(msg, this);

            _foundToolTipLocation = true;
        }

        protected override void OnMouseClick(MouseEventArgs e)
        {
            base.OnMouseClick(e);

            bool invalidateRequired = false;

            lock (oCollectionLock)
            {
                if (_killServerRect.Contains(e.Location))
                {
                    _cache.GetSlot().Unlock();
                    _killServerRect = Rectangle.Empty;
                }

                foreach (KeyValuePair<Rectangle, AutomationServiceException> kvp in _globalExceptionsRendered)
                {
                    //if the location is right and corresponds to an Exception
                    if (kvp.Key.Contains(e.Location))
                    {
                        //left click view it
                        if (e.Button == MouseButtons.Left)
                            WideMessageBox.Show(kvp.Value.Exception, title: "Exception from " + kvp.Value.MachineName + " at: " + kvp.Value.EventDate.ToString("g"));

                        //right click delete it with explanation
                        if (e.Button == MouseButtons.Right)
                        {
                            TypeTextOrCancelDialog dialog = new TypeTextOrCancelDialog("Type Explanation","Explanation",1000,"No Explanation Provided");
                            if (dialog.ShowDialog() == DialogResult.OK)
                            {
                                kvp.Value.Explanation = dialog.ResultText;
                                kvp.Value.SaveToDatabase();
                                _toolTip.Hide(this);
                            }
                        }
                    }
                }

                var viewLogJob = GetMouseIn(e, _viewLoggingRectangles);

                if (viewLogJob != null)
                    if (viewLogJob.LoggingServer_ID != null && viewLogJob.DataLoadRunID != null)
                    {
                        _activator.ActivateViewLog(
                            viewLogJob.Repository.GetObjectByID<ExternalDatabaseServer>(viewLogJob.LoggingServer_ID.Value),
                            viewLogJob.DataLoadRunID.Value);
                    }
                var cancelJob = GetMouseIn(e,_stopRectangles);

                if(cancelJob != null)
                {
                    if (IsCancellable(cancelJob))
                        cancelJob.IssueCancelRequest();
                    else
                        ForceDelete(cancelJob);

                    invalidateRequired = true;
                }
            }

            if(invalidateRequired)
                Invalidate();
        }

        private AutomationJob GetMouseIn(MouseEventArgs e, Dictionary<Rectangle, AutomationJob> rectangles)
        {
            var match = rectangles.Where(kvp => kvp.Key.Contains(e.Location)).ToArray();

            if (match.Length == 0)
                return null;

            return match[0].Value;
        }

        private void ForceDelete(AutomationJob job)
        {
            try
            {
                if (job.AutomationJobType == AutomationJobType.Cache)
                {
                    var window = job.GetCachingJobsPermissionWindowObjectIfAny();

                    if (window != null)
                        if (MessageBox.Show("Confirm also unlocking PermissionWindow " + window + "?", "Also Unlock Window?", MessageBoxButtons.YesNo) == DialogResult.Yes)
                            window.Unlock();
                }

                if (MessageBox.Show(
                    "Are you sure you want to delete the audit record for the AutomationJob.  You should only do this if the Automation server has crashed or the AutomationJob is somehow an orphan.",
                    "Confirm Force Deleting AutomationJob record", MessageBoxButtons.YesNo) == DialogResult.Yes)
                    job.DeleteInDatabase();
            }
            catch (Exception ex)
            {
                ExceptionViewer.Show("Could not force delete AutomationJob " + job, ex);
            }
        }

        private bool IsCancellable(AutomationJob job)
        {
            return job.LastKnownStatus == AutomationJobStatus.Running && !job.CancelRequested;
        }

        #endregion


        #region Painting
        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            lock (oCollectionLock)
            {
                //the rectangles where the timers are rendered, for Invalidate calls
                _timerTextRectangles.Clear();
                _viewLoggingRectangles.Clear();
                _stopRectangles.Clear();
                
                //render black background
                e.Graphics.FillRectangle(Brushes.Black, 0, 0, Width, Height);

                RenderFirstLine(e);
                
                if (_cache == null)
                    return;

                RenderServiceLevelExceptions(e);

                RenderJobs(e);
            }
            
        }

        private void RenderJobs(PaintEventArgs e)
        {
            var startRenderingJobListAtY = 48f;
            var jobBoxHeight = 24f;

            for (int i = 0; i < _cache.AutomationJobs.Length; i++)
            {
                var lineStartY = startRenderingJobListAtY + (i*jobBoxHeight);
                
                var job = _cache.AutomationJobs[i];
                var img = GetImageForJobType(job.AutomationJobType);

                Pen pen;
                Brush brush;

                if (job.LastKnownStatus == AutomationJobStatus.Crashed ||
                    job.LastKnownStatus == AutomationJobStatus.NotYetStarted)
                {

                    pen = _badPen;
                    brush = _badBrush;
                }
                else
                {

                    pen = _goodPen;
                    brush = _goodBrush;
                }

                //draw the icon
                e.Graphics.FillRectangle(Brushes.White,2,lineStartY,20,20);
                e.Graphics.DrawImage(img,2,lineStartY);

                //draw a box around the entire line
                e.Graphics.DrawRectangle(pen, 2, lineStartY, Width - 4, 20);

                //draw the job description
                e.Graphics.DrawString(job.Description, Font, brush, 24f, lineStartY + textHeightOffset);

                //draw the timer
                var timeRectangle = DrawTime(job,job.Lifeline, lineStartY,e.Graphics);

                //draw buttons
                if (job.DataLoadRunID != null)
                {
                    var rect = new Rectangle(timeRectangle.Left - 40,(int) (lineStartY + 1),19,19);
                    _viewLoggingRectangles.Add(rect, job);
                    e.Graphics.DrawImage(_loggingImage, rect);
                }

                var rectDelete = new Rectangle(timeRectangle.Left - 20, (int)(lineStartY + 1), 19, 19);
                _stopRectangles.Add(rectDelete, job);

                var killOrCancel = IsCancellable(job)
                    ? _cancelImage
                    : _killImage;

                e.Graphics.DrawImage(killOrCancel, rectDelete);

                //draw the state
                var statusString = job.LastKnownStatus.ToString();
                var statusStringWidth = e.Graphics.MeasureString(statusString, Font).Width;
                e.Graphics.DrawString(statusString, Font, brush, timeRectangle.Left - (40 + statusStringWidth), lineStartY + textHeightOffset);
            }
        }

        
        private Bitmap GetImageForJobType(AutomationJobType jobType)
        {
            switch (jobType)
            {
                case AutomationJobType.DQE:
                    return _dqeImage;
                case AutomationJobType.DLE:
                    return _dleImage;
                case AutomationJobType.Cache:
                    return _cachingImage;
                case AutomationJobType.UserCustomPipeline:
                    return _userPluginImage;
                default:
                    throw new ArgumentOutOfRangeException("jobType");
            }
        }

        private void RenderServiceLevelExceptions(PaintEventArgs e)
        {
            string header = "Exceptions:";
            var headerWidth = (int) e.Graphics.MeasureString(header, Font).Width;

            var rect = new Rectangle(headerWidth + 2, 26, Width - (3 + headerWidth), 20);
            
            e.Graphics.DrawRectangle(_goodPen,rect);
            e.Graphics.DrawString(header, Font, _goodBrush, 2, 26 + textHeightOffset);

            _globalExceptionsRendered = new Dictionary<Rectangle, AutomationServiceException>();
            for (int i = 0; i < _cache.GlobalServiceLevelExceptions.Length; i++)
            {
                var xLocation = rect.Left + 2 + (i*19);

                //don't bother rendering any more because they would be offscreen anyway
                if (xLocation > Width - 20)
                    break;

                var exceptionRectangle = new Rectangle(xLocation, rect.Top, 19, 19);
                e.Graphics.DrawImage(_errorImage,exceptionRectangle);
                _globalExceptionsRendered.Add(exceptionRectangle,_cache.GlobalServiceLevelExceptions[i]);
            }
            
        }

        private void RenderFirstLine(PaintEventArgs e)
        {
            //draw the console image with a white border in the top left
            e.Graphics.FillRectangle(Brushes.White, 1, 1, 21, 21);
            e.Graphics.DrawImage(_consoleImage,  2, 2);

            if (_cache == null || _cache.GetSlot() == null)
                RenderHeader(null,"No Server Selected Yet", null, false, e.Graphics);
            else
            {
                var server = _cache.GetSlot();

                if (string.IsNullOrWhiteSpace(server.LockHeldBy))
                    RenderHeader(null,"Automation Service Not Running", null, false, e.Graphics);
                else
                    RenderHeader(server,server.LockHeldBy, server.Lifeline, true, e.Graphics);
            }
        }

        private string GetTimeString(DateTime? lastKnownAliveTime)
        {
            if (lastKnownAliveTime == null)
                return EmptyTimeString;

            //it's a future date somehow! I guess maybe your automation server is running on a different clock
            if (lastKnownAliveTime > DateTime.Now)
                return EmptyTimeString;
            
            var elapsed = DateTime.Now - lastKnownAliveTime.Value;

            if (elapsed.TotalHours > 98)
                return ErrorString;

            return ((int)elapsed.TotalHours).ToString("D2") + ":" + elapsed.Minutes.ToString("D2") + ":" + elapsed.Seconds.ToString("D2");
        }

        private void RenderHeader(AutomationServiceSlot serverIfAny, string caption, DateTime? timer, bool isServerLocked, Graphics g)
        {
            var rect = new Rectangle(24, 2, Width - 26,20);

            //draw the title box
            if (isServerLocked)
                if (timer != null && IsGoodTime(timer))
                    g.DrawRectangle(_goodPen, rect);
                else
                    g.DrawRectangle(_badPen, rect);
            else
                g.DrawRectangle(_goodPenDash, rect);

            //draw the caption in the box
            g.DrawString(caption, Font, Brushes.White, rect.Left + 1, rect.Top + textHeightOffset);

            var timerRect = DrawTime(serverIfAny, timer, rect.Top + 1, g);

            if(isServerLocked)
            {
                //add kill button
                _killServerRect = new Rectangle(timerRect.X - 20, timerRect.Y, 15, 15);
                g.DrawImage(_killImage, _killServerRect);
            }
        }

        readonly Dictionary<object, List<DateTime>> _timesSeen = new Dictionary<object,List<DateTime>>();

        private Rectangle DrawTime(object caller, DateTime? timer, float lineStartY,Graphics g)
        {
            //work out space required to draw timer string
            string timerString = GetTimeString(timer);

            string toRender = "Last Checked In:" + timerString;
            var timerSize = g.MeasureString(toRender, Font);

            var timerRect = new Rectangle((int)(Width - timerSize.Width), (int)lineStartY + textHeightOffset, (int)timerSize.Width, (int)timerSize.Height);
            _timerTextRectangles.Add(timerRect);

            //if event was within the last 60 seconds draw a graph instead
            if (timer != null && caller != null)
            {
                //it's a new caller
                if (!_timesSeen.ContainsKey(caller))
                    _timesSeen.Add(caller, new List<DateTime>());

                //it's a new time for that caller
                if(!_timesSeen[caller].Contains(timer.Value))
                    _timesSeen[caller].Add(timer.Value);

                List<float> fractions = new List<float>();

                //compute the dot points as fractions
                foreach (var time in _timesSeen[caller].ToArray())
                {
                    var elapsedTime = DateTime.Now - time;
                    if (elapsedTime.TotalSeconds > 30) //remove times older than 30s
                    {
                        _timesSeen[caller].Remove(time);
                        continue;
                    }
                    
                    float fraction = (float)(elapsedTime.TotalSeconds / 30);
                    fraction = 1 - fraction;
                    fractions.Add(fraction);
                }

                if(fractions.Any())
                {
                    LinearGradientBrush br = new LinearGradientBrush(timerRect, Color.Black, Color.Black, 0 , false);


                    List<float> positions = new List<float>();
                    List<Color> colors = new List<Color>();

                    positions.Add(0);
                    colors.Add(Color.Black);

                    List<int> skippedFractions = new List<int>();

                    for (int i = 0; i < fractions.Count; i++)
                    {
                        var toAdd = fractions[i];

                        var leadPoint = toAdd - 0.0005f;
                        var midPoint = toAdd;
                        var trailPoint = toAdd + 0.05f;

                        //skip this one if it is so close to the last one
                        //or it's tail takes the trail past the end
                        if(leadPoint <= (positions.Any()? positions.Last():0f) || trailPoint >= 1)
                        {
                            skippedFractions.Add(i);
                            continue;
                        }
                        
                        positions.Add(leadPoint);
                        colors.Add(Color.Black);

                        positions.Add(midPoint);
                        colors.Add(Color.GreenYellow);

                        positions.Add(trailPoint);
                        colors.Add(Color.Black);
                    }

                    positions.Add(1);
                    colors.Add(Color.Black);

                    ColorBlend cb = new ColorBlend();
                    cb.Positions = positions.ToArray();
                    cb.Colors = colors.ToArray();
                    br.InterpolationColors= cb;

                    List<Point> points = new List<Point>();
                    
                    g.SmoothingMode = SmoothingMode.AntiAlias;
                    
                    var amplitude = timerRect.Height/2;

                    for (Int32 x = 0; x < timerRect.Width-2; x += 1)
                    {
                        points.Add(new Point(timerRect.Left + x, 
                            
                            //center line 
                            (timerRect.Top + amplitude/4) + 
                            
                            //sine wave
                            (int)(amplitude + amplitude * Math.Sin(2*x / Math.PI))));
                    }


                    var pen = new Pen(br);
                    pen.Width = 2;
                    g.DrawCurve(pen, points.ToArray());
                    
                    for (int i = 0; i < fractions.Count; i++)
                    {
                        if(skippedFractions.Contains(i))
                            continue;
                        
                        var dotPoint = points[Math.Min(points.Count - 1, (int)(fractions[i] * points.Count))];
                        g.FillRectangle(Brushes.White, dotPoint.X, dotPoint.Y, 2, 2);
                    }

                    g.DrawRectangle(Pens.ForestGreen, timerRect.X,timerRect.Y,timerRect.Width,timerRect.Height);

                    
                    return timerRect;
                }
            }
            
            //draw the time
            g.DrawString(toRender, Font, IsGoodTime(timer) ? _goodBrush : _badBrush, timerRect.X, timerRect.Y);


            return timerRect;
        }

        private bool IsGoodTime(DateTime? timer)
        {
            return
                //if there is no known time thats bad
                timer != null &&
                //or if the current time is 30+ minutes on 
                (DateTime.Now - timer.Value).TotalMinutes < ElapsedMinutesBeforeConsideredBadTime;
        }

        #endregion

        public void SetupFor(IActivateItems activator, AutomationServerMonitorUIObjectCache cache)
        {
            _activator = activator;
            _cache = cache;
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            
            Invalidate();
        }

        private void databaseRefreshTimer_Tick(object sender, EventArgs e)
        {
            lock (oCollectionLock)
            {
                if (_cache != null)
                {
                    bool invalidateRequired = false;
                    try
                    {
                        invalidateRequired = _cache.UpdateServerStateIfAny();
                    }
                    catch (Exception exception)
                    {
                        refreshTimer.Stop(); //it's broken so don't bother refreshing it
                        ExceptionViewer.Show(exception);
                    }

                    if (invalidateRequired)
                    {
                        Invalidate();
                    }

                }
                    
                        
            }
        }

        private void graphsTimer_Tick(object sender, EventArgs e)
        {
            lock (oCollectionLock)
            {
                foreach (Rectangle r in _timerTextRectangles)
                    Invalidate(r);
            }
        }
    }
}

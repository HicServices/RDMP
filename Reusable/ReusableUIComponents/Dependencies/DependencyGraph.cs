using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Threading;
using GraphX.Controls;
using GraphX.PCL.Common.Enums;
using GraphX.PCL.Logic.Algorithms.OverlapRemoval;
using GraphX.PCL.Logic.Models;
using QuickGraph;
using ReusableLibraryCode;

using ReusableUIComponents.Dependencies.Models;
using ReusableUIComponents.Dialogs;

namespace ReusableUIComponents.Dependencies
{
    /// <summary>
    /// Shows a network diagram of all the dependencies both upstream (objects depending on this object) and downstream (objects this object depends on) of a given object.  For example
    /// a Catalogue (dataset) depends on CatalogueItems which depend on ExtractionInformation and ColumnInfos which themselves depend on TableInfos.  The upstream dependencies of Catalogue
    /// include LoadMetadata (which would then have a downstream relationship into other Catalogues it loads as part of it's load).
    /// 
    /// <para>Unsurprisingly this can spiral into thousands of objects.  Use 'Dependency Depth' to limit the number of recursive dependency navigations (both up and downstream).  If you get lost 
    /// in a big network diagram you can tick Highlight on nodes of a given type e.g. Catalogue.</para>
    /// 
    /// <para>You can change the layout to other layouts such as ring or tree but LinLog is probably the easiest to interpret.  Some layouts take longer to calculate than others if you have very
    /// a large dependency network being displayed.</para>
    /// </summary>
    public partial class DependencyGraph : UserControl
    {
        private readonly IObjectVisualisation _visualiser;


        private static Dictionary<Type, bool> lastConfiguredVertexTypeFilters = null;
        private static Dictionary<string, bool> lastConfiguredVertexHighlightFilters = null;
        Dictionary<Type, bool> VertexTypeFilters = new Dictionary<Type, bool>();
        Dictionary<string, bool> VertexHighlightFilters = new Dictionary<string, bool>();
        private const LayoutAlgorithmTypeEnum _startingAlgorithm = LayoutAlgorithmTypeEnum.LinLog;
        private readonly bool _setupComplete;
        private DispatcherTimer timer;
        private GraphExample _graph;
        private GraphAreaExample _gArea;
        private bool _searchInProgress;
        private CheckBox rootCB;

        private object _oRelayoutSuspendedLock = new object();
        private bool _relayoutSuspended = false;

        public DependencyGraph(Type[] allowFilterOnTypes, IObjectVisualisation visualiser)
        {
            _visualiser = visualiser;
            InitializeComponent();
            //prevent visual studio crashes
            if (LicenseManager.UsageMode == LicenseUsageMode.Designtime)
                return;
            loadIcon.Visible = true;
            ddLayout.DataSource = Enum.GetValues(typeof(LayoutAlgorithmTypeEnum));
            elementHost1.Child = GenerateWpfVisuals();
            elementHost1.Child.MouseLeftButtonUp += (sender, args) => elementHost1.Focus();
            _gArea.GenerateGraphFinished += delegate { _zoomctrl.ZoomToFill(); };
            bool shouldCopyOldValues = false;
            _searchInProgress = false;

            if (allowFilterOnTypes == null)
                throw new ArgumentException("you must specify all types of IHasDependencies that you want to be able to filter on","allowFilterOnTypes");

            //form was opened previously opened and some checkboxes could have been messed with, if the Type collection is the same then we can get the same checkbox values to start with
            if (lastConfiguredVertexTypeFilters != null)
            {
                shouldCopyOldValues = true;
                foreach (var t in allowFilterOnTypes)
                    if (!lastConfiguredVertexTypeFilters.Keys.Contains(t)) //it is a novel type so the Type colleciton to filter on was different, lets not take the cached value (because there isn't one!)
                        shouldCopyOldValues = false;
            }

            if (lastConfiguredVertexHighlightFilters != null)
            {
                shouldCopyOldValues = true;
                foreach (var t in allowFilterOnTypes)
                    if (!lastConfiguredVertexHighlightFilters.Keys.Contains(t.ToString())) //it is a novel type so the Type colleciton to filter on was different, lets not take the cached value (because there isn't one!)
                        shouldCopyOldValues = false;
            }

            //add the checkboxes for filter types
            foreach (Type filterOnType in allowFilterOnTypes)
            {
                if (shouldCopyOldValues)
                {
                    VertexTypeFilters.Add(filterOnType, lastConfiguredVertexTypeFilters[filterOnType]);
                    VertexHighlightFilters.Add(filterOnType.ToString(), lastConfiguredVertexHighlightFilters[filterOnType.ToString()]);
                }
                else
                {
                    VertexTypeFilters.Add(filterOnType, false);
                    VertexHighlightFilters.Add(filterOnType.ToString(), false);
                }

                CheckBox cb = new CheckBox();
                CheckBox highlightcb = new CheckBox();

                cb.Text = filterOnType.Name;
                highlightcb.Text = filterOnType.Name;

                cb.Checked = VertexTypeFilters[filterOnType];

                cb.AutoSize = true;
                highlightcb.AutoSize = true;

                Type type = filterOnType;
                cb.CheckedChanged += delegate
                {
                    if (!_relayoutSuspended)
                        cbToggleAllShow.CheckState = CheckState.Indeterminate;

                    VertexTypeFilters[type] = cb.Checked;
                    if(Root!=null)
                        GraphDependenciesOf(Root);
                };
                highlightcb.CheckedChanged += delegate
                {
                    if(!_relayoutSuspended)
                        cbToggleAllHighlight.CheckState = CheckState.Indeterminate;

                    VertexHighlightFilters[type.ToString()] = highlightcb.Checked;
                    HighlightTypes();
                };
                pFilterCheckboxes.Controls.Add(cb);
                pHighlightCheckboxes.Controls.Add(highlightcb);
            }

            //Setup "Show All" and "Highlight All" checkboxes
            cbToggleAllHighlight.CheckedChanged += delegate
            {
                lock (_oRelayoutSuspendedLock)
                {
                    if(cbToggleAllHighlight.CheckState == CheckState.Indeterminate)
                        return;

                    _relayoutSuspended = true;

                    foreach (CheckBox cb in pHighlightCheckboxes.Controls)
                        cb.Checked = cbToggleAllHighlight.Checked;

                    _relayoutSuspended = false;
                    HighlightTypes();
                }
            };

            cbToggleAllShow.CheckedChanged += delegate
            {
                lock (_oRelayoutSuspendedLock)
                {
                    if (cbToggleAllShow.CheckState == CheckState.Indeterminate)
                        return;

                    _relayoutSuspended = true;

                    foreach (CheckBox cb in pFilterCheckboxes.Controls)
                        if (cb.Enabled)
                            cb.Checked = cbToggleAllShow.Checked;

                    _relayoutSuspended = false;
                    
                    if(Root != null)
                        GraphDependenciesOf(Root);
                }
            };
            _setupComplete = true;
        }

        private ZoomControl _zoomctrl;


        private UIElement GenerateWpfVisuals()
        {
            _zoomctrl = new ZoomControl();
            ZoomControl.SetViewFinderVisibility(_zoomctrl, Visibility.Visible);
            /* ENABLES WINFORMS HOSTING MODE --- >*/
            var logic = new GXLogicCore<DataVertex, DataEdge, BidirectionalGraph<DataVertex, DataEdge>>();
            _gArea = new GraphAreaExample() { EnableWinFormsHostingMode = true, LogicCore = logic };
            _gArea.LogicCore.AsyncAlgorithmCompute = true;

            logic.Graph = new GraphExample();
            logic.DefaultLayoutAlgorithm = _startingAlgorithm;
            logic.DefaultLayoutAlgorithmParams = _gArea.LogicCore.AlgorithmFactory.CreateLayoutParameters(_startingAlgorithm);

            logic.DefaultOverlapRemovalAlgorithm = OverlapRemovalAlgorithmTypeEnum.FSA;
            logic.DefaultOverlapRemovalAlgorithmParams = logic.AlgorithmFactory.CreateOverlapRemovalParameters(OverlapRemovalAlgorithmTypeEnum.FSA);

            ((OverlapRemovalParameters)logic.DefaultOverlapRemovalAlgorithmParams).HorizontalGap = 50;
            ((OverlapRemovalParameters)logic.DefaultOverlapRemovalAlgorithmParams).VerticalGap = 50;
            logic.DefaultEdgeRoutingAlgorithm = EdgeRoutingAlgorithmTypeEnum.None;


            _zoomctrl.Content = _gArea;

            var myResourceDictionary = new ResourceDictionary { Source = new Uri("/ReusableUIComponents;component/Dependencies/Templates/template.xaml", UriKind.RelativeOrAbsolute) };
            _zoomctrl.Resources.MergedDictionaries.Add(myResourceDictionary);
            return _zoomctrl;
        }


        public IHasDependencies Root { get; private set; }

        DependencyFinder _finder;
        object finderLock = new object();

        private Dictionary<IHasDependencies, IHasDependencies[]> _globalParentCache = new Dictionary<IHasDependencies, IHasDependencies[]>();
        private Dictionary<IHasDependencies, IHasDependencies[]> _globalChildCache = new Dictionary<IHasDependencies, IHasDependencies[]>();

        Thread workerThread;

        public void GraphDependenciesOf(IHasDependencies root)
        {
            if (_relayoutSuspended)
                return;

            lblThisMayTakeSomeTime.Visible = false;
            lblLoadProgress.Visible = true;
            lblLoadProgress.Text = "Initialising Load...";
            tbHighlight.Enabled = false;
            lblSearch.Visible = false;
            loadIcon.Visible = true;
            Stopwatch dependencyFindingStopwatch = new Stopwatch(); 
            Stopwatch layoutWatch = new Stopwatch();
            
            if (!_setupComplete)
                return;
            try
            {
                _gArea.CancelRelayout();
            }
            catch (Exception e)
            {
                ExceptionViewer.Show("Problem occurred trying to cancel previous relayout of dependency graph, maybe this isn't a quick fix you thought it was Thomas?",e);
            }

            //record the new root and "read only" its checkbox
            if(rootCB!=null)
                rootCB.Enabled = true;
            Root = root;
            SetRootCheckbox();
            rootCB.Checked = true;

            //if another finder is currently running
            if (_finder != null)
                _finder.Cancel();

            lastConfiguredVertexTypeFilters = VertexTypeFilters;
            lastConfiguredVertexHighlightFilters = VertexHighlightFilters;
            lblLoadProgress.Text = "Finding Dependencies...";

            

            workerThread = new Thread(() =>
            {
                lock (finderLock)
                {
                    dependencyFindingStopwatch.Start();
                    //if it is the same root node then we can reuse the cached answers
                    _finder = new DependencyFinder(Root, Convert.ToInt32(nDependencyDepth.Value), VertexTypeFilters,
                        _globalParentCache, _globalChildCache, _visualiser, VertexHighlightFilters);

                    //clear the existing graph
                    _graph = new GraphExample();

                    _finder.AddDependenciesTo(_graph);

                    dependencyFindingStopwatch.Stop();
                    //build up global knowledge cache
                    foreach (KeyValuePair<IHasDependencies, IHasDependencies[]> kvp in _finder.ParentCache)
                        if (!_globalParentCache.ContainsKey(kvp.Key))
                            _globalParentCache.Add(kvp.Key, kvp.Value);

                    foreach (KeyValuePair<IHasDependencies, IHasDependencies[]> kvp in _finder.ChildCache)
                        if (!_globalChildCache.ContainsKey(kvp.Key))
                            _globalChildCache.Add(kvp.Key, kvp.Value);


                    if (_finder.Cancelled)
                        return;

                    int timeout = 5000;
                    while (!IsHandleCreated && (timeout -= 100) > 0)
                        Thread.Sleep(100);

                    if (_finder.Cancelled)
                        return;

                    if(IsDisposed)
                        return;

                    //reset the graph 
                    BeginInvoke(

                        new MethodInvoker(() =>
                        {
                            lblLoadProgress.Text = "Generating Layout...";
                            layoutWatch.Start();
                            timer = new DispatcherTimer(DispatcherPriority.SystemIdle);
                            timer.Tick += LongLoadElapsed;
                            timer.Interval = TimeSpan.FromMilliseconds(10000);
                            timer.IsEnabled = true;
                            _gArea.GenerateGraph(_graph);

                            _gArea.GenerateGraphFinished += delegate
                            {
                                layoutWatch.Stop();
                                lblTimeSpentFindingDependencies.Text = "Dependency Find Time:" +
                                                                       dependencyFindingStopwatch.Elapsed;
                                lblTimeSpentLayingout.Text = "Layout Elapsed Time:" + layoutWatch.Elapsed;
                                loadIcon.Visible = false;
                                lblLoadProgress.Visible = false;
                                lblThisMayTakeSomeTime.Visible = false;
                                timer.IsEnabled = false;
                                tbHighlight.Enabled = true;
                                lblSearch.Visible = true;
                            };
                        })
                        );
                }

            });

            workerThread.Start();

            

            
        }

        public void ShowTypeList(List<Type> typesToShow)
        {
            foreach (Type type in typesToShow)
            {
                foreach (CheckBox cb in pFilterCheckboxes.Controls)
                {
                    if (cb.Text == type.Name)
                    {
                        cb.Checked = true;
                        VertexTypeFilters[type] = true;
                    }
                }
            }
        }
        public void ShowTypeListAll()
        {
            cbToggleAllShow.Checked = true;
        }


        private void SetRootCheckbox()
        {
            foreach (CheckBox cb in pFilterCheckboxes.Controls)
            {
                if (cb.Text == Root.GetType().Name)
                {
                    rootCB = cb;
                    rootCB.Enabled = false;
                }
            }
        }

        private void nDependencyDepth_ValueChanged(object sender, EventArgs e)
        {
            if (_gArea == null)
                return;

            //reset it
            GraphDependenciesOf(Root);
        }

        private void ddLayout_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_gArea == null)
                return;

            _gArea.LogicCore.DefaultLayoutAlgorithm = (LayoutAlgorithmTypeEnum)ddLayout.SelectedValue;
            _gArea.LogicCore.DefaultLayoutAlgorithmParams = _gArea.LogicCore.AlgorithmFactory.CreateLayoutParameters((LayoutAlgorithmTypeEnum)ddLayout.SelectedValue);
            
            if (Root == null)
                return;
            GraphDependenciesOf(Root);
            
        }

        private void DependencyGraph_Load(object sender, EventArgs e)
        {
            ddLayout.SelectedItem = _startingAlgorithm;
        }

        private void HighlightTypes()
        {
            if (_relayoutSuspended)
                return;

            Stopwatch highlightWatch = Stopwatch.StartNew();
            foreach (DataVertex vertex in _graph.Vertices)
                vertex.RefreshState(VertexHighlightFilters[vertex.CoreObjectType.FullName]);
            highlightWatch.Stop();
            lblTimeToHighlight.Text = "Highlight Time: " + highlightWatch.Elapsed;
        }

       private void tbHighlight_TextChanged(object sender, EventArgs e)
       {
           if (_searchInProgress)
               return;

           _searchInProgress = true;
           timer = new DispatcherTimer(DispatcherPriority.SystemIdle);
           timer.Tick += SearchTimer_Elapsed;
           timer.Interval = TimeSpan.FromMilliseconds(3000);
           timer.IsEnabled = true;
        }

        public void SearchTimer_Elapsed(object sender, EventArgs e)
        {
            timer.IsEnabled = false;
            if (tbHighlight.Text == "")
            {
                foreach (DataVertex dv in _graph.Vertices)
                {
                    dv.RefreshState(false);
                }
                lblSearch.Text = "Nodes Reset" + tbHighlight.Text;
            }
            else
            {
                lblSearch.Text = "Nodes Found For: " + tbHighlight.Text;
                foreach (DataVertex dv in _graph.Vertices)
                {
                    if (dv.NameAndType[0].Contains(tbHighlight.Text) || dv.NameAndType[1].Contains(tbHighlight.Text))
                    {
                        dv.RefreshState(true);
                    }
                    else
                        dv.RefreshState(false);
                }
            }
            _searchInProgress = false;
        }

        public void LongLoadElapsed(object sender, EventArgs e)
        {
            if(lblLoadProgress.Visible)
                lblThisMayTakeSomeTime.Visible = true;
        }

        
    }
}

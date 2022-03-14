using BrightIdeasSoftware;
using MapsDirectlyToDatabaseTable;
using MapsDirectlyToDatabaseTable.Attributes;
using Rdmp.Core;
using Rdmp.Core.CommandExecution;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Curation.Data.Cohort;
using Rdmp.Core.Curation.Data.DataLoad;
using Rdmp.Core.DataExport.Data;
using Rdmp.Core.Icons.IconProvision;
using Rdmp.Core.Providers;
using Rdmp.UI.Collections;
using Rdmp.UI.ItemActivation;
using Rdmp.UI.Theme;
using ReusableLibraryCode.Settings;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Rdmp.UI.SimpleDialogs
{
    public partial class SelectDialog2<T> : Form, IVirtualListDataSource
    {
        private readonly Dictionary<IMapsDirectlyToDatabaseTable, DescendancyList> _searchables;
        private readonly AttributePropertyFinder<UsefulPropertyAttribute> _usefulPropertyFinder;
        private ICoreIconProvider _coreIconProvider;
        private FavouritesProvider _favouriteProvider;

        private const int MaxMatches = 100;
        private List<IMapsDirectlyToDatabaseTable> _matches;
        private object oMatches = new object();

        private const float DrawMatchesStartingAtY = 50;
        private const float RowHeight = 20;

        private const int DiagramTabDistance = 20;

        private Task _lastFetchTask = null;
        private CancellationTokenSource _lastCancellationToken;
        private Type[] _types;
        private HashSet<string> _typeNames;

        private List<Type> showOnlyTypes = new List<Type>();
        private Type _alwaysFilterOn;
        private ToolStripTextBox _lblId;
        private IActivateItems _activator;
        private bool _allowDeleting;



        /// <summary>
        /// Hides the Type selection toggle buttons and forces results to only appear matching the given Type
        /// </summary>
        public Type AlwaysFilterOn
        {
            get => _alwaysFilterOn;
            set
            {
                if (value != null)
                    Controls.Remove(toolStrip1);
                else
                    Controls.Add(toolStrip1);

                _alwaysFilterOn = value;
                tbFilter_TextChanged(this, null);
            }
        }
        public bool AllowMultiSelect
        {
            get { return olv.MultiSelect; }
            set
            {
                olv.MultiSelect = value;
                if (value)
                {
                    if (!olv.AllColumns.Contains(olvSelected))
                    {
                        olv.AllColumns.Add(olvSelected);
                    }
                }
                else
                {
                    olv.AllColumns.Remove(olvSelected);
                }

                if (value)
                {
                    olv.ShowGroups = true;
                    olv.AlwaysGroupByColumn = olvSelected;
                    olv.AlwaysGroupBySortOrder = SortOrder.Descending;
                    olv.ShowItemCountOnGroups = true;
                }
                else
                {

                    olv.AlwaysGroupByColumn = null;
                    olv.ShowGroups = false;
                }

                olv.RebuildColumns();
            }
        }
        public HashSet<T> MultiSelected { get; }

        /// <summary>
        /// Object types that appear in the task bar as filterable types
        /// </summary>
        private Dictionary<Type, RDMPCollection> EasyFilterTypesAndAssociatedCollections = new Dictionary<Type, RDMPCollection>()
        {
            {typeof (Catalogue),RDMPCollection.Catalogue},
            {typeof (CatalogueItem),RDMPCollection.Catalogue},
            {typeof (SupportingDocument),RDMPCollection.Catalogue},
            {typeof (Project),RDMPCollection.DataExport},
            {typeof (ExtractionConfiguration),RDMPCollection.DataExport},
            {typeof (ExtractableCohort),RDMPCollection.SavedCohorts},
            {typeof (CohortIdentificationConfiguration),RDMPCollection.Cohort},
            {typeof (TableInfo),RDMPCollection.Tables},
            {typeof (ColumnInfo),RDMPCollection.Tables},
            {typeof (LoadMetadata),RDMPCollection.DataLoad},
        };


        /// <summary>
        /// Identifies which Types are checked by default when the NavigateToObjectUI is shown when the given RDMPCollection has focus
        /// </summary>
        public Dictionary<RDMPCollection, Type[]> StartingEasyFilters
            = new Dictionary<RDMPCollection, Type[]>()
            {
                {RDMPCollection.Catalogue, new[] {typeof (Catalogue)}},
                {RDMPCollection.Cohort, new[] {typeof (CohortIdentificationConfiguration)}},
                {RDMPCollection.DataExport, new[] {typeof (Project), typeof (ExtractionConfiguration)}},
                {RDMPCollection.DataLoad, new[] {typeof (LoadMetadata)}},
                {RDMPCollection.SavedCohorts, new[] {typeof (ExtractableCohort)}},
                {RDMPCollection.Tables, new[] {typeof (TableInfo)}},
                {RDMPCollection.None,new []{typeof(SupportingDocument),typeof(CatalogueItem)}} //Add all other Type checkboxes here so that they are recognised as Typenames
            };


        public SelectDialog2(DialogArgs args, IActivateItems activator, IEnumerable<T> toSelectFrom, bool allowSelectingNULL, bool allowDeleting, RDMPCollection focusedCollection = RDMPCollection.None)
        {
            _activator = activator;
            _allowDeleting = allowDeleting;
            _coreIconProvider = activator.CoreIconProvider;
            _favouriteProvider = activator.FavouritesProvider;

            InitializeComponent();

            if(IsDatabaseObjects())
            {
                _searchables = toSelectFrom.Cast<IMapsDirectlyToDatabaseTable>().ToDictionary(k => k, activator.CoreChildProvider.GetDescendancyListIfAnyFor);
            }
            else
            {
                _allObjects = toSelectFrom.ToArray();
            }

            taskDescriptionLabel1.SetupFor(args);


            Text = args.WindowTitle;

            _usefulPropertyFinder = new AttributePropertyFinder<UsefulPropertyAttribute>(_searchables.Keys);

            tbFilter.Focus();

            tbFilter.Text = args.InitialSearchText;
            tbFilter.TextChanged += tbFilter_TextChanged;
            tbFilter.KeyPress += (s, e) =>
            {
                //prevents windows 'bong' noise when you hit enter
                if (e.KeyChar == (int)Keys.Enter)
                    e.Handled = true;
            };

            StartPosition = FormStartPosition.CenterScreen;
            DoubleBuffered = true;

            _types = _searchables.Keys.Select(k => k.GetType()).Distinct().ToArray();
            _typeNames = new HashSet<string>(_types.Select(t => t.Name));

            foreach (Type t in StartingEasyFilters.SelectMany(v => v.Value))
            {
                if (!_typeNames.Contains(t.Name))
                    _typeNames.Add(t.Name);
            }

            Type[] startingFilters = null;

            if (focusedCollection != RDMPCollection.None && StartingEasyFilters.ContainsKey(focusedCollection))
                startingFilters = StartingEasyFilters[focusedCollection];

            BackColorProvider backColorProvider = new BackColorProvider();

            foreach (Type t in EasyFilterTypesAndAssociatedCollections.Keys)
            {
                var b = new ToolStripButton();
                b.Image = activator.CoreIconProvider.GetImage(t);
                b.CheckOnClick = true;
                b.Tag = t;
                b.DisplayStyle = ToolStripItemDisplayStyle.Image;

                string shortCode = SearchablesMatchScorer.ShortCodes.Single(kvp => kvp.Value == t).Key;

                b.Text = $"{t.Name} ({shortCode})";
                b.CheckedChanged += CollectionCheckedChanged;
                b.Checked = startingFilters != null && startingFilters.Contains(t);

                b.BackgroundImage = backColorProvider.GetBackgroundImage(b.Size, EasyFilterTypesAndAssociatedCollections[t]);

                toolStrip1.Items.Add(b);
            }

            toolStrip1.Items.Add(new ToolStripLabel("ID:"));
            toolStrip1.Items.Add(_lblId = new ToolStripTextBox());
            _lblId.TextChanged += tbFilter_TextChanged;

            if (UserSettings.AdvancedFindFilters)
            {
                AddUserSettingCheckbox(() => UserSettings.ShowInternalCatalogues, (v) => UserSettings.ShowInternalCatalogues = v, "I", "Include Internal");
                AddUserSettingCheckbox(() => UserSettings.ShowDeprecatedCatalogues, (v) => UserSettings.ShowDeprecatedCatalogues = v, "D", "Include Deprecated");
                AddUserSettingCheckbox(() => UserSettings.ShowColdStorageCatalogues, (v) => UserSettings.ShowColdStorageCatalogues = v, "C", "Include Cold Storage");
                AddUserSettingCheckbox(() => UserSettings.ShowProjectSpecificCatalogues, (v) => UserSettings.ShowProjectSpecificCatalogues = v, "P", "Include Project Specific");
                AddUserSettingCheckbox(() => UserSettings.ShowNonExtractableCatalogues, (v) => UserSettings.ShowNonExtractableCatalogues = v, "E", "Include Extractable");
            }




            taskDescriptionLabel1.Visible = false;

            //start at cancel so if they hit the X nothing is selected
            DialogResult = DialogResult.Cancel;

            olvID.AspectGetter = (m) => (m as IMapsDirectlyToDatabaseTable)?.ID ?? null;

            // don't add the ID column if we aren't talking about database objects
            if (!IsDatabaseObjects())
            {
                olv.AllColumns.Remove(olvID);
            }

            olvName.AspectGetter = (m) => m.ToString();

            olvName.ImageGetter = GetImage;
            olv.RowHeight = 19;

            if (!allowSelectingNULL)
            {
                //disable the option to select NULL
                btnSelectNULL.Visible = false;
            }

            //default to not allowing multi selection
            olv.MultiSelect = false;
            btnSelect.Enabled = false;

            // Setup olvSelected but leave it removed for now (IsVisible is problematic - especially for first columns)
            olvSelected.CheckBoxes = true;
            olvSelected.AspectGetter += Selected_AspectGetter;
            olvSelected.AspectPutter += Selected_AspectPutter;
            olv.AllColumns.Remove(olvSelected);

            olv.RebuildColumns();

            MultiSelected = new HashSet<T>();

            olvSelected.GroupWithItemCountFormat = "{0} ({1} objects)";
            olvSelected.GroupWithItemCountSingularFormat = "{0} (1 objects)";
            olvSelected.GroupKeyGetter += GroupKeyGetter;

            RDMPCollectionCommonFunctionality.SetupColumnTracking(olv, olvName, new Guid("298cda00-5ec8-423c-9230-71d78bec6bc4"));
            RDMPCollectionCommonFunctionality.SetupColumnTracking(olv, olvID, new Guid("bb0fe2f0-1e73-4b00-a5b7-4b6ce3510bab"));

            btnCancel.KeyPress += BtnKeypress;
            btnSelect.KeyPress += BtnKeypress;
            btnSelectNULL.KeyPress += BtnKeypress;

            // Prevents the object list view going 'BONG' every time the user hits space
            // to check/uncheck objects
            olv.KeyPress += (s, e) =>
            {
                if (e.KeyChar == ' ' || e.KeyChar == '\r' || e.KeyChar == '\n')
                    e.Handled = true;
            };

            FetchMatches(args.InitialSearchText, CancellationToken.None);

            olv.VirtualListDataSource = this;
        }

        private void BtnKeypress(object sender, KeyPressEventArgs e)
        {
            // if user is typing change the focus to the text box
            if (char.IsLetterOrDigit(e.KeyChar))
            {
                tbFilter.Text += e.KeyChar;
                tbFilter.Select(tbFilter.TextLength, 0);
                tbFilter.Focus();
            }
        }
        public Bitmap GetImage(object model)
        {
            var bmp = _activator.CoreIconProvider.GetImage(model);
            return bmp == _activator.CoreIconProvider.ImageUnknown ? null : bmp;
        }

        private bool buildGroupsRequired = false;
        private bool _isClosed;

        /// <summary>
        /// All the objects when T is not an IMapsDirectlyToDatabaseTable.
        /// </summary>
        private T[] _allObjects;

        private void timer1_Tick(object sender, EventArgs e)
        {
            if (buildGroupsRequired)
            {
                buildGroupsRequired = false;
                olv.BeginUpdate();
                olv.SuspendLayout();
                olv.BuildGroups();
                olv.EndUpdate();
                olv.ResumeLayout();
            }
        }

        private object GroupKeyGetter(object rowObject)
        {
            if (MultiSelected == null)
                return false;

            return MultiSelected.Contains((T)rowObject) ? "Selected" : "Not Selected";
        }

        private void Selected_AspectPutter(object rowobject, object newvalue)
        {
            timer1.Stop();
            timer1.Start();

            var b = (bool)newvalue;
            if (b)
                MultiSelected.Add((T)rowobject);
            else
                MultiSelected.Remove((T)rowobject);

            //olvObjects.BuildGroups();
            buildGroupsRequired = true;

            UpdateButtonEnabledness();
        }
        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            base.OnFormClosed(e);
            
            _isClosed = true;
        }
        private void UpdateButtonEnabledness()
        {
            if (AllowMultiSelect)
                btnSelect.Enabled = MultiSelected.Any();
            else
                btnSelect.Enabled = olv.SelectedObject != null;
        }

        private object Selected_AspectGetter(object rowObject)
        {
            if (!AllowMultiSelect)
                return null;

            return MultiSelected.Contains((T)rowObject);
        }

        private void FetchMatches(string text, CancellationToken cancellationToken)
        {
            var scorer = new SearchablesMatchScorer();
            scorer.RespectUserSettings = UserSettings.AdvancedFindFilters;
            scorer.TypeNames = _typeNames;
            scorer.BumpMatches = _activator.HistoryProvider.History.Select(h => h.Object).ToList();

            if (_lblId != null && int.TryParse(_lblId.Text, out int requireId))
                scorer.ID = requireId;

            if (AlwaysFilterOn != null)
                showOnlyTypes = new List<Type>(new[] { AlwaysFilterOn });

            var scores = scorer.ScoreMatches(_searchables, text, cancellationToken, showOnlyTypes);

            if (scores == null)
                return;
            lock (oMatches)
            {
                _matches = scorer.ShortList(scores, MaxMatches, _activator);
            }
        }

        private void listBox1_CellClick(object sender, CellClickEventArgs e)
        {
            throw new NotImplementedException();
        }
        private void listBox1_KeyUp(object sender, KeyEventArgs e)
        {
            throw new NotImplementedException();
        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            throw new NotImplementedException();
        }

        private void CollectionCheckedChanged(object sender, EventArgs e)
        {
            throw new NotImplementedException();
        }

        private bool IsDatabaseObjects()
        {
            return typeof(IMapsDirectlyToDatabaseTable).IsAssignableFrom(typeof(T));
        }

        private void tbFilter_TextChanged(object sender, EventArgs e)
        {
            //cancel the last execution if it has not completed yet
            if (_lastFetchTask != null && !_lastFetchTask.IsCompleted)
                _lastCancellationToken.Cancel();

            _lastCancellationToken = new CancellationTokenSource();

            var toFind = tbFilter.Text;

            _lastFetchTask = Task.Run(() => FetchMatches(toFind, _lastCancellationToken.Token))
                .ContinueWith(
                    (s) =>
                    {
                        if (_isClosed)
                            return;

                        try
                        {
                            if (_isClosed)
                                return;

                            if (_isClosed)
                                return;

                            // TODO: might need some magic here
                            olv.Sort();
                        }
                        catch (ObjectDisposedException)
                        {

                        }
                    }, TaskScheduler.FromCurrentSynchronizationContext());
            
        }

        public void AddObjects(ICollection modelObjects)
        {
            throw new NotImplementedException();
        }

        public object GetNthObject(int n)
        {
            throw new NotImplementedException();
        }

        public int GetObjectCount()
        {
            if(IsDatabaseObjects())
            {
                return _matches.Count;
            }

            return _allObjects.Length;            
        }

        public int GetObjectIndex(object model)
        {
            throw new NotImplementedException();
        }

        public void InsertObjects(int index, ICollection modelObjects)
        {
            throw new NotImplementedException();
        }

        public void PrepareCache(int first, int last)
        {
            throw new NotImplementedException();
        }

        public void RemoveObjects(ICollection modelObjects)
        {
            throw new NotImplementedException();
        }

        public int SearchText(string value, int first, int last, OLVColumn column)
        {
            throw new NotImplementedException();
        }

        public void SetObjects(IEnumerable collection)
        {
            throw new NotImplementedException();
        }

        public void Sort(OLVColumn column, SortOrder order)
        {
            throw new NotImplementedException();
        }

        public void UpdateObject(int index, object modelObject)
        {
            throw new NotImplementedException();
        }
        private void AddUserSettingCheckbox(Func<bool> getter, Action<bool> setter, string name, string toolTip)
        {
            var b = new ToolStripButton(name);
            b.CheckOnClick = true;
            b.ToolTipText = toolTip;
            b.DisplayStyle = ToolStripItemDisplayStyle.Text;
            b.Checked = getter();
            b.CheckedChanged += (s, e) =>
            {
                setter(b.Checked);

                //refresh the objects showing
                tbFilter_TextChanged(null, null);
            };

            toolStrip1.Items.Add(b);
        }
    }
}

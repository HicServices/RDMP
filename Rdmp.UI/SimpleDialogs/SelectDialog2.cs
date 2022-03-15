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
    public partial class SelectDialog2<T> : Form, IVirtualListDataSource where T : class
    {
        private readonly Dictionary<IMapsDirectlyToDatabaseTable, DescendancyList> _searchables;
        private readonly AttributePropertyFinder<UsefulPropertyAttribute> _usefulPropertyFinder;
        private ICoreIconProvider _coreIconProvider;
        private FavouritesProvider _favouriteProvider;

        private const int MaxMatches = 500;
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

        private bool _noSearchTerms = true;

        /// <summary>
        /// All the objects when T is not an IMapsDirectlyToDatabaseTable.
        /// </summary>
        private T[] _allObjects;
        private List<T> _objectsToDisplay = new List<T>();
        private List<IMapsDirectlyToDatabaseTable> _tempMatches;
        private List<IMapsDirectlyToDatabaseTable> _matches;
        bool stateChanged = true;

        /// <summary>
        /// The users final selection when not using mutli select mode
        /// </summary>
        public T Selected;
        public HashSet<T> MultiSelected { get; private set; }

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

                olv.RebuildColumns();
                olvSelected.DisplayIndex = 0;
            }
        }

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


        public SelectDialog2(DialogArgs args, IActivateItems activator, IEnumerable<T> toSelectFrom, bool allowDeleting, RDMPCollection focusedCollection = RDMPCollection.None)
        {
            _activator = activator;
            _allowDeleting = allowDeleting;
            _coreIconProvider = activator.CoreIconProvider;
            _favouriteProvider = activator.FavouritesProvider;

            InitializeComponent();

            if(IsDatabaseObjects())
            {
                _allObjects = toSelectFrom.ToArray();
                _searchables = _allObjects.Cast<IMapsDirectlyToDatabaseTable>().ToDictionary(k => k, activator.CoreChildProvider.GetDescendancyListIfAnyFor);
                _usefulPropertyFinder = new AttributePropertyFinder<UsefulPropertyAttribute>(_searchables.Keys);

                BuildToolStripForDatabaseObjects(focusedCollection);
            }
            else
            {
                _allObjects = toSelectFrom.ToArray();
                
                // don't bother with the tool strip because its not database objects so we can't filter by ID/type etc
                this.Controls.Remove(toolStrip1);
            }

            taskDescriptionLabel1.SetupFor(args);

            Text = args.WindowTitle;

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

            //start at cancel so if they hit the X nothing is selected
            DialogResult = DialogResult.Cancel;

            olvID.AspectGetter = (m) => (m as IMapsDirectlyToDatabaseTable)?.ID ?? null;

            // don't add the ID column if we aren't talking about database objects
            if (!IsDatabaseObjects())
            {
                olv.AllColumns.Remove(olvID);
            }

            olvName.AspectGetter = (m) => m?.ToString();

            olvName.ImageGetter = GetImage;
            olv.RowHeight = 19;

            if (!args.AllowSelectingNull)
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

            if(args.InitialObjectSelection != null)
            {
                SetInitialSelection(args.InitialObjectSelection.Cast<T>());
            }

            if(IsDatabaseObjects())
            {
                FetchMatches(args.InitialSearchText, CancellationToken.None);
            }

            olv.VirtualListDataSource = this;
        }

        private void BuildToolStripForDatabaseObjects(RDMPCollection focusedCollection)
        {
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
                b.Image = _activator.CoreIconProvider.GetImage(t);
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
        private void tbFilter_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Up || e.KeyCode == Keys.Down)
                if (!olv.Focused)
                {
                    olv.Focus();
                    SendKeys.Send(e.KeyCode == Keys.Up ? "{UP}" : "{DOWN}");
                }
        }

        private void tbFilter_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
                e.SuppressKeyPress = true;
        }

        public Bitmap GetImage(object model)
        {
            var bmp = _activator.CoreIconProvider.GetImage(model);
            return bmp == _activator.CoreIconProvider.ImageUnknown ? null : bmp;
        }

        private bool _isClosed;

        private void Selected_AspectPutter(object rowobject, object newvalue)
        {
            var b = (bool)newvalue;
            if (b)
                MultiSelected.Add((T)rowobject);
            else
                MultiSelected.Remove((T)rowobject);
            
            StateChanged();
        }

        private void StateChanged()
        {
            UpdateButtonEnabledness();
            stateChanged = true;
            olv.VirtualListDataSource = this;
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
            
            if (rowObject == null)
                return false;

            return MultiSelected.Contains((T)rowObject);
        }

        private void FetchMatches(string text, CancellationToken cancellationToken)
        {
            var scorer = new SearchablesMatchScorer();
            scorer.RespectUserSettings = UserSettings.AdvancedFindFilters;
            scorer.TypeNames = _typeNames;
            scorer.BumpMatches = _activator.HistoryProvider.History.Select(h => h.Object).ToList();

            _noSearchTerms = string.IsNullOrWhiteSpace(text) && showOnlyTypes.Count == 0;

            if (_lblId != null && int.TryParse(_lblId.Text, out int requireId))
            {
                scorer.ID = requireId;
                _noSearchTerms = false;
            }
                
            if (AlwaysFilterOn != null)
            {
                showOnlyTypes = new List<Type>(new[] { AlwaysFilterOn });
                _noSearchTerms = false;
            }
                
            var scores = scorer.ScoreMatches(_searchables, text, cancellationToken, showOnlyTypes);
            
            if (scores == null)
            {
                stateChanged = true;
                return;
            }

            lock (oMatches)
            {
                _tempMatches = scorer.ShortList(scores, MaxMatches, _activator);
            }
        }

        private void listBox1_CellClick(object sender, CellClickEventArgs e)
        {
            if (e.ClickCount >= 2)
            {
                Selected = olv.SelectedObject as T;

                if (Selected == null)
                    return;

                //double clicking on a row when several others are selected should not make it the only selected item
                if (AllowMultiSelect)
                {
                    //instead it should just add it to the multi selection
                    MultiSelected.Add(Selected);
                    UpdateButtonEnabledness();
                    return;
                }

                MultiSelected = new HashSet<T>(new[] { Selected });
                DialogResult = DialogResult.OK;
                this.Close();
            }
        }
        private void listBox1_KeyUp(object sender, KeyEventArgs e)
        {
            var deletable = olv.SelectedObject as IDeleteable;
            if (e.KeyCode == Keys.Delete && _allowDeleting && deletable != null)
            {
                if (MessageBox.Show("Confirm deleting " + deletable, "Really delete?", MessageBoxButtons.YesNoCancel) == DialogResult.Yes)
                {
                    try
                    {
                        deletable.DeleteInDatabase();
                        olv.RemoveObject(deletable);
                    }
                    catch (Exception exception)
                    {
                        ExceptionViewer.Show(exception);
                    }
                }
            }

            if (e.KeyCode == Keys.Enter && olv.SelectedObject != null)
            {
                DialogResult = DialogResult.OK;
                Selected = olv.SelectedObject is T s ? s : default(T);

                if (Selected == null)
                    return;

                MultiSelected = new HashSet<T>(new[] { Selected });
                this.Close();
            }

            //space flips the selectedness of the objects that are selected
            if (e.KeyCode == Keys.Space && AllowMultiSelect && olv.SelectedObjects != null)
            {
                foreach (T o in olv.SelectedObjects)
                {
                    if (MultiSelected.Contains(o))
                        MultiSelected.Remove(o);
                    else
                        MultiSelected.Add(o);
                }

                UpdateButtonEnabledness();

            }
        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            UpdateButtonEnabledness();
        }

        private void CollectionCheckedChanged(object sender, EventArgs e)
        {
            var button = (ToolStripButton)sender;

            var togglingType = (Type)button.Tag;

            if (button.Checked)
                showOnlyTypes.Add(togglingType);
            else
                showOnlyTypes.Remove(togglingType);

            //refresh the objects showing
            tbFilter_TextChanged(null, null);
        }

        private bool IsDatabaseObjects()
        {
            return typeof(IMapsDirectlyToDatabaseTable).IsAssignableFrom(typeof(T));
        }

        private void tbFilter_TextChanged(object sender, EventArgs e)
        {
            if (!IsDatabaseObjects())
            {
                StateChanged();
                return;
            }

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

                            if(s.IsCompleted)
                            {
                                lock (oMatches)
                                {
                                    // updates the list
                                    _matches = _tempMatches;
                                    StateChanged();
                                }
                            }

                        }
                        catch (ObjectDisposedException)
                        {

                        }
                    }, TaskScheduler.FromCurrentSynchronizationContext());
            
        }

        public void AddObjects(ICollection modelObjects)
        {

        }

        public object GetNthObject(int n)
        {
            lock (oMatches)
            {
                return n >= _objectsToDisplay.Count ? null : _objectsToDisplay[n];
            }
        }


        public int GetObjectCount()
        {
            lock (oMatches)
            {
                // regenerate the _toDisplayList because the state has changed
                if (stateChanged)
                {
                    if (IsDatabaseObjects())
                    {

                        // when returning search results always put checked items first
                        var toDisplay = new List<IMapsDirectlyToDatabaseTable>(MultiSelected.Cast<IMapsDirectlyToDatabaseTable>());

                        if (_noSearchTerms)
                        {
                            toDisplay.AddRange(_allObjects.Cast<IMapsDirectlyToDatabaseTable>().Except(toDisplay));
                            _objectsToDisplay = toDisplay.Cast<T>().ToList();

                        }
                        else
                        {
                            toDisplay.AddRange(_matches.Cast<IMapsDirectlyToDatabaseTable>().Except(toDisplay));
                            _objectsToDisplay = toDisplay.Cast<T>().ToList();

                        }
                    }
                    else
                    {
                        var searchText = tbFilter.Text;

                        _objectsToDisplay = string.IsNullOrWhiteSpace(searchText) ?
                            _allObjects.ToList() :
                            _allObjects.Where(o=>IsSimpleTextMatch(o, searchText)).ToList();
                    }

                    stateChanged = false;
                }

                return Math.Min(_objectsToDisplay.Count,MaxMatches);
            }
        }

        private bool IsSimpleTextMatch(T arg, string searchText)
        {
            var terms = searchText.Split(' ',StringSplitOptions.RemoveEmptyEntries);
            return terms.All(t=>arg.ToString().Contains(t,StringComparison.CurrentCultureIgnoreCase));
        }

        public int GetObjectIndex(object model)
        {
            return _objectsToDisplay.IndexOf((T)model);
        }

        public void InsertObjects(int index, ICollection modelObjects)
        {
            
        }

        public void PrepareCache(int first, int last)
        {
        }

        public void RemoveObjects(ICollection modelObjects)
        {
        }

        public int SearchText(string value, int first, int last, OLVColumn column)
        {
            // TODO: figure this out
            return 0;
        }

        public void SetObjects(IEnumerable collection)
        {
        }

        public void Sort(OLVColumn column, SortOrder order)
        {
        }

        public void UpdateObject(int index, object modelObject)
        {
            
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
        private void btnSelect_Click(object sender, EventArgs e)
        {
            if (!AllowMultiSelect)
                Selected = (T)olv.SelectedObject;

            DialogResult = DialogResult.OK;
            this.Close();
        }

        private void btnSelectNULL_Click(object sender, EventArgs e)
        {
            Selected = default(T);
            MultiSelected = null;
            DialogResult = DialogResult.OK;
            this.Close();
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            Selected = default(T);
            MultiSelected = null;
            DialogResult = DialogResult.Cancel;
            this.Close();
        }
        public void SetInitialSelection(IEnumerable<T> toSelect)
        {
            MultiSelected = new HashSet<T>(toSelect);
            tbFilter_TextChanged(null,null);
        }
    }
}

// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using BrightIdeasSoftware;
using Rdmp.Core;
using Rdmp.Core.CommandExecution;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Curation.Data.Cohort;
using Rdmp.Core.Curation.Data.DataLoad;
using Rdmp.Core.DataExport.Data;
using Rdmp.Core.Icons.IconOverlays;
using Rdmp.Core.Icons.IconProvision;
using Rdmp.Core.MapsDirectlyToDatabaseTable;
using Rdmp.Core.MapsDirectlyToDatabaseTable.Attributes;
using Rdmp.Core.Providers;
using Rdmp.Core.ReusableLibraryCode.Settings;
using Rdmp.UI.Collections;
using Rdmp.UI.ItemActivation;
using Rdmp.UI.Theme;

namespace Rdmp.UI.SimpleDialogs;
// IMPORTANT: To edit this in Designer rename 'SelectDialog`1.resx' to 'SelectDialog.resx'

public partial class SelectDialog<T> : Form, IVirtualListDataSource where T : class
{
    private readonly Dictionary<IMapsDirectlyToDatabaseTable, DescendancyList> _searchables;
    private readonly AttributePropertyFinder<UsefulPropertyAttribute> _usefulPropertyFinder;

    private const int MaxMatches = 500;
    private readonly object oMatches = new();

    private Task _lastFetchTask;
    private CancellationTokenSource _lastCancellationToken;
    private int _runCount;

    private Type[] _types;
    private HashSet<string> _typeNames;

    private List<Type> showOnlyTypes = new();
    private readonly Dictionary<Type, List<string>> showOnlyEnums = new();
    private Type _alwaysFilterOn;
    private ToolStripTextBox _lblId;
    private readonly DialogArgs _args;
    private readonly IActivateItems _activator;
    private readonly bool _allowDeleting;

    /// <summary>
    ///     All the objects when T is not an IMapsDirectlyToDatabaseTable.
    /// </summary>
    private readonly T[] _allObjects;

    private List<T> _objectsToDisplay = new();
    private List<IMapsDirectlyToDatabaseTable> _tempMatches;
    private List<IMapsDirectlyToDatabaseTable> _matches;
    private bool stateChanged = true;

    private bool _isClosed;
    private readonly RecentHistoryOfControls recentHistoryOfSearches;

    /// <summary>
    ///     The users final selection when not using multi select mode
    /// </summary>
    public T Selected;

    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public HashSet<T> MultiSelected { get; private set; }


    private HashSet<T> DefaultMultiSelected { get; }
    private T DefaultSelected { get; }

    /// <summary>
    ///     Hides the Type selection toggle buttons and forces results to only appear matching the given Type
    /// </summary>
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
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

    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public bool AllowMultiSelect
    {
        get => olv.MultiSelect;
        set
        {
            olv.MultiSelect = value;
            if (value)
            {
                if (!olv.AllColumns.Contains(olvSelected)) olv.AllColumns.Add(olvSelected);
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
    ///     Object types that appear in the task bar as filterable types
    /// </summary>
    private readonly Dictionary<Type, RDMPCollection> EasyFilterTypesAndAssociatedCollections = new()
    {
        { typeof(Catalogue), RDMPCollection.Catalogue },
        { typeof(CatalogueItem), RDMPCollection.Catalogue },
        { typeof(SupportingDocument), RDMPCollection.Catalogue },
        { typeof(Project), RDMPCollection.DataExport },
        { typeof(ExtractionConfiguration), RDMPCollection.DataExport },
        { typeof(ExtractableCohort), RDMPCollection.SavedCohorts },
        { typeof(CohortIdentificationConfiguration), RDMPCollection.Cohort },
        { typeof(TableInfo), RDMPCollection.Tables },
        { typeof(ColumnInfo), RDMPCollection.Tables },
        { typeof(LoadMetadata), RDMPCollection.DataLoad }
    };


    /// <summary>
    ///     Identifies which Types are checked by default when the dialog is shown when the given RDMPCollection has focus
    /// </summary>
    public Dictionary<RDMPCollection, Type[]> StartingEasyFilters
        = new()
        {
            { RDMPCollection.Catalogue, new[] { typeof(Catalogue) } },
            { RDMPCollection.Cohort, new[] { typeof(CohortIdentificationConfiguration) } },
            { RDMPCollection.DataExport, new[] { typeof(Project), typeof(ExtractionConfiguration) } },
            { RDMPCollection.DataLoad, new[] { typeof(LoadMetadata) } },
            { RDMPCollection.SavedCohorts, new[] { typeof(ExtractableCohort) } },
            { RDMPCollection.Tables, new[] { typeof(TableInfo) } },
            {
                RDMPCollection.None, new[] { typeof(SupportingDocument), typeof(CatalogueItem) }
            } //Add all other Type checkboxes here so that they are recognised as Typenames
        };


    public SelectDialog(DialogArgs args, IActivateItems activator, IEnumerable<T> toSelectFrom, bool allowDeleting,
        RDMPCollection focusedCollection = RDMPCollection.None)
    {
        _args = args;
        _activator = activator;
        _allowDeleting = allowDeleting;

        InitializeComponent();

        if (IsDatabaseObjects())
        {
            _allObjects = toSelectFrom.ToArray();
            _searchables = _allObjects.Cast<IMapsDirectlyToDatabaseTable>()
                .ToDictionary(k => k, activator.CoreChildProvider.GetDescendancyListIfAnyFor);
            _usefulPropertyFinder = new AttributePropertyFinder<UsefulPropertyAttribute>(_searchables.Keys);

            AddUsefulPropertiesIfHomogeneousTypes(_allObjects);

            BuildToolStripForDatabaseObjects(focusedCollection);
            if (focusedCollection != RDMPCollection.None)
            {
                StartingEasyFilters.TryGetValue(focusedCollection, out var focusedType);
                if (focusedType != null && focusedType.Any())
                {
                    showOnlyTypes.Add(focusedType.First());
                }
            }
        }
        else
        {
            _allObjects = toSelectFrom.ToArray();

            // don't bother with the tool strip because its not database objects so we can't filter by ID/type etc
            Controls.Remove(toolStrip1);
        }

        taskDescriptionLabel1.SetupFor(args);

        Text = args.WindowTitle;
        label1.Text = args.IsFind ? "Find:" : "Filter:";

        if (args.IsFind) pFilter.Dock = DockStyle.Top;

        tbFilter.Text = args.InitialSearchText;
        tbFilter.KeyPress += (s, e) =>
        {
            //prevents windows 'bong' noise when you hit enter
            if (e.KeyChar == (int)Keys.Enter)
                e.Handled = true;
        };

        StartPosition = FormStartPosition.CenterScreen;

        //start at cancel so if they hit the X nothing is selected
        DialogResult = DialogResult.Cancel;

        olvID.AspectGetter = m => (m as IMapsDirectlyToDatabaseTable)?.ID ?? null;

        // don't add the ID column if we aren't talking about database objects
        if (!IsDatabaseObjects()) olv.AllColumns.Remove(olvID);

        olvName.AspectGetter = m => m?.ToString();
        olvHierarchy.AspectGetter = GetHierarchy;
        olvHierarchy.IsVisible = IsDatabaseObjects();
        olvHierarchy.ImageGetter = GetHierarchyImage;
        olv.UseCellFormatEvents = true;
        olv.FormatCell += Olv_FormatCell;

        olvName.ImageGetter = GetImage;
        olv.RowHeight = 19;

        if (!args.AllowSelectingNull)
            //disable the option to select NULL
            btnSelectNULL.Visible = false;

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

        RDMPCollectionCommonFunctionality.SetupColumnTracking(olv, olvName,
            new Guid("298cda00-5ec8-423c-9230-71d78bec6bc4"));
        RDMPCollectionCommonFunctionality.SetupColumnTracking(olv, olvID,
            new Guid("bb0fe2f0-1e73-4b00-a5b7-4b6ce3510bab"));
        RDMPCollectionCommonFunctionality.SetupColumnTracking(olv, olvHierarchy,
            new Guid("9393c6f0-b2c5-4bf8-8675-3a0117a2c850"));

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

        if (args.InitialObjectSelection != null) SetInitialSelection(args.InitialObjectSelection.Cast<T>());


        if (args.InitialSearchTextGuid != null)
        {
            recentHistoryOfSearches = new RecentHistoryOfControls(tbFilter, args.InitialSearchTextGuid.Value);
            RecentHistoryOfControls.SetValueToMostRecentlySavedValue(tbFilter);
        }

        if (IsDatabaseObjects()) tbFilter_TextChanged(null, null);

        olv.VirtualListDataSource = this;

        Width = UserSettings.FindWindowWidth;
        Height = UserSettings.FindWindowHeight;

        Resize += (s, e) =>
        {
            UserSettings.FindWindowWidth = Width;
            UserSettings.FindWindowHeight = Height;
        };

        tbFilter.TextChanged += tbFilter_TextChanged;

        if (IsDatabaseObjects())
        {
            olv.CellToolTip.InitialDelay = UserSettings.TooltipAppearDelay;
            olv.CellToolTipShowing += (s, e) => RDMPCollectionCommonFunctionality.Tree_CellToolTipShowing(activator, e);
        }

        pbLoading.Visible = IsDatabaseObjects();
        DefaultMultiSelected = new HashSet<T>(MultiSelected);
        //DefaultSelected = new T(Selected);
    }

    private void AddUsefulPropertiesIfHomogeneousTypes(T[] mapsDirectlyToDatabaseTables)
    {
        // no objects
        if (mapsDirectlyToDatabaseTables.Length == 0)
            return;

        var type = mapsDirectlyToDatabaseTables.First().GetType();

        // types differ (use Any to jump out ASAP if there are a billion objects)
        if (mapsDirectlyToDatabaseTables.Any(m => m.GetType() != type))
            return;

        //all objects are the same Type

        //look for useful properties
        foreach (var propertyInfo in type.GetProperties())
        {
            var useful = _usefulPropertyFinder.GetAttribute(propertyInfo);
            if (useful == null) continue;
            //add a column
            var newCol = new OLVColumn(propertyInfo.Name, propertyInfo.Name);
            olv.AllColumns.Add(newCol);


            RDMPCollectionCommonFunctionality.SetupColumnTracking(olv, newCol, $"Useful_{propertyInfo.Name}");
        }
    }

    protected override void OnShown(EventArgs e)
    {
        base.OnShown(e);

        tbFilter.SelectAll();
        tbFilter.Focus();
    }

    private void Olv_FormatCell(object sender, FormatCellEventArgs e)
    {
        if (e.Column == olvHierarchy) e.SubItem.ForeColor = Color.Gray;
    }

    private Bitmap GetHierarchyImage(object rowObject)
    {
        if (rowObject is not IMapsDirectlyToDatabaseTable m)
            return null;

        lock (oMatches)
        {
            if (_searchables?.TryGetValue(m, out var searchable) != true) return null;

            var parent = searchable?.GetMostDescriptiveParent();
            return parent == null
                ? null
                : IconOverlayProvider.GetGreyscale(_activator.CoreIconProvider.GetImage(parent)).ImageToBitmap();
        }
    }

    private object GetHierarchy(object rowObject)
    {
        if (rowObject is not IMapsDirectlyToDatabaseTable m)
            return null;

        lock (oMatches)
        {
            if (_searchables?.TryGetValue(m, out var descendancy) == true)
                return descendancy != null
                    ? Backslashes().Replace(string.Join('\\', descendancy.GetUsefulParents()), "\\").Trim('\\')
                    : null;
        }

        return null;
    }

    private void BuildToolStripForDatabaseObjects(RDMPCollection focusedCollection)
    {
        _types = _searchables.Keys.Select(k => k.GetType()).Distinct().ToArray();
        _typeNames = new HashSet<string>(_types.Select(t => t.Name));

        foreach (var t in StartingEasyFilters.SelectMany(v => v.Value)) _typeNames.Add(t.Name);
        Type[] startingFilters = null;
        string[] startingFilterEnumValues = [];

        if (focusedCollection != RDMPCollection.None &&
            StartingEasyFilters.TryGetValue(focusedCollection, out var filter))
            startingFilters = filter;

        // if there are at least 2 Types of object let them filter
        if (_types.Length > 1)
            foreach (var t in EasyFilterTypesAndAssociatedCollections.Keys)
            {
                var shortCode = SearchablesMatchScorer.ShortCodes.Single(kvp => kvp.Value == t).Key;
                var b = new ToolStripButton
                {
                    Checked = startingFilters?.Contains(t) == true,
                    Image = _activator.CoreIconProvider.GetImage(t).ImageToBitmap(),
                    DisplayStyle = ToolStripItemDisplayStyle.Image,
                    CheckOnClick = true,
                    Tag = t,
                    Text = $"{t.Name} ({shortCode})"
                };

                b.BackgroundImage =
                    BackColorProvider.GetBackgroundImage(b.Size, EasyFilterTypesAndAssociatedCollections[t]);
                b.CheckedChanged += CollectionCheckedChanged;

                toolStrip1.Items.Add(b);
            }
        else if (_types.Length == 1)
            switch (_types.First().Name)
            {
                case "CatalogueItem":
                    foreach (var t in Enum.GetValues(typeof(ExtractionCategory)))
                    {
                        if (t.ToString() == ExtractionCategory.Any.ToString()) continue;
                        var b = new ToolStripButton
                        {
                            Checked = startingFilterEnumValues?.Contains(t) == true,
                            Image = _activator.CoreIconProvider.GetImage(t).ImageToBitmap(),
                            DisplayStyle = ToolStripItemDisplayStyle.Image,
                            CheckOnClick = true,
                            Text = $"{t}",
                            Tag = typeof(CatalogueItem)
                        };

                        b.CheckedChanged += EnumCheckedChanged;

                        toolStrip1.Items.Add(b);
                    }

                    break;
                default:
                    toolStripLabel1.Visible = false;
                    break;
            }
        else
            toolStripLabel1.Visible = false;

        toolStrip1.Items.Add(new ToolStripLabel("ID:"));
        toolStrip1.Items.Add(_lblId = new ToolStripTextBox());
        _lblId.TextChanged += tbFilter_TextChanged;

        if (UserSettings.AdvancedFindFilters)
        {
            AddUserSettingCheckbox(() => UserSettings.ShowInternalCatalogues,
                v => UserSettings.ShowInternalCatalogues = v, "I", "Include Internal");
            AddUserSettingCheckbox(() => UserSettings.ShowDeprecatedCatalogues,
                v => UserSettings.ShowDeprecatedCatalogues = v, "D", "Include Deprecated");
            AddUserSettingCheckbox(() => UserSettings.ShowProjectSpecificCatalogues,
                v => UserSettings.ShowProjectSpecificCatalogues = v, "P", "Include Project Specific");
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
        if (model is string)
            return CatalogueIcons.CatalogueFolder.ImageToBitmap();

        var bmp = _activator.CoreIconProvider.GetImage(model);
        return bmp == _activator.CoreIconProvider.ImageUnknown ? null : bmp.ImageToBitmap();
    }


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
        recentHistoryOfSearches?.AddResult(tbFilter.Text);
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

        return rowObject == null ? false : (object)MultiSelected.Contains((T)rowObject);
    }

    private void FetchMatches(string text, CancellationToken cancellationToken)
    {
        var scorer = new SearchablesMatchScorer
        {
            RespectUserSettings = UserSettings.AdvancedFindFilters,
            TypeNames = _typeNames,
            ReturnEmptyResultWhenNoSearchTerms = _args.IsFind,
            BumpMatches = _activator.HistoryProvider.History.Select(h => h.Object).ToList()
        };

        if (_lblId != null && int.TryParse(_lblId.Text, out var requireId)) scorer.ID = requireId;
        var _filtered = new Dictionary<IMapsDirectlyToDatabaseTable, DescendancyList>(_searchables);
        foreach (var key in showOnlyEnums.Keys)
            switch (key.Name)
            {
                case "CatalogueItem":
                    foreach (var k in _filtered.Keys)
                    {
                        showOnlyEnums.TryGetValue(k.GetType(), out var v1);
                        if (showOnlyEnums.TryGetValue(k.GetType(), out var v) &&
                            v.Count > 0 &&
                            !v.Contains(((CatalogueItem)k).ExtractionInformation.ExtractionCategory.ToString())
                           )
                            _filtered.Remove(k);
                    }

                    break;
            }

        if (AlwaysFilterOn != null) showOnlyTypes = new List<Type>(new[] { AlwaysFilterOn });

        var scores = scorer.ScoreMatches(_filtered, text, showOnlyTypes, cancellationToken);

        if (scores == null)
        {
            stateChanged = true;
            return;
        }

        lock (oMatches)
        {
            _tempMatches = SearchablesMatchScorer.ShortList(scores, MaxMatches, _activator);
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
                olv.RefreshObject(Selected);
                return;
            }

            MultiSelected = new HashSet<T>(new[] { Selected });
            DialogResult = DialogResult.OK;
            Close();
        }
    }

    private void listBox1_KeyUp(object sender, KeyEventArgs e)
    {
        if (e.KeyCode == Keys.Delete && _allowDeleting && olv.SelectedObject is IDeleteable deletable)
            if (MessageBox.Show($"Confirm deleting {deletable}", "Really delete?", MessageBoxButtons.YesNoCancel) ==
                DialogResult.Yes)
                try
                {
                    deletable.DeleteInDatabase();
                    olv.RemoveObject(deletable);
                }
                catch (Exception exception)
                {
                    ExceptionViewer.Show(exception);
                }

        if (e.KeyCode == Keys.Enter && olv.SelectedObject != null)
        {
            DialogResult = DialogResult.OK;
            Selected = olv.SelectedObject as T;

            // if there are some multi selected items already
            if (AllowMultiSelect && MultiSelected.Any())
            {
                // select only those
                Close();
                return;
            }

            if (Selected == null)
                return;

            MultiSelected = new HashSet<T>(new[] { Selected });
            Close();
        }

        //space flips the selectedness of the objects that are selected
        if (e.KeyCode == Keys.Space && AllowMultiSelect && olv.SelectedObjects != null)
        {
            foreach (var o in olv.SelectedObjects.Cast<T>().Where(o => !MultiSelected.Add(o)))
                MultiSelected.Remove(o);

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

    private void EnumCheckedChanged(object sender, EventArgs e)
    {
        var button = (ToolStripButton)sender;

        var togglingType = (Type)button.Tag;
        ;

        if (button.Checked)
        {
            if (showOnlyEnums.TryGetValue(togglingType, out var value))
            {
                value.Add(button.Text);
                showOnlyEnums[togglingType] = value;
            }
            else
            {
                showOnlyEnums[togglingType] = new List<string> { button.Text };
            }
        }
        else if (showOnlyEnums.TryGetValue(togglingType, out var value))
        {
            value = value.Where(v => v != button.Text).ToList();
            showOnlyEnums[togglingType] = value;
        }

        //refresh the objects showing
        tbFilter_TextChanged(null, null);
    }

    private static bool IsDatabaseObjects()
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
        if (_lastFetchTask is { IsCompleted: false })
            _lastCancellationToken.Cancel();

        _lastCancellationToken = new CancellationTokenSource();
        pbLoading.Visible = true;
        Interlocked.Increment(ref _runCount);
        var toFind = tbFilter.Text;

        _lastFetchTask = Task.Run(() => FetchMatches(toFind, _lastCancellationToken.Token))
            .ContinueWith(
                s =>
                {
                    if (Interlocked.Decrement(ref _runCount) == 0) pbLoading.Visible = false;

                    if (_isClosed)
                        return;

                    try
                    {
                        if (_isClosed)
                            return;

                        if (_isClosed)
                            return;

                        if (s.IsCompleted)
                            lock (oMatches)
                            {
                                // updates the list
                                _matches = _tempMatches;
                                StateChanged();
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
                    if (_matches == null)
                        return 0;

                    // when returning search results always put checked items first
                    var toDisplay =
                        new List<IMapsDirectlyToDatabaseTable>(MultiSelected.Cast<IMapsDirectlyToDatabaseTable>());

                    toDisplay.AddRange(_matches.Except(toDisplay));
                    _objectsToDisplay = toDisplay.Cast<T>().ToList();
                }
                else
                {
                    var searchText = tbFilter.Text;

                    _objectsToDisplay = string.IsNullOrWhiteSpace(searchText)
                        ? _allObjects.ToList()
                        : _allObjects.Where(o => IsSimpleTextMatch(o, searchText)).ToList();
                }

                stateChanged = false;
            }

            return Math.Min(_objectsToDisplay.Count, MaxMatches);
        }
    }

    private static bool IsSimpleTextMatch(T arg, string searchText)
    {
        var terms = searchText.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        return terms.All(t => arg.ToString().Contains(t, StringComparison.CurrentCultureIgnoreCase));
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
        var b = new ToolStripButton(name)
        {
            CheckOnClick = true,
            ToolTipText = toolTip,
            DisplayStyle = ToolStripItemDisplayStyle.Text,
            Checked = getter()
        };
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
        Close();
    }

    private void btnSelectNULL_Click(object sender, EventArgs e)
    {
        Selected = default;
        MultiSelected = new HashSet<T>();
        DialogResult = DialogResult.OK;
        Close();
    }

    private void btnCancel_Click(object sender, EventArgs e)
    {
        Selected = DefaultSelected;
        MultiSelected = DefaultMultiSelected;
        DialogResult = DialogResult.Cancel;
        Close();
    }

    public void SetInitialSelection(IEnumerable<T> toSelect)
    {
        MultiSelected = new HashSet<T>(toSelect);
        tbFilter_TextChanged(null, null);
    }

    [GeneratedRegex("\\\\+")]
    private static partial Regex Backslashes();
}
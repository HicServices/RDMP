using BrightIdeasSoftware;
using NPOI.SS.Formula.Functions;
using Rdmp.Core;
using Rdmp.Core.Curation.Data.Cohort;
using Rdmp.Core.Curation.Data.DataLoad;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.DataExport.Data;
using Rdmp.Core.Icons.IconOverlays;
using Rdmp.Core.Icons.IconProvision;
using Rdmp.Core.MapsDirectlyToDatabaseTable;
using Rdmp.Core.Providers;
using Rdmp.Core.ReusableLibraryCode.Settings;
using Rdmp.UI.ItemActivation;
using Rdmp.UI.Theme;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using Rdmp.Core.Sharing.Dependency.Gathering;
using Rdmp.Core.MapsDirectlyToDatabaseTable.Attributes;
using Rdmp.UI.TestsAndSetup.ServicePropogation;
using Rdmp.Core.CommandExecution.AtomicCommands;
using Rdmp.UI.Collections.Providers.Filtering;
namespace Rdmp.UI.SimpleDialogs
{
    public partial class NewfindUI : Form
    {
        private readonly IActivateItems _activator;
        private Dictionary<IMapsDirectlyToDatabaseTable, DescendancyList> _items;
        private Type[] _types;
        private List<Type> showOnlyTypes = new();
        private readonly HashSet<IMapsDirectlyToDatabaseTable> _allObjects = new();
        private readonly List<FindAndReplaceNode> _locationNodes = new();
        private readonly List<FindAndReplaceNode> _sqlNodes = new();

        private readonly bool _showReplaceOptions = false;

        private void SimulateClickForAutoFilter<T2>()
        {
            var item = newFindToolStrip.Items.Find(typeof(T2).Name, false).FirstOrDefault();
            item?.PerformClick();
        }

        private void PresetFiltersBasedOnFocusItem(RDMPUserControl focusItem)
        {
            var focusItemType = focusItem.GetType();
            var types = new List<Type>() {
                 typeof(LoadMetadata),
              typeof(ColumnInfo),
                typeof(Catalogue),
                typeof(CatalogueItem),
                typeof(SupportingDocument),
                typeof(Project),
               typeof(ExtractionConfiguration),
                typeof(ExtractableCohort),
                typeof(CohortIdentificationConfiguration),
                typeof(TableInfo)
            };
            foreach (var t in types.Select((value, i) => new { i, value }).Where(t => focusItemType.BaseType.BaseType.GenericTypeArguments.FirstOrDefault() == t.value))
            {
                switch (t.i)
                {
                    case 0:
                        SimulateClickForAutoFilter<LoadMetadata>();
                        break;
                    case 1:
                        SimulateClickForAutoFilter<ColumnInfo>();
                        break;
                    case 2:
                        SimulateClickForAutoFilter<Catalogue>();
                        break;
                    case 3:
                        SimulateClickForAutoFilter<CatalogueItem>();
                        break;
                    case 4:
                        SimulateClickForAutoFilter<SupportingDocument>();
                        break;
                    case 5:
                        SimulateClickForAutoFilter<Project>();
                        break;
                    case 6:
                        SimulateClickForAutoFilter<ExtractionConfiguration>();
                        break;
                    case 7:
                        SimulateClickForAutoFilter<ExtractableCohort>();
                        break;
                    case 8:
                        SimulateClickForAutoFilter<CohortIdentificationConfiguration>();
                        break;
                    case 9:
                        SimulateClickForAutoFilter<TableInfo>();
                        break;
                    default:
                        break;
                }
                break;
            }
        }

        public NewfindUI(IActivateItems activator, bool showReplaceOptions = true, RDMPUserControl focusItem = null)
        {

            _activator = activator;
            _items = _activator.CoreChildProvider.GetAllSearchables();
            InitializeComponent();
            _showReplaceOptions = showReplaceOptions;
            Text = _showReplaceOptions ? "Find and Replace" : "Find";
            tbReplace.Visible = false;
            label2.Visible = false;
            btnReplace.Visible = false;
            btnReplace.Enabled = false;
            olvID.AspectGetter = m => (m as IMapsDirectlyToDatabaseTable)?.ID ?? null;
            olvName.AspectGetter = m => m?.ToString();
            olvHierarchy.AspectGetter = GetHierarchy;
            olvHierarchy.ImageGetter = GetHierarchyImage;
            olvObject.ImageGetter += ImageGetter;
            olvProperty.AspectGetter += PropertyAspectGetter;
            olvValue.AspectGetter += ValueAspectGetter;
            olvName.ImageGetter = GetImage;
            folv.RowHeight = 19;
            BuildToolStripForDatabaseObjects(RDMPCollection.None);
            if (_showReplaceOptions)
            {
                rbStandard.Visible = false;
                cbSqlMode_checkChanged(null, null);
            }
            RefreshData();
            if (focusItem is not null)
                PresetFiltersBasedOnFocusItem(focusItem);
        }

        private Bitmap ImageGetter(object rowObject)
        {
            if (rowObject == null || rowObject.GetType() != typeof(FindAndReplaceNode)) return null;
            return _activator.CoreIconProvider.GetImage(((FindAndReplaceNode)rowObject).Instance).ImageToBitmap();
        }

        private object ValueAspectGetter(object rowObject)
        {
            if (rowObject == null || rowObject.GetType() != typeof(FindAndReplaceNode)) return null;
            return ((FindAndReplaceNode)rowObject).GetCurrentValue();
        }

        private object PropertyAspectGetter(object rowObject)
        {
            if (rowObject == null || rowObject.GetType() != typeof(FindAndReplaceNode)) return null;
            var node = (FindAndReplaceNode)rowObject;
            return node.PropertyName;
        }

        private void RefreshData()
        {
            GetAllObjects(_activator);
            IAttributePropertyFinder adjustableLocationPropertyFinder = new AttributePropertyFinder<AdjustableLocationAttribute>(_allObjects);
            IAttributePropertyFinder sqlPropertyFinder = new AttributePropertyFinder<SqlAttribute>(_allObjects);
            _locationNodes.Clear();
            _sqlNodes.Clear();
            foreach (var o in _allObjects.Where(adjustableLocationPropertyFinder.ObjectContainsProperty))
                foreach (var propertyInfo in adjustableLocationPropertyFinder.GetProperties(o))
                    _locationNodes.Add(new FindAndReplaceNode(o, propertyInfo));

            foreach (var o in _allObjects.Where(sqlPropertyFinder.ObjectContainsProperty))
                foreach (var propertyInfo in sqlPropertyFinder.GetProperties(o))
                    _sqlNodes.Add(new FindAndReplaceNode(o, propertyInfo));
            if (rbStandard.Checked)
            {
                _items = _activator.CoreChildProvider.GetAllSearchables();
                //filter based on showOnlyTypes
                if (showOnlyTypes.Count > 0)
                {
                    _items = _items.Where(i => showOnlyTypes.Contains(i.Key.GetType())).ToDictionary(i => i.Key, i => i.Value);
                }
                folv.BeginUpdate();

                folv.ClearObjects();
                folv.SetObjects(_items.Keys.ToArray());
                folv.RebuildColumns();
                folv.EndUpdate();
            }
            if (rbSqlMode.Checked)
            {
                folv.BeginUpdate();
                folv.ClearObjects();
                folv.AddObjects(_sqlNodes);
                folv.RebuildColumns();
                folv.EndUpdate();
            }
            if (rbLocations.Checked)
            {

                folv.BeginUpdate();
                folv.ClearObjects();
                folv.AddObjects(_locationNodes);
                folv.RebuildColumns();
                folv.EndUpdate();
            }
        }

        private void GetAllObjects(IActivateItems activator)
        {
            var g = new Gatherer(activator.RepositoryLocator);

            //We get these from the child provider because some objects (those below go off looking stuff up if you get them
            //and do not inject known good values first)
            foreach (var o in activator.CoreChildProvider.AllExtractionInformations)
                _allObjects.Add(o);

            foreach (var o in activator.CoreChildProvider.AllCatalogueItems)
                _allObjects.Add(o);

            if (activator.CoreChildProvider is DataExportChildProvider dxmChildProvider)
                foreach (var o in dxmChildProvider.GetAllExtractableColumns(
                             activator.RepositoryLocator.DataExportRepository))
                    _allObjects.Add(o);

            foreach (var o in g.GetAllObjectsInAllDatabases())
                _allObjects.Add(o);
        }


        private void tbFilter_TextChanged(object sender, EventArgs e)
        {
            folv.UseFiltering = true;
            var catalogueFilter = new CatalogueCollectionFilter(_activator.CoreChildProvider);
            var aggregateFilter = new CohortAggregateContainerFilter();


            if (cbRegex.Checked)
            {
                folv.ModelFilter = new CompositeAllFilter(new List<IModelFilter>() { TextMatchFilter.Regex(folv, new[] { tbFind.Text }), catalogueFilter, aggregateFilter });
            }
            else
            {
                var filter = new TextMatchFilter(folv, tbFind.Text, cbCaseSensitive.Checked ? StringComparison.CurrentCulture : StringComparison.CurrentCultureIgnoreCase);
                folv.ModelFilter = new CompositeAllFilter(new List<IModelFilter>() { filter, catalogueFilter, aggregateFilter });
            }
        }

        private void folv_CellClick(object sender, CellClickEventArgs e)
        {
            if (e.ClickCount >= 2)
            {
                var rowObject = e.HitTest.RowObject;
                if (rowObject is not FindAndReplaceNode node)
                {
                    _activator.Activate(rowObject);
                }
                else if (_showReplaceOptions)
                {
                    var cmd = new ExecuteCommandActivate(_activator, node.Instance);
                    if (!cmd.IsImpossible)
                        cmd.Execute();
                }
                if (!_showReplaceOptions)
                {
                    //the edit values disappear when we close the dialog, so keep them around when we're in replace mode
                    DialogResult = DialogResult.OK;
                    Close();
                }
                else
                {
                    RefreshData();
                }
            }
        }

        private static bool IsDatabaseObjects() => typeof(IMapsDirectlyToDatabaseTable).IsAssignableFrom(typeof(T));


        public Bitmap GetImage(object model)
        {
            if (model is string)
                return CatalogueIcons.CatalogueFolder.ImageToBitmap();

            var bmp = _activator.CoreIconProvider.GetImage(model);
            return bmp == _activator.CoreIconProvider.ImageUnknown ? null : bmp.ImageToBitmap();
        }
        private Bitmap GetHierarchyImage(object rowObject)
        {
            if (rowObject is not IMapsDirectlyToDatabaseTable m)
                return null;

            if (_items?.TryGetValue(m, out var searchable) != true) return null;

            var parent = searchable?.GetMostDescriptiveParent();
            return parent == null
                ? null
                : IconOverlayProvider.GetGreyscale(_activator.CoreIconProvider.GetImage(parent)).ImageToBitmap();
        }
        private object GetHierarchy(object rowObject)
        {
            if (rowObject is not IMapsDirectlyToDatabaseTable m)
                return null;

            if (_items?.TryGetValue(m, out var descendancy) == true)
                return descendancy != null
                    ? Backslashes().Replace(string.Join('\\', descendancy.GetUsefulParents()), "\\").Trim('\\')
                    : null;

            return null;
        }

        private readonly Dictionary<RDMPCollection, Type[]> StartingEasyFilters
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

        private void BuildToolStripForDatabaseObjects(RDMPCollection focusedCollection)
        {
            _types = _items.Keys.Select(k => k.GetType()).Distinct().ToArray();
            Type[] startingFilters = null;

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
                        Name = t.Name,
                        Text = $"{t.Name} ({shortCode})"
                    };

                    b.BackgroundImage =
                        BackColorProvider.GetBackgroundImage(b.Size, EasyFilterTypesAndAssociatedCollections[t]);
                    b.CheckedChanged += CollectionCheckedChanged;

                    newFindToolStrip.Items.Add(b);
                }

            if (UserSettings.AdvancedFindFilters)
            {
                // these filters do nothing currently
                AddUserSettingCheckbox(() => UserSettings.ShowInternalCatalogues,
                    v => UserSettings.ShowInternalCatalogues = v, "I", "Include Internal");
                AddUserSettingCheckbox(() => UserSettings.ShowDeprecatedCatalogues,
                    v => UserSettings.ShowDeprecatedCatalogues = v, "D", "Include Deprecated");
                AddUserSettingCheckbox(() => UserSettings.ShowProjectSpecificCatalogues,
                    v => UserSettings.ShowProjectSpecificCatalogues = v, "P", "Include Project Specific");
            }
        }

        private void btnReplace_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(tbReplace.Text)) return;
            if (_activator.YesNo(
                "Are you sure you want to do a system wide find and replace? This operation cannot be undone",
                "Are you sure"))
            {
                foreach (FindAndReplaceNode node in folv.FilteredObjects)
                    node.FindAndReplace(tbFind.Text, tbReplace.Text, !cbCaseSensitive.Checked);
                tbFind.Text = tbReplace.Text;
                tbReplace.Text = null;
                RefreshData();
            }

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

            newFindToolStrip.Items.Add(b);
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
            RefreshData();
            tbFilter_TextChanged(null, null);
        }


        [GeneratedRegex("\\\\+")]
        private static partial Regex Backslashes();

        private void cbCaseSensitive_CheckedChanged(object sender, EventArgs e)
        {
            cbCaseSensitive.Checked = !cbCaseSensitive.Checked;
            if (cbCaseSensitive.Checked)
                cbRegex.Checked = false;
            tbFilter_TextChanged(null, null);
        }

        private void cbRegex_CheckedChanged(object sender, EventArgs e)
        {
            cbRegex.Checked = !cbRegex.Checked;
            if (cbRegex.Checked)
                cbCaseSensitive.Checked = false;
            tbFilter_TextChanged(null, null);
        }

        private void cbSqlMode_checkChanged(object sender, EventArgs e)
        {
            if (!rbSqlMode.Checked)
            {
                if (_showReplaceOptions)
                {
                    tbReplace.Visible = true;
                    label2.Visible = true;
                    btnReplace.Visible = true;
                    btnReplace.Enabled = true;
                }
                newFindToolStrip.Visible = false;
                showOnlyTypes = new();
                rbSqlMode.Checked = true;
                rbLocations.Checked = false;
                rbStandard.Checked = false;
                RefreshData();

                olvObject.IsVisible = true;
                olvProperty.IsVisible = true;
                olvValue.IsVisible = true;
                olvName.IsVisible = false;
                olvID.Width = 0;
                olvHierarchy.IsVisible = false;
                olvID.IsVisible = false;
                folv.RebuildColumns();
            }
        }

        private void cbLocationMode_checkChanged(object sender, EventArgs e)
        {
            if (!rbLocations.Checked)
            {
                if (_showReplaceOptions)
                {
                    tbReplace.Visible = true;
                    label2.Visible = true;
                    btnReplace.Visible = true;
                    btnReplace.Enabled = true;
                }
                newFindToolStrip.Visible = false;
                showOnlyTypes = new();
                rbLocations.Checked = true;
                rbStandard.Checked = false;
                rbSqlMode.Checked = false;
                RefreshData();

                olvObject.IsVisible = true;
                olvProperty.IsVisible = true;
                olvValue.IsVisible = true;
                olvName.IsVisible = false;
                olvID.Width = 0;
                olvHierarchy.IsVisible = false;
                olvID.IsVisible = false;
                folv.RebuildColumns();
            }
        }

        private void cbStandard_checkChanged(object sender, EventArgs e)
        {
            if (!rbStandard.Checked)
            {
                if (_showReplaceOptions)
                {
                    tbReplace.Visible = false;
                    label2.Visible = false;
                    btnReplace.Visible = false;
                    btnReplace.Enabled = false;
                }
                newFindToolStrip.Visible = true;
                showOnlyTypes = new();
                rbStandard.Checked = true;
                rbLocations.Checked = false;
                rbSqlMode.Checked = false;
                RefreshData();

                olvObject.IsVisible = false;
                olvProperty.IsVisible = false;
                olvValue.IsVisible = false;
                olvName.IsVisible = true;
                olvID.Width = 50;
                olvHierarchy.IsVisible = true;
                olvID.IsVisible = true;
                folv.RebuildColumns();
            }
        }
    }
}

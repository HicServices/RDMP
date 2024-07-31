using BrightIdeasSoftware;
using NPOI.OpenXmlFormats.Spreadsheet;
using NPOI.SS.Formula.Functions;
using Rdmp.Core;
using Rdmp.Core.CommandExecution;
using Rdmp.Core.CommandLine.Runners;
using Rdmp.Core.Curation.Data.Cohort;
using Rdmp.Core.Curation.Data.DataLoad;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.DataExport.Data;
using Rdmp.Core.Icons.IconOverlays;
using Rdmp.Core.Icons.IconProvision;
using Rdmp.Core.MapsDirectlyToDatabaseTable;
using Rdmp.Core.Providers;
using Rdmp.Core.ReusableLibraryCode.Settings;
using Rdmp.UI.Collections;
using Rdmp.UI.ItemActivation;
using Rdmp.UI.Theme;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.ComponentModel.Design.ObjectSelectorEditor;
using Org.BouncyCastle.Asn1.BC;

namespace Rdmp.UI.SimpleDialogs
{
    public partial class NewfindUI : Form
    {
        private readonly IActivateItems _activator;
        private Dictionary<IMapsDirectlyToDatabaseTable, DescendancyList> _items;
        private Type[] _types;
        private HashSet<string> _typeNames;
        private List<Type> showOnlyTypes = new();
        private bool _showReplaceOptions = true;


        public NewfindUI(IActivateItems activator, bool showReplaceOptions = true)
        {
            _activator = activator;
            _showReplaceOptions = showReplaceOptions;
            _items = _activator.CoreChildProvider.GetAllSearchables();
            InitializeComponent();
            if (!showReplaceOptions)
            {
                tbReplace.Visible = false;
                label2.Visible = false;
                btnReplace.Visible = false;
                btnReplace.Enabled = false;
            }
            this.olvID.AspectGetter = m => (m as IMapsDirectlyToDatabaseTable)?.ID ?? null;
            olvName.AspectGetter = m => m?.ToString();
            olvHierarchy.AspectGetter = GetHierarchy;
            olvHierarchy.ImageGetter = GetHierarchyImage;
            olvName.ImageGetter = GetImage;
            folv.RowHeight = 19;
            BuildToolStripForDatabaseObjects(RDMPCollection.None);
            RefreshData();
        }


        private void RefreshData()
        {
            _items = _activator.CoreChildProvider.GetAllSearchables();
            //filter based on showOnlyTypes
            if (showOnlyTypes.Count > 0)
            {
                _items = _items.Where(i => showOnlyTypes.Contains(i.Key.GetType())).ToDictionary(i => i.Key, i => i.Value);
            }
            this.folv.SetObjects(_items.Keys.ToArray());
            this.folv.RebuildColumns();
        }


        private void tbFilter_TextChanged(object sender, EventArgs e)
        {
            folv.UseFiltering = true;

            if (cbRegex.Checked)
            {
                folv.ModelFilter = TextMatchFilter.Regex(folv, new[] { tbFind.Text });
            }
            else
            {
                var x = new TextMatchFilter(folv, tbFind.Text, cbCaseSensitive.Checked ? StringComparison.CurrentCulture : StringComparison.CurrentCultureIgnoreCase);
                folv.ModelFilter = x;//TextMatchFilter.Contains(folv, new[] { tbFind.Text },cbCaseSensitive.Checked? StringComparison.CurrentCulture: StringComparison.CurrentCultureIgnoreCase);
            }
        }

        private void folv_CellClick(object sender, CellClickEventArgs e)
        {
            if (e.ClickCount >= 2)
            {
                var rowObject = e.HitTest.RowObject;
                _activator.Activate(rowObject);
                DialogResult = DialogResult.OK;
                Close();
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

            //lock (oMatches)
            //{
            if (_items?.TryGetValue(m, out var searchable) != true) return null;

            var parent = searchable?.GetMostDescriptiveParent();
            return parent == null
                ? null
                : IconOverlayProvider.GetGreyscale(_activator.CoreIconProvider.GetImage(parent)).ImageToBitmap();
            //}
        }
        private object GetHierarchy(object rowObject)
        {
            if (rowObject is not IMapsDirectlyToDatabaseTable m)
                return null;

            //lock (oMatches)
            //{
            if (_items?.TryGetValue(m, out var descendancy) == true)
                return descendancy != null
                    ? Backslashes().Replace(string.Join('\\', descendancy.GetUsefulParents()), "\\").Trim('\\')
                    : null;
            //}

            return null;
        }

        private Dictionary<RDMPCollection, Type[]> StartingEasyFilters
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

        private Dictionary<Type, RDMPCollection> EasyFilterTypesAndAssociatedCollections = new()
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
            _typeNames = new HashSet<string>(_types.Select(t => t.Name));

            foreach (var t in StartingEasyFilters.SelectMany(v => v.Value)) _typeNames.Add(t.Name);
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
                        Text = $"{t.Name} ({shortCode})"
                    };

                    b.BackgroundImage =
                        BackColorProvider.GetBackgroundImage(b.Size, EasyFilterTypesAndAssociatedCollections[t]);
                    b.CheckedChanged += CollectionCheckedChanged;

                    newFindToolStrip.Items.Add(b);
                }
            //else
            //    toolStripLabel1.Visible = false;

            if (UserSettings.AdvancedFindFilters)
            {
                AddUserSettingCheckbox(() => UserSettings.ShowInternalCatalogues,
                    v => UserSettings.ShowInternalCatalogues = v, "I", "Include Internal");
                AddUserSettingCheckbox(() => UserSettings.ShowDeprecatedCatalogues,
                    v => UserSettings.ShowDeprecatedCatalogues = v, "D", "Include Deprecated");
                AddUserSettingCheckbox(() => UserSettings.ShowColdStorageCatalogues,
                    v => UserSettings.ShowColdStorageCatalogues = v, "C", "Include Cold Storage");
                AddUserSettingCheckbox(() => UserSettings.ShowProjectSpecificCatalogues,
                    v => UserSettings.ShowProjectSpecificCatalogues = v, "P", "Include Project Specific");
                AddUserSettingCheckbox(() => UserSettings.ShowNonExtractableCatalogues,
                    v => UserSettings.ShowNonExtractableCatalogues = v, "E", "Include Extractable");
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
            if(cbRegex.Checked)
                cbCaseSensitive.Checked = false;
            tbFilter_TextChanged(null, null);
        }
    }
}

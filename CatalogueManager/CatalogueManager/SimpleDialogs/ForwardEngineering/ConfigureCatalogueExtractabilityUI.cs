using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using BrightIdeasSoftware;
using CatalogueLibrary.Data;
using CatalogueLibrary.Repositories;
using CatalogueManager.Collections;
using CatalogueManager.Icons.IconProvision;
using CatalogueManager.ItemActivation;
using CatalogueManager.Refreshing;
using DataExportLibrary.Data.DataTables;
using ReusableLibraryCode;

namespace CatalogueManager.SimpleDialogs.ForwardEngineering
{
    /// <summary>
    /// Allows you to choose whether to mark all columns in a newly created Catalogue as Extractable.  Also lets you specify which Column contains the patient identifier (used to link
    /// the records with those in the other tables).
    /// </summary>
    public partial class ConfigureCatalogueExtractabilityUI : UserControl
    {
        public bool MakeAllColumnsExtractable { get { return cbMakeAllColumnsExtractable.Checked; } }
        public ColumnInfo ExtractionIdentifier { get { return ddExtractionIdentifier.SelectedItem as ColumnInfo; } }
        public Dictionary<ColumnInfo, ExtractionCategory?> ColumnExtractionCategories { get; private set; } 
        private readonly object[] _extractionCategories;

        public ConfigureCatalogueExtractabilityUI()
        {
            ColumnExtractionCategories = new Dictionary<ColumnInfo, ExtractionCategory?>();
            InitializeComponent();

            _extractionCategories = new object[]
            {
                ExtractionCategory.Core, ExtractionCategory.Supplemental,
                ExtractionCategory.SpecialApprovalRequired,
                ExtractionCategory.Internal, ExtractionCategory.Deprecated
            };
            ddCategoriseMany.Items.AddRange(_extractionCategories);

            olvExtractionCategory.AspectGetter += ExtractionCategoryAspectGetter;
            olvExtractable.AspectGetter += ExtractableAspectGetter;
            olvColumnExtractability.AlwaysGroupByColumn = olvExtractable;

            olvColumnExtractability.CellEditStarting += TlvColumnExtractabilityOnCellEditStarting;
            olvColumnExtractability.CellEditFinishing += TlvColumnExtractabilityOnCellEditFinishing;
            olvColumnExtractability.CellEditActivation = ObjectListView.CellEditActivateMode.SingleClick;
            olvColumnExtractability.ItemChecked += OlvColumnExtractabilityOnItemChecked;
        }

        private void OlvColumnExtractabilityOnItemChecked(object sender, ItemCheckedEventArgs itemCheckedEventArgs)
        {
            var columnInfo = (ColumnInfo) olvColumnExtractability.GetItem(itemCheckedEventArgs.Item.Index).RowObject;
            if (itemCheckedEventArgs.Item.Checked)
                ColumnExtractionCategories.Add(columnInfo, null);
            else
                ColumnExtractionCategories.Remove(columnInfo);
        }

        private object ExtractableAspectGetter(object rowobject)
        {
            var ci = rowobject as ColumnInfo;
            try
            {
                if (ci != null)
                    return ColumnExtractionCategories.ContainsKey(ci) ? "Extractable" : "Not Extractable";
            }
            catch (Exception)
            {
                return "Error";
            }

            return null;
        }

        private object ExtractionCategoryAspectGetter(object rowobject)
        {
            var ci = rowobject as ColumnInfo;
            try
            {
                if (ci != null)
                {
                    if (ColumnExtractionCategories.ContainsKey(ci))
                        return ColumnExtractionCategories[ci];
                    
                    return null;
                }
            }
            catch (Exception)
            {
                return "Error";
            }

            return null;
        }

        private void TlvColumnExtractabilityOnCellEditFinishing(object sender, CellEditEventArgs cellEditEventArgs)
        {
            if (cellEditEventArgs.Column == olvExtractionCategory)
            {
                var cbx = (ComboBox) cellEditEventArgs.Control;
                var selectedColumnInfo = (ColumnInfo)olvColumnExtractability.GetItem(olvColumnExtractability.SelectedIndex).RowObject;

                var category = (ExtractionCategory) cbx.SelectedItem;
                ColumnExtractionCategories[selectedColumnInfo] = category;
            }
        }

        private void TlvColumnExtractabilityOnCellEditStarting(object sender, CellEditEventArgs cellEditEventArgs)
        {
            if (cellEditEventArgs.Column == olvColumnInfoName)
                cellEditEventArgs.Cancel = true;

            if (cellEditEventArgs.Column == olvExtractionCategory)
            {
                var cbx = new ComboBox
                {
                    DropDownStyle = ComboBoxStyle.DropDownList,
                    Bounds = cellEditEventArgs.CellBounds
                };
                cbx.Items.AddRange(_extractionCategories);

                var selectedItem = olvColumnExtractability.GetItem(olvColumnExtractability.SelectedIndex);
                if (selectedItem.Checked)
                    cellEditEventArgs.Control = cbx;
                else
                    cellEditEventArgs.Cancel = true;
                
                var selectedColumnInfo = (ColumnInfo)selectedItem.RowObject;
                if (ColumnExtractionCategories.ContainsKey(selectedColumnInfo))
                    cbx.SelectedItem = ColumnExtractionCategories[selectedColumnInfo];
            }
        }
        
        public void SetUp(ColumnInfo[] columnInfos, IActivateItems activator)
        {
            ddExtractionIdentifier.Items.AddRange(columnInfos);
            gbMarkAllExtractable.Enabled = true;

            olvColumnInfoName.ImageGetter = delegate
            {
                return activator.CoreIconProvider.GetImage(RDMPConcept.ColumnInfo);
            };

            ColumnExtractionCategories.Clear();
            olvColumnExtractability.ClearObjects();
            olvColumnExtractability.AddObjects(columnInfos);
            
            olvColumnExtractability.MultiSelect = true;
            olvColumnExtractability.CheckBoxes = true;
        }

        private void cbMakeAllColumnsExtractable_CheckedChanged(object sender, System.EventArgs e)
        {
            ddExtractionIdentifier.Enabled = cbMakeAllColumnsExtractable.Checked;
        }

        public void MarkExtractionIdentifier(IActivateItems activator ,ExtractionInformation[] eis)
        {
            if (ExtractionIdentifier != null)
            {
                //make the ExtractionInformation associated with the ColumnInfo an IsExtractionIdentifier
                var identifier = eis.Single(e => e.ColumnInfo.Equals(ExtractionIdentifier));
                identifier.IsExtractionIdentifier = true;
                identifier.SaveToDatabase();

                //and make the Catalogue an ExtractableDataSet
                var cata = identifier.CatalogueItem.Catalogue;

                if (activator.RepositoryLocator.DataExportRepository != null)
                {
                    //make sure catalogue is not already extractable
                    if (!activator.RepositoryLocator.DataExportRepository.GetAllObjectsWithParent<ExtractableDataSet>(cata).Any())
                    {
                        var ds = new ExtractableDataSet(activator.RepositoryLocator.DataExportRepository, cata);
                        activator.RefreshBus.Publish(this,new RefreshObjectEventArgs(ds));
                    }
                }
            }
        }

        private void btnClear_Click(object sender, System.EventArgs e)
        {
            ddExtractionIdentifier.SelectedItem = null;
        }

        private void tbFilter_TextChanged(object sender, EventArgs e)
        {
            olvColumnExtractability.UseFiltering = true;

            var textFilter = new TextMatchFilter(olvColumnExtractability, tbFilter.Text);
            textFilter.Columns = new[] {olvColumnInfoName};
            olvColumnExtractability.ModelFilter = textFilter;
        }

        private void lbPastedColumns_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.V && e.Control)
            {
                lbPastedColumns.Items.AddRange(
                        UsefulStuff.GetInstance().GetArrayOfColumnNamesFromStringPastedInByUser(Clipboard.GetText()).ToArray());

                btnClearFilterMany.Enabled = true;

                FilterMany();
            }

            if (e.KeyCode == Keys.Delete & lbPastedColumns.SelectedItem != null)
            {
                lbPastedColumns.Items.Remove(lbPastedColumns.SelectedItem);

                if (lbPastedColumns.Items.Count == 0)
                    btnClearFilterMany.Enabled = false;

                FilterMany();
            }
        }

        private void FilterMany()
        {
            olvColumnExtractability.UseFiltering = true;

            var textFilter = TextMatchFilter.Contains(olvColumnExtractability, lbPastedColumns.Items.OfType<string>().ToArray());
            textFilter.Columns = new[] {olvColumnInfoName};
            olvColumnExtractability.ModelFilter = textFilter;
        }

        private void btnClearFilterMany_Click(object sender, EventArgs e)
        {
            lbPastedColumns.Items.Clear();
            btnClearFilterMany.Enabled = false;
            FilterMany();
        }

        private void ddCategoriseMany_SelectedIndexChanged(object sender, EventArgs e)
        {
            var filteredColumnInfos = olvColumnExtractability.FilteredObjects.OfType<ColumnInfo>();
            foreach (var filteredColumnInfo in filteredColumnInfos)
            {
                if (ColumnExtractionCategories.ContainsKey(filteredColumnInfo))
                    ColumnExtractionCategories[filteredColumnInfo] = (ExtractionCategory) ddCategoriseMany.SelectedItem;
                //todo else make it obvious Category can't be set without being checked as extractable
            }
        }
    }
}

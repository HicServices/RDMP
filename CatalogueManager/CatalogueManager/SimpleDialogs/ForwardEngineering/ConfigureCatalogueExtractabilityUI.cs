using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using BrightIdeasSoftware;
using CatalogueLibrary;
using CatalogueLibrary.Data;
using CatalogueLibrary.DataHelper;
using CatalogueLibrary.Repositories;
using CatalogueManager.Collections;
using CatalogueManager.CommandExecution.AtomicCommands;
using CatalogueManager.Icons.IconProvision;
using CatalogueManager.ItemActivation;
using CatalogueManager.Refreshing;
using DataExportLibrary.Data.DataTables;
using MapsDirectlyToDatabaseTable;
using MapsDirectlyToDatabaseTableUI;
using ReusableLibraryCode;
using ReusableUIComponents;

namespace CatalogueManager.SimpleDialogs.ForwardEngineering
{
    /// <summary>
    /// Allows you to choose whether to mark all columns in a newly created Catalogue as Extractable.  Also lets you specify which Column contains the patient identifier (used to link
    /// the records with those in the other tables).
    /// </summary>
    public partial class ConfigureCatalogueExtractabilityUI : Form
    {
        private readonly object[] _extractionCategories;

        private readonly Dictionary<CatalogueItem, object> _extractabilityDictionary = new Dictionary<CatalogueItem, object>();
        private IActivateItems _activator;

        private string NotExtractable = "Not Extractable";
        private Catalogue _catalogue;
        private TableInfo _tableInfo;
        private bool _choicesFinalised;
        public Catalogue CatalogueCreatedIfAny { get { return _catalogue; }}
        public TableInfo TableInfoCreated{get { return _tableInfo; }}


        public ConfigureCatalogueExtractabilityUI(IActivateItems activator, ITableInfoImporter importer)
        {
            InitializeComponent();

            _activator = activator;
                    ColumnInfo[] cols;
                    importer.DoImport(out _tableInfo, out cols);

            var forwardEngineer = new ForwardEngineerCatalogue(_tableInfo, cols, false);
            CatalogueItem[] cis;
            ExtractionInformation[] eis;
            forwardEngineer.ExecuteForwardEngineering(out _catalogue,out cis,out eis);
            
            //Every CatalogueItem is either mapped to a ColumnInfo (not extractable) or a ExtractionInformation (extractable).  To start out with they are not extractable
            foreach (CatalogueItem ci in cis)
                _extractabilityDictionary.Add(ci, cols.Single(col => ci.ColumnInfo_ID == col.ID));

            olvColumnExtractability.ClearObjects();
            olvColumnExtractability.AddObjects(cols);

            _extractionCategories = new object[]
            {
                NotExtractable,
                ExtractionCategory.Core,
                ExtractionCategory.Supplemental,
                ExtractionCategory.SpecialApprovalRequired,
                ExtractionCategory.Internal,
                ExtractionCategory.Deprecated
            };
            ddCategoriseMany.Items.AddRange(_extractionCategories);

            olvExtractionCategory.AspectGetter += ExtractionCategoryAspectGetter;
            olvExtractable.AspectGetter += ExtractableAspectGetter;
            olvColumnExtractability.AlwaysGroupByColumn = olvExtractable;

            olvColumnExtractability.CellEditStarting += TlvColumnExtractabilityOnCellEditStarting;
            olvColumnExtractability.CellEditFinishing += TlvColumnExtractabilityOnCellEditFinishing;
            olvColumnExtractability.CellEditActivation = ObjectListView.CellEditActivateMode.SingleClick;
            olvColumnExtractability.ItemChecked += OlvColumnExtractabilityOnItemChecked;

            olvIsExtractionIdentifier.AspectGetter += IsExtractionIdentifier_AspectGetter;

            olvColumnInfoName.ImageGetter = (o) => activator.CoreIconProvider.GetImage(o);

            olvColumnExtractability.MultiSelect = true;
            olvColumnExtractability.CheckBoxes = true;

            olvColumnExtractability.RebuildColumns();
            
        }

        private object IsExtractionIdentifier_AspectGetter(object rowObject)
        {
            var ei = rowObject as ExtractionInformation;

            if (ei == null)
                return false;

            return ei.IsExtractionIdentifier;
        }

        private void OlvColumnExtractabilityOnItemChecked(object sender, ItemCheckedEventArgs itemCheckedEventArgs)
        {
            MakeExtractable(olvColumnExtractability.GetItem(itemCheckedEventArgs.Item.Index).RowObject, itemCheckedEventArgs.Item.Checked);
        }

        private void MakeExtractable(object o, bool shouldBeExtractable, ExtractionCategory? category = null)
        {
            var ei = o as ExtractionInformation;
            

            if(ei != null)
            {
                if(shouldBeExtractable)
                {
                    //if they want to change the extraction category
                    if(category.HasValue && ei.ExtractionCategory != category.Value)
                    {
                        ei.ExtractionCategory = category.Value;
                        ei.SaveToDatabase();
                        olvColumnExtractability.RefreshObject(ei);
                    }
                    return;
                }
                else
                {
                    //find underlying column info
                    var columnInfo = ei.ColumnInfo;
                    var catalogueItem = ei.CatalogueItem;
                    
                    olvColumnExtractability.RemoveObject(ei);
                    ei.DeleteInDatabase();

                    _extractabilityDictionary[catalogueItem] = columnInfo;
                    
                    olvColumnExtractability.AddObject(columnInfo);
                    olvColumnExtractability.UncheckObject(columnInfo);
                }
            }

            //if the model object is a column info
            var colinfo = o as ColumnInfo;
            if(colinfo != null)
                if(!shouldBeExtractable) //it's already not extractable job done
                    return;
                else
                {
                    var catalogueItem = _extractabilityDictionary.Keys.Single(ci => ci.ColumnInfo_ID == colinfo.ID);

                    //make it extractable
                    var newExtractionInformation = new ExtractionInformation((ICatalogueRepository) colinfo.Repository, catalogueItem, colinfo,colinfo.Name);

                    if (category.HasValue)
                    {
                        newExtractionInformation.ExtractionCategory = category.Value;
                        newExtractionInformation.SaveToDatabase();
                    }

                    _extractabilityDictionary[catalogueItem] = newExtractionInformation;

                    olvColumnExtractability.RemoveObject(colinfo);
                    olvColumnExtractability.AddObject(newExtractionInformation);
                    olvColumnExtractability.CheckObject(newExtractionInformation);
                }
        }

        private object ExtractableAspectGetter(object rowobject)
        {
            return !(rowobject is ColumnInfo);
        }

        private object ExtractionCategoryAspectGetter(object rowobject)
        {
            var ei = rowobject as ExtractionInformation;

            if (ei == null)
                return null;

            return ei.ExtractionCategory;
        }

        private void TlvColumnExtractabilityOnCellEditFinishing(object sender, CellEditEventArgs cellEditEventArgs)
        {
            if (cellEditEventArgs.Column == olvExtractionCategory)
            {
                var cbx = (ComboBox) cellEditEventArgs.Control;
                var ei = cellEditEventArgs.RowObject as ExtractionInformation;
                if(ei == null)
                    return;

                ei.ExtractionCategory = (ExtractionCategory) cbx.SelectedItem;
                ei.SaveToDatabase();
            }
        }

        private void TlvColumnExtractabilityOnCellEditStarting(object sender, CellEditEventArgs cellEditEventArgs)
        {
            if (cellEditEventArgs.Column == olvColumnInfoName)
                cellEditEventArgs.Cancel = true;

            if (cellEditEventArgs.Column == olvExtractionCategory)
            {
                if (cellEditEventArgs.RowObject is ColumnInfo)
                {
                    cellEditEventArgs.Cancel = true;
                    return;
                }
                
                var cbx = new ComboBox
                {
                    DropDownStyle = ComboBoxStyle.DropDownList,
                    Bounds = cellEditEventArgs.CellBounds
                };
                cbx.Items.AddRange(_extractionCategories);

                var ei = (ExtractionInformation) cellEditEventArgs.RowObject;
                cbx.SelectedItem = ei.ExtractionCategory;
                cellEditEventArgs.Control = cbx;
            }
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
            var filteredObjects = olvColumnExtractability.FilteredObjects.Cast<object>().ToArray();
            object toChangeTo = ddCategoriseMany.SelectedItem;
            
            if (MessageBox.Show("Set " + filteredObjects.Length + " to '" + toChangeTo + "'?",
                    "Confirm Overwrite?", MessageBoxButtons.OKCancel) == DialogResult.OK)
            {

                foreach (object o in filteredObjects)
                {
                    if (toChangeTo.Equals(NotExtractable))
                        MakeExtractable(o, false);
                    else
                        MakeExtractable(o, true, (ExtractionCategory) toChangeTo);
                }
            }

        }

        private void FinaliseExtractability()
        {
            _choicesFinalised = true;
            var ei = _extractabilityDictionary.Values.OfType<ExtractionInformation>().FirstOrDefault(c=>c.IsExtractionIdentifier);

            if (ei != null)
            {
                var cmd = new ExecuteCommandChangeExtractability(_activator, _catalogue, true);
                if(!cmd.IsImpossible)
                    cmd.Execute();
            }
        }

        private void btnAddToExisting_Click(object sender, EventArgs e)
        {
            var eis = _extractabilityDictionary.Values.OfType<ExtractionInformation>().ToArray();

            if (!eis.Any())
            {
                MessageBox.Show("You must set at least one column to extractable before you can add them to another Catalogue");
                return;
            }
            

            var dialog = new SelectIMapsDirectlyToDatabaseTableDialog(_activator.CoreChildProvider.AllCatalogues, false, false);
                if (dialog.ShowDialog() == DialogResult.OK)

                    if (MessageBox.Show("This will add " + eis.Length + " new columns to " + dialog.Selected + ". Are you sure this is what you want?","Add to existing", MessageBoxButtons.YesNo) == DialogResult.Yes)
                        AddToExistingCatalogue((Catalogue) dialog.Selected,eis);
        }

        private void AddToExistingCatalogue(Catalogue addToInstead, ExtractionInformation[] eis)
        {
            //move all the CatalogueItems to the other Catalogue instead
            foreach (ExtractionInformation ei in eis)
            {
                var ci = ei.CatalogueItem;
                ci.Catalogue_ID = addToInstead.ID;
                ci.SaveToDatabase();
            }
            
            _choicesFinalised = true;
            _catalogue.DeleteInDatabase();
            _catalogue = null;
            Close();
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void btnOk_Click(object sender, EventArgs e)
        {
            if (!_extractabilityDictionary.Values.OfType<ExtractionInformation>().Any())
            {
                MessageBox.Show("You have not marked any columns as extractable, try selecting an ExtractionCategory for your columns");
                return;
            }
            
            FinaliseExtractability();
            DialogResult = DialogResult.OK;
            Close();
        }

        private void ConfigureCatalogueExtractabilityUI_FormClosing(object sender, FormClosingEventArgs e)
        {
            if(!_choicesFinalised)
            {
                if (MessageBox.Show("Your data table will still exist but no Catalogue will be created",
                    "Confirm", MessageBoxButtons.OKCancel) == DialogResult.OK)
                {
                    DialogResult = DialogResult.Cancel;
                    _catalogue.DeleteInDatabase();
                    _catalogue = null;
                }
                else
                    e.Cancel = true;
            }
            else
            {
                if(CatalogueCreatedIfAny != null)
                    _activator.RefreshBus.Publish(this, new RefreshObjectEventArgs(CatalogueCreatedIfAny));
            }
        }
    }
}

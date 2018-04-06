using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using BrightIdeasSoftware;
using CatalogueLibrary.Data;
using CatalogueLibrary.Repositories;
using CatalogueManager.Collections;
using CatalogueManager.CommandExecution.AtomicCommands;
using CatalogueManager.Icons.IconProvision;
using CatalogueManager.ItemActivation;
using CatalogueManager.Refreshing;
using DataExportLibrary.Data.DataTables;
using ReusableLibraryCode;
using ReusableUIComponents;

namespace CatalogueManager.SimpleDialogs.ForwardEngineering
{
    /// <summary>
    /// Allows you to choose whether to mark all columns in a newly created Catalogue as Extractable.  Also lets you specify which Column contains the patient identifier (used to link
    /// the records with those in the other tables).
    /// </summary>
    public partial class ConfigureCatalogueExtractabilityUI : UserControl
    {
        private readonly object[] _extractionCategories;

        private readonly Dictionary<CatalogueItem, object> _extractabilityDictionary = new Dictionary<CatalogueItem, object>();
        private IActivateItems _activator;

        private string NotExtractable = "Not Extractable";

        public ConfigureCatalogueExtractabilityUI()
        {
            InitializeComponent();

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
        
        public void SetUp(CatalogueItem[] catalogueItems, ColumnInfo[] columnInfos, IActivateItems activator)
        {
            _activator = activator;
            //Every CatalogueItem is either mapped to a ColumnInfo (not extractable) or a ExtractionInformation (extractable).  To start out with they are not extractable
            foreach (CatalogueItem ci in catalogueItems)
                _extractabilityDictionary.Add(ci, columnInfos.Single(col => ci.ColumnInfo_ID == col.ID));

            gbMarkAllExtractable.Enabled = true;

            olvColumnInfoName.ImageGetter = (o)=> activator.CoreIconProvider.GetImage(o);

            olvColumnExtractability.ClearObjects();
            olvColumnExtractability.AddObjects(columnInfos);
            
            olvColumnExtractability.MultiSelect = true;
            olvColumnExtractability.CheckBoxes = true;
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

        public void FinaliseExtractability()
        {
            var ei = _extractabilityDictionary.Values.OfType<ExtractionInformation>().FirstOrDefault();

            if (ei != null)
            {
                var cmd = new ExecuteCommandChangeExtractability(_activator, ei.CatalogueItem.Catalogue, true);
                cmd.Execute();
            }
        }
    }
}

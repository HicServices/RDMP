// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Common;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using CatalogueLibrary.Data;
using CatalogueLibrary.Repositories;
using CatalogueManager.ItemActivation;
using CatalogueManager.TestsAndSetup.ServicePropogation;
using ReusableLibraryCode;
using ReusableUIComponents;

using ReusableUIComponents.ScintillaHelper;
using ScintillaNET;

namespace CatalogueManager.SimpleDialogs
{
    /// <summary>
    /// It is highly likely that you will have columns in different datasets which are conceptually the same (e.g. patient identifier).  Maintaining a central description of this concept is
    /// important, it is no use having 10 slightly different descriptions of 'PatientCareNumber' for example.
    /// 
    /// <para>This dialog appears any time you save a description of a column/transform (CatalogueItem) and there is another column in any of your other datasets which has the same name.  It shows
    /// you the other columns that share the same name and lets you view their descriptions and the differences between their descriptions and your new description.  To view the changes
    /// select one of the properties you changed on the right listbox (e.g. Description) and then scroll through the objects on the left to view the differences in descriptions.</para>
    /// 
    /// <para>Next you must decide whether your new description applies to all the other objects too or whether the software made a mistake and actually you want to maintain the unique descriptions
    /// (for example it is likely if you have a column EventDate it might have different descriptions in each dataset).</para>
    /// 
    /// <para>Select either:
    /// Cancel - Nothing will be saved and your column description change will be lost
    /// No (Save only this one) - Only the original column description you were modifying will be saved
    /// Yes (Copy over changes) - The original column and ALL OTHER TICKED columns will all be set to have the same description (that you originally saved).</para>
    /// </summary>
    public partial class PropagateSaveChangesToCatalogueItemToSimilarNamedCatalogueItemsUI : RDMPForm
    {
        private readonly CatalogueItem _catalogueItemBeingSaved;
        protected List<PropertyInfo>ChangedProperties;
        private ScintillaNET.Scintilla previewOldValue;
        private ScintillaNET.Scintilla previewNewValue;

        Dictionary<string,CatalogueItem> FriendlyNamedListOfCatalogueItems = new Dictionary<string, CatalogueItem>();
        Dictionary<string, PropertyInfo> FriendlyNamedListOfPropertiesChanged = new Dictionary<string, PropertyInfo>();

        public PropagateSaveChangesToCatalogueItemToSimilarNamedCatalogueItemsUI(IActivateItems activator, CatalogueItem catalogueItemBeingSaved, out bool shouldDialogBeDisplayed): base(activator)
        {
            _catalogueItemBeingSaved = catalogueItemBeingSaved;
            InitializeComponent();

            if (VisualStudioDesignMode || catalogueItemBeingSaved == null)
            {
                shouldDialogBeDisplayed = false;
                return;
            }

            ChangedProperties = DetermineChangedProperties(catalogueItemBeingSaved);

            CatalogueItem[] OtherCatalogueItemsThatShareName = GetAllCatalogueItemsSharingNameWith(catalogueItemBeingSaved);

            //if Name changed then they probably dont want to also rename all associated CatalogueItems
            shouldDialogBeDisplayed = !ChangedProperties.Any(prop => prop.Name.Equals("Name"));

            if (OtherCatalogueItemsThatShareName.Length == 0)
                shouldDialogBeDisplayed = false;

            if (!ChangedProperties.Any())
                shouldDialogBeDisplayed = false;

            if(!shouldDialogBeDisplayed)
                return;

            setupCheckListBoxes(OtherCatalogueItemsThatShareName, ChangedProperties);

            #region Query Editor setup

            previewOldValue = new ScintillaTextEditorFactory().Create();
            previewOldValue.ReadOnly = true;

            previewNewValue = new ScintillaTextEditorFactory().Create();
            previewNewValue.ReadOnly = true;

            splitContainer2.Panel1.Controls.Add(previewOldValue);
            splitContainer2.Panel2.Controls.Add(previewNewValue);
    

            #endregion
        }

        private void setupCheckListBoxes(CatalogueItem[] otherCatalogueItemsThatShareName, List<PropertyInfo> changedProperties)
        {
            //work out a suitable name to display on the UI for the catalogue items and changed properties ( all will have the same base name so lets specify it with the Catalogue too)
            foreach (CatalogueItem catalogueItem in otherCatalogueItemsThatShareName)
            {
                Catalogue c = catalogueItem.Catalogue;
                string nameToDisplay = c.Name + "." + catalogueItem.Name + " (ID=" + catalogueItem.ID+")";

                FriendlyNamedListOfCatalogueItems.Add(nameToDisplay, catalogueItem);
            }

            clbCatalogues.Items.AddRange(FriendlyNamedListOfCatalogueItems.Keys.ToArray());

            foreach (PropertyInfo changedProperty in changedProperties)
            {
                string nameToDisplay = changedProperty.Name;
                FriendlyNamedListOfPropertiesChanged.Add(nameToDisplay,changedProperty);
            }

            clbChangedProperties.Items.AddRange(FriendlyNamedListOfPropertiesChanged.Keys.ToArray());
        }


        public static List<PropertyInfo> DetermineChangedProperties(CatalogueItem newVersionInMemory)
        {
            return newVersionInMemory.HasLocalChanges().Differences.Select(d => d.Property).ToList();
        }

        private CatalogueItem[] GetAllCatalogueItemsSharingNameWith(CatalogueItem catalogueItemBeingSaved)
        {
            return Activator.CoreChildProvider.AllCatalogueItems
                .Where(ci=>
                        ci.Name.Equals(catalogueItemBeingSaved.Name,StringComparison.CurrentCultureIgnoreCase) 
                        && ci.ID != catalogueItemBeingSaved.ID)
                        .ToArray();
        }

        private void clbCatalogues_SelectedIndexChanged(object sender, EventArgs e)
        {
            displayPreview();
        }

        private void clbChangedProperties_SelectedIndexChanged(object sender, EventArgs e)
        {
           displayPreview();

        }

        public void displayPreview()
        {
            
            string piAsString = clbChangedProperties.SelectedItem as string;
            string ciAsString = clbCatalogues.SelectedItem as string;

            PropertyInfo pi = null;
            CatalogueItem ci = null;

            if (!string.IsNullOrWhiteSpace(piAsString))
                pi = FriendlyNamedListOfPropertiesChanged[piAsString];

            if (!string.IsNullOrWhiteSpace(ciAsString))
                ci = FriendlyNamedListOfCatalogueItems[ciAsString];

            if (pi != null && ci != null)
            {
                previewOldValue.ReadOnly = false;
                previewOldValue.Text = Convert.ToString(pi.GetValue(ci, null));
                previewOldValue.ReadOnly = true;

                previewNewValue.ReadOnly = false;
                previewNewValue.Text = Convert.ToString(pi.GetValue(_catalogueItemBeingSaved, null));
                previewNewValue.ReadOnly = true;

                highlightDifferencesBetweenPreviewPanes();

            }
        }

        private void cbSelectAllCatalogues_CheckedChanged(object sender, EventArgs e)
        {
            for (int i = 0; i < clbCatalogues.Items.Count; i++)
                clbCatalogues.SetItemChecked(i, cbSelectAllCatalogues.Checked);
        }

        private void cbSelectAllFields_CheckedChanged(object sender, EventArgs e)
        {
            for (int i = 0; i < clbChangedProperties.Items.Count; i++)
                clbChangedProperties.SetItemChecked(i, cbSelectAllFields.Checked);
        }

        private void highlightDifferencesBetweenPreviewPanes()
        {
            string sOld = previewOldValue.Text;
            string sNew = previewNewValue.Text;

            var highlighter = new ScintillaLineHighlightingHelper();

            highlighter.ClearAll(previewNewValue);
            highlighter.ClearAll(previewOldValue);

            Diff diff = new Diff();
            foreach (Diff.Item item in diff.DiffText(sOld, sNew))
            {
                
                for (int i = item.StartA; i < item.StartA + item.deletedA; i++)
                    highlighter.HighlightLine(previewOldValue,i,Color.Pink);
                
                //if it is single line change
                for (int i = item.StartB; i < item.StartB + item.insertedB; i++)
                    highlighter.HighlightLine(previewNewValue, i, Color.LawnGreen);

            }
        }

        //yes = do save and do propogate
        private void btnYes_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Yes;

            
            foreach (string catalogueItemAsString in clbCatalogues.CheckedItems)
            {
                CatalogueItem cataItem = FriendlyNamedListOfCatalogueItems[catalogueItemAsString];

                foreach (string propertyAsString in clbChangedProperties.CheckedItems)
                {
                    PropertyInfo p = FriendlyNamedListOfPropertiesChanged[propertyAsString];                 
                    p.SetValue(cataItem,p.GetValue(_catalogueItemBeingSaved,null),null);
                }

                cataItem.SaveToDatabase();
            }

            this.Close();
        }

        //no = do save but dont propogate
        private void btnNo_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.No;
            this.Close();
        }

        //cancel = dont save this and dont propogate
        private void btnCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            this.Close();
            
        }

        
    }
}

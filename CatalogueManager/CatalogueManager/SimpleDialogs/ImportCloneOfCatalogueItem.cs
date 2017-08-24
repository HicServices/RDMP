using System;
using System.Collections.Generic;
using System.Windows.Forms;
using CatalogueLibrary;
using CatalogueLibrary.Cloning;
using CatalogueLibrary.Data;
using System.Linq;
using CatalogueLibrary.Repositories;
using MapsDirectlyToDatabaseTable;
using ReusableUIComponents;


namespace CatalogueManager.SimpleDialogs
{
    /// <summary>
    /// Allows you to copy descriptive metadata (CatalogueItems) between datasets.  This is useful for maintaining a 'single version of the truth' e.g. if every dataset has a field called 
    /// 'NHS Number' then the description of this column should be the same in every case.  Using this form you can import/copy the description from another column.  While this covers you
    /// for setting up new fields, the synchronizing of this description over time (e.g. when a data analyst edits one of the other 'NHS Number' fields) is done through propagation
    /// (See PropagateSaveChangesToCatalogueItemToSimilarNamedCatalogueItems)
    /// </summary>
    public partial class ImportCloneOfCatalogueItem : Form
    {
        private readonly Catalogue _cataToImportTo;
        private readonly CatalogueItem _overwriteTarget;
        readonly object oKeyUpLock = new object();
        private IRepository _repository;

        /// <summary>
        /// Pass in target Catalogue in order to allow the user to import an existing CatalogueItem into the Catalogue.  (Alternatively pass in an overwrite target
        /// which must belong to cataToImportTo) in order to simply copy across fields between the two catalogue items (into overwrite target)
        /// </summary>
        /// <param name="cataToImportTo">The catalogue that the new / overwritten CatalogueItem is to be/in</param>
        /// <param name="overwriteTarget"></param>
        /// <param name="onlyShowCatalogueItemsWithSameName"></param>
        public ImportCloneOfCatalogueItem(Catalogue cataToImportTo, CatalogueItem overwriteTarget = null, bool onlyShowCatalogueItemsWithSameName = false)
        {
            _cataToImportTo = cataToImportTo;
            _overwriteTarget = overwriteTarget;

            InitializeComponent();

            if (cataToImportTo == null)
                return;

            _repository = cataToImportTo.Repository;

            if (_overwriteTarget == null)
            {
                Text = "Import Clone";
                lbl_CatalogueToImportInto.Text = "Importing (Clone of) CatalogueItem into Catalogue named:" +
                                                 _cataToImportTo.Name;
            }
            else
            {
                Text = "Import Description";
                lbl_CatalogueToImportInto.Text = "Import Descriptive CatalogueItem data into " + _overwriteTarget.Name;
                
            }

            
            var repo = ((CatalogueRepository)cataToImportTo.Repository);

            var all = repo.GetFullNameOfAllCatalogueItems();
            List<FriendlyNamedCatalogueItem> toAdd = new List<FriendlyNamedCatalogueItem>();

            if (overwriteTarget == null)
                toAdd = all;
            else
            if (onlyShowCatalogueItemsWithSameName) // only show those with same name but dont show the ovewrite target obviously, dont want to overwrite into itself
                toAdd =
                    all.Where(c => c.FriendlyName.ToLower().Contains(overwriteTarget.Name.ToLower()) && c.ID != overwriteTarget.ID).ToList();
            else
                toAdd = all.Where(c => c.ID != overwriteTarget.ID).ToList();

            cbx_CatalogueToImportFrom.Items.AddRange(toAdd.ToArray());
        }

        private void DoAutocomplete()
        {

            if (cbx_CatalogueToImportFrom.Tag == null) //backup the original list in the Tag of the combo box
            {
                cbx_CatalogueToImportFrom.Tag = new FriendlyNamedCatalogueItem[cbx_CatalogueToImportFrom.Items.Count];
                cbx_CatalogueToImportFrom.Items.CopyTo((FriendlyNamedCatalogueItem[])cbx_CatalogueToImportFrom.Tag, 0);
            }

            FriendlyNamedCatalogueItem[] toFilter = (FriendlyNamedCatalogueItem[])cbx_CatalogueToImportFrom.Tag;

            while (cbx_CatalogueToImportFrom.Items.Count > 0)
                cbx_CatalogueToImportFrom.Items.RemoveAt(0);

            if (toFilter.Length > 0)
            {
                foreach (FriendlyNamedCatalogueItem s in toFilter)
                    if (s.FriendlyName.ToLower().Contains(cbx_CatalogueToImportFrom.Text.ToLower()))
                        cbx_CatalogueToImportFrom.Items.Add(s);

                if (cbx_CatalogueToImportFrom.DroppedDown == false)
                {
                    string txt = cbx_CatalogueToImportFrom.Text;
                    int selectionStart = cbx_CatalogueToImportFrom.SelectionStart;
                    int selectionLength = cbx_CatalogueToImportFrom.SelectionLength;

                    cbx_CatalogueToImportFrom.DroppedDown = true;

                    cbx_CatalogueToImportFrom.SelectedIndex = -1;
                    //un bugger the combo box, apparently windows flips out when you set DroppedDown to true and starts trying to select stuff
                    Cursor.Current = Cursors.Default;
                    cbx_CatalogueToImportFrom.Text = txt;
                    cbx_CatalogueToImportFrom.SelectionStart = selectionStart;
                    cbx_CatalogueToImportFrom.SelectionLength = selectionLength;

                }
            }    
        }
        private void cbx_CatalogueToImportFrom_KeyUp(object sender, KeyEventArgs e)
        {
            lock (oKeyUpLock)
            {
                if (char.ToLower((char)e.KeyValue) >= 'a' && char.ToLower((char)e.KeyValue) <= 'z')
                    DoAutocomplete();
                if (char.ToLower((char)e.KeyValue) >= '0' && char.ToLower((char)e.KeyValue) <= '9')
                    DoAutocomplete();
                if (e.KeyCode == Keys.Space)
                    DoAutocomplete();
                if (e.KeyCode == Keys.Delete)
                    DoAutocomplete();
                if (e.KeyCode == Keys.Back)
                    DoAutocomplete();
                if(e.KeyCode == Keys.Enter)
                {
                    btnImport_Click(null,null);
                    e.SuppressKeyPress = true;
                }
            }
        }
        private void DoImport()
        {
            if(cbx_CatalogueToImportFrom.SelectedItem == null)
            {
                MessageBox.Show("You must select a CatalogueItem to clone, or press Escape/Cancel");
                return;
            }

            CatalogueItem ci =
                _repository.GetObjectByID<CatalogueItem>(
                    ((FriendlyNamedCatalogueItem) cbx_CatalogueToImportFrom.SelectedItem).ID);
            
            if (_overwriteTarget == null)
                ci.CloneCatalogueItemWithIDIntoCatalogue(_cataToImportTo);
            else
            {
                CatalogueCloner.CopyValuesFromCatalogueItemIntoCatalogueItem(ci,_overwriteTarget,true);
                this.Close();
            }

        }

        private void btnImport_Click(object sender, EventArgs e)
        {
            try
            {
                DoImport();

                //only tell the user about the success if it is a fresh import because if it is a fresh import they can do multiple columns at once without the dialog closing
                if(_overwriteTarget == null)
                    MessageBox.Show("Import Successful");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void ImportCloneOfCatalogueItem_KeyUp(object sender, KeyEventArgs e)
        {
            if(e.KeyCode == Keys.Escape)
                btnClose_Click(null, null);

            if (e.KeyCode == Keys.Enter)
                btnImport_Click(null, null);
        }
    }
}

using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using CatalogueLibrary.Data;
using CatalogueLibrary.Repositories;
using CatalogueManager.TestsAndSetup.ServicePropogation;
using MapsDirectlyToDatabaseTable;
using ReusableLibraryCode;
using ReusableUIComponents;



namespace CatalogueManager.SimpleDialogs
{
    /// <summary>
    /// Sometimes you will be called upon to host datasets that are a mile wide (e.g. 200 columns) from which researchers only ever receive/care about 10 or 20.  In this case it can be
    /// very useful to be able to bulk process CatalogueItem/ColumnInfo relationships and create/delete ExtractionInformation on mass.  This dialog lets you do that for a given Catalogue
    /// (dataset).
    /// 
    /// The starting point is to choose which CatalogueItems are to be bulk processed (Apply Transform To).  Either 'All CatalogueItems' or 'Only those matching paste list'.  If you choose
    /// to paste in a list this is done in the left hand listbox.  The window is very flexible about what you can paste in such that you can for example 'Script Select Top 1000' in Microsoft
    /// Sql Management Studio and paste the entire query in and it will work out the columns (it looks for the last bit of text on each line.
    /// 
    /// Once you have configured the bulk process target you can choose what operation to do.  These include:
    /// 
    /// Making all fields Extractable (with the given ExtractionCategory e.g. Core / Supplemental etc)
    /// 
    /// Make all fields Unextractable (Delete Extraction Information)
    /// 
    /// Delete all underlying ColumnInfos (useful if you are trying to migrate your descriptive metadata to a new underlying table in your database e.g. MyDb.Biochemistry to 
    /// MyDb.NewBiochemistry without losing CatalogueItem column descriptions and validation rules etc).
    /// 
    /// Guess New Associated Columns from a given TableInfo (stage 2 in the above example), which will try to match up descriptive CatalogueItems by name to a new underlying TableInfo
    /// 
    ///  Delete All CatalogueItems (If you really want to nuke the lot of them!) 
    /// </summary>
    public partial class BulkProcessCatalogueItems : Form
    {
        public Catalogue Catalogue { get; set; }

        public BulkProcessCatalogueItems(Catalogue catalogue)
        {
            Catalogue = catalogue;
            InitializeComponent();

            if(Catalogue == null)
                return;

            RefreshUIFromDatabase();

            ddExtractionCategory.DataSource = Enum.GetValues(typeof (ExtractionCategory));
        }
        private void RefreshUIFromDatabase()
        {
            lbCatalogueItems.Items.Clear();
            lbCatalogueItems.Items.AddRange(Catalogue.CatalogueItems.ToArray());
            
            cbTableInfos.Items.Clear();
            cbTableInfos.Items.AddRange(Catalogue.GetTableInfoList(true));

        }


        private void groupBox2_Enter(object sender, EventArgs e)
        {

        }

        private void lbPastedColumns_KeyUp(object sender, KeyEventArgs e)
        {
            if(e.KeyCode == Keys.V && e.Control)
            {
                lock (oDrawLock)
                    lbPastedColumns.Items.AddRange(
                        UsefulStuff.GetInstance().GetArrayOfColumnNamesFromStringPastedInByUser(Clipboard.GetText()).ToArray());
                
                rbApplyToMatching.Checked = true;
                        
            }

            if(e.KeyCode == Keys.Delete & lbPastedColumns.SelectedItem != null)
                lbPastedColumns.Items.Remove(lbPastedColumns.SelectedItem);

            


        }

        private readonly object oDrawLock = new object();
        private void lbCatalogueItems_DrawItem(object sender, DrawItemEventArgs e)
        {
            lock (oDrawLock)
            {
                if (e.Index != -1)
                {
                    var catalogueItem = (CatalogueItem) lbCatalogueItems.Items[e.Index];
                    
                    if(lbPastedColumns.Items.Contains(catalogueItem.Name))
                        e.Graphics.FillRectangle(new SolidBrush(Color.LawnGreen), e.Bounds);

                }
                else 
                    e.Graphics.FillRectangle(new SolidBrush(Color.White), e.Bounds);


                e.Graphics.DrawString(lbCatalogueItems.Items[e.Index].ToString(), lbCatalogueItems.Font, new SolidBrush(Color.Black), e.Bounds);
                
            }
        }

        private void btnApplyTransform_Click(object sender, EventArgs e)
        {

            if(rbDelete.Checked)
                if(MessageBox.Show("Are you sure you want to delete?","Confirm delete",MessageBoxButtons.YesNo) != DialogResult.Yes)
                    return;

            ColumnInfo[] guessPoolColumnInfo = null;

            if (rbGuessNewAssociatedColumns.Checked)
            {
                var tableInfo = cbTableInfos.SelectedItem as TableInfo;

                if (tableInfo == null)
                {
                    MessageBox.Show("You must select a TableInfo from the dropdown first");
                    return;
                }

                guessPoolColumnInfo = tableInfo.ColumnInfos.ToArray();
            }
                

            int deleteCount = 0;
            int countExtractionInformationsCreated = 0;
            int countOfColumnInfoAssociationsCreated = 0;

            foreach (CatalogueItem catalogueItem in lbCatalogueItems.Items)
            {
                if (ShouldTransformCatalogueItem(catalogueItem))
                {
                    //bulk operation is delete
                    if(rbDelete.Checked)
                    {
                        catalogueItem.DeleteInDatabase();
                        deleteCount++;
                    }

                    //delete relationship between columnInfo and CatalogueItem (IMPORTANT: this does not delete either the ColumnInfo - which could be used by other Catalogues or the CatalogueItem)
                    if (rbDeleteAssociatedColumnInfos.Checked)
                        if (catalogueItem.ColumnInfo_ID != null)
                        {
                            deleteCount++;
                            catalogueItem.SetColumnInfo(null);
                        }

                    //delete extraction information only, this leaves the underlying relationship between the columnInfo and the CatalogueItem (which must exist in the first place before ExtractionInformation could have been configured) intact 
                    if (rbDeleteExtrctionInformation.Checked)
                        if (catalogueItem.ExtractionInformation != null)
                        {
                            catalogueItem.ExtractionInformation.DeleteInDatabase();
                            deleteCount++;
                        }

                    //user wants to guess ColumnInfo associations between the supplied catalogue and underlying table (and the column doesnt have any existing ones already
                    if (rbGuessNewAssociatedColumns.Checked && catalogueItem.ColumnInfo_ID == null)
                    {
                        ColumnInfo[] guesses = catalogueItem.GuessAssociatedColumn(guessPoolColumnInfo).ToArray();

                        //exact matches are straight up accepted
                        if(guesses.Length == 1)
                        {
                            catalogueItem.SetColumnInfo(guesses[0]);
                            countOfColumnInfoAssociationsCreated++;
                        }
                        else
                        {
                            //multiple matches so ask the user what one he wants
                            for (int i = 0; i < guesses.Length; i++) //note that this sneakily also deals with case where guesses is empty
                            {
                                DialogResult dialogResult = MessageBox.Show("Found multiple matches, approve match?:" + Environment.NewLine + catalogueItem.Name + Environment.NewLine + guesses[i], "Multiple matched guesses", MessageBoxButtons.YesNo);

                                if (dialogResult == DialogResult.Yes)
                                {
                                    catalogueItem.SetColumnInfo(guesses[i]);
                                    countOfColumnInfoAssociationsCreated++;
                                    break;
                                }
                            }
                        }
                    }

                    //user wants to mark existing associated columns as extractable (will be created with the default SELECT transformation which is verbatim, no changes) 
                    if (rbMarkExtractable.Checked)
                    {
                        //get the associated columns
                        var col = catalogueItem.ColumnInfo;

                        //do not try to mark missing column info as extractable
                        if (col == null)
                            continue;

                        //column already has ExtractionInformation configured for it so ignore it
                        if (catalogueItem.ExtractionInformation != null)
                        {
                            //unless user wants to do reckless recategorisation
                            if(cbRecategorise.Checked)
                            {

                                var ei = catalogueItem.ExtractionInformation;
                                ei.ExtractionCategory = (ExtractionCategory) ddExtractionCategory.SelectedItem;
                                ei.SaveToDatabase();
                            }

                            continue;
                        }

                        //we got to here so we have a legit 1 column info to cataitem we can enable for extraction
                        ExtractionInformation created = new ExtractionInformation((CatalogueRepository) catalogueItem.Repository, catalogueItem, col, null);

                        if(ddExtractionCategory.SelectedItem != null)
                        {
                            created.ExtractionCategory = (ExtractionCategory) ddExtractionCategory.SelectedItem;
                            created.SaveToDatabase();
                        }

                        countExtractionInformationsCreated++;
                    }
                }
            }

            string message = "";
            
            if (deleteCount != 0)
                message += "Performed " + deleteCount + " delete operations" + Environment.NewLine;

            if (countExtractionInformationsCreated !=0)
                message += "Created  " + countExtractionInformationsCreated + " ExtractionInformations" + Environment.NewLine;

            if (countOfColumnInfoAssociationsCreated != 0)
                message += "Created  " + countOfColumnInfoAssociationsCreated + " assocations between CatalogueItems and ColumnInfos" + Environment.NewLine;

            if (!string.IsNullOrWhiteSpace(message))
                MessageBox.Show(message);

            RefreshUIFromDatabase();
        }


        private bool ShouldTransformCatalogueItem(CatalogueItem catalogueItem)
        {
            if (rbApplyToAll.Checked)
                return true;

            return lbPastedColumns.Items.Contains(catalogueItem.Name);

        }

        private void btnClear_Click(object sender, EventArgs e)
        {
            lock (oDrawLock)
            {
                lbPastedColumns.Items.Clear();   
            }
        }

        private void ddExtractionCategory_SelectedIndexChanged(object sender, EventArgs e)
        {
            rbMarkExtractable.Checked = true;
        }

        private void cbTableInfos_SelectedIndexChanged(object sender, EventArgs e)
        {
            rbGuessNewAssociatedColumns.Checked = true;
        }

        private void rbMarkExtractable_CheckedChanged(object sender, EventArgs e)
        {

        }
    }
}

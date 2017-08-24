using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using CatalogueLibrary;
using CatalogueLibrary.Data;
using CatalogueLibrary.DataHelper;
using CatalogueLibrary.QueryBuilding.Parameters;
using CatalogueManager.ExtractionUIs.FilterUIs;
using CatalogueManager.ExtractionUIs.FilterUIs.ParameterUIs;
using CatalogueManager.ExtractionUIs.FilterUIs.ParameterUIs.Options;
using CatalogueManager.TestsAndSetup.ServicePropogation;
using CatalogueManager.Validation;
using MapsDirectlyToDatabaseTable;
using MapsDirectlyToDatabaseTable.Revertable;
using ReusableLibraryCode;
using ReusableUIComponents;
using ReusableUIComponents.ChecksUI;


namespace CatalogueManager.MainFormUITabs.SubComponents
{
    /// <summary>
    /// Allows you to change a table reference (TableInfo) to point at a new location.  This should only be used when you have moved a dataset to a new database or server and you should select
    /// 'Synchronize' after you make this change. 
    /// 
    /// The 'Synchronize' button will connect to the referenced server/database and check that it exists and that the columns in the database match the ColumnInfo collection in the Catalogue 
    /// database.  Synchronization happens automatically within the RDMP at some points (e.g. data load) but it is useful to manually do it sometimes if you know you have made a change to your
    /// database schema and want to update the Catalogue.
    /// 
    /// If your TableInfo is pointed at a Table-valued Function then you can select 'Default Table Valued Function Parameters...' to launch a ParameterCollectionUI which contains all the defaults
    /// that the Catalogue will use when invoking your SQL function.  The RDMP requires (and will automatically create) an SQL parameter (e.g. @myExcitingParameter) for each argument taken by
    /// your Table-valued function of a matching datatype and name to the argument as declared in your database.  In practice these default parameter values will usually be overridden at a higher
    /// level (e.g. during cohort identification).
    /// 
    /// This interface also allows you to mark a TableInfo 'Is Primary Extraction Table' which means that the QueryBuilder will start JOIN statements with this table where it is part of a complex
    /// multi table query.
    /// </summary>
    public partial class TableInfoUI : RDMPForm
    {
        private TableInfo _tableInfo;

        public event EventHandler Saved;
        private bool bLoading = false;

        public TableInfoUI(TableInfo t)
        {
            InitializeComponent();

            Text = "TableInfoUI: editing " + t;
            _tableInfo = t;

            if (VisualStudioDesignMode || t == null)
                return;

            tbTableInfoID.Text = _tableInfo.ID.ToString();
            cbIsPrimaryExtractionTable.Checked = _tableInfo.IsPrimaryExtractionTable;
            tbTableInfoName.Text = _tableInfo.Name;
            tbTableInfoDatabaseAccess.Text = _tableInfo.Server;
            tbTableInfoDatabaseName.Text = _tableInfo.Database;

            btnParameters.Enabled = _tableInfo.IsTableValuedFunction;
        }


        protected override bool ProcessKeyPreview(ref Message m)
        {

            PreviewKey p = new PreviewKey(ref m, ModifierKeys);

            if (p.IsKeyDownMessage && p.e.KeyCode == Keys.S && p.e.Control)
            {
                btnTableInfoSave_Click(null, null);
                p.Trap(this);
            }

            return base.ProcessKeyPreview(ref m);
        }



        private void cbIsPrimaryExtractionTable_CheckedChanged(object sender, EventArgs e)
        {
            _tableInfo.IsPrimaryExtractionTable = cbIsPrimaryExtractionTable.Checked;
            _tableInfo.SaveToDatabase();
        }
        
        private void SaveTableInfoAndOfferRefactoring(TableInfo tableInfoInMemory)
        {
            try
            {
                var nameChange = _tableInfo.HasLocalChanges().Differences.SingleOrDefault(d => d.Property.Name.Equals("Name"));
                if (nameChange != null)
                {
                    DialogResult dialogResult = MessageBox.Show("You have just renamed a TableInfo, would you like to refactor your changes into ExtractionInformations?", "Apply Code Refactoring?", MessageBoxButtons.YesNo);

                    if (dialogResult == DialogResult.No)
                        return;

                    DoRefactoring(nameChange);

                    if (Saved != null)
                        Saved(this, new EventArgs());
                }
            }
            finally
            {
                tableInfoInMemory.SaveToDatabase();
            }

            if (Saved != null)
                Saved(this, new EventArgs());
        }

        private void DoRefactoring(RevertablePropertyDifference nameChange)
        {

            string toReplace = RDMPQuerySyntaxHelper.EnsureMultiPartValueIsWrapped(nameChange.DatabaseValue.ToString());
            string toReplaceWith = RDMPQuerySyntaxHelper.EnsureMultiPartValueIsWrapped(nameChange.LocalValue.ToString());

            int updatesMade = 0;

            List<ExtractionInformation> unchanged = new List<ExtractionInformation>();

            foreach (ColumnInfo columnInfo in _tableInfo.ColumnInfos)
            {
                ExtractionInformation[] extractionInformations = columnInfo.ExtractionInformations.ToArray();

                foreach (ExtractionInformation extractionInformation in extractionInformations)
                {
                    if (extractionInformation.SelectSQL.Contains(toReplace))
                    {
                        string newvalue = extractionInformation.SelectSQL.Replace(toReplace, toReplaceWith);
                        
                        if(extractionInformation.SelectSQL.Equals(newvalue))
                            unchanged.Add(extractionInformation);
                        else
                        {
                            extractionInformation.SelectSQL = newvalue;
                            extractionInformation.SaveToDatabase();
                            updatesMade++;
                        }
                    }
                }
            }

            //rename all ColumnInfos that belong to this TableInfo 
            foreach (ColumnInfo columnInfo in _tableInfo.ColumnInfos)
            {
                columnInfo.Name = columnInfo.Name.Replace(toReplace + ".", toReplaceWith + ".");
                columnInfo.SaveToDatabase();
            }

            WideMessageBox.Show("Made " + updatesMade + " replacements in ExtractionInformations, the following ExtractionInformations could not be refactored:" +
                                unchanged.Aggregate(Environment.NewLine,(s,n)=>s + "ID="+n.ID +Environment.NewLine + "Select SQL ="+n.SelectSQL));

        }

       private void tbTableInfoName_TextChanged(object sender, EventArgs e)
        {
            if (bLoading)
                return;

           _tableInfo.Name = tbTableInfoName.Text;
           btnSaveChanges.Enabled = true;
        }
        
        private void tbTableInfoDatabaseAccess_TextChanged(object sender, EventArgs e)
        {
            if (bLoading)
                return;
            _tableInfo.Server = tbTableInfoDatabaseAccess.Text;
            btnSaveChanges.Enabled = true;
        }

        private void tbTableInfoDatabaseName_TextChanged(object sender, EventArgs e)
        {
            if (bLoading)
                return;
            _tableInfo.Database = ((TextBox) sender).Text;
            btnSaveChanges.Enabled = true;
        }

        private void btnTableInfoSave_Click(object sender, EventArgs e)
        {
            SaveTableInfoAndOfferRefactoring(_tableInfo);
            btnSaveChanges.Enabled = false;
        }

        private void btnParameters_Click(object sender, EventArgs e)
        {
            ParameterCollectionUI.ShowAsDialog(new ParameterCollectionUIOptionsFactory().Create(_tableInfo));
        }

        private void btnSynchronize_Click(object sender, EventArgs e)
        {
            try
            {
                bool isSync = new TableInfoSynchronizer(_tableInfo).Synchronize(new MakeChangePopup(new YesNoYesToAllDialog()));

                if (isSync)
                    MessageBox.Show("TableInfo is synchronized");

                if(Saved != null)
                    Saved(this,new EventArgs());
            }
            catch (Exception exception)
            {
                ExceptionViewer.Show(exception);
            }
        }
    }
}

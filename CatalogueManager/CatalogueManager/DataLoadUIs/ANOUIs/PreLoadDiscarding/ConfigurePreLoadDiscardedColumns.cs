using System;
using System.Diagnostics;
using System.Linq;
using System.Windows.Forms;
using System.Windows.Media.Animation;
using CatalogueLibrary.Data;
using CatalogueLibrary.Data.DataLoad;
using CatalogueLibrary.Repositories;
using DataLoadEngine.DataFlowPipeline.Components.Anonymisation;
using HIC.Logging;
using MapsDirectlyToDatabaseTable;
using ReusableLibraryCode;
using ReusableLibraryCode.Checks;
using ReusableUIComponents;


namespace CatalogueManager.DataLoadUIs.ANOUIs.PreLoadDiscarding
{
    /// <summary>
    /// 
    /// BACKGROUND:
    /// PreLoadDiscardedColumn(s) are an alternative (to ANOTables) way of anonymising dataset columns.  A well implemented anonymisation protocol will include both ANOTable substitutions 
    /// and PreLoadDiscardedColumns.  A PreLoadDiscardedColumn is a column containing identifiable which is NOT REQUIRED by anyone to use the dataset.  For example if you have a demography
    /// dataset with a PatientIdentifier, Forename and Surname then you can safely configure Forename and Surname as discarded columns because you have the unique patient identifier (which
    /// should have an ANOTable transform on it btw) to distinguish between patients when doing linkage. 
    /// 
    /// There are 3 types of PreLoadDiscardedColumn, each is treated differently at data load time:
    /// 
    /// DiscardedColumnDestination:
    /// Oblivion - This column DOES NOT exist in the live data table but is created in the RAW load bubble so that the data can be loaded from supplied files.  The data is then deleted prior
    /// to the migration to STAGING
    /// StoreInIdentifiersDump - This column DOES NOT exist in the live data table but is created in the RAW load bubble, instead of being migrated to STAGING the data is stored in an 'identifiers
    /// only' area (the Identifier Dump) along with the PrimaryKeys of the data, this allows you to use the identifiers in debugging or to change your mind about anonymisation later on and reintroduce
    /// the discarded data back into your live database. 
    /// Dilute - This column DOES exist in the live data table but is diluted during load e.g. date of birth 2001-05-03 might be diluted to the first of the month (I know, its insane right but hey
    /// governance wants it!).
    /// 
    /// The theory is that data analysts do not need to know patient level identifiable data to do their jobs and researchers certainly don't.  This is all entirely optional and if you 
    /// do not want to anonymise your data repository then don't worry about this window.
    /// 
    /// USING WINDOW:
    /// Before using the form, make sure you have configured at least one IdentifierDump server.  This can be done through ManageExternalServers.  Select the Identifier Dump Server. 
    /// Next create some PreLoadDiscardedColumn(s) that correspond to supplied fields you do not want to go through to your live database during data load.  Each column must have a
    /// name and SQLDataType that matches what you are trying to load.  
    /// 
    /// If you already have a data table and you want to drop some of the columns from it then you can paste in a list of column names and any that match known columns will automatically 
    /// get created as the appropriate datatype/name.  After doing that you will have to manually drop the columns yourself on your server though.
    ///  
    /// </summary>
    public partial class ConfigurePreLoadDiscardedColumns : Form
    {
        public TableInfo TableInfo { get; set; }
        
        bool loading = true;
        private ICatalogueRepository _repository;

        public ConfigurePreLoadDiscardedColumns(TableInfo tableInfo)
        {
            TableInfo = tableInfo;
            InitializeComponent();

            if(tableInfo == null)
                return;

            _repository = (ICatalogueRepository)tableInfo.Repository;   

            ddIdentifierDump.Items.AddRange(((CatalogueRepository)_repository).GetAllTier2Databases(Tier2DatabaseType.IdentifierDump));
            ddIdentifierDump.Items.Add("<<NONE>>");

            //select the TableInfos dump server from the dropdown
            if (TableInfo.IdentifierDumpServer_ID != null)
                for (int i = 0; i < ddIdentifierDump.Items.Count; i++)
                {
                    ExternalDatabaseServer externalDatabaseServer = ddIdentifierDump.Items[i] as ExternalDatabaseServer;
                    if (externalDatabaseServer == null && TableInfo.IdentifierDumpServer_ID == null)
                        ddIdentifierDump.SelectedIndex = i; //select NONE
                    else
                    if(externalDatabaseServer != null && externalDatabaseServer.ID == TableInfo.IdentifierDumpServer_ID)
                        ddIdentifierDump.SelectedIndex = i;//select the server
                }
            
            RefreshUIFromDatabase();
            preLoadDiscardedColumnUI1.Saved += (s,e)=>RefreshUIFromDatabase();
            lblTableInfoName.Text = TableInfo.GetRuntimeName();
            loading = false;
        }

        private void RefreshUIFromDatabase()
        {
            lbPreLoadDiscardedColumns.Items.Clear();
            lbPreLoadDiscardedColumns.Items.AddRange(TableInfo.PreLoadDiscardedColumns);
        }

        private void lbPreLoadDiscardedColumns_SelectedIndexChanged(object sender, EventArgs e)
        {
        }

        private void btnNewColumn_Click(object sender, EventArgs e)
        {
            new PreLoadDiscardedColumn(_repository,TableInfo);
            RefreshUIFromDatabase();
        }

        private void lbPreLoadDiscardedColumns_KeyDown(object sender, KeyEventArgs e)
        {
            //copy (to clipboard)
            if (e.KeyCode == Keys.C && e.Control)
            {
                string text = lbPreLoadDiscardedColumns.SelectedItems.Cast<PreLoadDiscardedColumn>().Aggregate("", (current, column) => current + column.RuntimeColumnName + "," + Environment.NewLine);
                text = text.TrimEnd(new[] { ',', '\n', '\r' });
                Clipboard.SetText(text);
                e.Handled = true;
            }

            //delete
            if (e.KeyCode == Keys.Delete)
            {
                if (lbPreLoadDiscardedColumns.SelectedItems.Count != 0)
                {
                    string thingUserIsTryingToDelete;

                    if (lbPreLoadDiscardedColumns.SelectedItems.Count == 1)
                        thingUserIsTryingToDelete =
                            ((PreLoadDiscardedColumn)lbPreLoadDiscardedColumns.SelectedItems[0]).RuntimeColumnName;
                    else
                        thingUserIsTryingToDelete = lbPreLoadDiscardedColumns.SelectedItems.Count +
                                                    " PreLoadDiscardedColumns";

                    if (
                        MessageBox.Show(
                            "Are you sure you want to delete " + thingUserIsTryingToDelete + " if you have been doing data loads you could be orphaning identifiers in the ANO database!",
                            "Confirm delete", MessageBoxButtons.YesNo) == DialogResult.Yes)
                    {
                        foreach (PreLoadDiscardedColumn column in lbPreLoadDiscardedColumns.SelectedItems)
                            column.DeleteInDatabase();

                        RefreshUIFromDatabase(); 
                        
                        e.Handled = true;
                   }
                }
            }
            //paste
            if (e.KeyCode == Keys.V && e.Control)
            {
                var columnInfos = TableInfo.ColumnInfos.ToArray();

                //for each thing they pasted
                foreach (string s in UsefulStuff.GetInstance().GetArrayOfColumnNamesFromStringPastedInByUser(Clipboard.GetText()))
                {
                    var newcol = new PreLoadDiscardedColumn(_repository,TableInfo, s);

                    ColumnInfo isCopyOf = columnInfos.SingleOrDefault(c => c.GetRuntimeName().Equals(s));

                    //see if they are pasting an existing column, in which case copy the datatype
                    if (isCopyOf != null)
                    {
                        //fetch it again because we learned that the user pasted in this field but it actually has a ColumnInfo (presumably the user plans to nuke this column at some point and is just configuring the ANO state of it in the mean time) at any rate it has the same Datatype as this ColumnInfo (probably)
                        newcol.SqlDataType = isCopyOf.Data_type;
                        newcol.SaveToDatabase();
                    }
                }

                RefreshUIFromDatabase();
                e.Handled = true;
            }
        }

        private void ConfigurePreLoadDiscardedColumns_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
                this.Close();
        }

        private void ddIdentifierDump_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (TableInfo != null)
            {
                ExternalDatabaseServer server = ddIdentifierDump.SelectedItem as ExternalDatabaseServer;
                
                if (server == null)
                    TableInfo.IdentifierDumpServer_ID = null; //user selected NONE
                else
                    TableInfo.IdentifierDumpServer_ID = server.ID;
                
                TableInfo.SaveToDatabase();
            }
        }

    }
}

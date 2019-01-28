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
using CatalogueManager.Collections;
using CatalogueManager.ExtractionUIs.FilterUIs;
using CatalogueManager.ExtractionUIs.FilterUIs.ParameterUIs;
using CatalogueManager.ExtractionUIs.FilterUIs.ParameterUIs.Options;
using CatalogueManager.ItemActivation;
using CatalogueManager.SimpleControls;
using CatalogueManager.TestsAndSetup.ServicePropogation;
using CatalogueManager.Validation;
using MapsDirectlyToDatabaseTable;
using MapsDirectlyToDatabaseTable.Revertable;
using ReusableLibraryCode;
using ReusableUIComponents;
using ReusableUIComponents.ChecksUI;
using ReusableUIComponents.Dialogs;


namespace CatalogueManager.MainFormUITabs.SubComponents
{
    /// <summary>
    /// Allows you to change a table reference (TableInfo) to point at a new location.  This should only be used when you have moved a dataset to a new database or server and you should select
    /// 'Synchronize' after you make this change. 
    /// 
    /// <para>The 'Synchronize' button will connect to the referenced server/database and check that it exists and that the columns in the database match the ColumnInfo collection in the Catalogue 
    /// database.  Synchronization happens automatically within the RDMP at some points (e.g. data load) but it is useful to manually do it sometimes if you know you have made a change to your
    /// database schema and want to update the Catalogue.</para>
    /// 
    /// <para>If your TableInfo is pointed at a Table-valued Function then you can select 'Default Table Valued Function Parameters...' to launch a ParameterCollectionUI which contains all the defaults
    /// that the Catalogue will use when invoking your SQL function.  The RDMP requires (and will automatically create) an SQL parameter (e.g. @myExcitingParameter) for each argument taken by
    /// your Table-valued function of a matching datatype and name to the argument as declared in your database.  In practice these default parameter values will usually be overridden at a higher
    /// level (e.g. during cohort identification).</para>
    /// 
    /// <para>This interface also allows you to mark a TableInfo 'Is Primary Extraction Table' which means that the QueryBuilder will start JOIN statements with this table where it is part of a complex
    /// multi table query.</para>
    /// </summary>
    public partial class TableInfoUI : TableInfoUI_Design, ISaveableUI
    {
        private TableInfo _tableInfo;

        public TableInfoUI()
        {
            InitializeComponent();
            AssociatedCollection = RDMPCollection.Tables;
            objectSaverButton1.BeforeSave += objectSaverButton1_BeforeSave;
        }

        public override void SetDatabaseObject(IActivateItems activator, TableInfo databaseObject)
        {
            base.SetDatabaseObject(activator, databaseObject);

            _tableInfo = databaseObject;

            ragSmiley1.StartChecking(_tableInfo);

            tbTableInfoID.Text = _tableInfo.ID.ToString();
            cbIsPrimaryExtractionTable.Checked = _tableInfo.IsPrimaryExtractionTable;
            tbTableInfoName.Text = _tableInfo.Name;
            tbTableInfoDatabaseAccess.Text = _tableInfo.Server;
            tbTableInfoDatabaseName.Text = _tableInfo.Database;
            tbSchema.Text = _tableInfo.Schema;

            btnParameters.Enabled = _tableInfo.IsTableValuedFunction;
            
            //if it's a Lookup table, don't let them try to make it IsPrimaryExtractionTable (but let them disable that if they have already made that mistake somehow)
            if (_tableInfo.IsLookupTable())
                if (!cbIsPrimaryExtractionTable.Checked)
                    cbIsPrimaryExtractionTable.Enabled = false;
        }

        
        private void cbIsPrimaryExtractionTable_CheckedChanged(object sender, EventArgs e)
        {
            _tableInfo.IsPrimaryExtractionTable = cbIsPrimaryExtractionTable.Checked;
            _tableInfo.SaveToDatabase();
        }


        bool objectSaverButton1_BeforeSave(DatabaseEntity arg)
        {
            //do not mess with the table name if it is a table valued function
            if (_tableInfo.IsTableValuedFunction)
                return true;
            
            var newName = _tableInfo.GetFullyQualifiedName();

            var oldName = _tableInfo.Repository.GetObjectByID<TableInfo>(_tableInfo.ID).GetFullyQualifiedName();

            if (oldName != newName)
            {
                DialogResult dialogResult = MessageBox.Show("You have just renamed a TableInfo, would you like to refactor your changes into ExtractionInformations?", "Apply Code Refactoring?", MessageBoxButtons.YesNo);

                if (dialogResult == DialogResult.Yes)
                    DoRefactoring(oldName,newName);

            }

            
            _tableInfo.Name = newName;

            return true;
        }
        private void DoRefactoring(string toReplace, string toReplaceWith)
        {
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
                updatesMade++;
            }

            if (unchanged.Any())
                WideMessageBox.Show("Updates made","Made " + updatesMade + " replacements in ExtractionInformation/ColumnInfos, the following ExtractionInformations could not be refactored:" + 
                    string.Join(Environment.NewLine,unchanged.Select(n => "ID=" + n.ID + Environment.NewLine + "Select SQL =" + n.SelectSQL)),WideMessageBoxTheme.Help);
            else
                MessageBox.Show("Made " + updatesMade + " replacements in ExtractionInformation/ColumnInfos.");
        }

        private void tbTableInfoName_TextChanged(object sender, EventArgs e)
        {
           _tableInfo.Name = tbTableInfoName.Text;
        }
        
        private void tbTableInfoDatabaseAccess_TextChanged(object sender, EventArgs e)
        {
            _tableInfo.Server = tbTableInfoDatabaseAccess.Text;
        }

        private void tbTableInfoDatabaseName_TextChanged(object sender, EventArgs e)
        {
            _tableInfo.Database = ((TextBox) sender).Text;
        }

        private void tbSchema_TextChanged(object sender, EventArgs e)
        {
            _tableInfo.Schema = ((TextBox) sender).Text;
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
            }
            catch (Exception exception)
            {
                ExceptionViewer.Show(exception);
            }
        }

        public ObjectSaverButton GetObjectSaverButton()
        {
            return objectSaverButton1;
        }

    }

    [TypeDescriptionProvider(typeof(AbstractControlDescriptionProvider<TableInfoUI_Design, UserControl>))]
    public abstract class TableInfoUI_Design : RDMPSingleDatabaseObjectControl<TableInfo>
    {
        
    }
}

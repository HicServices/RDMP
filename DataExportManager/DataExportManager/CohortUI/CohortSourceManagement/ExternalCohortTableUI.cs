using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using CatalogueManager.Collections;
using CatalogueManager.ItemActivation;
using CatalogueManager.Refreshing;
using CatalogueManager.TestsAndSetup.ServicePropogation;
using DataExportLibrary.Data.DataTables;
using ReusableUIComponents;

namespace DataExportManager.CohortUI.CohortSourceManagement
{
    /// <summary>
    /// Allows you to edit an external cohort reference.  This is the location of a cohort database and includes the names of the Cohort table, the custom data table and the names of 
    /// private/release identifiers in the database
    /// </summary>
    public partial class ExternalCohortTableUI : ExternalCohortTableUI_Design
    {
        private ExternalCohortTable _externalCohortTable;
        bool _bLoading;
        
        public ExternalCohortTable ExternalCohortTable
        {
            get { return _externalCohortTable; }
            private set
            {
                _bLoading = true;
                _externalCohortTable = value;

                if (value == null)
                {
                    tbID.Text = "";
                    tbName.Text = "";
                    tbServer.Text = "";
                    tbDatabase.Text = "";

                    tbUsername.Text = "";
                    tbPassword.Text = "";

                    tbTableName.Text = "";
                    tbPrivateIdentifierField.Text = "";
                    tbReleaseIdentifierField.Text = "";
                    tbDefinitionTableForeignKeyField.Text = "";

                    tbDefinitionTableName.Text = "";
                }
                else
                {
                    tbID.Text = value.ID.ToString();
                    tbName.Text = value.Name;
                    tbServer.Text = value.Server;
                    tbDatabase.Text = value.Database;

                    tbUsername.Text = value.Username;
                    tbPassword.Text = value.Password;

                    tbTableName.Text = value.TableName;
                    tbPrivateIdentifierField.Text = value.PrivateIdentifierField;
                    tbReleaseIdentifierField.Text = value.ReleaseIdentifierField;
                    tbDefinitionTableForeignKeyField.Text = value.DefinitionTableForeignKeyField;

                    tbDefinitionTableName.Text = value.DefinitionTableName;
                }

                btnSave.Enabled = false;
                _bLoading = false;
            }
        }

        public ExternalCohortTableUI()
        {
            InitializeComponent();

            AssociatedCollection = RDMPCollection.SavedCohorts;
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            if(ExternalCohortTable != null)
            {
                ExternalCohortTable.SaveToDatabase();
                btnSave.Enabled = false;
                _activator.RefreshBus.Publish(this,new RefreshObjectEventArgs(ExternalCohortTable));
            }
        }

        private void tb_TextChanged(object sender, EventArgs e)
        {
            
            if (ExternalCohortTable == null || _bLoading)
                return;

            var tbSender = (TextBox) sender;
            var value = tbSender.Text;

            if(tbSender == tbName)
                ExternalCohortTable.Name = value;
            if (tbSender == tbServer)
                ExternalCohortTable.Server = value;
            if (tbSender == tbDatabase)
                ExternalCohortTable.Database = value;

            if (tbSender == tbUsername)
                ExternalCohortTable.Username = value;
            if (tbSender == tbPassword)
                ExternalCohortTable.Password = value;

            if (tbSender == tbTableName)
                ExternalCohortTable.TableName = value;
            if (tbSender == tbPrivateIdentifierField)
                ExternalCohortTable.PrivateIdentifierField = value;
            if (tbSender == tbReleaseIdentifierField)
                ExternalCohortTable.ReleaseIdentifierField = value;
            if (tbSender == tbDefinitionTableForeignKeyField)
                ExternalCohortTable.DefinitionTableForeignKeyField = value;
            if (tbSender == tbDefinitionTableName)
                ExternalCohortTable.DefinitionTableName = value;

            btnSave.Enabled = true;
        }

        private void btnCheck_Click(object sender, EventArgs e)
        {
            checksUI1.StartChecking(ExternalCohortTable);
        }

        public override void SetDatabaseObject(IActivateItems activator, ExternalCohortTable databaseObject)
        {
            base.SetDatabaseObject(activator,databaseObject);
            ExternalCohortTable = databaseObject;
        }
    }

    [TypeDescriptionProvider(typeof(AbstractControlDescriptionProvider<ExternalCohortTableUI_Design, UserControl>))]
    public abstract class ExternalCohortTableUI_Design : RDMPSingleDatabaseObjectControl<ExternalCohortTable>
    {
         
    }
}

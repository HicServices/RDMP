using System;
using System.ComponentModel;
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
    /// Allows you to edit an external cohort reference.  This is the location of a cohort database and includes the names of the Cohort table and the names of 
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

                {
                    tbID.Text = value.ID.ToString();
                    tbName.Text = value.Name;

                    serverDatabaseTableSelector1.DatabaseType = value.DatabaseType;

                    string password = null;
                    try
                    {
                        password = value.GetDecryptedPassword();
                    }
                    catch (Exception)
                    {
                        password = null;
                    }

                    serverDatabaseTableSelector1.SetExplicitDatabase(value.Server, value.Database, value.Username, password);

                    tbTableName.Text = value.TableName;
                    tbPrivateIdentifierField.Text = value.PrivateIdentifierField;
                    tbReleaseIdentifierField.Text = value.ReleaseIdentifierField;
                    tbDefinitionTableForeignKeyField.Text = value.DefinitionTableForeignKeyField;

                    tbDefinitionTableName.Text = value.DefinitionTableName;
                }

                _bLoading = false;
            }
        }

        public ExternalCohortTableUI()
        {
            InitializeComponent();

            AssociatedCollection = RDMPCollection.SavedCohorts;

            serverDatabaseTableSelector1.HideTableComponents();
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            if(ExternalCohortTable != null)
            {
                SaveDatabaseSettings();
                ExternalCohortTable.SaveToDatabase();
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
        }

        private void btnCheck_Click(object sender, EventArgs e)
        {
            SaveDatabaseSettings();
            checksUI1.StartChecking(ExternalCohortTable);
        }

        private void SaveDatabaseSettings()
        {
            var db = serverDatabaseTableSelector1.GetDiscoveredDatabase();

            if(db == null)
                return;

            ExternalCohortTable.Server = db.Server.Name;
            ExternalCohortTable.Database = db.GetRuntimeName();
            ExternalCohortTable.Username = db.Server.ExplicitUsernameIfAny;
            ExternalCohortTable.Password = db.Server.ExplicitPasswordIfAny;
            ExternalCohortTable.DatabaseType = db.Server.DatabaseType;
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

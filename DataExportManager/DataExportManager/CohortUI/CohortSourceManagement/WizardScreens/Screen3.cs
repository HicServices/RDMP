using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.SqlClient;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DataExportManager.CohortUI.CohortSourceManagement.WizardScreens
{
    /// <summary>
    /// Once you have understood and configured your cohort database schema including private / release identifier datatypes (See Screen2) you can now choose which database/server to 
    /// create the database on (Sql Server).  Enter the server details.  If you omit username and password then Windows Authentication (Integrated Security) is used, if you enter a
    /// username/password then these will be stored in the Data Export Manager database in encrypted form (See PasswordEncryptionKeyLocationUI) and used to do Sql Authentication when
    /// doing data extractions.
    /// </summary>
    public partial class Screen3 : UserControl
    {
        private readonly CreateNewCohortDatabaseWizardUI _wizardUI;
        private readonly Screen2 _screen2;

        public Screen3(CreateNewCohortDatabaseWizardUI wizardUI,Screen2 screen2)
        {
            _wizardUI = wizardUI;
            _screen2 = screen2;
            InitializeComponent();
        }
        
        private void btnGo_Click(object sender, EventArgs e)
        {
            var builder = new SqlConnectionStringBuilder()
            {
                DataSource = tbServer.Text,
            };

            if (string.IsNullOrWhiteSpace(tbPassword.Text))
                builder.IntegratedSecurity = true;
            else
            {
                builder.UserID = tbUsername.Text;
                builder.Password = tbPassword.Text;
            }


            _wizardUI.ExternalCohortTableCreatedIfAny = _screen2.Wizard.CreateDatabase(
                _screen2.PrivateIdentifierPrototype,
                _screen2.Strategy,
                builder, tbDatabase.Text,tbCohortSourceName.Text, checksUI1);

            if(_wizardUI.ExternalCohortTableCreatedIfAny != null)
                if(MessageBox.Show("Database succesfully created, close Form?","Success",MessageBoxButtons.YesNo) == DialogResult.Yes)
                    ParentForm.Close();
        }
    }
}

// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using CatalogueManager.TestsAndSetup.ServicePropogation;
using DataExportLibrary.CohortDatabaseWizard;
using DataExportLibrary.Data.DataTables;
using ReusableLibraryCode.Checks;
using ReusableUIComponents.ChecksUI;

namespace DataExportManager.CohortUI.CohortSourceManagement.WizardScreens
{
    /// <summary>
    /// <para>Allows you to specify the private and release identifier column name/datatypes for the cohort database you are creating.  It is anticipated that you will have some datasets already
    /// configured in the Data Catalogue Database and have marked your patient identifier columns as IsExtractionIdentifier (See ExtractionInformationUI, ImportSQLTable and 
    /// ForwardEngineerCatalogue).</para>
    /// 
    /// <para>Once you have understood and configured your cohort database schema including private / release identifier datatypes you can now choose which database/server to 
    /// create the database on (Sql Server).  Enter the server details.  If you omit username and password then Windows Authentication (Integrated Security) is used, if you enter a
    /// username/password then these will be stored in the Data Export Manager database in encrypted form (See PasswordEncryptionKeyLocationUI) and used to do Sql Authentication when
    /// doing data extractions.</para> 
    ///  
    /// <para>After you have chosen the correct private identifier you should choose a strategy for allocating release identifiers.  Because each agency handles governance and identifier assignment
    /// differently you can select 'Allow Null Release Identifiers' and provide allocate them yourself manually (e.g. through a stored proceedure).</para>
    /// </summary>
    partial class Screen2 : RDMPUserControl
    {
        public Screen2()
        {
            InitializeComponent();
            helpIcon1.SetHelpText("Null Release Identifiers",
                @"In RMDP a cohort is a list of private identifiers paired to release identifiers.  Normally these release identifiers are allocated as part of the committing pipeline (e.g. as a new GUID).  If you want to allocate these later yourself e.g. with a stored proceedure then you can tick 'AllowNullReleaseIdentifiers' to create a cohort schema where the release identifier can be null.");
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            serverDatabaseTableSelector1.HideTableComponents();
            Wizard = new CreateNewCohortDatabaseWizard(null, Activator.RepositoryLocator.CatalogueRepository, Activator.RepositoryLocator.DataExportRepository, false);
        }

        public CreateNewCohortDatabaseWizard Wizard { get; private set; }
        public PrivateIdentifierPrototype PrivateIdentifierPrototype { get; private set; }

        public ExternalCohortTable ExternalCohortTableCreatedIfAny { get; private set; }

        private void btnDiscoverExtractionIdentifiers_Click(object sender, EventArgs e)
        {
            listView1.ClearObjects();
            listView1.AddObjects(Wizard.GetPrivateIdentifierCandidates());

            if (listView1.GetItemCount() == 0)
                MessageBox.Show("It looks like none of the ExtractionInformations in your Catalogue database are marked as IsExtractionIdentifier");
        }

        private void btnNext_Click(object sender, EventArgs e)
        {
            var db = serverDatabaseTableSelector1.GetDiscoveredDatabase();
            if (db == null)
            {
                MessageBox.Show("You must select a database");
                return;
            }
            if (PrivateIdentifierPrototype == null)
            {
                MessageBox.Show("You must select a private identifier datatype");
                return;
            }

            Wizard = new CreateNewCohortDatabaseWizard(db, Activator.RepositoryLocator.CatalogueRepository, Activator.RepositoryLocator.DataExportRepository, cbAllowNullReleaseIdentifiers.Checked);

            var popup = new PopupChecksUI("Creating Cohort Table", false);
            ExternalCohortTableCreatedIfAny = Wizard.CreateDatabase(PrivateIdentifierPrototype, popup);

            

            if(popup.GetWorst() <= CheckResult.Warning)
                if(MessageBox.Show("Close Form?","Close",MessageBoxButtons.YesNo ) == DialogResult.Yes)
                    ParentForm.Close();
        }

        

        private void listView1_SelectedIndexChanged(object sender, EventArgs e)
        {
            PrivateIdentifierPrototype = (PrivateIdentifierPrototype) listView1.SelectedObject;
        }
    }
}

// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.ComponentModel;
using System.Windows.Forms;
using Rdmp.Core.CohortCommitting;
using Rdmp.Core.DataExport.Data;
using Rdmp.Core.Icons.IconProvision;
using Rdmp.Core.ReusableLibraryCode.Checks;
using Rdmp.UI.ItemActivation;
using Rdmp.UI.SingleControlForms;
using Rdmp.UI.TestsAndSetup.ServicePropogation;
using PopupChecksUI = Rdmp.UI.ChecksUI.PopupChecksUI;

namespace Rdmp.UI.CohortUI.CohortSourceManagement;

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
/// <para>You can use <see cref="CreateNewCohortDatabaseWizard"/> to create a suitable database based on the private identifiers you
/// hold in your existing datasets.</para>
/// </summary>
internal partial class CreateNewCohortDatabaseWizardUI : RDMPUserControl
{
    public CreateNewCohortDatabaseWizardUI(IActivateItems activator)
    {
        InitializeComponent();
        helpIcon1.SetHelpText("Null Release Identifiers",
            @"In RMDP a cohort is a list of private identifiers paired to release identifiers.  Normally these release identifiers are allocated as part of the committing pipeline (e.g. as a new GUID).  If you want to allocate these later yourself e.g. with a stored proceedure then you can tick 'AllowNullReleaseIdentifiers' to create a cohort schema where the release identifier can be null.");

        helpIcon2.SetHelpText("Cohort Databases", "Click to view a diagram of what a cohort store is");
        helpIcon2.SuppressClick = true;

        serverDatabaseTableSelector1.SetItemActivator(activator);
    }

    protected override void OnLoad(EventArgs e)
    {
        base.OnLoad(e);

        serverDatabaseTableSelector1.HideTableComponents();
        Wizard = new CreateNewCohortDatabaseWizard(null, Activator.RepositoryLocator.CatalogueRepository,
            Activator.RepositoryLocator.DataExportRepository, false);
    }

    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public CreateNewCohortDatabaseWizard Wizard { get; private set; }
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public PrivateIdentifierPrototype PrivateIdentifierPrototype { get; private set; }
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]

    public ExternalCohortTable ExternalCohortTableCreatedIfAny { get; private set; }

    private void btnDiscoverExtractionIdentifiers_Click(object sender, EventArgs e)
    {
        listView1.ClearObjects();
        listView1.AddObjects(Wizard.GetPrivateIdentifierCandidates());

        if (listView1.GetItemCount() == 0)
            MessageBox.Show(
                "It looks like none of the ExtractionInformations in your Catalogue database are marked as IsExtractionIdentifier");
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

        Wizard = new CreateNewCohortDatabaseWizard(db, Activator.RepositoryLocator.CatalogueRepository,
            Activator.RepositoryLocator.DataExportRepository, cbAllowNullReleaseIdentifiers.Checked);

        var popup = new PopupChecksUI("Creating Cohort Table", false);
        ExternalCohortTableCreatedIfAny = Wizard.CreateDatabase(PrivateIdentifierPrototype, popup);

        if (popup.GetWorst() <= CheckResult.Warning)
            if (Activator.YesNo("Close Form?", "Close"))
                ParentForm.Close();
    }


    private void listView1_SelectedIndexChanged(object sender, EventArgs e)
    {
        PrivateIdentifierPrototype = (PrivateIdentifierPrototype)listView1.SelectedObject;
    }

    private void HelpIcon2_Click(object sender, EventArgs e)
    {
        var bmp = CatalogueIcons.WhatIsACohort.ImageToBitmap();

        var pb = new PictureBox
        {
            Image = bmp,
            Size = new System.Drawing.Size(bmp.Width, bmp.Height)
        };

        new SingleControlForm(pb).Show();
    }
}
// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.ComponentModel;
using System.Windows.Forms;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Curation.Data.DataLoad;
using Rdmp.UI.ItemActivation;
using Rdmp.UI.TestsAndSetup.ServicePropogation;

namespace Rdmp.UI.DataLoadUIs;

/// <summary>
/// There is a M-1 relationship between Catalogues (datasets) and LoadMetadata (data load recipes).  This window is accessed by right clicking a Catalogue and choosing
/// to configure its LoadMetadata (how data is loaded).  You can either select an existing LoadMetadata (which will probably need modifying such that it correctly loads
/// the new table in addition to what other datasets it already loaded.  Or you can create a new LoadMetadata and create a load from scratch.
/// 
/// <para>Once selected you will be taken to the dataset load configuration screen (See LoadMetadataUI)</para>
/// 
/// </summary>
public partial class CreateNewLoadMetadataUI : RDMPForm
{
    private readonly Catalogue _catalogue;

    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public LoadMetadata LoadMetadataCreatedIfAny { get; set; }

    public CreateNewLoadMetadataUI(Catalogue catalogue, IActivateItems activator) : base(activator)
    {
        _catalogue = catalogue;
        InitializeComponent();

        chooseLoggingTaskUI1.SetItemActivator(activator);
        chooseLoggingTaskUI1.Catalogue = catalogue;
    }

    private void tbLoadMetadataNameToCreate_TextChanged(object sender, EventArgs e)
    {
        btnCreate.Enabled = !string.IsNullOrWhiteSpace(tbLoadMetadataNameToCreate.Text);
    }

    private void btnCreate_Click(object sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(_catalogue.LoggingDataTask))
        {
            MessageBox.Show("You must configure a logging task first");
            return;
        }

        LoadMetadataCreatedIfAny = new LoadMetadata(Activator.RepositoryLocator.CatalogueRepository,
            tbLoadMetadataNameToCreate.Text);

        DialogResult = DialogResult.OK;
        Close();
    }
}
// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.ComponentModel;
using System.Windows.Forms;
using Rdmp.Core.Curation.Data.DataLoad;
using Rdmp.Core.DataLoad.Modules.DataProvider;
using Rdmp.Core.Repositories;

namespace Rdmp.UI.DataLoadUIs.ModuleUIs.DataProvider;

/// <summary>
/// Allows you to specify and store an encrypted set of credentials in the Catalogue database for a web service endpoint.  The exact interpretation of Endpoint, MaxBufferSize and
/// MaxReceivedMessageSize are up to the specific use case of the dialog.  The dialog allows [DemandsInitialization] arguments of plugin classes to securely store the location of
/// a web service in the Catalogue database.
///</summary>
public partial class WebServiceConfigurationUI : Form, ICustomUI<WebServiceConfiguration>
{
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public ICatalogueRepository CatalogueRepository { get; set; }

    public WebServiceConfigurationUI()
    {
        InitializeComponent();
        DialogResult = DialogResult.Cancel;
    }

    public void SetGenericUnderlyingObjectTo(ICustomUIDrivenClass value)
    {
        SetUnderlyingObjectTo((WebServiceConfiguration)value);
    }

    public void SetUnderlyingObjectTo(WebServiceConfiguration value)
    {
        var config = value ?? new WebServiceConfiguration(CatalogueRepository);
        tbEndpoint.Text = config.Endpoint;
        tbUsername.Text = config.Username;

        try
        {
            tbPassword.Text = config.GetDecryptedPassword();
        }
        catch (Exception)
        {
            if (
                MessageBox.Show("Could not decrypt password, would you like to clear it?", "Clear Password",
                    MessageBoxButtons.YesNo) == DialogResult.Yes)
                config.Password = "";
            else
                throw;
        }

        tbMaxBufferSize.Text = config.MaxBufferSize.ToString();
        tbMaxReceivedMessageSize.Text = config.MaxReceivedMessageSize.ToString();
    }

    public ICustomUIDrivenClass GetFinalStateOfUnderlyingObject() =>
        new WebServiceConfiguration(CatalogueRepository)
        {
            Endpoint = tbEndpoint.Text,
            Username = tbUsername.Text,
            Password = tbPassword.Text,
            MaxBufferSize = Convert.ToInt32(tbMaxBufferSize.Text),
            MaxReceivedMessageSize = Convert.ToInt32(tbMaxReceivedMessageSize.Text)
        };

    private void btnSave_Click(object sender, EventArgs e)
    {
        DialogResult = DialogResult.OK;
        Close();
    }

    private void WebServiceConfigurationUI_FormClosing(object sender, FormClosingEventArgs e)
    {
        if (CatalogueRepository == null)
            return;

        if (DialogResult != DialogResult.OK)
            if (MessageBox.Show("Close without saving?", "Cancel Changes", MessageBoxButtons.YesNo) !=
                DialogResult.Yes)
                e.Cancel = true;
    }
}
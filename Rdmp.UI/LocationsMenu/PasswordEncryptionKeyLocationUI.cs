// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using Rdmp.Core.Repositories;
using Rdmp.Core.Repositories.Managers;
using Rdmp.Core.ReusableLibraryCode;
using Rdmp.UI.SimpleDialogs;
using Rdmp.UI.TestsAndSetup.ServicePropogation;


namespace Rdmp.UI.LocationsMenu;

/// <summary>
/// RDMP supports both Integrated Security (Windows User Account Security) and SQL Authentication.  The later requires the storing of usernames and passwords for sending at query time
/// to the destination server.  In order to do this in a secure way RDMP encrypts them using 4096-bit RSA public/private key encryption.  By default this will use a PrivateKey that is
/// part of the RDMP codebase but it is recommended that you create your own Private Key.  Without your own private key someone could decompile this software and decrypt the passwords
/// stored in your RDMP database if it ever became compromised.
/// 
/// <para>This control lets you create a custom 4096 bit RSA Private Key file.  The location of this file is stored in the RDMP database but the file itself should be held under access 
/// control (see UserManual.md).  This ensures that passwords are only compromised if both the RDMP database and the Windows user account file system (where the private key is held)
/// are both compromised.</para>
/// 
/// <para>It is only possible to have one key at any one time and once you generate a new one all your previously created passwords will be irretrievable so it is advisable to set this up
/// on day one otherwise you will have to reset all the passwords stored in the RDMP database.</para>
/// </summary>
public partial class PasswordEncryptionKeyLocationUI : RDMPUserControl
{
    private PasswordEncryptionKeyLocation _location;

    public PasswordEncryptionKeyLocationUI()
    {
        InitializeComponent();
    }


    protected override void OnLoad(EventArgs e)
    {
        base.OnLoad(e);

        if (VisualStudioDesignMode) //don't go looking up the key if you are visual studio in designer mode!
            return;

        _location = new PasswordEncryptionKeyLocation(
            (CatalogueRepository)Activator.RepositoryLocator.CatalogueRepository);

        SetEnabledness();
    }

    private void SetEnabledness()
    {
        var keyLocation = _location.GetKeyFileLocation();
        tbCertificate.Text = keyLocation;

        btnShowDirectory.Enabled = keyLocation != null;
        btnDeleteKeyLocation.Enabled = keyLocation != null;
        btnCreateKeyFile.Enabled = keyLocation == null;
    }

    private void tbCertificate_TextChanged(object sender, EventArgs e)
    {
        try
        {
            _location.ChangeLocation(tbCertificate.Text);
            lblLocationInvalid.Text = "";
            btnShowDirectory.Enabled = true;
        }
        catch (Exception ex)
        {
            lblLocationInvalid.Text = ex.Message;
            lblLocationInvalid.ForeColor = Color.Red;
            btnShowDirectory.Enabled = false;
        }
    }

    private void btnShowDirectory_Click(object sender, EventArgs e)
    {
        try
        {
            UsefulStuff.ShowPathInWindowsExplorer(new FileInfo(tbCertificate.Text));
        }
        catch (Exception exception)
        {
            ExceptionViewer.Show(exception);
        }
    }

    private void btnCreateKeyFile_Click(object sender, EventArgs e)
    {
        try
        {
            using var sfd = new SaveFileDialog
            {
                FileName = "MyRDMPKey.key",
                CreatePrompt = true,
                Filter = "*.key|RDMP RSA Parameters Key File",
                CheckPathExists = true
            };

            if (sfd.ShowDialog() == DialogResult.OK)
                _location.CreateNewKeyFile(sfd.FileName);

            SetEnabledness();
        }
        catch (Exception exception)
        {
            ExceptionViewer.Show(exception);
        }
    }

    private void btnDeleteKeyLocation_Click(object sender, EventArgs e)
    {
        try
        {
            if (MessageBox.Show(
                    "You are about to delete the RDMPs record of where the key file is to decrypt passwords, if you do this all currently configured password will become inaccessible (EVEN IF YOU CREATE A NEW KEY, YOU WILL NOT BE ABLE TO GET THE CURRENT PASSWORDS BACK), are you sure you want to do this?",
                    "Confirm deleting location of decryption key file", MessageBoxButtons.YesNo, MessageBoxIcon.Stop) ==
                DialogResult.Yes)
                _location.DeleteKey();

            SetEnabledness();
        }
        catch (Exception exception)
        {
            ExceptionViewer.Show(exception);
        }
    }
}
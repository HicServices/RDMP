// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Diagnostics;
using System.Windows.Forms;
using Rdmp.Core.ReusableLibraryCode.Settings;
using Rdmp.UI.SimpleDialogs;


namespace ResearchDataManagementPlatform.WindowManagement.Licenses;

/// <summary>
/// Displays the open source license for RDMP and so shows the license for all the third party plugins.  You must either accept or decline the license .
/// Declining will close the Form.  This form is shown for the first time on startup or again any time you have declined the conditions.
/// </summary>
public partial class LicenseUI : Form
{
    public LicenseUI()
    {
        InitializeComponent();

        try
        {
            var main = new License("LICENSE");
            _thirdParth = new License("LIBRARYLICENSES");

            rtLicense.Text = main.GetLicenseText();
            rtLicense.KeyDown += (s, e) =>
            {
                if (e.KeyCode == Keys.Enter)
                    btnAccept_Click(btnAccept, EventArgs.Empty);

                // prevents it going BONG!
                e.SuppressKeyPress = true;
            };


            rtThirdPartyLicense.Text = _thirdParth.GetLicenseText();
        }
        catch (Exception ex)
        {
            ExceptionViewer.Show(ex);
        }
    }

    private bool allowClose;

    private License _thirdParth;

    private void btnAccept_Click(object sender, EventArgs e)
    {
        UserSettings.LicenseAccepted = _thirdParth.GetHashOfLicense();
        allowClose = true;
        Close();
    }

    private void btnDecline_Click(object sender, EventArgs e)
    {
        UserSettings.LicenseAccepted = null;
        allowClose = true;
        Process.GetCurrentProcess().Kill();
    }

    private void LicenseUI_FormClosing(object sender, FormClosingEventArgs e)
    {
        if (UserSettings.LicenseAccepted != _thirdParth.GetHashOfLicense() && !allowClose)
        {
            e.Cancel = true;
            MessageBox.Show("You have not accepted/declined the license");
        }
    }
}
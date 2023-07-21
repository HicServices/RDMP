// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Windows.Forms;
using Rdmp.Core.Repositories.Managers;
using Rdmp.UI.ItemActivation;
using Rdmp.UI.TestsAndSetup.ServicePropogation;

namespace Rdmp.UI.SimpleDialogs;

/// <summary>
///     As part of a data extraction, a ReleaseDocument is generated.  This is a Microsoft Word document which lists in
///     tabular format the datasets released, the filters applied, the number
///     of rows extracted, distinct patient identifiers etc.  This document can optionally include a statement about use of
///     the data / accreditation or a disclaimer or whatever else message
///     you want researchers to read.
///     <para>
///         You can only have one message at a time and it is constant, we suggest something like "this data was supplied
///         by blah, please accredit us and the NHS as the data provider... etc"
///     </para>
/// </summary>
public partial class ConfigureDisclaimerUI : RDMPForm
{
    private bool _allowClose;

    public ConfigureDisclaimerUI(IActivateItems activator) : base(activator)
    {
        InitializeComponent();
    }

    protected override void OnLoad(EventArgs e)
    {
        if (VisualStudioDesignMode)
            return;

        var value =
            Activator.RepositoryLocator.DataExportRepository.DataExportPropertyManager.GetValue(DataExportProperty
                .ReleaseDocumentDisclaimer);

        tb.Text = value ?? GetDefaultText();

        _allowClose = true;

        base.OnLoad(e);
    }

    private static string GetDefaultText()
    {
        return
            @"*****************************************************************************************************************************
Please acknowledge HIC as a data source in any publications/reports which contain results generated from our data.  We suggest adding the following:
We acknowledge the support of the Health Informatics Centre, University of Dundee for managing and supplying the anonymised data.
Once you have finished your project, please inform HIC who will make arrangements to recover and archive your project.
*****************************************************************************************************************************";
    }

    private void btnCancelChanges_Click(object sender, EventArgs e)
    {
        _allowClose = true;
        DialogResult = DialogResult.Cancel;
        Close();
    }

    private void btnSaveAndClose_Click(object sender, EventArgs e)
    {
        Activator.RepositoryLocator.DataExportRepository.DataExportPropertyManager.SetValue(
            DataExportProperty.ReleaseDocumentDisclaimer, tb.Text);
        _allowClose = true;
        DialogResult = DialogResult.OK;
        Close();
    }

    private void ConfigureDisclaimer_FormClosing(object sender, FormClosingEventArgs e)
    {
        if (VisualStudioDesignMode)
            return;

        if (!_allowClose)
            switch (MessageBox.Show(
                        "Save changes? Yes - save and close, No - discard changes and close, Cancel - Do not close",
                        "Save Changes?", MessageBoxButtons.YesNoCancel))
            {
                case DialogResult.Cancel:
                    e.Cancel = true;
                    break;
                case DialogResult.Yes:
                    btnSaveAndClose_Click(null, null);
                    break;
                case DialogResult.No:
                    btnCancelChanges_Click(null, null);
                    break;
                default:
                    e.Cancel = true;
                    break;
            }
    }


    private void tb_TextChanged(object sender, EventArgs e)
    {
        _allowClose = false;
    }
}
// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.ComponentModel;
using System.IO;
using System.Windows.Forms;
using Rdmp.Core;
using Rdmp.Core.Curation.Data.Governance;
using Rdmp.Core.ReusableLibraryCode;
using Rdmp.UI.Rules;
using Rdmp.UI.SimpleControls;
using Rdmp.UI.TestsAndSetup.ServicePropogation;

namespace Rdmp.UI.SimpleDialogs.Governance;

/// <summary>
/// The RDMP is designed to store sensitive clinical datasets and make them available in research ready (anonymous) form.  This usually requires governance approval from the data
/// provider.  It is important to store the document trail and schedule (e.g. do you require yearly re-approval) for audit purposes.  The RDMP does this through Governance Periods
/// (See GovernancePeriodUI).
/// 
/// <para>This control allows you to configure/view attachments of a GovernancePeriod (e.g. an email, a scan of a signed approval letter etc). For ease of reference you should describe
/// what is in the document (e.g. 'letter to Fife healthboard (Mary Sue) listing the datasets we host and requesting re-approval for 2016.  Letter is signed by Dr Governancer.)'</para>
/// </summary>
public partial class GovernanceDocumentUI : GovernanceDocumentUI_Design, ISaveableUI
{
    public GovernanceDocumentUI()
    {
        InitializeComponent();
        AssociatedCollection = RDMPCollection.Catalogue;
    }

    protected override void SetBindings(BinderWithErrorProviderFactory rules, GovernanceDocument databaseObject)
    {
        base.SetBindings(rules, databaseObject);

        Bind(tbID, "Text", "ID", g => g.ID);
        Bind(tbName, "Text", "Name", g => g.Name);
        Bind(tbDescription, "Text", "Description", g => g.Description);
        Bind(tbPath, "Text", "URL", g => g.URL);
    }

    private void btnBrowse_Click(object sender, EventArgs e)
    {
        var ofd = new OpenFileDialog
        {
            CheckFileExists = true
        };

        if (ofd.ShowDialog() == DialogResult.OK) tbPath.Text = ofd.FileName;
    }

    private void btnOpenContainingFolder_Click(object sender, EventArgs e)
    {
        try
        {
            UsefulStuff.ShowPathInWindowsExplorer(new FileInfo(tbPath.Text));
        }
        catch (Exception exception)
        {
            ExceptionViewer.Show(exception);
        }
    }
}

[TypeDescriptionProvider(typeof(AbstractControlDescriptionProvider<GovernanceDocumentUI_Design, UserControl>))]
public abstract class GovernanceDocumentUI_Design : RDMPSingleDatabaseObjectControl<GovernanceDocument>;
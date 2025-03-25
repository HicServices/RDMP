// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using Rdmp.Core;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.ReusableLibraryCode;
using Rdmp.UI.ItemActivation;
using Rdmp.UI.Rules;
using Rdmp.UI.SimpleControls;
using Rdmp.UI.TestsAndSetup.ServicePropogation;

namespace Rdmp.UI.SimpleDialogs;

/// <summary>
///     The RDMP allows you at attach both documents and auxiliary tables (SupportingSQLTable) to your datasets
///     (Catalogue).  These artifacts are then available to data analysts who
///     want to understand the dataset better.  Also if you tick IsExtractable then whenever the Catalogue is extracted the
///     table/document is automatically copied and extracted into
///     project extraction directory for provision to the researcher.
///     <para>
///         Enter the name, description and file path to the file you want attached to your dataset.  Make sure the path is
///         on a network drive or otherwise available to all system users
///         otherwise other data analysts will not be able to view the file.
///     </para>
///     <para>
///         Tick Extractable if you want a copy of the document to be automatically created whenever the dataset is
///         extracted and supplied to a researcher as part of a project extraction.
///     </para>
///     <para>
///         If you tick IsGlobal then the table will be extracted regardless of what dataset is selected in a researchers
///         data request (useful for global documents e.g. terms of use of
///         data).
///     </para>
/// </summary>
public partial class SupportingDocumentUI : SupportingDocumentUI_Design, ISaveableUI
{
    private SupportingDocument _supportingDocument;

    public SupportingDocumentUI()
    {
        InitializeComponent();

        ticketingControl1.TicketTextChanged += ticketingControl1_TicketTextChanged;
        AssociatedCollection = RDMPCollection.Catalogue;
    }

    public override void SetDatabaseObject(IActivateItems activator, SupportingDocument databaseObject)
    {
        base.SetDatabaseObject(activator, databaseObject);

        _supportingDocument = databaseObject;

        //populate various textboxes
        ticketingControl1.TicketText = _supportingDocument.Ticket;
        tbUrl.Text = _supportingDocument.URL != null ? _supportingDocument.URL.AbsoluteUri : "";

        CommonFunctionality.AddHelp(cbExtractable, "SupportingDocument.Extractable");
        CommonFunctionality.AddHelp(cbIsGlobal, "SupportingSqlTable.IsGlobal");
    }

    public override void SetItemActivator(IActivateItems activator)
    {
        base.SetItemActivator(activator);
        ticketingControl1.SetItemActivator(activator);
    }

    protected override void SetBindings(BinderWithErrorProviderFactory rules, SupportingDocument databaseObject)
    {
        base.SetBindings(rules, databaseObject);

        Bind(tbID, "Text", "ID", s => s.ID);
        Bind(tbDescription, "Text", "Description", s => s.Description);
        Bind(tbName, "Text", "Name", s => s.Name);
        Bind(cbExtractable, "Checked", "Extractable", s => s.Extractable);
        Bind(cbIsGlobal, "Checked", "IsGlobal", s => s.IsGlobal);
    }

    private void tbUrl_TextChanged(object sender, EventArgs e)
    {
        SetUriPropertyOn(tbUrl, "URL", _supportingDocument);
    }

    private void btnOpen_Click(object sender, EventArgs e)
    {
        if (_supportingDocument == null) return;
        try
        {
            UsefulStuff.ShowPathInWindowsExplorer(_supportingDocument.GetFileName());
        }
        catch (Exception ex)
        {
            MessageBox.Show($"unable to open file:{ex.Message}");
        }
    }

    private static void SetUriPropertyOn(TextBox tb, string propertyToSet, object toSetOn)
    {
        if (toSetOn == null) return;
        try
        {
            tb.ForeColor = Color.Black;

            var target = toSetOn.GetType().GetProperty(propertyToSet);

            target.SetValue(toSetOn, new Uri(tb.Text), null);
            tb.ForeColor = Color.Black;
        }
        catch (UriFormatException)
        {
            tb.ForeColor = Color.Red;
        }
    }

    private void ticketingControl1_TicketTextChanged(object sender, EventArgs e)
    {
        if (_supportingDocument != null)
            _supportingDocument.Ticket = ticketingControl1.TicketText;
    }

    private void btnBrowse_Click(object sender, EventArgs e)
    {
        var ofd = new OpenFileDialog
        {
            CheckFileExists = true
        };
        if (ofd.ShowDialog() == DialogResult.OK)
            tbUrl.Text = ofd.FileName;
    }
}

[TypeDescriptionProvider(typeof(AbstractControlDescriptionProvider<SupportingDocumentUI_Design, UserControl>))]
public abstract class SupportingDocumentUI_Design : RDMPSingleDatabaseObjectControl<SupportingDocument>
{
}
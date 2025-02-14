// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.ComponentModel;
using System.Linq;
using System.Windows.Forms;
using Rdmp.Core;
using Rdmp.Core.CommandExecution.AtomicCommands;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Curation.Data.Defaults;
using Rdmp.UI.Copying;
using Rdmp.UI.ItemActivation;
using Rdmp.UI.Rules;
using Rdmp.UI.ScintillaHelper;
using Rdmp.UI.SimpleControls;
using Rdmp.UI.TestsAndSetup.ServicePropogation;
using ScintillaNET;

namespace Rdmp.UI.SimpleDialogs;

/// <summary>
/// The RDMP allows you at attach both documents and auxiliary tables (SupportingSQLTable) to your datasets (Catalogue).  These artifacts are then available to data analysts who
/// want to understand the dataset better.  Also if you tick IsExtractable then whenever the Catalogue is extracted the table/document is automatically copied and extracted into
/// project extraction directory for provision to the researcher.
/// 
/// <para>If you have Lookup tables (that you don't want to configure as Lookup objects, see LookupConfiguration) or complex dictionary tables etc which are required/helpful in understanding or
/// processing the data in your dataset then you should configure it as a SupportingSQLTable.  Make sure to put in an appropriate name and description of what is in the table.  You
/// must select the server on which the SQL should be run (See ManageExternalServers), if you setup a single reference to your data repository with Database='master' and then ensure
/// that all your SupportingSQLTables are fully qualified (e.g. [MyDb].dbo.[MyTable]) then you can avoid having to create an ExternalDatabaseServer for each different database.</para>
/// 
/// <para>If you tick IsGlobal then the table will be extracted regardless of what dataset is selected in a researchers data request (useful for global lookups that contain cross dataset
/// codes).  </para>
/// 
/// <para>IMPORTANT: Make sure your SQL query DOES NOT return any identifiable data if it is marked as IsExtractable as this SQL is executed 'as is' and does not undergo any project level
/// anonymisation.</para>
/// </summary>
public partial class SupportingSQLTableUI : SupportingSQLTableUI_Design, ISaveableUI
{
    private Scintilla QueryPreview;
    private SupportingSQLTable _supportingSQLTable;

    private const string NoExternalServer = "<<NONE>>";

    public SupportingSQLTableUI()
    {
        InitializeComponent();

        #region Query Editor setup

        if (VisualStudioDesignMode)
            return;

        QueryPreview = new ScintillaTextEditorFactory().Create(new RDMPCombineableFactory());
        QueryPreview.ReadOnly = false;
        QueryPreview.TextChanged += QueryPreview_TextChanged;

        pSQL.Controls.Add(QueryPreview);

        #endregion

        tcTicket.TicketTextChanged += TcTicketOnTicketTextChanged;
        AssociatedCollection = RDMPCollection.Catalogue;
    }


    public override void SetDatabaseObject(IActivateItems activator, SupportingSQLTable databaseObject)
    {
        base.SetDatabaseObject(activator, databaseObject);
        _supportingSQLTable = databaseObject;

        _bLoading = true;

        QueryPreview.Text = _supportingSQLTable.SQL;

        //if it has an external server configured
        ddExternalServers.Text = _supportingSQLTable.ExternalDatabaseServer_ID != null
            ? _supportingSQLTable.ExternalDatabaseServer.ToString()
            : NoExternalServer;

        tcTicket.TicketText = _supportingSQLTable.Ticket;

        _bLoading = false;

        CommonFunctionality.AddHelp(cbExtractable, "SupportingSQLTable.Extractable");
        CommonFunctionality.AddHelp(cbGlobal, "SupportingSqlTable.IsGlobal");

        RefreshUIFromDatabase();
    }

    protected override void SetBindings(BinderWithErrorProviderFactory rules, SupportingSQLTable databaseObject)
    {
        base.SetBindings(rules, databaseObject);

        Bind(tbID, "Text", "ID", s => s.ID);
        Bind(tbName, "Text", "Name", s => s.Name);
        Bind(tbDescription, "Text", "Description", s => s.Description);
        Bind(cbExtractable, "Checked", "Extractable", s => s.Extractable);
        Bind(cbGlobal, "Checked", "IsGlobal", s => s.IsGlobal);
    }

    public override void SetItemActivator(IActivateItems activator)
    {
        base.SetItemActivator(activator);
        tcTicket.SetItemActivator(activator);
    }


    private void RefreshUIFromDatabase()
    {
        ddExternalServers.Items.Clear();
        ddExternalServers.Items.Add(NoExternalServer);
        ddExternalServers.Items.AddRange(_supportingSQLTable.Repository.GetAllObjects<ExternalDatabaseServer>()
            .ToArray());

        if (_supportingSQLTable != null)
            ddExternalServers.SelectedItem = _supportingSQLTable.ExternalDatabaseServer;
    }

    private bool _bLoading;


    private void QueryPreview_TextChanged(object sender, EventArgs e)
    {
        if (_supportingSQLTable != null)
            _supportingSQLTable.SQL = QueryPreview.Text;
    }

    private void cbGlobal_CheckedChanged(object sender, EventArgs e)
    {
        if (_bLoading)
            return;

        if (_supportingSQLTable != null)
        {
            if (cbGlobal.Checked)
            {
                _supportingSQLTable.IsGlobal = true;
            }
            else
            {
                if (
                    Activator.YesNo(
                        "Are you sure you want to tie this SQL to this specific Catalogue? and stop it being Globally viewable to all Catalogues?",
                        "Disable Globalness?"))
                    _supportingSQLTable.IsGlobal = false;
                else
                    cbGlobal.Checked = true;
            }
        }
    }

    private void tbDescription_KeyPress(object sender, KeyPressEventArgs e)
    {
        //apparently that is S when the control key is held down
        if (e.KeyChar == 19 && ModifierKeys == Keys.Control)
            e.Handled = true;
    }

    private void ddExternalServers_SelectedIndexChanged(object sender, EventArgs e)
    {
        if (_supportingSQLTable == null)
            return;

        //user selected NONE
        if (ReferenceEquals(ddExternalServers.SelectedItem, NoExternalServer))
            _supportingSQLTable.ExternalDatabaseServer_ID = null;
        else
            //user selected a good server
            _supportingSQLTable.ExternalDatabaseServer_ID = ((ExternalDatabaseServer)ddExternalServers.SelectedItem).ID;
    }

    private void TcTicketOnTicketTextChanged(object sender, EventArgs eventArgs)
    {
        if (_supportingSQLTable != null)
            _supportingSQLTable.Ticket = tcTicket.TicketText;
    }

    private void btnAdd_Click(object sender, EventArgs e)
    {
        var cmd = new ExecuteCommandCreateNewExternalDatabaseServer(Activator, null, PermissableDefaults.None);
        cmd.Execute();
        RefreshUIFromDatabase();
    }
}

[TypeDescriptionProvider(typeof(AbstractControlDescriptionProvider<SupportingSQLTableUI_Design, UserControl>))]
public abstract class SupportingSQLTableUI_Design : RDMPSingleDatabaseObjectControl<SupportingSQLTable>;
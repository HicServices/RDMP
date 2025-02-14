// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.ComponentModel;
using System.Linq;
using System.Windows.Forms;
using FAnsi;
using Rdmp.Core;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Databases;
using Rdmp.UI.ItemActivation;
using Rdmp.UI.Rules;
using Rdmp.UI.SimpleControls;
using Rdmp.UI.TestsAndSetup.ServicePropogation;

namespace Rdmp.UI.MainFormUITabs.SubComponents;

/// <summary>
/// Allows you to change the connection strings of a known ExternalDatabaseServer.
/// 
/// <para>ExternalDatabaseServers are references to existing servers.  They have a logistical name (what you want to call it) and servername.  Optionally you can
/// specify a database (required in the case of references to specific databases e.g. Logging Database), if you omit it then the 'master' database will be used.
/// If you do not specify a username/password then Integrated Security will be used when connecting (the preferred method).  Usernames and passwords are stored
/// in encrypted form (See PasswordEncryptionKeyLocationUI).</para>
/// </summary>
public partial class ExternalDatabaseServerUI : ExternalDatabaseServerUI_Design, ISaveableUI
{
    private ExternalDatabaseServer _server;
    private bool bloading;

    public ExternalDatabaseServerUI()
    {
        InitializeComponent();
        AssociatedCollection = RDMPCollection.Tables;

        ddDatabaseType.DataSource = Enum.GetValues(typeof(DatabaseType));
    }

    public override void SetDatabaseObject(IActivateItems activator, ExternalDatabaseServer databaseObject)
    {
        base.SetDatabaseObject(activator, databaseObject);
        _server = databaseObject;

        bloading = true;

        try
        {
            SetupDropdownItems();

            tbPassword.Text = _server.GetDecryptedPassword();
            ddDatabaseType.SelectedItem = _server.DatabaseType;
            pbDatabaseProvider.Image = Activator.CoreIconProvider.GetImage(_server.DatabaseType).ImageToBitmap();

            pbServer.Image = Activator.CoreIconProvider.GetImage(_server).ImageToBitmap();

            CommonFunctionality.AddChecks(databaseObject);
        }
        finally
        {
            bloading = false;
        }
    }

    protected override void SetBindings(BinderWithErrorProviderFactory rules, ExternalDatabaseServer databaseObject)
    {
        base.SetBindings(rules, databaseObject);

        Bind(tbID, "Text", "ID", s => s.ID);
        Bind(tbName, "Text", "Name", s => s.Name);
        Bind(tbServerName, "Text", "Server", s => s.Server);
        Bind(tbMappedDataPath, "Text", "MappedDataPath", s => s.MappedDataPath);
        Bind(tbDatabaseName, "Text", "Database", s => s.Database);
        Bind(tbUsername, "Text", "Username", s => s.Username);
        Bind(ddSetKnownType, "Text", "CreatedByAssembly", s => s.CreatedByAssembly);
    }

    private void SetupDropdownItems()
    {
        ddSetKnownType.Items.Clear();

        var manager = new PatcherManager();

        ddSetKnownType.Items.AddRange(manager
            .GetAllPatchers()
            .Select(static p => p.Name)
            .ToArray());
    }

    private void tbPassword_TextChanged(object sender, EventArgs e)
    {
        if (!bloading)
            _server.Password = tbPassword.Text;
    }

    private void btnClearKnownType_Click(object sender, EventArgs e)
    {
        _server.CreatedByAssembly = null;
        ddSetKnownType.SelectedItem = null;
        ddSetKnownType.Text = null;
    }

    private void ddDatabaseType_SelectedIndexChanged(object sender, EventArgs e)
    {
        if (_server == null)
            return;

        var type = (DatabaseType)ddDatabaseType.SelectedValue;
        _server.DatabaseType = type;
        pbDatabaseProvider.Image = Activator.CoreIconProvider.GetImage(type).ImageToBitmap();
    }
}

[TypeDescriptionProvider(typeof(AbstractControlDescriptionProvider<ExternalDatabaseServerUI_Design, UserControl>))]
public abstract class ExternalDatabaseServerUI_Design : RDMPSingleDatabaseObjectControl<ExternalDatabaseServer>;
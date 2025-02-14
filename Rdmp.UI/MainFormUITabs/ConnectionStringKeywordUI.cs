// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.ComponentModel;
using System.Windows.Forms;
using FAnsi;
using Rdmp.Core;
using Rdmp.Core.Curation.Data;
using Rdmp.UI.ItemActivation;
using Rdmp.UI.Rules;
using Rdmp.UI.SimpleControls;
using Rdmp.UI.TestsAndSetup.ServicePropogation;

namespace Rdmp.UI.MainFormUITabs;

/// <summary>
/// Allows you to set up a <see cref="ConnectionStringKeyword"/> which will be used with all connections made against databases of the given <see cref="DatabaseType"/>.
/// Take great care when doing this as you can easily render your datasources unreachable by all system users.
/// </summary>
public partial class ConnectionStringKeywordUI : ConnectionStringKeywordUI_Design, ISaveableUI
{
    private ConnectionStringKeyword _keyword;

    public ConnectionStringKeywordUI()
    {
        InitializeComponent();
        AssociatedCollection = RDMPCollection.Tables;

        ddDatabaseType.DataSource = Enum.GetValues(typeof(DatabaseType));
    }

    public override void SetDatabaseObject(IActivateItems activator, ConnectionStringKeyword databaseObject)
    {
        base.SetDatabaseObject(activator, databaseObject);
        _keyword = databaseObject;

        ddDatabaseType.SelectedItem = _keyword.DatabaseType;
        tbName.Text = _keyword.Name;
        tbValue.Text = _keyword.Value;
        tbID.Text = _keyword.ID.ToString();

        pbDatabaseProvider.Image = Activator.CoreIconProvider.GetImage(_keyword.DatabaseType).ImageToBitmap();

        tbCommandToDelete.Text = "DELETE FROM ConnectionStringKeyword";

        CommonFunctionality.AddChecks(databaseObject);
        CommonFunctionality.StartChecking();
    }

    protected override void SetBindings(BinderWithErrorProviderFactory rules, ConnectionStringKeyword databaseObject)
    {
        base.SetBindings(rules, databaseObject);

        Bind(tbID, "Text", "ID", k => k.ID);
        Bind(tbName, "Text", "Name", k => k.Name);
        Bind(tbValue, "Text", "Value", k => k.Value);
        Bind(ddDatabaseType, "Text", "DatabaseType", k => k.DatabaseType);
    }

    private void ddDatabaseType_SelectedIndexChanged(object sender, EventArgs e)
    {
        if (_keyword == null)
            return;

        var type = (DatabaseType)ddDatabaseType.SelectedValue;
        pbDatabaseProvider.Image = Activator.CoreIconProvider.GetImage(type).ImageToBitmap();
    }
}

[TypeDescriptionProvider(typeof(AbstractControlDescriptionProvider<ConnectionStringKeywordUI_Design, UserControl>))]
public abstract class ConnectionStringKeywordUI_Design : RDMPSingleDatabaseObjectControl<ConnectionStringKeyword>;
// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System.ComponentModel;
using System.Windows.Forms;
using Rdmp.Core;
using Rdmp.Core.Curation.Data;
using Rdmp.UI.Rules;
using Rdmp.UI.SimpleControls;
using Rdmp.UI.TestsAndSetup.ServicePropogation;


namespace Rdmp.UI.CredentialsUIs;

/// <summary>
/// Allows you to change a stored username/password (DataAccessCredentials).  For more information about how passwords are encrypted See PasswordEncryptionKeyLocationUI
/// </summary>
public partial class DataAccessCredentialsUI : DataAccessCredentialsUI_Design, ISaveableUI
{
    public DataAccessCredentialsUI()
    {
        InitializeComponent();

        AssociatedCollection = RDMPCollection.Tables;
    }

    protected override void SetBindings(BinderWithErrorProviderFactory rules, DataAccessCredentials databaseObject)
    {
        base.SetBindings(rules, databaseObject);

        Bind(tbName, "Text", "Name", c => c.Name);
        Bind(tbUsername, "Text", "Username", c => c.Username);
        Bind(tbPassword, "Text", "Password", c => c.Password);
    }
}

[TypeDescriptionProvider(typeof(AbstractControlDescriptionProvider<DataAccessCredentialsUI_Design, UserControl>))]
public abstract class DataAccessCredentialsUI_Design : RDMPSingleDatabaseObjectControl<DataAccessCredentials>;
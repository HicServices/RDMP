// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System.ComponentModel;
using System.Windows.Forms;
using Rdmp.Core;
using Rdmp.Core.Curation.Data.Remoting;
using Rdmp.UI.Rules;
using Rdmp.UI.SimpleControls;
using Rdmp.UI.TestsAndSetup.ServicePropogation;


namespace Rdmp.UI.SimpleDialogs.Remoting;

/// <summary>
/// Lets you change the settings for a RemoteRDMP which is a set of web credentials / url to reach another RDMP instance across the network/internet via a web service.
/// </summary>
public partial class RemoteRDMPUI : RemoteRDMPUI_Design, ISaveableUI
{
    public RemoteRDMPUI()
    {
        InitializeComponent();
        AssociatedCollection = RDMPCollection.Tables;
    }

    protected override void SetBindings(BinderWithErrorProviderFactory rules, RemoteRDMP databaseObject)
    {
        base.SetBindings(rules, databaseObject);

        Bind(tbID, "Text", "ID", r => r.ID);
        Bind(tbName, "Text", "Name", r => r.Name);
        Bind(tbBaseUrl, "Text", "URL", r => r.URL);
        Bind(tbUsername, "Text", "Username", r => r.Username);
        Bind(tbPassword, "Text", "Password", r => r.Password);
    }
}

[TypeDescriptionProvider(typeof(AbstractControlDescriptionProvider<RemoteRDMPUI_Design, UserControl>))]
public abstract class RemoteRDMPUI_Design : RDMPSingleDatabaseObjectControl<RemoteRDMP>;
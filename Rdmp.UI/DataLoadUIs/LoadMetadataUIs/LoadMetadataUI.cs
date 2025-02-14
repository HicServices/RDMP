// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System.ComponentModel;
using System.Windows.Forms;
using Rdmp.Core;
using Rdmp.Core.Curation.Data.DataLoad;
using Rdmp.UI.Rules;
using Rdmp.UI.SimpleControls;
using Rdmp.UI.TestsAndSetup.ServicePropogation;


namespace Rdmp.UI.DataLoadUIs.LoadMetadataUIs;

/// <summary>
/// Allows you to record a user friendly indepth description for your LoadMetadata, how it works and how to use it/maintain it.
/// </summary>
public partial class LoadMetadataUI : LoadMetadataUI_Design, ISaveableUI
{
    public LoadMetadataUI()
    {
        InitializeComponent();
        AssociatedCollection = RDMPCollection.DataLoad;
    }

    protected override void SetBindings(BinderWithErrorProviderFactory rules, LoadMetadata databaseObject)
    {
        base.SetBindings(rules, databaseObject);

        Bind(tbID, "Text", "ID", l => l.ID);
        Bind(tbName, "Text", "Name", l => l.Name);
        Bind(tbDescription, "Text", "Description", l => l.Description);
    }
}

[TypeDescriptionProvider(typeof(AbstractControlDescriptionProvider<LoadMetadataUI_Design, UserControl>))]
public abstract class LoadMetadataUI_Design : RDMPSingleDatabaseObjectControl<LoadMetadata>;
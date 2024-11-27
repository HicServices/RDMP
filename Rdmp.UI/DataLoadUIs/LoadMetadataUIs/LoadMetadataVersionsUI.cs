// Copyright (c) The University of Dundee 2024-2024
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Windows.Forms;
using BrightIdeasSoftware;
using Rdmp.Core;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Curation.Data.DataLoad;
using Rdmp.UI.Collections;
using Rdmp.UI.ItemActivation;
using Rdmp.UI.Rules;
using Rdmp.UI.SimpleControls;
using Rdmp.UI.TestsAndSetup.ServicePropogation;


namespace Rdmp.UI.DataLoadUIs.LoadMetadataVersionsUIs;

/// <summary>
/// Allows you to view and restore saved version of the load metadata
/// </summary>
public partial class LoadMetadataVersionsUI : LoadMetadataVersionsUI_Design
{

    public LoadMetadataVersionsUI()
    {
        InitializeComponent();
        AssociatedCollection = RDMPCollection.DataLoad;
        tlvLoadMetadata.RowHeight = 19;
        olvValue.AspectGetter = s => (s as IArgument)?.Value;
    }

    public override void SetItemActivator(IActivateItems activator)
    {
        base.SetItemActivator(activator);
        var CommonTreeFunctionality = new RDMPCollectionCommonFunctionality();

        CommonTreeFunctionality.SetUp(
            RDMPCollection.DataLoad,
            tlvLoadMetadata,
            activator,
            olvName,
        olvName);
    }

    protected override void SetBindings(BinderWithErrorProviderFactory rules, LoadMetadata databaseObject)
    {
        base.SetBindings(rules, databaseObject);

        var versions = Activator.RepositoryLocator.CatalogueRepository.GetAllObjectsWhere<LoadMetadata>("RootLoadMetadata", databaseObject.ID).ToList();
        tlvLoadMetadata.AddObject(BuildStructure(databaseObject,versions));
    }


    private FolderNode<LoadMetadata> BuildStructure(LoadMetadata baseLoadMetadata, List<LoadMetadata> historicLoadMetadata)
    {
        var folder = new FolderNode<LoadMetadata>(baseLoadMetadata.Name)
        {
            ChildObjects = historicLoadMetadata
        };
        var x = Activator.CoreChildProvider.LoadMetadataRootFolder;
        return folder;
    }
}

[TypeDescriptionProvider(typeof(AbstractControlDescriptionProvider<LoadMetadataVersionsUI_Design, UserControl>))]
public abstract class LoadMetadataVersionsUI_Design : RDMPSingleDatabaseObjectControl<LoadMetadata>
{
}
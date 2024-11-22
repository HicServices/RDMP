// Copyright (c) The University of Dundee 2018-2024
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using Rdmp.Core;
using Rdmp.Core.CommandExecution.AtomicCommands;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Curation.Data.Cohort;
using Rdmp.Core.Icons.IconProvision;
using Rdmp.Core.Providers.Nodes.CohortNodes;
using Rdmp.UI.CommandExecution.AtomicCommands;
using Rdmp.UI.CommandExecution.AtomicCommands.UIFactory;
using Rdmp.UI.ItemActivation;
using Rdmp.UI.Refreshing;

namespace Rdmp.UI.Collections;

/// <summary>
/// Displays all the cohort identification configurations you have configured in RDMP. Cohort Identification Configurations (CIC) are created to identify specific patients e.g. 'all patients
/// with 3 or more prescriptions for a diabetes drug or who have been hospitalised for an amputation'.  Each CIC achieves its goal by combining Cohort Sets with Set operations (UNION,
/// INTERSECT, EXCEPT) for example Cohort Set 1 '3+ diabetes drug prescriptions' UNION 'hospital admissions for amputations'.  Cohort sets can be from the same or different data sets (as
/// long as they have a common identifier).
/// </summary>
public partial class CohortIdentificationCollectionUI : RDMPCollectionUI
{
    private bool _firstTime = true;


    //for expand all/ collapse all
    public CohortIdentificationCollectionUI()
    {
        InitializeComponent();
        olvFrozen.AspectGetter = FrozenAspectGetter;
    }

    public override void SetItemActivator(IActivateItems activator)
    {
        base.SetItemActivator(activator);

        //important to register the setup before the lifetime subscription so it gets priority on events
        CommonTreeFunctionality.SetUp(
            RDMPCollection.Cohort,
            tlvCohortIdentificationConfigurations,
            Activator,
            olvName, //column with the icon
            olvName, //column that can be renamed
            tbFilter
        );
        CommonTreeFunctionality.AxeChildren = new[]
        {
            typeof(CohortIdentificationConfiguration), typeof(Core.Curation.Data.Aggregation.AggregateConfiguration)
        };

        CommonTreeFunctionality.MaintainRootObjects = new[]
        {
            typeof(FolderNode<CohortIdentificationConfiguration>),
            typeof(AllOrphanAggregateConfigurationsNode),
            typeof(AllTemplateAggregateConfigurationsNode)
        };
        var rootFolder = Activator.CoreChildProvider.CohortIdentificationConfigurationRootFolderWithoutVersionedConfigurations;
        rootFolder.ChildFolders = new List<FolderNode<CohortIdentificationConfiguration>>();
        rootFolder.ChildObjects = new List<CohortIdentificationConfiguration>();
        tlvCohortIdentificationConfigurations.AddObject(rootFolder);
        tlvCohortIdentificationConfigurations.AddObject(Activator.CoreChildProvider.OrphanAggregateConfigurationsNode);
        tlvCohortIdentificationConfigurations.AddObject(Activator.CoreChildProvider
            .TemplateAggregateConfigurationsNode);


        tlvCohortIdentificationConfigurations.CanExpandGetter = delegate(object x)
        {
            if (x is CohortIdentificationConfiguration configuration) return configuration.GetVersions().Count > 0;

            return Activator.CoreChildProvider.GetChildren(x).Length > 0;
        };

        tlvCohortIdentificationConfigurations.ChildrenGetter = delegate(object x)
        {
            if (x is CohortIdentificationConfiguration cic) return cic.GetVersions();

            return Activator.CoreChildProvider.GetChildren(x);
        };

        CommonTreeFunctionality.WhitespaceRightClickMenuCommandsGetter = static a => new IAtomicCommand[]
        {
            new ExecuteCommandCreateNewCohortIdentificationConfiguration(a),
            new ExecuteCommandMergeCohortIdentificationConfigurations(a, null)
        };

        Activator.RefreshBus.EstablishLifetimeSubscription(this);

        var factory = new AtomicCommandUIFactory(activator);

        CommonFunctionality.Add(
            factory.CreateMenuItem(new ExecuteCommandCreateNewCohortIdentificationConfiguration(Activator)), "New...");
        CommonFunctionality.Add(
            factory.CreateMenuItem(new ExecuteCommandMergeCohortIdentificationConfigurations(Activator, null)
                { OverrideCommandName = "By Merging Existing..." }), "New...");

        if (!_firstTime) return;

        CommonTreeFunctionality.SetupColumnTracking(olvName, new Guid("f8a42259-ce5a-4006-8ab8-e0305fce05aa"));
        CommonTreeFunctionality.SetupColumnTracking(olvFrozen, new Guid("d1e155ef-a28f-41b5-81e4-b763627ddb3c"));

        tlvCohortIdentificationConfigurations.Expand(rootFolder);
        var refresh = new ToolStripMenuItem
        {
            Visible = true,
            Image = FamFamFamIcons.arrow_refresh.ImageToBitmap(),
            Alignment = ToolStripItemAlignment.Right,
            ToolTipText = "Refresh Object"
        };
        refresh.Click += delegate
        {
            var cic = Activator.CoreChildProvider.AllCohortIdentificationConfigurations.First();
            if (cic is null) return;

            var cmd = new ExecuteCommandRefreshObject(Activator, cic);
            cmd.Execute();
        };
        CommonFunctionality.Add(refresh);

        _firstTime = false;
    }

    public static bool IsRootObject(object root) =>
        // The root CohortIdentificationConfiguration FolderNode is a root element in this tree
        root is FolderNode<CohortIdentificationConfiguration> f
            ? f.Name == FolderHelper.Root
            : root is AllOrphanAggregateConfigurationsNode or AllTemplateAggregateConfigurationsNode;

    public void RefreshBus_RefreshObject(object sender, RefreshObjectEventArgs e)
    {
    }

    private string FrozenAspectGetter(object o) =>
        o is CohortIdentificationConfiguration cic ? cic.Frozen ? "Yes" : "No" : null;
}
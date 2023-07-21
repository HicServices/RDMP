// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using Rdmp.Core;
using Rdmp.Core.CommandExecution.AtomicCommands;
using Rdmp.Core.CommandExecution.AtomicCommands.CatalogueCreationCommands;
using Rdmp.Core.CommandExecution.AtomicCommands.CohortCreationCommands;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Curation.Data.Cohort;
using Rdmp.Core.Curation.Data.DataLoad;
using Rdmp.Core.DataExport.Data;
using Rdmp.UI.CommandExecution.AtomicCommands;
using Rdmp.UI.CommandExecution.AtomicCommands.UIFactory;
using Rdmp.UI.ItemActivation;
using Rdmp.UI.Refreshing;
using Rdmp.UI.TestsAndSetup.ServicePropogation;

namespace ResearchDataManagementPlatform.WindowManagement.HomePane;

/// <summary>
/// The starting page of RDMP.  Provides a single easy access entry point into RDMP functionality for common tasks e.g. Data Management, Project Extraction etc.  Click the links of commands
/// you want to carry out to access wizards that offer streamlined access to the RDMP functionality.
/// 
/// <para>You can access the HomeUI at any time by clicking the home icon in the top left of the RDMP tool bar.</para>
/// </summary>
public partial class HomeUI : RDMPUserControl, ILifetimeSubscriber
{
    private readonly IActivateItems _activator;
    private readonly AtomicCommandUIFactory _uiFactory;

    public HomeUI(IActivateItems activator)
    {
        _activator = activator;
        _uiFactory = new AtomicCommandUIFactory(activator);
        InitializeComponent();
    }

    private void BuildCommandLists()
    {
        boxCatalogue.SetUp(Activator, "Catalogue", typeof(Catalogue), _uiFactory,
            new ExecuteCommandCreateNewCatalogueByImportingFileUI(_activator)
            {
                OverrideCommandName = GlobalStrings.FromFile
            },
            new ExecuteCommandCreateNewCatalogueByImportingExistingDataTable(_activator)
            {
                OverrideCommandName = GlobalStrings.FromDatabase
            });
        boxProject.SetUp(Activator, "Project", typeof(Project), _uiFactory,
            new ExecuteCommandCreateNewDataExtractionProject(_activator));

        boxCohort.SetUp(Activator, "Cohort Builder", typeof(CohortIdentificationConfiguration), _uiFactory,
            new ExecuteCommandCreateNewCohortIdentificationConfiguration(_activator)
            {
                OverrideCommandName = "Cohort Builder Query",
                PromptToPickAProject = true
            },
            new ExecuteCommandCreateNewCohortFromFile(_activator, null, null)
            {
                OverrideCommandName = GlobalStrings.FromFile
            }
        );
        boxDataLoad.SetUp(Activator, "Data Load", typeof(LoadMetadata), _uiFactory,
            new ExecuteCommandCreateNewLoadMetadata(_activator));
    }

    protected override void OnLoad(EventArgs e)
    {
        base.OnLoad(e);

        SetItemActivator(_activator);

        BuildCommandLists();

        _activator.RefreshBus.EstablishLifetimeSubscription(this);
    }

    public void RefreshBus_RefreshObject(object sender, RefreshObjectEventArgs e)
    {
        BuildCommandLists();
    }
}
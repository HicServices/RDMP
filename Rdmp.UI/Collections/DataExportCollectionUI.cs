// Copyright (c) The University of Dundee 2018-2024
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Linq;
using System.Windows.Forms;
using Rdmp.Core;
using Rdmp.Core.CommandExecution.AtomicCommands;
using Rdmp.Core.CommandExecution.AtomicCommands.CatalogueCreationCommands;
using Rdmp.Core.CommandExecution.AtomicCommands.CohortCreationCommands;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Curation.Data.Cohort;
using Rdmp.Core.DataExport.Data;
using Rdmp.Core.Icons.IconProvision;
using Rdmp.Core.Providers;
using Rdmp.Core.ReusableLibraryCode.Icons.IconProvision;
using Rdmp.UI.CommandExecution.AtomicCommands;
using Rdmp.UI.CommandExecution.AtomicCommands.UIFactory;
using Rdmp.UI.ItemActivation;
using Rdmp.UI.Refreshing;

namespace Rdmp.UI.Collections;

/// <summary>
/// Contains a list of all the currently configured data export projects you have.  A data export Project is a collection of one or more datasets combined with a cohort (or multiple
/// if you have sub ExtractionConfigurations within the same Project e.g. cases/controls).
/// 
/// <para>Data in these datasets will be linked against the cohort and anonymised on extraction (to flat files / database etc).</para>
/// </summary>
public partial class DataExportCollectionUI : RDMPCollectionUI, ILifetimeSubscriber
{
    private bool _isFirstTime = true;

    private const string NewMenu = "New...";

    private IActivateItems _activator;

    public DataExportCollectionUI()
    {
        InitializeComponent();

        olvProjectNumber.IsEditable = false;
        olvProjectNumber.AspectGetter = ProjectNumberAspectGetter;

        olvCohortSource.IsEditable = false;
        olvCohortSource.AspectGetter = CohortSourceAspectGetter;

        olvCohortVersion.IsEditable = false;
        olvCohortVersion.AspectGetter = CohortVersionAspectGetter;
    }

    private object CohortSourceAspectGetter(object rowObject)
    {
        //if it is a cohort or something masquerading as a cohort
        var cohort = rowObject is IMasqueradeAs masquerader
            ? masquerader.MasqueradingAs() as ExtractableCohort
            : rowObject as ExtractableCohort;

        //serve up the ExternalCohortTable name
        return cohort?.ExternalCohortTable.Name;
    }

    private object ProjectNumberAspectGetter(object rowObject)
    {
        return rowObject switch
        {
            Project p => p.ProjectNumber,
            IMasqueradeAs masquerade when masquerade.MasqueradingAs() is ExtractableCohort c => c.ExternalProjectNumber,
            _ => null
        };
    }

    private object CohortVersionAspectGetter(object rowObject) =>
        rowObject is IMasqueradeAs masquerade && masquerade.MasqueradingAs() is ExtractableCohort c
            ? c.ExternalVersion
            : (object)null;

    public override void SetItemActivator(IActivateItems activator)
    {
        base.SetItemActivator(activator);
        _activator = activator;

        CommonTreeFunctionality.SetUp(
            RDMPCollection.DataExport,
            tlvDataExport,
            Activator,
            olvName,
            olvName,
            tbFilter
        );

        CommonTreeFunctionality.WhitespaceRightClickMenuCommandsGetter = a => GetWhitespaceRightClickMenu();

        CommonTreeFunctionality.MaintainRootObjects = new Type[]
            { typeof(ExtractableDataSetPackage), typeof(FolderNode<Project>) };

        var dataExportChildProvider = activator.CoreChildProvider as DataExportChildProvider;

        if (dataExportChildProvider != null)
        {
            tlvDataExport.AddObjects(dataExportChildProvider.AllPackages);
            tlvDataExport.AddObject(dataExportChildProvider.ProjectRootFolder);
        }

        if (_isFirstTime)
        {
            CommonTreeFunctionality.SetupColumnTracking(olvName, new Guid("00a384ce-08fa-43fd-9cf3-7ddbbf5cec1c"));
            CommonTreeFunctionality.SetupColumnTracking(olvProjectNumber,
                new Guid("2a1764d4-8871-4488-b068-8940b777f90e"));
            CommonTreeFunctionality.SetupColumnTracking(olvCohortSource,
                new Guid("c4dabcc3-ccc9-4c9b-906b-e8106e8b616c"));
            CommonTreeFunctionality.SetupColumnTracking(olvCohortVersion,
                new Guid("2d0f8d32-090d-4d2b-8cfe-b6d16f5cc419"));

            if (dataExportChildProvider != null) tlvDataExport.Expand(dataExportChildProvider.ProjectRootFolder);

            _isFirstTime = false;
        }

        SetupToolStrip();

        Activator.RefreshBus.EstablishLifetimeSubscription(this, typeof(Project).ToString());
        Activator.RefreshBus.EstablishLifetimeSubscription(this, typeof(CohortIdentificationConfiguration).ToString());
        Activator.RefreshBus.EstablishLifetimeSubscription(this, typeof(Catalogue).ToString());
        Activator.RefreshBus.EstablishLifetimeSubscription(this, typeof(ExternalCohortTable).ToString());

    }

    public void RefreshBus_RefreshObject(object sender, RefreshObjectEventArgs e)
    {
        SetupToolStrip();
    }

    private void SetupToolStrip()
    {
        CommonFunctionality.ClearToolStrip();
        CommonFunctionality.Add(new ExecuteCommandCreateNewDataExtractionProject(Activator), "Project", null, NewMenu);
        CommonFunctionality.Add(new ToolStripSeparator(), NewMenu);

        var _refresh = new ToolStripMenuItem
        {
            Visible = true,
            Image = FamFamFamIcons.arrow_refresh.ImageToBitmap(),
            Alignment = ToolStripItemAlignment.Right,
            ToolTipText = "Refresh Object"
        };
        var dataExportChildProvider = _activator.CoreChildProvider as DataExportChildProvider;

        _refresh.Click += delegate (object sender, EventArgs e)
        {
            var project = dataExportChildProvider.Projects.First();
            if (project is not null)
            {
                var cmd = new ExecuteCommandRefreshObject(Activator, project);
                cmd.Execute();
            }
        };
        CommonFunctionality.Add(_refresh);

        CommonFunctionality.Add(new ExecuteCommandCreateNewCohortIdentificationConfiguration(Activator)
        { PromptToPickAProject = true }, "Cohort Builder Query", null, NewMenu);

        var uiFactory = new AtomicCommandUIFactory(Activator);
        var cohortSubmenu = new ToolStripMenuItem("Cohort");
        cohortSubmenu.DropDownItems.AddRange(
            new[]
            {
                // from cic
                uiFactory.CreateMenuItem(
                    new ExecuteCommandCreateNewCohortByExecutingACohortIdentificationConfiguration(Activator, null)
                        { OverrideCommandName = "From Cohort Builder Query" }),

                // from file
                uiFactory.CreateMenuItem(new ExecuteCommandCreateNewCohortFromFile(Activator, null)
                    { OverrideCommandName = "From File" }),

                // from catalogue
                uiFactory.CreateMenuItem(new ExecuteCommandCreateNewCohortFromCatalogue(Activator, (Catalogue)null)
                    { OverrideCommandName = "From Catalogue" }),

                // from table
                uiFactory.CreateMenuItem(new ExecuteCommandCreateNewCohortFromTable(Activator, null)
                    { OverrideCommandName = "From Table" })
            });
        CommonFunctionality.Add(cohortSubmenu, NewMenu);
        CommonFunctionality.Add(new ToolStripSeparator(), NewMenu);

        CommonFunctionality.Add(new ExecuteCommandCreateNewExtractionConfigurationForProject(Activator),
            "Extraction Configuration", null, NewMenu);
        CommonFunctionality.Add(new ToolStripSeparator(), NewMenu);

        var mi = new ToolStripMenuItem("Project Specific Catalogue",
            Activator.CoreIconProvider.GetImage(RDMPConcept.ProjectCatalogue, OverlayKind.Add).ImageToBitmap());

        var factory = new AtomicCommandUIFactory(Activator);
        mi.DropDownItems.Add(factory.CreateMenuItem(new ExecuteCommandCreateNewCatalogueByImportingFileUI(Activator)
        {
            OverrideCommandName = "From File...",
            PromptForProject = true
        }));

        mi.DropDownItems.Add(factory.CreateMenuItem(
            new ExecuteCommandCreateNewCatalogueByImportingExistingDataTable(Activator)
            {
                OverrideCommandName = "From Database...",
                PromptForProject = true
            }));

        CommonFunctionality.Add(mi, NewMenu);
        CommonFunctionality.Add(new ExecuteCommandCreateNewExtractableDataSetPackage(Activator), "Package", null,
            NewMenu);
    }


    private IAtomicCommand[] GetWhitespaceRightClickMenu()
    {
        return new IAtomicCommand[]
        {
            new ExecuteCommandCreateNewDataExtractionProject(Activator)
                { OverrideCommandName = "Add New Project", Weight = -10 },
            new ExecuteCommandCreateNewCohortIdentificationConfiguration(Activator)
            {
                PromptToPickAProject = true, OverrideCommandName = "Add New Cohort Builder Query", Weight = -4.95f
            },
            new ExecuteCommandCreateNewCohortByExecutingACohortIdentificationConfiguration(Activator, null)
            {
                OverrideCommandName = "From Cohort Builder Query", SuggestedCategory = "Add New Cohort", Weight = -4.9f
            },
            new ExecuteCommandCreateNewCohortFromFile(Activator, null)
                { OverrideCommandName = "From File", SuggestedCategory = "Add New Cohort", Weight = -4.8f },
            new ExecuteCommandCreateNewCohortFromCatalogue(Activator, (Catalogue)null)
            {
                OverrideCommandName = "From Catalogue", SuggestedCategory = "Add New Cohort", Weight = -4.7f
            },
            new ExecuteCommandCreateNewCohortFromTable(Activator, null)
                { OverrideCommandName = "From Table", SuggestedCategory = "Add New Cohort", Weight = -4.6f },
            new ExecuteCommandCreateNewExtractionConfigurationForProject(Activator)
                { OverrideCommandName = "Add New Extraction Configuration", Weight = -2f },
            new ExecuteCommandCreateNewCatalogueByImportingFileUI(Activator)
            {
                OverrideCommandName = "From File...", SuggestedCategory = "Add New Project Specific Catalogue",
                Weight = -1.9f
            },
            new ExecuteCommandCreateNewCatalogueByImportingExistingDataTable(Activator)
            {
                OverrideCommandName = "From Database...", SuggestedCategory = "Add New Project Specific Catalogue",
                Weight = -1.8f
            },
            new ExecuteCommandCreateNewExtractableDataSetPackage(Activator)
                { OverrideCommandName = "Add New Package", Weight = -1.7f }
        };
    }

    public static bool IsRootObject(object root) => root is FolderNode<Project> || root is ExtractableDataSetPackage;
}
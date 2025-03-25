// Copyright (c) The University of Dundee 2018-2024
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Linq;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Curation.Data.Cohort;
using Rdmp.Core.Curation.Data.Pipelines;
using Rdmp.Core.DataExport.Data;
using Rdmp.Core.Icons.IconProvision;
using Rdmp.Core.Providers;
using Rdmp.Core.Repositories.Construction;
using Rdmp.Core.ReusableLibraryCode.Icons.IconProvision;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace Rdmp.Core.CommandExecution.AtomicCommands.CohortCreationCommands;

/// <summary>
///     Generates and runs an SQL query based on a <see cref="CohortIdentificationConfiguration" /> and pipes the resulting
///     private identifier list to create a new <see cref="ExtractableCohort" />.
/// </summary>
public class ExecuteCommandCreateNewCohortByExecutingACohortIdentificationConfiguration : CohortCreationCommandExecution
{
    private CohortIdentificationConfiguration _cic;

    public ExecuteCommandCreateNewCohortByExecutingACohortIdentificationConfiguration(IBasicActivateItems activator,
        ExternalCohortTable externalCohortTable) :
        this(activator, null, externalCohortTable, null, null, null)
    {
        var allConfigurations = activator.CoreChildProvider.AllCohortIdentificationConfigurations;

        if (!allConfigurations.Any())
            SetImpossible(
                "You do not have any CohortIdentificationConfigurations yet, you can create them through the 'Cohorts Identification Toolbox' accessible through Window=>Cohort Identification");

        UseTripleDotSuffix = true;
    }

    [UseWithObjectConstructor]
    public ExecuteCommandCreateNewCohortByExecutingACohortIdentificationConfiguration(IBasicActivateItems activator,
        [DemandsInitialization("The cohort builder query that should be executed")]
        CohortIdentificationConfiguration cic,
        [DemandsInitialization(Desc_ExternalCohortTableParameter)]
        ExternalCohortTable ect,
        [DemandsInitialization(Desc_CohortNameParameter)]
        string cohortName,
        [DemandsInitialization(Desc_ProjectParameter)]
        Project project,
        [DemandsInitialization(
            "Pipeline for executing the query, performing any required transforms on the output list and allocating release identifiers")]
        IPipeline pipeline) :
        base(activator, ect, cohortName, project, pipeline)
    {
        _cic = cic;
    }

    public override string GetCommandHelp()
    {
        return
            "Run the cohort identification configuration (query) and save the resulting final cohort identifier list into a saved cohort database";
    }

    public override void Execute()
    {
        base.Execute();

        var cic = _cic ?? (CohortIdentificationConfiguration)BasicActivator.SelectOne("Select Cohort Builder Query",
            BasicActivator.GetAll<CohortIdentificationConfiguration>().ToArray());
        if (cic == null)
            return;

        if (BasicActivator.IsInteractive)
        {
            var promptForVersionOnCohortCommit = false;
            var promptForVersionOnCohortCommitSetting = BasicActivator.RepositoryLocator.CatalogueRepository
                .GetAllObjects<Setting.Setting>().FirstOrDefault(static s => s.Key == "PromptForVersionOnCohortCommit");
            if (promptForVersionOnCohortCommitSetting is not null)
                promptForVersionOnCohortCommit = Convert.ToBoolean(promptForVersionOnCohortCommitSetting.Value);
            if (promptForVersionOnCohortCommit && BasicActivator.YesNo(
                    "It is recommended to save your cohort as a new version before committing. Would you like to do this?",
                    "Save cohort as new version before committing?"))
            {
                var newVersion = new ExecuteCommandCreateVersionOfCohortConfiguration(BasicActivator, cic);
                newVersion.Execute();
                var versions = cic.GetVersions();
                cic = versions.Last();
            }
        }

        if (Project == null && BasicActivator.CoreChildProvider is DataExportChildProvider dx)
        {
            var projAssociations = dx.AllProjectAssociatedCics
                .Where(c => c.CohortIdentificationConfiguration_ID == cic.ID).ToArray();
            if (projAssociations.Length == 1) Project = projAssociations[0].Project;
        }

        var request = GetCohortCreationRequest(ExtractableCohortAuditLogBuilder.GetDescription(cic));

        //user choose to cancel the cohort creation request dialogue
        if (request == null)
            return;

        request.CohortIdentificationConfiguration = cic;

        var configureAndExecute = GetConfigureAndExecuteControl(request, $"Execute CIC {cic} and commit results", cic);

        configureAndExecute.PipelineExecutionFinishedsuccessfully += (s, u) => OnImportCompletedSuccessfully(cic);

        configureAndExecute.Run(BasicActivator.RepositoryLocator, null, null, null);
    }

    private void OnImportCompletedSuccessfully(CohortIdentificationConfiguration cic)
    {
        //see if we can associate the cic with the project
        var cmd = new ExecuteCommandAssociateCohortIdentificationConfigurationWithProject(BasicActivator)
            .SetTarget((Project)Project).SetTarget(cic);

        //we can!
        if (!cmd.IsImpossible)
            cmd.Execute();
    }

    public override Image<Rgba32> GetImage(IIconProvider iconProvider)
    {
        return iconProvider.GetImage(RDMPConcept.CohortIdentificationConfiguration, OverlayKind.Import);
    }

    public override IAtomicCommandWithTarget SetTarget(DatabaseEntity target)
    {
        base.SetTarget(target);

        if (target is CohortIdentificationConfiguration cohortIdentificationConfiguration)
            _cic = cohortIdentificationConfiguration;

        return this;
    }
}
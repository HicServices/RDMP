// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.Linq;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.DataExport.Data;
using Rdmp.Core.Icons.IconProvision;
using Rdmp.Core.Providers;
using Rdmp.Core.Repositories.Construction;
using Rdmp.Core.ReusableLibraryCode.Icons.IconProvision;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace Rdmp.Core.CommandExecution.AtomicCommands;

/// <summary>
/// Creates a new <see cref="ExtractionConfiguration"/> under a given <see cref="Project"/>
/// </summary>
public sealed class ExecuteCommandCreateNewExtractionConfigurationForProject : BasicCommandExecution
{
    private readonly Project _project;
    private readonly string _name;
    private ExtractableCohort _cohort;

    /// <summary>
    /// True to prompt the user to pick an <see cref="ExtractableCohort"/> after creating the <see cref="ExtractionConfiguration"/>
    /// </summary>
    public bool PromptForCohort { get; set; } = true;

    /// <summary>
    /// True to prompt the user to pick some <see cref="ExtractableDataSet"/> after creating the <see cref="ExtractionConfiguration"/>
    /// </summary>
    public bool PromptForDatasets { get; set; } = true;

    /// <summary>
    /// An explicit cohort to assign for the <see cref="ExtractionConfiguration"/>
    /// </summary>
    public ExtractableCohort CohortIfAny
    {
        get => _cohort;
        set
        {
            if (!GetProjects(value).Any())
                SetImpossible($"There are no Projects with ProjectNumber {value.ExternalProjectNumber}");
            _cohort = value;
        }
    }

    private IEnumerable<Project> GetProjects(ExtractableCohort cohortIfAny)
    {
        if (cohortIfAny is null)
            // no cohort so all Project are valid
            return BasicActivator.RepositoryLocator.DataExportRepository.GetAllObjects<Project>();

        // we have a cohort so can only create an ExtractionConfiguration for Projects that share
        // the cohorts project number

        return BasicActivator.CoreChildProvider is DataExportChildProvider dx
            ? dx.Projects.Where(p => p.ProjectNumber == cohortIfAny.ExternalProjectNumber)
            : Enumerable.Empty<Project>();
    }

    [UseWithObjectConstructor]
    public ExecuteCommandCreateNewExtractionConfigurationForProject(IBasicActivateItems activator,
        [DemandsInitialization("The Project under which to create the new ExtractionConfiguration")]
        Project project,
        [DemandsInitialization("The name for the new ExtractionConfiguration")]
        string name = "") : base(activator)
    {
        _project = project;
        _name = name;
    }

    public ExecuteCommandCreateNewExtractionConfigurationForProject(IBasicActivateItems activator) : base(activator)
    {
        if (!activator.GetAll<IProject>().Any())
            SetImpossible("You do not have any projects yet");
    }

    public override string GetCommandHelp() =>
        "Starts a new extraction for the project containing one or more datasets linked against a given cohort";

    public override Image<Rgba32> GetImage(IIconProvider iconProvider) =>
        iconProvider.GetImage(RDMPConcept.ExtractionConfiguration, OverlayKind.Add);

    public override void Execute()
    {
        base.Execute();

        var p = _project;

        if (p == null)
            if (!SelectOne(new DialogArgs
                {
                    WindowTitle = "Select Project",
                    TaskDescription = GetTaskDescription()
                }, GetProjects(CohortIfAny).ToList(), out p))
                return;

        if (p == null)
            return;

        var name = _name;

        // if we don't have a name and we are running in interactive mode
        if (string.IsNullOrWhiteSpace(name) && BasicActivator.IsInteractive)
            if (!BasicActivator.TypeText(new DialogArgs
            {
                WindowTitle = "New Extraction Configuration",
                TaskDescription = "Enter a name for the new Extraction Configuration",
                EntryLabel = "Name"
            }, 255, $"{p.ProjectNumber} {DateTime.Now:yyyy-MM-dd} Extraction".Trim(), out name, false))
                return;

        // create the new config
        var newConfig = new ExtractionConfiguration(BasicActivator.RepositoryLocator.DataExportRepository, p, name);

        if (CohortIfAny != null)
        {
            newConfig.Cohort_ID = CohortIfAny.ID;
            newConfig.SaveToDatabase();
        }
        else
        {
            var chooseCohort = new ExecuteCommandChooseCohort(BasicActivator, newConfig) { NoPublish = true };
            if (PromptForCohort && BasicActivator.IsInteractive && !chooseCohort.IsImpossible) chooseCohort.Execute();
        }

        // user didn't cancel picking a cohort so get them to pick datasets too.
        if (newConfig.Cohort_ID != null)
        {
            var chooseDatasetsCommand = new ExecuteCommandAddDatasetsToConfiguration(BasicActivator, newConfig)
            { NoPublish = true };

            if (PromptForDatasets && BasicActivator.IsInteractive && !chooseDatasetsCommand.IsImpossible)
                chooseDatasetsCommand.Execute();
        }

        //refresh the project
        Publish(p);
        Activate(newConfig);
        Emphasise(newConfig);
    }

    private string GetTaskDescription() =>
        CohortIfAny == null
            ? "Select which Project to create the ExtractionConfiguration under"
            : $"Select which Project to create the ExtractionConfiguration under.  Only Projects with ProjectNumber {CohortIfAny.ExternalProjectNumber} are shown.  This is because you are using ExtractableCohort '{CohortIfAny}' for this operation.";
}
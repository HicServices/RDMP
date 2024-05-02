// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System.Collections.Generic;
using System.Linq;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.DataExport.Data;
using Rdmp.Core.Icons.IconProvision;
using Rdmp.Core.Providers;
using Rdmp.Core.ReusableLibraryCode.Icons.IconProvision;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace Rdmp.Core.CommandExecution.AtomicCommands;

public class ExecuteCommandChooseCohort : BasicCommandExecution, IAtomicCommand
{
    private readonly ExtractionConfiguration _extractionConfiguration;
    private readonly List<ExtractableCohort> _compatibleCohorts = new();
    private readonly ExtractableCohort _pick;

    public ExecuteCommandChooseCohort(IBasicActivateItems activator,
        [DemandsInitialization("The configuration to change the cohort on")]
        ExtractionConfiguration extractionConfiguration,
        [DemandsInitialization("The cohort to pick")]
        ExtractableCohort cohort = null) : base(activator)
    {
        _extractionConfiguration = extractionConfiguration;

        var project = _extractionConfiguration.Project;

        if (extractionConfiguration.IsReleased)
        {
            SetImpossible("ExtractionConfiguration has already been released");
            return;
        }

        if (!project.ProjectNumber.HasValue)
        {
            SetImpossible("Project does not have a ProjectNumber, this determines which cohorts are eligible");
            return;
        }

        if (BasicActivator.CoreChildProvider is not DataExportChildProvider childProvider)
        {
            SetImpossible("Activator.CoreChildProvider is not an DataExportChildProvider");
            return;
        }

        //find cohorts that match the project number
        if (childProvider.ProjectNumberToCohortsDictionary.TryGetValue(project.ProjectNumber.Value, out var value))
            _compatibleCohorts = value.Where(c => !c.IsDeprecated).ToList();

        //if there's only one compatible cohort and that one is already selected
        if (_compatibleCohorts.Count == 1 && _compatibleCohorts.Single().ID == _extractionConfiguration.Cohort_ID)
            SetImpossible("The currently select cohort is the only one available");

        if (!_compatibleCohorts.Any())
            SetImpossible(
                $"There are no cohorts currently configured with ProjectNumber {project.ProjectNumber.Value}");

        _pick = cohort;

        if (_pick != null && !_compatibleCohorts.Contains(_pick))
            SetImpossible(
                $"Specified cohort {_pick} was not compatible with Project.  Check the cohorts ProjectNumber matches");
    }

    public override Image<Rgba32> GetImage(IIconProvider iconProvider)
    {
        return iconProvider.GetImage(RDMPConcept.ExtractableCohort, OverlayKind.Link);
    }

    public override void Execute()
    {
        base.Execute();

        var pick = _pick;

        if (pick == null)
            if (SelectOne(new DialogArgs
                    {
                        WindowTitle = "Select Saved Cohort",
                        TaskDescription =
                            "Select the existing Cohort you would like to be used for your Extraction Configuration."
                    },
                    _compatibleCohorts.Where(c => c.ID != _extractionConfiguration.Cohort_ID && !c.IsDeprecated)
                        .ToList(),
                    out var selected))
                pick = selected;

        if (pick != null)
        {
            //clear current one
            _extractionConfiguration.Cohort_ID = pick.ID;
            _extractionConfiguration.SaveToDatabase();
            Publish(_extractionConfiguration);
        }
    }
}
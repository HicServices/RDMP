// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System.Collections.Generic;
using System.Linq;
using Rdmp.Core.DataExport.Data;
using Rdmp.Core.Icons.IconProvision;
using Rdmp.Core.Repositories.Construction;
using Rdmp.Core.ReusableLibraryCode.Icons.IconProvision;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace Rdmp.Core.CommandExecution.AtomicCommands.CohortCreationCommands;

public class ExecuteCommandImportAlreadyExistingCohort : BasicCommandExecution, IAtomicCommand
{
    private readonly ExternalCohortTable _externalCohortTable;
    private readonly IProject _specificProject;
    private readonly int? _explicitOriginIDToImport;

    public ExecuteCommandImportAlreadyExistingCohort(IBasicActivateItems activator,
        ExternalCohortTable externalCohortTable, IProject specificProject) : base(activator)
    {
        _externalCohortTable = externalCohortTable;
        _specificProject = specificProject;

        if (specificProject is { ProjectNumber: null }) SetImpossible("Project does not have a ProjectNumber yet");
    }


    [UseWithObjectConstructor]
    public ExecuteCommandImportAlreadyExistingCohort(IBasicActivateItems activator,
        ExternalCohortTable externalCohortTable, int originIDToImport) : this(activator, externalCohortTable, null)
    {
        _explicitOriginIDToImport = originIDToImport;
    }

    public override void Execute()
    {
        base.Execute();

        var ect = _externalCohortTable;
        if (ect == null)
        {
            var available = BasicActivator.RepositoryLocator.DataExportRepository.GetAllObjects<ExternalCohortTable>();
            if (!SelectOne(available, out ect, null, true)) return;
        }

        var newId = _explicitOriginIDToImport ?? GetWhichCohortToImport(ect);
        if (!newId.HasValue) return;

        _ = new ExtractableCohort(BasicActivator.RepositoryLocator.DataExportRepository, ect, newId.Value);
        Publish(ect);
    }

    private int? GetWhichCohortToImport(ExternalCohortTable ect)
    {
        // the cohorts in the database
        var available = ExtractableCohort.GetImportableCohortDefinitions(ect).Where(c => c.ID.HasValue).ToArray();


        // the ones we already know about
        var existing = new HashSet<int>(BasicActivator.RepositoryLocator.DataExportRepository
            .GetAllObjects<ExtractableCohort>().Where(c => c.ExternalCohortTable_ID == ect.ID).Select(c => c.OriginID));

        // new ones we don't know about yet
        available = available.Where(c => !existing.Contains(c.ID.Value)).ToArray();

        // if there are no new ones
        if (!available.Any())
        {
            BasicActivator.Show("There are no new cohorts");
            return null;
        }

        // we only care about ones associated to this project
        if (_specificProject != null)
        {
            available = available.Where(a => a.ProjectNumber == _specificProject.ProjectNumber).ToArray();

            if (!available.Any())
            {
                BasicActivator.Show(
                    $"There are no new cohorts to import for ProjectNumber {_specificProject.ProjectNumber}");
                return null;
            }
        }

        // pick which one to import
        return BasicActivator.SelectObject("Import Cohort", available, out var cd) ? cd.ID : null;
    }

    public override Image<Rgba32> GetImage(IIconProvider iconProvider)
    {
        return iconProvider.GetImage(RDMPConcept.CohortAggregate, OverlayKind.Import);
    }
}
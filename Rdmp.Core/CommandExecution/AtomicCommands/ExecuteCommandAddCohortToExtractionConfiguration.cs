// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System.Linq;
using Rdmp.Core.CommandExecution.Combining;
using Rdmp.Core.DataExport.Data;
using Rdmp.Core.Repositories.Construction;

namespace Rdmp.Core.CommandExecution.AtomicCommands;

public class ExecuteCommandAddCohortToExtractionConfiguration : BasicCommandExecution
{
    private readonly ExtractableCohortCombineable _sourceExtractableCohortComand;
    private readonly ExtractionConfiguration _targetExtractionConfiguration;

    [UseWithObjectConstructor]
    public ExecuteCommandAddCohortToExtractionConfiguration(IBasicActivateItems activator, ExtractableCohort cohort,
        ExtractionConfiguration targetExtractionConfiguration)
        : this(activator, new ExtractableCohortCombineable(cohort), targetExtractionConfiguration)
    {
    }

    public ExecuteCommandAddCohortToExtractionConfiguration(IBasicActivateItems activator,
        ExtractableCohortCombineable sourceExtractableCohortComand,
        ExtractionConfiguration targetExtractionConfiguration) : base(activator)
    {
        _sourceExtractableCohortComand = sourceExtractableCohortComand;
        _targetExtractionConfiguration = targetExtractionConfiguration;

        if (_sourceExtractableCohortComand.Cohort.IsDeprecated)
        {
            SetImpossible("Cohort is deprecated");
            return;
        }

        if (_sourceExtractableCohortComand.ErrorGettingCohortData != null)
        {
            SetImpossible(
                $"Could not fetch Cohort data:{_sourceExtractableCohortComand.ErrorGettingCohortData.Message}");
            return;
        }

        if (_targetExtractionConfiguration.IsReleased)
        {
            SetImpossible("Extraction is Frozen because it has been released and is readonly, try cloning it instead");
            return;
        }

        if (!sourceExtractableCohortComand.CompatibleExtractionConfigurations.Contains(_targetExtractionConfiguration))
        {
            SetImpossible(
                $"Cohort has project number {sourceExtractableCohortComand.ExternalProjectNumber} so can only be added to ExtractionConfigurations belonging to Projects with that same number");
            return;
        }

        if (_targetExtractionConfiguration.Cohort_ID != null)
        {
            SetImpossible(_targetExtractionConfiguration.Cohort_ID == sourceExtractableCohortComand.Cohort.ID
                ? "ExtractionConfiguration already uses this cohort"
                : "ExtractionConfiguration already uses a different cohort (delete the relationship to the old cohort first)");
            return;
        }
    }

    public override void Execute()
    {
        base.Execute();

        _targetExtractionConfiguration.Cohort_ID = _sourceExtractableCohortComand.Cohort.ID;
        _targetExtractionConfiguration.SaveToDatabase();
        Publish(_targetExtractionConfiguration);
    }
}
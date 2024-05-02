// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System.Linq;
using Rdmp.Core.DataExport.Data;
using Rdmp.Core.Providers.Nodes;
using Rdmp.Core.Providers.Nodes.ProjectCohortNodes;

namespace Rdmp.Core.Providers;

/// <summary>
///     Identifies all rapidly detectable problems with the configurations of Data Export items
/// </summary>
public class DataExportProblemProvider : ProblemProvider
{
    private DataExportChildProvider _exportChildProvider;

    /// <inheritdoc />
    public override void RefreshProblems(ICoreChildProvider childProvider)
    {
        _exportChildProvider = childProvider as DataExportChildProvider;
    }


    /// <inheritdoc />
    protected override string DescribeProblemImpl(object o)
    {
        return o switch
        {
            Project project => DescribeProblem(project),
            ProjectSavedCohortsNode node => DescribeProblem(node),
            ExtractionConfigurationsNode node => DescribeProblem(node),
            SelectedDataSets sets => DescribeProblem(sets),
            ExtractionConfiguration configuration => DescribeProblem(configuration),
            ExternalCohortTable table => DescribeProblem(table),
            ExtractionDirectoryNode node => DescribeProblem(node),
            _ => null
        };
    }

    private string DescribeProblem(ExternalCohortTable externalCohortTable)
    {
        return _exportChildProvider != null && _exportChildProvider.ForbidListedSources.Contains(externalCohortTable)
            ? "Cohort Source database was unreachable"
            : null;
    }

    private string DescribeProblem(ExtractionConfiguration extractionConfiguration)
    {
        if (extractionConfiguration.Cohort_ID == null)
            return "Configuration has no Cohort configured. You must add a Saved Cohort to enable extraction.";

        if (_exportChildProvider != null)
            if (!_exportChildProvider.GetDatasets(extractionConfiguration).Any()) //there are no selected datasets!
                return
                    "Configuration has no selected datasets. Add existing Datasets or Packages to enable extraction.";

        return null;
    }

    private string DescribeProblem(SelectedDataSets selectedDataSets)
    {
        if (_exportChildProvider.IsMissingExtractionIdentifier(selectedDataSets))
            return "There are no IsExtractionIdentifier columns in dataset";

        return selectedDataSets.GetCatalogue()?.IsDeprecated ?? false ? "Dataset is deprecated" : null;
    }

    private string DescribeProblem(ExtractionConfigurationsNode extractionConfigurationsNode)
    {
        if (_exportChildProvider.Projects.Contains(extractionConfigurationsNode.Project))
            if (!_exportChildProvider.GetConfigurations(extractionConfigurationsNode.Project).Any())
                return
                    "Project has no ExtractionConfigurations. Add a new ExtractionConfiguration to define how data is extracted for this Project.";

        return null;
    }

    private string DescribeProblem(ProjectSavedCohortsNode projectSavedCohortsNode)
    {
        return _exportChildProvider.ProjectHasNoSavedCohorts(projectSavedCohortsNode.Project)
            ? "Project has no Cohorts. Commit new Cohort(s) from File/Cohort Query Builder to use with this Project's ExtractionConfigurations"
            : null;
    }


    private static string DescribeProblem(ExtractionDirectoryNode edn)
    {
        return edn.GetDirectoryInfoIfAny() == null
            ? "No Extraction Directory has been specified"
            : null;
    }

    private static string DescribeProblem(Project project)
    {
        if (project.ProjectNumber == null)
            return "Project has no ProjectNumber";

        return string.IsNullOrWhiteSpace(project.ExtractionDirectory)
            ? "Project has no Extraction Directory configured"
            : null;
    }
}
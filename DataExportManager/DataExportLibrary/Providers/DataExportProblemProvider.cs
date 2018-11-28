using System.Linq;
using CatalogueLibrary.Providers;
using DataExportLibrary.Data.DataTables;
using DataExportLibrary.Data.LinkCreators;
using DataExportLibrary.Providers.Nodes;
using DataExportLibrary.Providers.Nodes.ProjectCohortNodes;

namespace DataExportLibrary.Providers
{
    /// <summary>
    /// Identifies all rapidly detectable problems with the configurations of Data Export items
    /// </summary>
    public class DataExportProblemProvider:IProblemProvider
    {
        private DataExportChildProvider _exportChildProvider;

        /// <inheritdoc/>
        public void RefreshProblems(ICoreChildProvider childProvider)
        {
            _exportChildProvider = childProvider as DataExportChildProvider;
        }

        /// <inheritdoc/>
        public bool HasProblem(object o)
        {
            return DescribeProblem(o) != null;
        }

        /// <inheritdoc/>
        public string DescribeProblem(object o)
        {
            if (o is Project)
                return DescribeProblem((Project) o);

            if (o is ProjectSavedCohortsNode)
                return DescribeProblem((ProjectSavedCohortsNode)o);

            if (o is ExtractionConfigurationsNode)
                return DescribeProblem((ExtractionConfigurationsNode) o);

            if (o is SelectedDataSets)
                return DescribeProblem((SelectedDataSets)o);

            if (o is ExtractionConfiguration)
                return DescribeProblem((ExtractionConfiguration) o);

            if (o is ExternalCohortTable)
                return DescribeProblem((ExternalCohortTable) o);

            return null;
        }

        private string DescribeProblem(ExternalCohortTable externalCohortTable)
        {
            if (_exportChildProvider != null && _exportChildProvider.BlackListedSources.Contains(externalCohortTable))
                return "Cohort Source database was unreachable";

            return null;
        }

        private string DescribeProblem(ExtractionConfiguration extractionConfiguration)
        {
            if (extractionConfiguration.Cohort_ID == null)
                return "Configuration has no Cohort configured";

            if (_exportChildProvider != null)
                if (!_exportChildProvider.GetDatasets(extractionConfiguration).Any()) //there are no selected datasets!
                    return "Configuration has no selected datasets";

            return null;
        }

        private string DescribeProblem(SelectedDataSets selectedDataSets)
        {
            var cols = _exportChildProvider.GetColumnsIn(selectedDataSets);

            if (!cols.Any(c => c.IsExtractionIdentifier))
                return "There are no IsExtractionIdentifier columns in dataset";

            return null;
        }

        private string DescribeProblem(ExtractionConfigurationsNode extractionConfigurationsNode)
        {
            if (_exportChildProvider.Projects.Contains(extractionConfigurationsNode.Project))
                if (!_exportChildProvider.GetConfigurations(extractionConfigurationsNode.Project).Any())
                    return "Project has no ExtractionConfigurations";

            return null;
        }

        private string DescribeProblem(ProjectSavedCohortsNode projectSavedCohortsNode)
        {
            if (_exportChildProvider.ProjectHasNoSavedCohorts(projectSavedCohortsNode.Project))
                return "Project has no cohorts";

            return null;
        }

        private string DescribeProblem(Project project)
        {
            if (project.ProjectNumber == null)
                return "Project has no ProjectNumber";

            if (string.IsNullOrWhiteSpace(project.ExtractionDirectory))
                return "Project has no Extraction Directory configured";

            return null;
        }
    }
}

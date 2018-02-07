using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CatalogueLibrary.Providers;
using DataExportLibrary.Data.DataTables;
using DataExportLibrary.Data.LinkCreators;
using DataExportManager.Collections.Nodes;
using DataExportManager.Collections.Nodes.UsedByProject;

namespace DataExportManager.Collections.Providers
{
    public class DataExportProblemProvider:IProblemProvider
    {
        public Dictionary<object, string>  _problems = new Dictionary<object, string>();
        private DataExportChildProvider _exportChildProvider;

        public DataExportProblemProvider()
        {
            
        }
        public DataExportProblemProvider(ICoreChildProvider coreChildProvider):this()
        {
            RefreshProblems(coreChildProvider);
        }

        private void FindProblemsIn(Project project)
        {
            if (_exportChildProvider.Projects.Contains(project))
                if (!_exportChildProvider.GetConfigurations(project).Any())//it has some configurations
                {
                    _problems.Add(project,"Project has no ExtractionConfigurations");
                    return;
                }

            if(project.ProjectNumber == null)
            {
                _problems.Add(project, "Project has no ProjectNumber");
                return;
            }

            if (string.IsNullOrWhiteSpace(project.ExtractionDirectory))
            {
                _problems.Add(project, "Project has no Extraction Directory configured");
                return;
            }

            if (_exportChildProvider.ProjectHasNoSavedCohorts(project))
            {
                _problems.Add(project, "Project has no cohorts");
                return;
            }
        }

        private void FindProblemsIn(ExtractionConfiguration config)
        {
            if (config.Cohort_ID == null)
            {
                _problems.Add(config,"Configuration has no Cohort configured");
                return;
            }

            if (_exportChildProvider != null)
                if (!_exportChildProvider.GetDatasets(config).Any())//there are no selected datasets!
                {
                    _problems.Add(config, "Configuration has no selected datasets");
                    return;
                }

            if (_problems.Keys.OfType<Project>().Any(p => p.ID == config.Project_ID))
            {
                _problems.Add(config, "Parent Project has problems");
                return;
            }
        }
        
        public void RefreshProblems(ICoreChildProvider childProvider)
        {
            _exportChildProvider = childProvider as DataExportChildProvider;

            if (_exportChildProvider == null)
                return;

            _problems = new Dictionary<object, string>();

            foreach (var config in _exportChildProvider.ExtractionConfigurations)
                FindProblemsIn(config);

            foreach (var selectedDataset in _exportChildProvider.SelectedDataSets)
                FindProblemsIn(selectedDataset);

            foreach (var project in _exportChildProvider.Projects)
                FindProblemsIn(project);
        }

        private void FindProblemsIn(SelectedDataSets selectedDataset)
        {
            var cols = _exportChildProvider.GetColumnsIn(selectedDataset);

            if(!cols.Any(c=>c.IsExtractionIdentifier))
                _problems.Add(selectedDataset,"There are no IsExtractionIdentifier columns in dataset");
        }

        public bool HasProblem(object o)
        {
            return _problems.ContainsKey(o);
        }

        public string DescribeProblem(object o)
        {
            return HasProblem(o) ? _problems[o] : null;
        }
    }
}

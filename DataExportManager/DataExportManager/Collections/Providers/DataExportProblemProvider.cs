using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DataExportLibrary.Data.DataTables;
using DataExportManager.Collections.Nodes;
using DataExportManager.Collections.Nodes.UsedByProject;

namespace DataExportManager.Collections.Providers
{
    public class DataExportProblemProvider
    {
        private readonly DataExportChildProvider _childProvider;

        public Dictionary<ExtractionConfiguration, string>  ProblemsWithExtractionConfigurations = new Dictionary<ExtractionConfiguration, string>();
        public Dictionary<Project,string> ProblemsWithProjects=new Dictionary<Project, string>();

        public DataExportProblemProvider(DataExportChildProvider childProvider)
        {
            _childProvider = childProvider;
        }

        public void FindProblems()
        {
            foreach (var config in _childProvider.ExtractionConfigurations)
                FindProblemsIn(config);

            foreach (var project in _childProvider.Projects)
                FindProblemsIn(project);
        }

        private void FindProblemsIn(Project project)
        {
            if (_childProvider.Projects.Contains(project))
                if (!_childProvider.GetConfigurations(project).Any())//it has some configurations
                {
                    ProblemsWithProjects.Add(project,"Project has no ExtractionConfigurations");
                    return;
                }

            if(project.ProjectNumber == null)
            {
                ProblemsWithProjects.Add(project, "Project has no ProjectNumber");
                return;
            }

            if (string.IsNullOrWhiteSpace(project.ExtractionDirectory))
            {
                ProblemsWithProjects.Add(project, "Project has no Extraction Directory configured");
                return;
            }

            if(_childProvider.GetChildren(project).OfType<CohortSourceUsedByProjectNode>().All(s=>s.IsEmptyNode))
            {
                ProblemsWithProjects.Add(project,"Project has no cohorts");
                return;
            }
        }

        private void FindProblemsIn(ExtractionConfiguration config)
        {
            if (config.Cohort_ID == null)
            {
                ProblemsWithExtractionConfigurations.Add(config,"Configuration has no Cohort configured");
                return;
            }

            if (_childProvider != null)
                if (!_childProvider.GetDatasets(config).Any())//there are no selected datasets!
                {
                    ProblemsWithExtractionConfigurations.Add(config, "Configuration has no selected datasets");
                    return;
                }
 
            if(ProblemsWithProjects.Keys.Any(p=>p.ID == config.Project_ID))
            {
                ProblemsWithExtractionConfigurations.Add(config, "Parent Project has problems");
                return;
            }
        }

        public bool HasProblems(ExtractionConfiguration config)
        {
            return ProblemsWithExtractionConfigurations.ContainsKey(config);
        }

        public bool HasProblems(Project project)
        {
            return ProblemsWithProjects.ContainsKey(project);
        }


        public string DescribeProblem(Project project)
        {
            if(HasProblems(project))
                return ProblemsWithProjects[project];
            return null;
        }
        public string DescribeProblem(ExtractionConfiguration config)
        {
            if (HasProblems(config))
                return ProblemsWithExtractionConfigurations[config];
            return null;
        }
    }
}

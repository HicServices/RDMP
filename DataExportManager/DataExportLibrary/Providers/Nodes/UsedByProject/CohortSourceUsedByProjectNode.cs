using System.Collections.Generic;
using CatalogueLibrary.Nodes.UsedByNodes;
using DataExportLibrary.Data.DataTables;

namespace DataExportLibrary.Providers.Nodes.UsedByProject
{
    /// <summary>
    /// Collection of all cohort databases which contain cohorts that can be used in a given <see cref="Project"/>
    /// </summary>
    public class CohortSourceUsedByProjectNode : ObjectUsedByOtherObjectNode<Project,ExternalCohortTable>
    {
        public List<ObjectUsedByOtherObjectNode<CohortSourceUsedByProjectNode,ExtractableCohort>> CohortsUsed { get; set; }

        public CohortSourceUsedByProjectNode(Project project,ExternalCohortTable source): base(project,source)
        {
            CohortsUsed = new List<ObjectUsedByOtherObjectNode<CohortSourceUsedByProjectNode, ExtractableCohort>>();
        }
    }
}

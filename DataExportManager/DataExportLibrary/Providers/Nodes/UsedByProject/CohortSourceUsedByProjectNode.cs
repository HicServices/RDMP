using System.Collections.Generic;
using CatalogueLibrary.Nodes.UsedByNodes;
using DataExportLibrary.Data.DataTables;

namespace DataExportLibrary.Providers.Nodes.UsedByProject
{
    public class CohortSourceUsedByProjectNode : ObjectUsedByOtherObjectNode<Project,ExternalCohortTable>
    {
        public List<ObjectUsedByOtherObjectNode<CohortSourceUsedByProjectNode,ExtractableCohort>> CohortsUsed { get; set; }

        public CohortSourceUsedByProjectNode(Project project,ExternalCohortTable source): base(project,source)
        {
            CohortsUsed = new List<ObjectUsedByOtherObjectNode<CohortSourceUsedByProjectNode, ExtractableCohort>>();
        }
    }
}

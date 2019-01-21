using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CatalogueLibrary.Nodes.CohortNodes
{
    /// <summary>
    /// Collection of all the cohort building queries (CohortIdentificationConfiguration) you have built which are associated with
    /// one or more a extraction projects.
    /// </summary>
    public class AllProjectCohortIdentificationConfigurationsNode : SingletonNode
    {
        public AllProjectCohortIdentificationConfigurationsNode(): base("Project Associated Configurations")
        {
            
        }
    }
}

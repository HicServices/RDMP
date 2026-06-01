using Rdmp.Core.Curation.Data.Cohort;
using Rdmp.Core.DataExport.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rdmp.Core.Providers.Nodes.CohortNodes
{
    public class AssociatedCohortIdentificationTemplatesNode:Node, IOrderable
    {
        public Project Project { get; set; }

        public AssociatedCohortIdentificationTemplatesNode(Project project)
        {
            Project = project;
        }
        public override string ToString() => "Associated Cohort ConfigurationTemplates";

        protected bool Equals(AssociatedCohortIdentificationTemplatesNode other) =>
        Equals(Project, other.Project);

        public override bool Equals(object obj)
        {
            if (obj is null) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((AssociatedCohortIdentificationTemplatesNode)obj);
        }

        public override int GetHashCode() => Project != null ? Project.GetHashCode() : 0;

        public int Order
        {
            get => 1;
            set { }
        }
    }
}

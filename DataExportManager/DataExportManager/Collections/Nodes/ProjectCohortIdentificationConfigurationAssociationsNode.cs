using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DataExportLibrary.Data.DataTables;

namespace DataExportManager.Collections.Nodes
{
    public class ProjectCohortIdentificationConfigurationAssociationsNode
    {
        public Project Project { get; set; }

        public ProjectCohortIdentificationConfigurationAssociationsNode(Project project)
        {
            Project = project;
        }

        public override string ToString()
        {
            return "Cohort Builder Queries";
        }

        protected bool Equals(ProjectCohortIdentificationConfigurationAssociationsNode other)
        {
            return Equals(Project, other.Project);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((ProjectCohortIdentificationConfigurationAssociationsNode) obj);
        }

        public override int GetHashCode()
        {
            return (Project != null ? Project.GetHashCode() : 0);
        }
    }
}

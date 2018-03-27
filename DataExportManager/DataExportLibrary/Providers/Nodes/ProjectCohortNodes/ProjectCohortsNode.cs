using CatalogueLibrary.Data.Cohort;
using DataExportLibrary.Data.DataTables;

namespace DataExportLibrary.Providers.Nodes.ProjectCohortNodes
{
    /// <summary>
    /// Container folder immediately under a Project which contains subnodes 'ProjectSavedCohortsNode' and 'ProjectCohortIdentificationConfigurationAssociationsNode'
    /// </summary>
    public class ProjectCohortsNode:IOrderable
    {
        public Project Project { get; set; }

        public ProjectCohortsNode(Project project)
        {
            Project = project;
        }

        public override string ToString()
        {
            return "Cohort";
        }

        public int Order { get { return 1; } set { } }

        protected bool Equals(ProjectCohortsNode other)
        {
            return Project.Equals(other.Project);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((ProjectCohortsNode) obj);
        }

        public override int GetHashCode()
        {
            return Project.GetHashCode();
        }

        public static bool operator ==(ProjectCohortsNode left, ProjectCohortsNode right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(ProjectCohortsNode left, ProjectCohortsNode right)
        {
            return !Equals(left, right);
        }
    }
}

using CatalogueLibrary.Data.Cohort;
using DataExportLibrary.Data.DataTables;

namespace DataExportLibrary.Providers.Nodes.ProjectCohortNodes
{
    /// <summary>
    /// Collection of all <see cref="CohortIdentificationConfiguration"/> (queries for identifying patient lists) which are associated 
    /// with a <see cref="Project"/>.  
    /// 
    /// <para>A <see cref="CohortIdentificationConfiguration"/> can be associated with multiple Projects</para>
    /// </summary>
    public class ProjectCohortIdentificationConfigurationAssociationsNode:IOrderable
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

        public int Order { get { return 1; } set{} }
    }
}

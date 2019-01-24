using CatalogueLibrary.Data.Cohort;
using DataExportLibrary.Data.DataTables;

namespace DataExportLibrary.Providers.Nodes.ProjectCohortNodes
{
    /// <summary>
    /// Collection of all saved cohort lists (<see cref="ExtractableCohort"/>) which were saved against a <see cref="Project"/>.  Cohorts are 
    /// associated with the <see cref="Project"/> via the project number.
    /// </summary>
    public class ProjectSavedCohortsNode:IOrderable
    {
        public Project Project { get; set; }

        public ProjectSavedCohortsNode(Project project)
        {
            Project = project;
        }

        public override string ToString()
        {
            return "Saved Cohorts";
        }

        protected bool Equals(ProjectSavedCohortsNode other)
        {
            return Equals(Project, other.Project);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((ProjectSavedCohortsNode) obj);
        }

        public override int GetHashCode()
        {
            return (Project != null ? Project.GetHashCode() : 0);
        }

        public int Order { get { return 2; } set{}}
    }
}

using CatalogueLibrary.Data.Cohort;
using DataExportLibrary.Data.DataTables;

namespace DataExportLibrary.Providers.Nodes
{
    /// <summary>
    /// Collection of all <see cref="ExtractionConfiguration"/> in a given <see cref="Project"/>
    /// </summary>
    public class ExtractionConfigurationsNode:IOrderable
    {
        public Project Project { get; set; }

        public ExtractionConfigurationsNode(Project project)
        {
            Project = project;
        }

        public override string ToString()
        {
            return "Extraction Configurations";
        }

        protected bool Equals(ExtractionConfigurationsNode other)
        {
            return Equals(Project, other.Project);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((ExtractionConfigurationsNode) obj);
        }

        public override int GetHashCode()
        {
            return (Project != null ? Project.GetHashCode() : 0);
        }

        public int Order { get { return 3; } set{} }

    }
}

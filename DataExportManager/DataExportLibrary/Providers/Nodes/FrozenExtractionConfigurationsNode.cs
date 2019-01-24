using DataExportLibrary.Data.DataTables;

namespace DataExportLibrary.Providers.Nodes
{
    /// <summary>
    /// Collection of all previously extracted (and now readonly) <see cref="ExtractionConfiguration"/>s in a given <see cref="Project"/>
    /// </summary>
    class FrozenExtractionConfigurationsNode
    {
        public Project Project { get; set; }

        public FrozenExtractionConfigurationsNode(Project project)
        {
            Project = project;
        }

        public override string ToString()
        {
            return "Frozen Extraction Configurations";
        }

        protected bool Equals(FrozenExtractionConfigurationsNode other)
        {
            return Equals(Project, other.Project);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((FrozenExtractionConfigurationsNode) obj);
        }

        public override int GetHashCode()
        {
            return (Project != null ? Project.GetHashCode() : 0);
        }
    }
}

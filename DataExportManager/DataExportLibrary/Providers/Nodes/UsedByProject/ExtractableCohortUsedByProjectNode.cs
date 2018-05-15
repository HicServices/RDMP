using System.Collections.Generic;
using System.Linq;
using DataExportLibrary.Data.DataTables;

namespace DataExportLibrary.Providers.Nodes.UsedByProject
{
    public class ExtractableCohortUsedByProjectNode:IObjectUsedByProjectNode
    {
        public Project Project { get; set; }
        public object ObjectBeingUsed { get { return Cohort; }}

        public ExtractableCohort Cohort { get; set; }
        
        public ExtractableCohortUsedByProjectNode(ExtractableCohort cohort, Project project)
        {
            Cohort = cohort;
            Project = project;
        }

        public override string ToString()
        {
            return Cohort.ToString();
        }

        protected bool Equals(ExtractableCohortUsedByProjectNode other)
        {
            return Equals(Project, other.Project) && Equals(Cohort, other.Cohort);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((ExtractableCohortUsedByProjectNode) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((Project != null ? Project.GetHashCode() : 0)*397) ^ (Cohort != null ? Cohort.GetHashCode() : 0);
            }
        }

        public object MasqueradingAs()
        {
            return ObjectBeingUsed;
        }
    }
}
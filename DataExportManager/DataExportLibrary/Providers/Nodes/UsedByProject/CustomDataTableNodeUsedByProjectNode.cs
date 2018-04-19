using DataExportLibrary.Data.DataTables;

namespace DataExportLibrary.Providers.Nodes.UsedByProject
{
    public class CustomDataTableNodeUsedByProjectNode:IObjectUsedByProjectNode
    {
        public Project Project { get; set; }
        public object ObjectBeingUsed { get { return CustomTable; }}

        public CustomDataTableNode CustomTable { get; set; }
        
        public CustomDataTableNodeUsedByProjectNode(CustomDataTableNode customTable,Project project)
        {
            CustomTable = customTable;
            Project = project;
        }

        public override string ToString()
        {
            return CustomTable.ToString();
        }

        protected bool Equals(CustomDataTableNodeUsedByProjectNode other)
        {
            return Equals(Project, other.Project) && Equals(CustomTable, other.CustomTable);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((CustomDataTableNodeUsedByProjectNode) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((Project != null ? Project.GetHashCode() : 0)*397) ^ (CustomTable != null ? CustomTable.GetHashCode() : 0);
            }
        }

        public object MasqueradingAs()
        {
            return ObjectBeingUsed;
        }
    }
}
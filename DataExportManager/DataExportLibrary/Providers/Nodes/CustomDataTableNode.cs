using DataExportLibrary.Data.DataTables;

namespace DataExportLibrary.Providers.Nodes
{
    public class CustomDataTableNode
    {
        public  ExtractableCohort Cohort { get; private set; }
        public  string TableName { get; private set; }
        public bool Active { get; private set; }

        public CustomDataTableNode(ExtractableCohort cohort, string tableName, bool active)
        {
            Cohort = cohort;
            TableName = tableName;
            Active = active;
        }

        public override string ToString()
        {
            return TableName;
        }

        protected bool Equals(CustomDataTableNode other)
        {
            return string.Equals(TableName, other.TableName) && Equals(Cohort, other.Cohort);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((CustomDataTableNode) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((TableName != null ? TableName.GetHashCode() : 0)*397) ^ (Cohort != null ? Cohort.GetHashCode() : 0);
            }
        }
    }
}

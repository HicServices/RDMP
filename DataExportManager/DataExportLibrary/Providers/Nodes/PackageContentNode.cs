using DataExportLibrary.Data.DataTables;
using DataExportLibrary.Data.DataTables.DataSetPackages;
using MapsDirectlyToDatabaseTable;

namespace DataExportLibrary.Providers.Nodes
{
    public class PackageContentNode:IDeleteable
    {
        private readonly ExtractableDataSetPackageContents _contents;
        public ExtractableDataSetPackage Package { get; set; }
        public ExtractableDataSet DataSet { get; set; }

        public PackageContentNode(ExtractableDataSetPackage package, ExtractableDataSet dataSet,ExtractableDataSetPackageContents contents)
        {
            _contents = contents;
            Package = package;
            DataSet = dataSet;
        }

        public override string ToString()
        {
            return DataSet.ToString();
        }
        
        protected bool Equals(PackageContentNode other)
        {
            return Equals(Package, other.Package) && Equals(DataSet, other.DataSet);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((PackageContentNode) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((Package != null ? Package.GetHashCode() : 0)*397) ^ (DataSet != null ? DataSet.GetHashCode() : 0);
            }
        }

        public void DeleteInDatabase()
        {
            _contents.RemoveDataSetFromPackage(Package, DataSet);
        }
    }
}

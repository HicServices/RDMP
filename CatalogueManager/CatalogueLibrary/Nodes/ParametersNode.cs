using System.Linq;
using CatalogueLibrary.Data;

namespace CatalogueLibrary.Nodes
{
    public class ParametersNode
    {
        private readonly ISqlParameter[] _parameters;
        public ICollectSqlParameters Collector { get; set; }

        public ParametersNode(ICollectSqlParameters collector, ISqlParameter[] parameters)
        {
            _parameters = parameters;
            Collector = collector;
        }

        public override string ToString()
        {
            return _parameters.Length + " parameters (" + string.Join(",", _parameters.Select(p=>p.ParameterName)) + ")";
        }

        public override int GetHashCode()
        {
            return Collector.GetHashCode() * typeof(ParametersNode).GetHashCode();
        }

        public override bool Equals(object obj)
        {
            var other = obj as ParametersNode;

            if (other == null)
                return false;

            return other.GetHashCode() == GetHashCode();
        }
    }
}

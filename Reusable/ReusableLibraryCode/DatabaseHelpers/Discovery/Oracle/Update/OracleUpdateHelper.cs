using System.Collections.Generic;
using ReusableLibraryCode.DatabaseHelpers.Discovery.QuerySyntax;
using ReusableLibraryCode.DatabaseHelpers.Discovery.QuerySyntax.Update;

namespace ReusableLibraryCode.DatabaseHelpers.Discovery.Oracle.Update
{
    public class OracleUpdateHelper : UpdateHelper
    {
        protected override string BuildUpdateImpl(DiscoveredTable table1, DiscoveredTable table2, List<CustomLine> lines)
        {
            throw new System.NotImplementedException();
        }
    }
}
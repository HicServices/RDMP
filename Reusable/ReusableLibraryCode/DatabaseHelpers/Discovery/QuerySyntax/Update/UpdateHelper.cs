using System;
using System.Collections.Generic;
using System.Linq;

namespace ReusableLibraryCode.DatabaseHelpers.Discovery.QuerySyntax.Update
{
    public abstract class UpdateHelper:IUpdateHelper
    {
        /// <summary>
        /// You only have to support CustomLines that fulfil this role in the query i.e. no parameter support etc
        /// </summary>
        protected QueryComponent[] permissableLocations = new[] {QueryComponent.SET, QueryComponent.JoinInfoJoin, QueryComponent.WHERE};

        public string BuildUpdate(DiscoveredTable table1, DiscoveredTable table2, List<CustomLine> lines)
        {
            var illegalLine = lines.FirstOrDefault(l => !permissableLocations.Contains(l.LocationToInsert));
            if(illegalLine != null)
                throw new NotSupportedException();

            return BuildUpdateImpl(table1, table2, lines);
        }

        protected abstract string BuildUpdateImpl(DiscoveredTable table1, DiscoveredTable table2, List<CustomLine> lines);
    }
}
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CatalogueLibrary.DataFlowPipeline;
using ReusableLibraryCode.Checks;
using ReusableLibraryCode.Progress;

namespace DataExportLibrary.CohortCreationPipeline.Destinations.IdentifierAllocation
{
    /// <summary>
    /// Allocates a Guid for each private identifier supplied.  This will not keep track of duplicates (every call results in a new guid regardless of the input).
    /// </summary>
    public class GuidReleaseIdentifierAllocator : IAllocateReleaseIdentifiers
    {
        public object AllocateReleaseIdentifier(object privateIdentifier)
        {
            return Guid.NewGuid().ToString();
        }
    }
}

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
    public class GuidReleaseIdentifierAllocator : IAllocateReleaseIdentifiers
    {
        public object AllocateReleaseIdentifier(object privateIdentifier)
        {
            return Guid.NewGuid().ToString();
        }
    }
}

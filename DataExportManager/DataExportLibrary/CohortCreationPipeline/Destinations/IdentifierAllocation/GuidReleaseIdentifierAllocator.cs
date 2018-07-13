using System;

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

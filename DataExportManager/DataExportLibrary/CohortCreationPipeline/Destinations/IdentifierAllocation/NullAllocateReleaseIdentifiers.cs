using System;

namespace DataExportLibrary.CohortCreationPipeline.Destinations.IdentifierAllocation
{
    class NullAllocateReleaseIdentifiers : IAllocateReleaseIdentifiers
    {
        public object AllocateReleaseIdentifier(object privateIdentifier)
        {
            return DBNull.Value;
        }
    }
}
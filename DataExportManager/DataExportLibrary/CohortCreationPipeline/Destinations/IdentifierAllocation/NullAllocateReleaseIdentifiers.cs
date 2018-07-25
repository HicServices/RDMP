using System;
using DataExportLibrary.Interfaces.Pipeline;

namespace DataExportLibrary.CohortCreationPipeline.Destinations.IdentifierAllocation
{
    class NullAllocateReleaseIdentifiers : IAllocateReleaseIdentifiers
    {
        public object AllocateReleaseIdentifier(object privateIdentifier)
        {
            return DBNull.Value;
        }

        public void Initialize(ICohortCreationRequest request)
        {
            
        }
    }
}
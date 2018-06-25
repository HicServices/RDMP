using System.ComponentModel.Composition;

namespace DataExportLibrary.CohortCreationPipeline.Destinations.IdentifierAllocation
{
    [InheritedExport(typeof(IAllocateReleaseIdentifiers))]
    public interface IAllocateReleaseIdentifiers
    {
        object AllocateReleaseIdentifier(object privateIdentifier);
    }
}
using System.ComponentModel.Composition;
using DataExportLibrary.Interfaces.Pipeline;

namespace DataExportLibrary.CohortCreationPipeline.Destinations.IdentifierAllocation
{
    /// <summary>
    /// Class responsible for allocating Release Identifiers for a Cohort that is being committed (see <see cref="BasicCohortDestination"/>) when the user has not supplied any in 
    /// the file/cohort he is uploading.
    /// </summary>
    [InheritedExport(typeof(IAllocateReleaseIdentifiers))]
    public interface IAllocateReleaseIdentifiers
    {
        object AllocateReleaseIdentifier(object privateIdentifier);
        void Initialize(ICohortCreationRequest request);
    }
}
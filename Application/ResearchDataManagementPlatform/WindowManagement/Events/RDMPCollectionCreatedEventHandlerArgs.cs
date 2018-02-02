using CatalogueManager.Collections;

namespace ResearchDataManagementPlatform.WindowManagement.Events
{
    /// <summary>
    /// Arguments for when an RDMPCollection has been made visible (opened)
    /// </summary>
    public class RDMPCollectionCreatedEventHandlerArgs
    {
        public readonly RDMPCollection Collection;

        public RDMPCollectionCreatedEventHandlerArgs(RDMPCollection collection)
        {
            Collection = collection;
        }
    }
}
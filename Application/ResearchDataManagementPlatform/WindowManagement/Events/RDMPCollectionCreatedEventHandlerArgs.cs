namespace ResearchDataManagementPlatform.WindowManagement.Events
{
    public class RDMPCollectionCreatedEventHandlerArgs
    {
        public readonly RDMPCollection Collection;

        public RDMPCollectionCreatedEventHandlerArgs(RDMPCollection collection)
        {
            Collection = collection;
        }
    }
}
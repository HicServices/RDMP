namespace CatalogueLibrary.Data
{
    public class CatalogueExtractabilityStatus
    {
        public bool IsExtractable { get; private set; }
        public bool IsProjectSpecific { get; private set; }

        public CatalogueExtractabilityStatus(bool isExtractable, bool isProjectSpecific)
        {
            IsExtractable = isExtractable;
            IsProjectSpecific = isProjectSpecific;
        }
    }
}
namespace CatalogueLibrary.Data
{
    /// <summary>
    /// <para>Describes whether a Catalogue can be extracted in data export projects and if so, whether it is only permitted in a single Project.</para>
    /// 
    /// <para>See <see cref="Catalogue.GetExtractabilityStatus"/></para>
    /// </summary>
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
namespace CatalogueLibrary.Data
{
    /// <summary>
    /// <para>Describes whether a Catalogue can be extracted in data export projects and if so, whether it is only permitted in a single Project.</para>
    /// 
    /// <para>See <see cref="Catalogue.GetExtractabilityStatus"/></para>
    /// </summary>
    public class CatalogueExtractabilityStatus
    {
        /// <summary>
        /// The <see cref="Catalogue"/> is extractable as an ExtractableDataSet in data export database
        /// </summary>
        public bool IsExtractable { get; private set; }

        /// <summary>
        /// The <see cref="Catalogue"/> is extractable as an ExtractableDataSet in data export dabase but only for use in a single
        /// Project.
        /// </summary>
        public bool IsProjectSpecific { get; private set; }

        /// <summary>
        /// Creates a new confirmed extractability knowledge for a <see cref="Catalogue"/>
        /// </summary>
        /// <param name="isExtractable"></param>
        /// <param name="isProjectSpecific"></param>
        public CatalogueExtractabilityStatus(bool isExtractable, bool isProjectSpecific)
        {
            IsExtractable = isExtractable;
            IsProjectSpecific = isProjectSpecific;
        }
    }
}